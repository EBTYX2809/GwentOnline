# Code Architecture — Gwent_Release (WPF Client)

Total source: ~3,360 lines of C# across ~28 files. Single project, `net8.0-windows`, WPF
(`UseWPF=true`), `WinExe`. No unit tests, no server code (server lives in a separate repo,
`EBTYX2809/ServerForGwent`, not part of this checkout — see `docs/NETWORKING.md`).

The author (solo developer, self-taught on this project) has described the code as rough by their
own admission — treat the "known issues" callouts in each section as an honest map of where the
code will fight you, not criticism to fix reflexively. Some of it (the Singleton `GameContext`,
absolute-Canvas UI, view-layer game logic) is load-bearing enough that a web port is more
"reimplement using this as the spec" than "refactor in place."

## 1. Namespace / folder map

```
Gwent_Release/
├── App.xaml(.cs)                  WPF application entry point (near-empty, no App-level DI)
├── Models/
│   ├── GameContext.cs             Singleton: whole-game state machine (turns, rounds, players)
│   ├── TurnManager.cs             Static class: serializes/deserializes turns over the network
│   ├── Client.cs                  Raw TCP client (connect, send/receive UTF-8 strings)
│   ├── LanguageManager.cs         Loads per-language card name/description JSON into CardInfo
│   ├── MusicBG.cs                 Background music playlist/volume (static, wraps MediaElement)
│   ├── Fractions.cs               enum: NorthKingdoms, Nilfgaard, Neutral
│   ├── BattleRows.cs              enum: None, Melee, Middle, Siege, WeatherCardsBattleRow
│   ├── CardsNS/
│   │   ├── Card.cs                Base card class (INotifyPropertyChanged, CopyCard prototype)
│   │   ├── UnitCard.cs            Card with DefaultCardScore (baseline for buffs/debuffs)
│   │   ├── ActionCard.cs          Non-board one-shot cards (Decoy, Scorch, Leaders, Horn item)
│   │   ├── HeroCard.cs            Cards immune to power modification (per Gwent rules)
│   │   ├── WeatherCard.cs         Card + DropBattleRow/ActionBattleRow routing
│   │   ├── CardInfo.cs            POCO: localized Name + Description
│   │   └── CardsStore.cs          Static master card lists (the "database"): Neutral/NK/Nilfgaard
│   ├── EffectsNS/
│   │   ├── Effect.cs              Action<UnitCard> wrapper — one-shot card behavior
│   │   ├── EffectModifier.cs      Effect + Priority — continuous row-score modifier
│   │   └── EffectModifiersStore.cs Static singleton instances of every effect + their logic
│   ├── PlayerNS/
│   │   ├── Player.cs              Facade: hand/deck/discard/rows/leader/score for one player
│   │   ├── BattleRow.cs           Facade: one row's cards, horn slot, modifiers, scoring
│   │   ├── Deck.cs                Thin ObservableCollection<Card> wrapper (+ CardsCount)
│   │   └── Hand.cs                Thin ObservableCollection<Card> wrapper (+ CardsCount)
│   └── RoundWinnerManagerNS/
│       ├── RoundWinnerManager.cs              Strategy pattern scaffold (IWinRound, RoundWinner)
│       ├── RoundWinnerManager.Player1WinRound.cs
│       ├── RoundWinnerManager.Player2WinRound.cs
│       └── RoundWinnerManager.DrawInRound.cs  Nilfgaard tie-break logic lives here
├── ViewModels/
│   ├── MenuViewModel.cs           Menu screen state + connect/matchmake flow
│   └── RelayCommand.cs            Standard ICommand implementation
└── Views/
    ├── MainWindow.xaml(.cs)       The entire game board — 800 lines of code-behind, heavy logic
    ├── Menu.xaml(.cs)             Landing screen (name, IP, faction, language, card gallery)
    └── CardPointsToColorConverter.cs  IValueConverter: green/red/black/white score coloring
```

`Properties/Resources.Designer.cs` and `Settings.Designer.cs` are VS-generated boilerplate, not
hand-written — ignore them when reasoning about game logic.

## 2. Design patterns actually in use

- **Singleton** — `GameContext.Instance`. Owns both `Player` objects, whose turn it is, the shared
  weather row, and drives the round/turn state machine. Implements `INotifyPropertyChanged`
  directly (inherits from `RoundWinnerManager` to get access to its protected round-winner
  helpers — an unusual inheritance choice used purely for code reuse, not an "is-a" relationship).
- **Prototype** — `Card.CopyCard()` / overrides in each subclass. `CardsStore` holds one canonical
  instance of each card; `Player.CreateDeck` clones them per-match so mutable state
  (`ActualCardScore`, board position) never leaks between games or between two players who picked
  the same faction.
- **Strategy** — `RoundWinnerManagerNS`: `IWinRound` + `Player1WinRound`/`Player2WinRound`/
  `DrawInRound`, selected at runtime via `RoundWinner.SetWinner()`.
- **Facade** — `Player` and `BattleRow` expose high-level operations (`AddCardToBattleRow`,
  `TakeCard`, `ScoringGeneralScore`) so `MainWindow` and `EffectModifiersStore` don't manipulate
  raw `ObservableCollection`s directly (though in practice several call sites reach into
  `.BattleRowCards`/`.HandCards` directly anyway — the facade is not strictly enforced).
- **Delegate-based "Strategy for cards"** — every card's unique behavior is a `static readonly`
  `Effect`/`EffectModifier` instance in `EffectModifiersStore`, assigned by reference to any number
  of `Card.Effect` properties. Card identity for behavior purposes is checked via
  `card.Effect == EffectModifiersStore.Spy` (reference equality against the store's singletons) —
  **not** a card-type enum or string tag. This is the single most important mechanism to
  understand before touching card logic: effects are pluggable objects, not attributes.
- **MVVM (partial)** — `Menu`/`MenuViewModel` follow MVVM reasonably cleanly via `RelayCommand`
  and bindings. `MainWindow`, however, has **no ViewModel** — it binds directly to
  `GameContext.Instance` (a Model) via `{x:Static}` and puts all interaction logic (drag/drop,
  turn orchestration, UI highlighting) in code-behind. This is the biggest architectural
  divergence from "clean" MVVM and the biggest single obstacle to a straightforward port.

## 3. The turn/effect execution pipeline (read this before changing game logic)

This is the part most worth understanding deeply, since it's the part a web/AI port has to
reproduce exactly.

1. **User drags a card** onto a row (`Player1MeleeBattleRow_Drop` etc. in `MainWindow.xaml.cs`) or
   clicks the leader/pass button. The handler validates the drop is legal (row match, Spy can't be
   dropped on your own row, etc.), removes the card from `Hand.HandCards`, then calls
   `BattleRow.AddCardToBattleRow(card)`.
2. **`BattleRow.AddCardToBattleRow`** branches on `card.Effect` by reference equality
   (Medic / Bond / Horn get special insertion logic — Bond inserts adjacent to its duplicate,
   Medic may await a UI card-picker via the `GiveMedicCardToContext` `Func` callback). For the
   common case it just adds the card, then:
   - If `Effect` is an `EffectModifier` → `AddEffectModifier` (registers it on the row, triggers
     rescoring).
   - If `Effect` is a plain `Effect` → `ActivateEffect(null)` fires immediately (Scorch, Spy, etc.
     run right here).
   - Always ends with `ScoringPointsInRow()` (walks every `EffectModifier` on the row, applies them
     in `Priority` order to every unit card) then fires `CardAddedToBattleRow` (→
     `Player.ScoringGeneralScore`, recomputes the player's total) and finally calls
     `TurnManager.PlayedCards.Add(card)` + `GameContext.Instance.StartTurn(card)`.
3. **`GameContext.StartTurn(card)`** is the turn-completion checkpoint for *every* card play and
   for passing (`card == null`). It flips `TurnManager` bookkeeping
   (`GiveTurn`/`ClearTurnInfo` depending on which physical player you are — see
   `docs/NETWORKING.md`), fires the on-screen `TurnAnnouncement`, and calls `EndTurn`.
4. **`GameContext.EndTurn`** checks both players' `HasPassed`. If both passed → `EndRound`
   (scores compared, `RoundWinnerManager` strategy invoked, board cleared, next round started or
   game-over `MessageBox` shown). Otherwise it flips `ActivePlayer`/`PassivePlayer`
   (`SwitchTurn`), and if the new active player is the network opponent, kicks off
   `TurnManager.EnemyTurn()` to await their move over the socket.
5. **`TurnManager.EnemyTurn`** is the receiving half: it blocks on `client.ReceiveInfo()`, parses
   the pipe-delimited move string back into real `Card` objects (matched by `JsonNameKey` against
   the local player's hand/discard/rows/deck — both clients maintain a full mirrored simulation,
   nothing is server-authoritative), then re-plays the same `AddCardToBattleRow` / effect-activate
   codepath the local player would have taken. This means **card logic only needs to be
   implemented once** — both the human-driven and network-driven paths funnel through the same
   `BattleRow`/`EffectModifiersStore` methods. A future AI opponent should hook in at this same
   point (feed `TurnManager.PlayedCards` + call the same `AddCardToBattleRow`/`StartTurn`
   sequence) rather than reimplementing rules.

### Known issues in this pipeline (be aware, don't "fix" blindly mid-task)
- Game rules, network deserialization, and WPF drag/drop are **not separated** — `BattleRow`,
  `EffectModifiersStore`, and `TurnManager` all reach into `GameContext.Instance` /
  `MainWindow`-owned state directly. There is no pure "rules engine" module you can lift out
  today; extracting one is effectively the first milestone of any web port.
- Trust model: each client is fully authoritative over its own simulated opponent state and only
  exchanges terse move strings. Fine for "call a friend and both launch the game," not fine as-is
  for a public web service (no server-side rule validation — a modified client could send illegal
  moves). Flag this explicitly if the web version will have any adversarial users.
- Several effects (`Decoy`, `Medic`) directly manipulate `GameContext.Instance.ActivePlayer.*`
  and UI callback `Func`s rather than returning data — makes them hard to unit test or run
  headless (e.g., for an AI's lookahead/simulation).
- Typos preserved from the original code you'll see throughout: `"Oponent"` (Opponent),
  `"Balista"` (Ballista, in JSON localization only, not code), `BatteRowScore` (BattleRowScore).
  Keep them if consistency with existing bindings/JSON matters more than fixing typos in a given
  task; call out to the user before doing a sweeping rename.

## 4. `CardsStore` — the card database

`Models/CardsNS/CardsStore.cs` is a static class with three `List<Card>` fields
(`NorthKingdomsDeck`, `NilfgaardDeck`, `NeutralDeck`) populated in a static constructor. Each card
is instantiated once here with its `JsonNameKey` (must exactly match a key in
`JsonLanguages/CardsInfo{EN,RU,UA}.json` and an image file `Images/{JsonNameKey}.jpg`), base power,
`Fractions`, `BattleRows`, and (if applicable) an `Effect`/`EffectModifier` reference from
`EffectModifiersStore`. See `docs/GWENT_RULES.md` §2 for the full deck contents and §6 for what
each effect does. **This file is effectively the single source of truth for "what cards exist" —**
any new card must be added here, given a JSON entry in all 3 language files, and given a matching
image asset.

## 5. Persistence & external data

- **No database.** All state is in-memory, per-process, reset when both windows close.
- **Localization**: `LanguageManager.SetLanguage("EN"|"RU"|"UA")` loads
  `JsonLanguages/CardsInfo{lang}.json` (a flat `{ "JsonNameKey": { "Name", "Description" } }` map)
  and pushes a `CardInfo` onto every card in all three `CardsStore` lists. Called once at static
  init (`EN` default) and again whenever the menu's language `ComboBox` changes.
- **Images**: all card art and UI chrome are embedded WPF `Resource`s (`pack://application:,,,/Images/...`),
  declared individually in the `.csproj` (`<Resource Include="Images\...jpg" />` per file — very
  verbose, machine-generated-looking list). Music is `Content` with `PreserveNewest` instead of an
  embedded resource (loaded via `pack://siteoforigin`).
- **Settings**: `Properties/Settings.Designer.cs` is present but effectively unused by game logic
  (no `Settings.settings` values referenced in the hand-written code beyond what VS scaffolds).

## 6. Build / run

- Requires Windows + .NET 8 SDK (`net8.0-windows`, WPF) — **cannot build or run on non-Windows
  hosts**, and there's no headless/console mode.
- `dotnet build Gwent.sln` or open `Gwent.sln` in Visual Studio 2022+.
- Running requires a reachable Gwent server (see `docs/NETWORKING.md`) for anything past the menu
  screen — there is no offline/practice mode against a bot yet.
- CI (`.github/workflows/auto_update_workflow.yml`) builds a Docker image on version tags
  (`v*`), extracts a published `Gwent.zip`, and ships it to a self-hosted runner — this pipeline
  builds the release artifact, it does not run tests (none exist).
