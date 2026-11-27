# 004 Plan

## Approach
One new static class, `UiStyle`, holds the whole style: colours, outline width, corner radius, and the code-generated
sprites (like `PadIcons.Disc` today, no image assets). Everything that draws a button or slider asks it.

- **Sprites:** a rounded rectangle generated once at runtime and used as a 9-sliced `Image` (any size, same corners).
  It is generated in a few cached variants that carry both colours, because the fill and the edge must swap on selection
  (a single tinted white mask can't have a black edge around a white fill):
  - `Button`: white fill, black edge (normal);
  - `ButtonSelected`: black fill, white edge (pad / keyboard selection);
  - `Track` / `TrackSelected`: black fill, thin / thick white edge (slider background);
  - `Plain`: white fill, no edge (slider fill);
  - `Knob`: white fill, black edge, round (slider handle, "A" disc).
  The Image colour stays white, so Unity's own colour tint keeps working for "pressed".
- **Buttons:** `PauseMenu.MakeButton` (the one place every button is made, start screen included through the delegate)
  calls `UiStyle.Button(button)`: sprite `Button`, sliced; text black; `ColorBlock` normal / highlighted / selected = white,
  pressed = 0.78 grey (2023 default), fade 0.1 s. Highlighted and selected are white, so a tap (which also "selects" in the
  EventSystem) never leaves a grey or black button behind (AC6).
- **Selection:** `UiStyle.Select(button, on)` swaps sprite and text colour (black <-> white). `PadIcons.Paint` and
  `PadIcons.SelectFx` call it instead of setting colours. The pulse stays where it is (`SelectFx.Update`, `StartScreen`);
  for sliders `SelectFx` swaps the track to `TrackSelected` and pulses the handle instead of the whole slider.
- **Width tunable:** the menu-width scale ("Menu button width x") moves from `Brush.Register` / `ReapplyAll` to
  `UiStyle.RegisterWidth` / `ReapplyWidths` (no sprite rebuild needed any more: the sliced sprite fits any width).
- **Remove:** `Brush.cs` (Paint / Rhombus generator, `buttonStyle` pref), the Settings "Button style" item and `CycleStyle`,
  and the four shape tunables (`uiSkew`, `uiRough`, `uiDry`, `uiSeed`) from `GameSettings.UiTunables`.
- **Pad icons:** `PadIcons.Disc` uses the `Knob` sprite (white, black edge) with a black letter, no `Outline` component;
  `PadIcons.Pill` uses the `Button` sprite (black edge) with black text.

Rejected: Unity's `Outline` component on buttons (draws four shifted copies of the shape: looks like a smear at the
rounded corners and doubles the draw cost); a second child Image for the edge (two Images per button, and selection
would have to recolour both); Unity's built-in `UISprite` (its edge is thin light grey, not the solid black edge in the spec,
and it is not available through `DefaultControls.Resources` at runtime without loading the editor resource).

## Files
- New: `Assets/Scripts/UiStyle.cs` (+ `.meta`): colours, sprites, `Button`, `Select`, `Slider`, `SelectSlider`, `RegisterWidth`, `ReapplyWidths`.
- Changed: `Assets/Scripts/PauseMenu.cs`: `MakeButton` uses `UiStyle.Button`; `MakeSlider` uses `UiStyle.Slider`; Settings
  item "Button style" and `CycleStyle` / `styleLabel` removed; `item` and `GameSettings.UiChanged` use `UiStyle` width helpers.
- Changed: `Assets/Scripts/PadIcons.cs`: `Paint`, `SelectFx`, `Disc`, `Pill` use `UiStyle`; `Normal` / `Selected` colours go.
- Changed: `Assets/Scripts/StartScreen.cs`: `Brush.Register` -> `UiStyle.RegisterWidth`.
- Changed: `Assets/Scripts/GameSettings.cs`: `UiTunables` keeps `uiPulse` and `uiBtnScale` only; comment updated.
- Deleted: `Assets/Scripts/Brush.cs` (+ `.meta`).
- Changed: `tools/menu-check.sh`: one DPAD_DOWN fewer to reach "Build bN" on Settings (the removed item was in between).
- Docs: new `specs/system/ui-style.md`; `specs/system/README.md`, `menus.md`, `dev-mode.md`, `settings.md`,
  `specs/constitution.md` rule 7; `CHANGELOG.md`, `HANDOFF.md` (at verify).

## Constitution check
- **1 Updater / 2 identity, signing:** untouched (no `BuildInfo`, app id or signing change; no release tag).
- **3 Debug hidden:** the UI tweak panel stays behind dev mode; it only loses sliders. No new debug UI.
- **5 Honest testing:** phone checks are required by the spec; whatever can't be run is reported as not tested.
- **6 Every input:** navigation code, pages, B / Esc, touch-only `Navigation.None` are unchanged; only colours and sprites
  change. Selection stays visible (black button among white ones). `menu-check.sh` covers it (AC11).
- **7 The look:** this is the change; rule 7 gets the pointer to `ui-style.md` (approved by the owner).

## Risks
- **Sliced sprite edge thickness** depends on canvas scale (reference 1920x1080, match 0.5): the edge is set in sprite
  pixels with `pixelsPerUnitMultiplier`, so it should stay about the same on phone and editor; checked on screenshots.
- **Selected black button on the 75% black backdrop:** relies on the white edge; checked on screenshots (AC5).
- **Slider fill over the track edge:** the fill is inset so the white edge stays visible at both ends; if it covers the
  edge at full value, the fill area offsets are adjusted.
- **First batch build after adding `UiStyle.cs`** can fail with CS0246 (known); build twice, `grep "error CS"`.
- **Deleting `Brush.cs`:** any reference left over breaks the build; grep for `Brush.` before building.
- **Old prefs** (`buttonStyle`, `t_uiSkew` ...) stay on installed copies but are never read; harmless.
- **Environment (not code):** Unity 2022.2.8f1 with Android is at `G:\Unity\2022.2.8f1` (use it, not the 2022.3.49f1 under
  Program Files, which would upgrade the project); its adb is `...\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe`.
  The release key is **not** on this PC (`~\keys\serenity-release.jks` missing): without it the build refuses, so the phone
  checks wait for the key (or for a build from the machine that has it).

## Build and test
- Unity `G:\Unity\2022.2.8f1`; adb from its Android SDK (phone: USB debugging on, `adb devices`); release key copied to
  `~\keys\` (never into the repo). Headless build per `specs/system/build-test-release.md` (Windows paths),
  `grep "error CS"` in the log. Compile check without the key: batch mode with `-quit` and no build method, then the log.
- **Editor** (play mode, both scenes; arrow keys / Enter act as a pad): AC1-AC5, AC7-AC10.
- **Phone** (the developer's phone over adb): `OUT=<dir> tools/ui-check.sh Builds/project-serenity-release.apk <device>`
  screenshots for AC1-AC4, AC8, AC13; by hand: AC6 (touch only, no black button), AC7 (press); AC5 / AC9 with a gamepad if one
  is available, otherwise keyboard-as-pad in the editor and the pad part marked not tested.
- **menu-check** `tools/menu-check.sh <apk> <device>`: AC10 (no "Button style" between Mode and Build), AC11.
- **Review:** AC12, AC13 (diff touches no backdrop / help / timer / hint / health bar / FPS code).
