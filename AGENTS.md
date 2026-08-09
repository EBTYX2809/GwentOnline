# AGENTS.md — GwentOnline

Read this before making changes. It's the entry point for working in this repo; deep-dive docs it
links to live in `docs/`. Keep this file and the linked docs updated as the project evolves —
especially once the web port begins, since that will invalidate parts of the WPF-specific sections.

## What this project is

A standalone C#/WPF recreation of the Gwent card minigame from *The Witcher 3: Wild Hunt*, built
solo by the repo owner as a personal project (not a commercial/licensed product). Current state:

- **Online 1v1 only**, played by two human players who both launch the client and connect to a
  Gwent-specific TCP server (a **separate repo**, `EBTYX2809/ServerForGwent` — not in this
  checkout). There is no offline mode and no AI opponent yet.
- Two selectable factions (**North Kingdoms**, **Nilfgaard**) plus a shared Neutral card pool.
  Scoia'tael and Monsters were deliberately left out (see root `README.md`).
- Full ruleset otherwise matches source-game Gwent: 3 rows, weather, Bond/Horn/Buff/Medic/Spy/
  Scorch/Decoy, best-of-3 rounds, faction abilities. See `docs/GWENT_RULES.md` for the exact
  implemented mechanics (not the general internet ruleset — this project's specific rules).
- The author has flagged the code as rough/self-taught ("говнокод"). Expect inconsistent patterns,
  logic embedded in UI code-behind, and some genuine bugs — see "Known issues" pointers throughout
  `docs/`. Don't assume odd-looking code is a bug without checking; a lot of it is intentional
  (e.g. reference-equality dispatch on effect singletons — see `docs/ARCHITECTURE.md` §2-3).

## Stated near-term direction (context for design decisions)

1. **Port to a web version.** Not started. The current WPF client cannot run outside Windows and
   uses an absolutely-positioned `Canvas` UI at a fixed resolution with heavy game logic living in
   the View's code-behind (`MainWindow.xaml.cs`) — see `docs/ARCHITECTURE.md` §3 and
   `docs/UI_VISUALS.md` §6 for what specifically won't survive the port as-is, and
   `docs/NETWORKING.md` §5 for the transport-layer implications (raw TCP won't work from a
   browser; the protocol itself is also client-authoritative and undocumented outside this repo).
   How exactly the *online multiplayer* piece moves to the web is still an open decision — don't
   assume a specific architecture (keep the C# server, rewrite it, go serverless, etc.) unless the
   user has said so in the current conversation.
2. **AI opponent.** Explicitly deferred — the user intends to build this themselves later with
   help. When that work starts, `docs/ARCHITECTURE.md` §3 points at the natural integration seam:
   `TurnManager.EnemyTurn()` currently deserializes a *human* opponent's network move and replays
   it through the same `BattleRow`/`EffectModifiersStore` calls a local play would use. An AI
   should plug into that same call surface (decide a move, then drive the existing
   `AddCardToBattleRow`/`StartTurn` pipeline) rather than duplicating rule logic. Don't start
   building this unless explicitly asked — the user said "later," and it's called out here only so
   future-you doesn't have to rediscover the seam.

## Where to look for what

| Question | Doc |
|---|---|
| What are the actual game rules/mechanics as coded? | `docs/GWENT_RULES.md` |
| How is the C# codebase structured, what patterns, what's the turn/effect pipeline? | `docs/ARCHITECTURE.md` |
| How does client-server communication work today? | `docs/NETWORKING.md` |
| What does the UI look like, how does drag/drop and highlighting work, what's the layout? | `docs/UI_VISUALS.md` |
| High-level pitch, screenshots, "why this exists" | root `README.md` |

## Build & run

- Windows-only, .NET 8 SDK, WPF (`net8.0-windows`). No cross-platform build target exists.
- `dotnet build Gwent.sln` (or open in Visual Studio 2022+).
- Running past the menu screen requires a reachable Gwent server — see `docs/NETWORKING.md`. There
  is currently no way to test game logic without a live second client + server connection (no
  local/offline/bot mode, no unit tests, no headless harness).
- CI (`.github/workflows/auto_update_workflow.yml`) only builds & ships a release artifact on
  version tags; it does not run any tests.

## Repo layout at a glance

```
Gwent.sln
Gwent_Release/          the entire WPF app (single project)
  Models/                game state, cards, effects, players, networking client
  ViewModels/             menu screen only (MainWindow has no ViewModel — see docs/ARCHITECTURE.md)
  Views/                  XAML + code-behind for Menu and the main game board
  Images/, BGMusic/       embedded art & audio assets, referenced individually in the .csproj
  JsonLanguages/          EN/RU/UA card name+description localization
docs/                    the documents linked above
```

## Working conventions for this repo

- **No test suite exists.** Don't claim something is "tested" beyond a manual run — this project
  has no automated verification today. If you add tests, note that clearly as new infrastructure,
  not a gap you silently filled.
- **Card identity is reference-equality on `EffectModifiersStore` singletons**, not an enum or
  string tag (`card.Effect == EffectModifiersStore.Spy`). When adding a new card behavior, follow
  this pattern rather than introducing a parallel dispatch mechanism, unless you're deliberately
  refactoring it (call that out explicitly — it's a large, repo-wide pattern change).
- **Adding a new card** touches at minimum four places: `CardsStore.cs` (instantiate it),
  all three `JsonLanguages/CardsInfo{EN,RU,UA}.json` files (name/description), an image asset in
  `Images/{JsonNameKey}.jpg`, and the `.csproj` `<Resource Include="Images\...">` entry for that
  image. Missing any one of these will either crash on load or silently show a broken/blank card.
- **Don't "fix" the pervasive typos** (`Oponent`, `BatteRowScore`, etc.) or restructure the
  code-behind-heavy `MainWindow` incidentally while doing something else — these are called out in
  `docs/ARCHITECTURE.md`/`docs/UI_VISUALS.md` as known rough edges, not open invitations to
  refactor. Raise it with the user first if a task would naturally touch them at scale (e.g. the
  web port will).
- Commit messages: English, imperative mood, explain *why* not just *what*, no mention of AI
  assistance — per the user's global git conventions.
