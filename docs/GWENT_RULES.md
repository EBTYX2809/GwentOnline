# Gwent Rules — As Implemented in This Codebase

This document describes the ruleset **actually coded** in `Gwent_Release`, not the full official
Witcher 3 Gwent ruleset. Only two factions are implemented (North Kingdoms, Nilfgaard); Scoia'tael
and Monsters do not exist in this project (see `README.md` for why — the author judged them weak
or repetitive). Treat this file as the authoritative spec when porting or extending game logic —
if behavior described here disagrees with the code, the code is a bug to reconcile, not this doc.

## 1. Match structure

- A match is **best of 3 rounds**. First player to lose 2 rounds loses the match
  (`Player.IsSecondRoundLoose`). See `RoundWinnerManagerNS/*`.
- Each player has a **leader card** (an `ActionCard`, one per faction) usable once per match.
- Two players only. No AI opponent exists yet (`TurnManager.EnemyTurn` currently deserializes the
  *human* opponent's network move — see `docs/NETWORKING.md`).

## 2. Deck building

- Deck = faction deck (North Kingdoms or Nilfgaard, chosen at the menu) + the shared Neutral deck.
- `Player.CreateDeck(Fractions)` (`Models/PlayerNS/Player.cs`) builds the deck:
  1. Copy all `CardsStore.NeutralDeck` cards into the deck.
  2. Copy all cards of the chosen faction deck into the deck.
  3. The **last card of the faction list is always the Leader** — it's pulled out via
     `Deck.DeckCards.Last()` and removed from the drawable deck. This is why `CreateNorthKingdomsDeck`
     / `CreateNilfgaardDeck` always append the leader card (`Foltest` / `Emhyr`) last.
  4. Remaining cards are shuffled (`Random` + `OrderBy`).
  5. Player draws **10 cards** to hand (`TakeCard(10)`).
- Deck sizes: North Kingdoms = 26 non-leader cards + Foltest; Nilfgaard = 23 non-leader cards +
  Emhyr; Neutral = 14 cards. See `docs/ARCHITECTURE.md` §CardsStore for the full card list.

## 3. Mulligan

- After drawing the opening hand, each player may **swap up to 2 cards** back into the deck for
  random replacements (`MainWindow.StartMuligan`, client-side only, 1-per-click via the discard
  overlay UI repurposed as a mulligan picker).
- Mulligan is **not communicated over the network** during the swap itself — only the final hand
  is sent afterward via `Client.SwapDeck`.

## 4. Turn structure

- Players alternate turns; a coin toss (`Client.TossCoin`) decides who starts (`StarterPlayer`).
- On your turn you either:
  - **Play exactly one card** (occasionally two, for effects that target another card — see §6),
    or
  - **Pass** (`HasPassed = true`). Once passed, a player takes no more actions this round; the
    opponent keeps playing alone until they also pass.
- A round ends when **both players have passed** (`GameContext.EndTurn` → `EndRound`).
- If a player has already passed, the game does **not** wait for their input again this round —
  turns are simply skipped to the still-active player.

## 5. Board layout

Three battle rows per player, matching the source game:

| Row | Enum | Typical cards |
|---|---|---|
| Melee (Close Combat) | `BattleRows.MeleeBattleRow` | Blue Stripes Commando, Impera Brigade Guard, most Heroes |
| Ranged (Middle) | `BattleRows.MiddleBattleRow` | Crinfrid Reavers Dragon, Black Infantry Archer |
| Siege | `BattleRows.SiegeBattleRow` | Ballista, Catapult, Trebuchet |

Plus a shared **Weather row** (`BattleRows.WeatherCardsBattleRow`) that isn't owned by either
player — weather cards live there and apply their debuff to the matching row on *both* sides.

Each row also has a single **Horn slot** (`BattleRow.HornSlot`) for a played Commander's Horn.

## 6. Card types and mechanics

Implemented via the `Effect` / `EffectModifier` delegate system
(`Models/EffectsNS/EffectModifiersStore.cs`) — see `docs/ARCHITECTURE.md` for the mechanism.
`EffectModifier`s recompute a row's score continuously (score-affecting, ordered by `Priority`);
plain `Effect`s fire once when the card is played (one-shot triggers).

### One-shot effects (`Effect`)
- **Medic** — On play, revive one unit card from your own discard pile and play it. UI-driven via
  `GiveMedicCardToContext` (a `Func` the View wires up to show a discard picker).
  Cards: Dun Banner Medic, Menno Coehoorn, Etolian Auxiliary Archers, Yennefer of Vengerberg.
- **Spy** — On play, the card is placed in the **opponent's** row (see `SpawnCard`), and you draw
  2 cards. Cards: Prince Stennis, Sigismund Dijkstra, Thaler, Shilard Fitz-Oesterlen,
  Stefan Skellen, Vattier de Rideaux, Avallac'h (Hero).
- **Scorch** — Destroys **all** unit cards (both players, all rows) tied for the single highest
  power on the board. Card: Scorch (neutral action card).
- **Melee Scorch** — Destroys the highest-power unit(s) in **both players' melee rows only**
  (ties across both sides all die). Card: Villentretenmerth.
- **Decoy** — Swap a card already on your board back to hand, replacing it in the row with the
  Decoy card itself (used to re-trigger Spy effects or save a unit from Scorch/weather).
- **Clear Weather** — Removes all weather cards and their row-effects from the board.
- **Emhyr card steal** (leader-only, Nilfgaard) — Take a unit card from the opponent's discard
  pile and play it on your own board.
- **Foltest card from deck** (leader-only, North Kingdoms) — Take a unit card from your own deck
  (not hand) and play it directly.

### Continuous modifiers (`EffectModifier`, sorted by `Priority` before recompute)
| Modifier | Priority | Effect |
|---|---|---|
| Frost / Fog / Rain (weather) | 0 | Sets every unit card's power in the affected row to 1 |
| Bond | 1 | If N copies of the same base unit are in a row, each copy's power = `DefaultPower × N` (or `1 × N` if the row is also under a weather effect — weather applies first due to priority) |
| Buff | 2 | +1 power (Kaedweni Siege Expert) |
| Horn (unit-form, e.g. Dandelion) / Commander's Horn (action card) | 3 | Doubles every unit's power in the row |

Row score priority order matters: weather (0) zeroes cards to 1 before Bond (1) multiplies, before
Buff (2) adds, before Horn (3) doubles the final total — this order is load-bearing for correct
Gwent math and must be preserved in any reimplementation.

### Weather cards (`WeatherCard`)
- **Biting Frost** → debuffs Melee row.
- **Impenetrable Fog** → debuffs Ranged/Middle row.
- **Torrential Rain** → debuffs Siege row.
- Weather cards are placed in the shared weather row (`DropBattleRow`) but apply their modifier to
  a *different* row (`ActionBattleRow`) on both players' boards simultaneously.

### Commander's Horn
- Exists as **both** a dedicated `ActionCard` (neutral, occupies the row's horn slot only, no
  board slot) and as an inherent ability on the unit card **Dandelion** (occupies a normal board
  slot *and* acts as the row's horn). Only one horn effect applies per row (`HornSlot == null`
  gate in `BattleRow.AddCardToBattleRow`).

## 7. Faction abilities

- **Nilfgaard**: wins any round that ends in a score draw (`RoundWinnerManager.DrawInRound`).
  If *both* leaders are Nilfgaard, or *both* are North Kingdoms, a draw stays a draw (both players
  take a round loss) — the ability only breaks ties against a *different* faction.
- **North Kingdoms**: draws 1 extra card at the start of the round immediately **after winning** a
  round (`Player1WinRound` / `Player2WinRound`, `if Leader.Fraction == NorthKingdoms → TakeCard(1)`).

## 8. Scoring & round end

- `Player.GeneralScore` = sum of all three `BattleRow.BatteRowScore` values, recomputed live on
  every card add/remove/effect change (event-driven, not polled).
- Highest `GeneralScore` wins the round; ties resolve per §7 (Nilfgaard) or count as a mutual loss
  for both players otherwise.
- At round end, all board rows are cleared to both discard piles (`StartNewRound`) and weather is
  cleared; hands and discard piles persist between rounds (this matches source-game Gwent).
- Whoever lost the *previous* round goes first in the next round
  (`GameContext.StartNewRound` flips active player relative to `StarterPlayer`).

## 9. Not implemented (intentional scope cuts, per README)

- Scoia'tael and Monsters factions (and their unique mechanics: Elven mercenaries, self-wounding
  Monster row stacking, etc.)
- Single-player / AI opponent (planned future work — see `AGENTS.md`)
- Any ranked/matchmaking system beyond "both players type the same server and wait ~1 minute"
