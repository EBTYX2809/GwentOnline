# UI / Visual Design — WPF Client

This documents the *visual and interaction design* of the current WPF app, for two reasons: (1) it's
the closest thing to a design spec for a web reimplementation, and (2) "completely replicated from
the original game" (README's own words) means most of these details are direct references to
Witcher 3's Gwent minigame UI, not arbitrary choices — preserve them unless the user says otherwise.

## 1. Screens

### Menu (`Views/Menu.xaml`)
Fixed 1680×1050 canvas, `Menu_Background.jpg` full-bleed background, maximized/non-resizable window.
- Center panel (semi-transparent white, `#40E0E0E0`) containing, top to bottom:
  - **Play** button (large, top).
  - **Name** text field (left) with a "Name" placeholder label that disappears once text is typed
    (`isNamePlaceholderVisible`, manually toggled in `MenuViewModel`, not a native WPF placeholder).
  - **IP** text field (right), same placeholder pattern, placeholder text is the default server
    address itself (`http://gwent-server.duckdns.org`) so leaving it blank still connects there.
  - **Faction picker**: a single `ToggleButton` with a custom `ControlTemplate` split into two
    halves side-by-side, each half's background is that faction's deck-cover art
    (`NorthKingdomsCover.jpg` / `NilfgaardCover.jpg`), with a dark overlay (`Opacity 0→0.2`)
    highlighting whichever half is currently selected. This "two images forming one toggle" pattern
    is worth replicating faithfully on the web — it's the primary faction-selection UX.
  - **Show/Hide cards list** toggle.
  - **Language** `ComboBox` (English/Russian/Ukrainian) — changes card name/description text
    live via `LanguageManager.SetLanguage`, does not affect UI chrome language (menu labels stay
    English regardless).
- Right-hand side: a scrollable full **card gallery** (`WrapPanel` of every card in the game,
  neutral + both factions), toggled by the "Show/Hide cards list" button — lets a player browse the
  full card pool (with hover tooltips for name/description) before picking a faction.
- "Waiting for opponent..." full-screen text overlay while matchmaking (`isMenuVisible == false`).

### Game board (`Views/MainWindow.xaml`)
Fixed 1680×1050 `Canvas` (`GameDesk`), maximized/non-resizable, background `Deck.jpg`. **Every
element is absolutely positioned** (`Canvas.Left`/`Canvas.Top`, hand-tuned pixel coordinates) —
there is no responsive layout, no `Grid`-based flow, nothing that would survive a straightforward
resize. This is the single largest incompatibility with the web (browsers, unlike a fixed desktop
window, need to handle arbitrary viewport sizes) — a web version will need a real layout system,
even if the *visual arrangement* it targets is this same one at a reference resolution.

Layout, top to bottom (mirrors the source game's board):
1. **Opponent (Player2) side**, rows ordered Siege → Middle → Melee (siege at the *top* of the
   opponent's stack, closest to the "back" of the board):
   - Each row: an `ItemsControl` bound to `PlayerX.<Row>.BattleRowCards`, `ItemsPanel` = a
     `UniformGrid` whose `Columns` count is data-bound to the row's live card count (cards shrink
     to fit as more are added — no scrolling, no overflow).
   - A horn slot to the row's left, and a row-score number further left/center.
   - Opponent's leader card + name + hand-card-count + deck (art + remaining count) + discard
     (shows only the *top* card, click opens the full discard-browsing overlay) laid out around
     the melee row.
   - A turn-indicator `Ellipse` (dull gold, `#c9be44`, only visible when it's the opponent's turn)
     and two round-loss "crystal" markers (`Path`, black diamond shape, one per round lost) near
     the score display.
2. **Weather row** (`WeatherCardsBattleRow`), positioned center-left, `UniformGrid` fixed at 3
   columns (max 3 weather effects — Frost/Fog/Rain — can be active at once by rule).
3. **General score numbers** for each player, large text, positioned between the rows and the
   center weather row.
4. **Local player (Player1) side**, mirrored: Melee → Middle → Siege (melee closest to the weather
   row / board center, siege at the very bottom), then **Hand** (`ItemsControl`, same
   `UniformGrid`-shrink-to-fit behavior as board rows) at the very bottom edge.
5. **Pass button** (top-right of the board), **volume slider** (far right, vertical), background
   `MediaElement` (invisible, drives `MusicBG`).
6. **Announcement banner** (`AnnouncementBox`): full-width black translucent bar, centered
   vertically, large bold text, shown for exactly 1 second (`Task.Delay(1000)`) on turn-passed /
   round-result / game-start events, then hidden again — a transient toast, not a persistent log.
7. **Discard / mulligan / Medic-revive overlay** (`ShowDiscardMenu`): full-screen semi-transparent
   panel, `GameDesk` gets a `BlurEffect` (radius 10) behind it. Reused for three different
   purposes by swapping its `ItemsSource` and the bottom button's label/behavior:
   - Viewing a discard pile (read-only browse, "Hide discard" closes it).
   - Mulligan (button reads "Accept", clicking a card swaps it for a random deck card, closes
     itself after 2 picks or on Accept).
   - Medic / leader-revive card picking (button hidden entirely; picking a card resolves an
     awaited `TaskCompletionSource`).

## 2. The card visual (`CardTemplate` `DataTemplate`, duplicated identically in both XAML files)

- 76×106px `Grid`, background = the card's own art (`ImageBrush`, bound to `Card.Image`, a
  `pack://application:,,,/Images/{JsonNameKey}.jpg` URI).
- Top-left corner: current power number (`ActualCardScore`), color-coded via
  `CardPointsToColorConverter`:
  - White → Hero card (power can't be modified, so no debuff/buff color makes sense).
  - Green → currently buffed above `DefaultCardScore`.
  - Red → currently debuffed below `DefaultCardScore`.
  - Black → at default/unmodified power.
- Hover tooltip: brown background (`#8B4513`), vertical stack of card `Name` + `Description`
  (localized via `CardInfo`, sourced from the active-language JSON file).
- Thin border that highlights (`CardGrid_MouseEnter`/`Leave`, swaps `BorderBrush` between
  transparent and the shared "pick" highlight color) on hover, independent of the row-level
  highlight described below.

## 3. Interaction: drag & drop

- Only the **local player's hand** is draggable (`Card_MouseLeftButtonDown` gates on
  `GameContext.Instance.ActivePlayer == Player1` and the source `ItemsControl == Hand`).
- On drag start, a **manually cloned visual** of the card (`CreateCardVisual`) is added directly to
  the `GameDesk` canvas and manually repositioned under the cursor (`GameDesk_DragOver`) — WPF's
  native `DragDrop` doesn't render a drag ghost by default, so this whole clone-and-follow-the-mouse
  mechanism exists purely to fake one. It clones geometry, the score `TextBlock`, the tooltip
  content, and the background image, but not event handlers (`IsHitTestVisible = false`).
- **Legal-drop highlighting** (`HighlightBattleRow`, called on drag start): the specific row(s) a
  card *can* legally land on light up translucent green (`CardPickColor`,
  `#40B3FF00` — note `#40` alpha ≈ 25%); if that row already has an active weather tint, the
  highlight blends to a lighter green-yellow (`CardPickPlusWeatherColor`) instead of overwriting
  the weather color outright.
  - **Spy cards** highlight the *opponent's* matching row instead of your own (since Spy cards are
    played onto the opponent's board).
  - **Weather cards** highlight the shared weather row.
  - **Horn cards** (the `ActionCard` variant only — `Dandelion` the unit doesn't use this path)
    highlight every currently-empty horn slot across your three rows simultaneously.
  - Everything else highlights the single row matching the card's `BattleRow`.
- Weather-affected rows carry a **persistent** translucent tint even with no drag in progress:
  cyan (`FrostColor`, `#4000FFFF`) / gray (`FogColor`, `#40B1BDBD`) / blue (`RainColor`,
  `#400000FF`) — applied/cleared by `PaintRowsByWeather`, invoked whenever
  `GameContext.WeatherCardsBattleRow` changes.
- Dropping is validated **again** in each row's own `_Drop` handler (row match, Spy-only-on-enemy-
  row-when-it's-not-their-turn-to-receive-it, Horn-only-on-`ActionCard`, etc.) — the highlight
  logic and the drop-acceptance logic are two independently maintained sets of rules that must be
  kept in sync by hand; a web port should unify these into one "can this card legally go here"
  predicate used for both.

## 4. Audio

- `MusicBG` cycles through 3 embedded MP3 tracks pulled from the original game's soundtrack
  (`BGMusic/*.mp3`, shipped as `Content` with `PreserveNewest`), auto-advancing on
  `MediaElement.MediaEnded`.
- Volume controlled by a vertical `Slider` (0–1, default 0.5) on the game board only — no
  persistence of the chosen volume between sessions.

## 5. Fonts, colors, chrome

- No custom font family is set anywhere — everything renders in the WPF default (Segoe UI on
  Windows). If visual fidelity to the original matters, a web port choosing a period-appropriate
  fantasy/serif font for headers would be a deliberate *improvement* over the current app, not a
  regression to avoid.
- Color palette is otherwise defined ad hoc per-element (hex literals scattered through both XAML
  files) rather than centralized in a resource dictionary/theme — there's no single "theme" file to
  port; a web version should extract one from the values cataloged above.

## 6. Known UX rough edges (worth flagging, not silently "fixing" mid-task)

- Several error paths use blocking `MessageBox.Show(...)` calls (connection failure, opponent
  disconnect, deck exhausted, session timeout, round/game-over summary) — freezes the single UI
  thread until dismissed, and has no web equivalent (would need to become a modal/toast).
- The hand and each row display their live card count **twice**: once implicitly via the
  `UniformGrid` shrinking, and once as a separate bound `TextBlock` number positioned nearby
  (e.g. hand-count label at `Canvas.Left="236"` beside the `Hand` `ItemsControl` itself) — a
  leftover from iterative UI development, not an intentional redundant-affordance design.
- `ResizeMode="CanMinimize"` + `WindowState="Maximized"` on both windows means the *actual* window
  chrome is disabled from resizing, but nothing in the layout is DPI/scale-aware either — a web
  port inherits none of these constraints and should treat the 1680×1050 numbers as a **reference
  layout to adapt**, not literal pixel targets.
