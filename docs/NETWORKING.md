# Networking — Online Play Protocol

The client in this repo talks to a **separate server project**
(`https://github.com/EBTYX2809/ServerForGwent`, not part of this checkout). Everything below is
reverse-engineered from the client side (`Models/Client.cs`, `Models/TurnManager.cs`,
`ViewModels/MenuViewModel.cs`) — if the server repo's actual behavior ever disagrees with this
doc, the server is the source of truth for wire format, but this doc is the source of truth for
*how the client interprets and drives it*.

## 1. Transport

- Raw **TCP socket** (`System.Net.Sockets.TcpClient`), default port hardcoded to `10000`.
- Default server address: `http://gwent-server.duckdns.org` (a plain TCP host despite the
  `http://` in the string — `TcpClient.Connect` ignores the scheme; this is a naming leftover, not
  actual HTTP). User can override via the IP field in the menu.
- Payloads are raw **UTF-8 strings**, no length-prefixing or framing — `SendInfo` writes bytes
  directly; `ReceiveInfo` reads into a fixed 1024-byte buffer per call and decodes whatever arrived
  in that read. This assumes each logical message fits in one TCP read and arrives as one segment;
  it will break on large/fragmented messages (README's own "there are only 2 decks" scope-limiting
  suggests this was never stress-tested against long move strings).
- No TLS, no auth, no reconnect/resume logic. If the socket errors, the user sees a `MessageBox`
  and the session is effectively dead (`GameContext.ReturnToMenuWindow`).
- The README notes the *actual* server implementation is WebSockets + TCP — this client only
  speaks the raw TCP side of whatever the server exposes.

## 2. Session / matchmaking handshake

Sequence, driven by `MenuViewModel.Play` → `MainWindow.MainWindow_Loaded`:

1. **Connect**: `Client.Connect(ip, 10000)`. Shows a blocking `MessageBox` on success/failure
   (this blocks the UI thread — a known rough edge, worth fixing before any real web equivalent).
2. **Queue for opponent**: `Client.WaitingSecondPlayer()` sends `"Ready|{PlayerName}"`, then waits
   for a response:
   - `"Ready|{OpponentName}"` → matched; opponent's name stored in `Player2.Name`; proceed.
   - `"Timeout"` → server gave up after ~1 minute of no second player; shows a message, matchmaking
     aborts, user stays on the menu.
3. **Local player build their deck & mulligan** entirely client-side (no network traffic during
   mulligan — see `docs/GWENT_RULES.md` §3).
4. **Exchange decks**: `Client.SwapDeck(List<Card>)` sends
   `"{LeaderJsonNameKey}|{card1}|{card2}|...|{cardN}"` (leader first, then hand + remaining deck in
   whatever order they currently sit in `Hand.HandCards` + `Deck.DeckCards`), and awaits the
   identical format from the opponent. The receiving side (`Player.CreateDeck(string)`) always
   treats the **first 10 non-leader entries as the received hand** and the rest as their deck —
   order matters and is implicitly relied upon by both ends agreeing on "hand size = 10."
5. **Coin toss**: `Client.TossCoin()` sends `"Toss coin."`, expects back the literal string
   `"active"` or `"passive"` — server decides and tells each client its role; client sets
   `ActivePlayer`/`StarterPlayer`/`PassivePlayer`/`IsPlayer1Turn` accordingly.
6. Game proper begins; if the network opponent is active first, `TurnManager.EnemyTurn()` is
   invoked immediately to wait for their opening move.

## 3. Per-turn move format

Built by `TurnManager.GiveTurn()` right before your local player's `StartTurn` handoff:

```
turnInfo = string.Join("|", PlayedCards.Select(c => c.JsonNameKey));
if (HornRow > 0) turnInfo += "|" + HornRow;
if (turnInfo == "") turnInfo = "pass";
```

- **Passing** a turn sends the literal string `"pass"`.
- **Playing a card** sends just its `JsonNameKey` (e.g. `"Scorch"`).
- **Playing two related cards in one turn** (e.g. Medic reviving a discard card, Decoy swapping a
  board card, Emhyr/Foltest stealing/pulling a card) sends **both** `JsonNameKey`s pipe-joined,
  first-played first: `"Dun Banner Medic|Ves"`. `TurnManager.PlayedCards` accumulates every card
  touched during the local turn and is flushed as one message.
- **Playing to a Horn slot** appends the **numeric row value** of the `BattleRows` enum
  (`Melee=1, Middle=2, Siege=3`) as an extra trailing segment: `"Commander's Horn|1"`. This is why
  `TurnManager.EnemyTurn`'s parser special-cases any segment equal to the literal string `"1"`,
  `"2"`, or `"3"` as a horn-row indicator rather than a card name — **card names must never
  collide with these digit strings** (not a risk today since `JsonNameKey`s are always English
  card titles, but worth remembering if card keys are ever machine-generated).
- After sending, `PlayedCards`/`HornRow`/`turnInfo` are all cleared locally — this buffer only
  ever represents "cards played so far *this* turn," not turn history.

### Receiving (`TurnManager.EnemyTurn`)

1. `await client.ReceiveInfo()` blocks for the opponent's move string.
2. If `"pass"` → calls `GameContext.Instance.StartTurn(null)` (drives the same pass codepath as a
   local pass) and returns.
3. Otherwise splits on `|`. Builds a lookup pool of **every card the opponent could legitimately
   reference** — their hand, discard, all three board rows, *and*, conditionally, their deck
   (only if their leader is Foltest — `FoltestCardFromDeckPlay`) or the local player's discard
   pile (only if their leader is Emhyr — `EmhyrCardSteal`). Each pipe segment is matched by
   `JsonNameKey` against that pool (digit segments `1`/`2`/`3` are captured as `HornRow` instead of
   looked up).
4. The resolved `Card` objects are pushed into the same `TurnManager.PlayedCards` list a local play
   would populate, then the method **replays the exact same effect-dispatch branching** `MainWindow`
   would have used locally (Horn → row's horn slot; Emhyr/Foltest steal → `ActivateEffect` +
   optional Medic chain; Decoy → `ActivateEffect`; Medic → `MedicRevive`; weather → add to weather
   row + `StartTurn`; Scorch/ClearWeather → `ActivateEffect(null)` + `StartTurn`; anything else →
   `EffectModifiersStore.SpawnCard`, which places the card on the *opponent's own* row, or the
   *local* player's row if the card's effect is `Spy`).

**Implication for an AI opponent or a web port**: the wire format is a thin serialization of "which
cards moved where," not a command/intent protocol — the receiving side re-derives *all* game
consequences locally by re-running the same rule functions the sender used. Both sides must own an
identical copy of the rules and card database, and nothing here validates that the opponent's move
was legal. A trusted single-source-of-truth server (validating moves once, broadcasting resolved
state) would be a meaningful architecture change, not just a transport swap.

## 4. Disconnect handling

- `Client.ReceiveInfo()` treats the literal string `"Disconnect"` as a special signal: shows
  "Opponent disconnected," then calls `GameContext.Instance.ReturnToMenuWindow(this)`.
- `GameContext.ReturnToMenuWindow` opens a fresh `Menu` window, resets `GameContext` (via a private
  `RestartGameContext` that preserves only `Player1.Name`), and — as its **last** step — sends
  `"Disconnect"` to the (soon-to-be-closed) socket, notifying the *other* end. This double-duty
  (both the trigger for reacting to a disconnect and the message sent when you cause one) is worth
  knowing before touching this code, since it's easy to create a signal loop if refactored
  carelessly.
- No heartbeat/ping — a hard network drop without an explicit `"Disconnect"` string will just hang
  the awaiting `ReceiveInfo()` call indefinitely (or throw, caught by a bare `catch (Exception ex)
  {}` that swallows the error silently in `SendInfo`, and shows a `MessageBox` in `ReceiveInfo`).

## 5. Porting notes for the web version

- Browsers can't open raw TCP sockets — a web client needs **WebSockets** (the README already
  claims the *server* speaks WebSockets in addition to TCP, so the desktop client may simply be
  using the wrong half of an already-dual-protocol server; worth checking the server repo before
  assuming a server rewrite is required).
- The pipe-delimited, position-and-context-dependent string format (digits as horn markers,
  variable card counts per message, implicit "first 10 = hand" contracts) is fragile and
  undocumented outside this file and the code itself. A web rewrite is a natural point to move to
  a typed JSON message protocol — but if the existing server must be kept, the new client has to
  reproduce this exact parsing behavior, including the `EnemyTurn` "which cards *could* the
  opponent mean" resolution scope (hand ∪ discard ∪ rows ∪ conditional deck/opponent-discard).
- Because move resolution is fully duplicated client-side rather than server-authoritative, a web
  version open to the public (vs. "call a friend") should not carry this trust model forward
  as-is — flag this to the user explicitly if server-authoritative validation isn't already planned.
