# UI style

The one look for every button, slider and controller icon, in both scenes, for players and developer mode alike.
New UI follows it (constitution rule 7). Code: `Assets/Scripts/UiStyle.cs` (colours, sizes, generated sprites);
every button is made by `PauseMenu.MakeButton`, which applies it, so a new button gets the style for free.

## Where it comes from
The original Windows game (last commit before the fork, `657f0ef`, 2023-03-24) had two buttons, Play and Exit in
`MainMenu.unity`: Unity's built-in `UISprite` (white rounded box with a thin outline), label in Alegreya SC Bold,
dark text (`#323232`), pressed tint 78% grey. Feature 004 brought every button back to that look, with a solid black
outline instead of the thin grey one so a white button stays visible on the white half of the screen.

## Colours
Only black (`#000000`) and white (`#FFFFFF`), plus the 78% grey press tint. No colour accents on controls
(the green health bar is a HUD element, not a control).

## Buttons
| State | Fill | Outline | Text |
|-------|------|---------|------|
| Normal (always, with touch) | white | black, 4 units | black |
| Selected (pad / keyboard focus only) | black | white, 4 units | white, and the button pulses (`uiPulse`, default 3%) |
| Pressed | 78% grey tint over the current state, 0.1 s fade | | |

- **Shape:** rounded rectangle, corner radius 20 units (canvas reference 1920x1080, 1 unit = 1 sprite pixel), 9-sliced so it
  fits any size. Small buttons (e.g. slider "Default", 140x50) get proportionally smaller corners.
- **Text:** `UiFont` (Alegreya SC Bold), centred. Sizes per button: 48 for menu items and Play / Exit, 36 for the top-corner
  buttons (Menu, Exit, Update), 26 for "Default".
- **Hover and tap** leave the button white: with touch a button is never shown black.
- **Width:** the menu column and Play / Exit scale with "Menu button width x" (`uiBtnScale`, UI tweak panel, dev mode).
- There is one shape. The former Classic / Paint / Rhombus option is gone (feature 004).

## Sliders
| Part | Look |
|------|------|
| Track (background) | black, white outline 2 units, corner radius 10 |
| Fill (value so far) | white, no outline |
| Handle | white, black outline 3 units, corner radius 12, 44 units wide |
| Selected (pad / keyboard) | track outline 5 units, the handle pulses; colours stay the same |

Labels above sliders: white text on the dark menu backdrop, value and "(default)" (see `menus.md`).

## Controller icons
- **"A" disc** (next to Play in pad mode): white disc, black outline 4 units, black letter.
- **"START" pill** (next to Menu / II in pad mode): the button shape, white, black outline, black text.
Placeholder shapes until real controller art exists.

## Backdrops and text (unchanged by 004, listed so new UI matches)
- Menu backdrop: black at 75% over the game; How to play 92%; tuning page 90%; UI tweak panel 60%.
- Text over the white / black split (timer, "Touch to start"): two copies, each clipped to its half: black with a white
  outline on the white half, white with a black outline on the black half.

## Adding UI
- Buttons: `MakeButton(...)` (styled already); add `PadIcons.AddFx(button)` if it is pad-navigable.
- Sliders: `MakeSlider(...)` (styled already).
- Anything else: use `UiStyle.Light` / `UiStyle.Dark` and the sprites in `UiStyle`, not new colours.
