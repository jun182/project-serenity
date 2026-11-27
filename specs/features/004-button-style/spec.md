# 004 One button style: white, black outline, black text

- **Status:** done
- **Source:** request from NightAnimal333 (chat, 2026-11-27)
- **Approved:** 2026-11-27, NightAnimal333 (chat), including the constitution rule 7 change
- **Branch:** `feature/button-style`

## Problem
The original Windows game (last commit before the fork: `657f0ef`, 2023-03-24) had few buttons, all in one style:
Play and Exit in `MainMenu.unity` were white rounded boxes with a thin outline (Unity's built-in `UISprite`) and dark text
in Alegreya SC Bold. The Android fork builds its UI in code and drifted away from that: buttons are dark grey with
white text, the pad-selected button turns white, a Settings option switches between three shapes (Classic / Paint /
Rhombus), sliders are grey and green, and the "A" icon is a black disc. Nothing records what the style should be,
so every new screen picks its own.

## Goal
Every button in the game looks the same and goes back to the 2023 look (white, outline, black text). The common UI
style is written down in `specs/system/ui-style.md`, so later work can follow it, and constitution rule 7 points to it.

## Scope
- **One button style for every button** (both scenes, players and developer mode alike):
  - menu pages: main, settings, controls, dev, debug;
  - start screen Play / Exit (pad mode);
  - touch-only buttons: Menu (start screen, top-right), II (game, top-left), Exit (start screen, top-left), Update;
  - small and sub-page buttons: slider "Default" buttons, How to play "Close", tuning page "Reset all to defaults" and "Back".
- **Button look:** rounded rectangle, white fill, solid black outline (a few pixels, clearly visible on the white half of
  the screen), pure black text in the existing UI font (Alegreya SC Bold). Positions, sizes and font sizes stay as they are.
- **Pad / keyboard selection:** the selected button is inverted: black fill, white outline, white text, and it keeps its
  small pulse. Touch players never have a selected button, so they always see the white look.
- **Pressed:** a pressed button darkens briefly (like the 2023 default: about 78% grey tint), then returns.
- **Sliders** (Settings > Obstacles, debug Game speed, tuning page, UI tweak panel): black track with a thin white outline,
  white fill, white handle with a black outline. A pad-selected slider gets a thicker track outline and its handle pulses;
  the colours stay the same, so the value stays readable.
- **Pad icons:** the "A" disc becomes white with a black letter and a black outline; the "START" pill (already white
  with black text) gets the same black outline.
- **Shape setting removed:** Settings no longer has "Button style"; the Classic / Paint / Rhombus shapes are gone.
  The UI tweak panel (dev mode) loses the four shape-only sliders (Button slant, Edge roughness, Dry-brush streaks,
  Stroke variant); it keeps "Selection pulse %" and "Menu button width x".
- **Style recorded:** new `specs/system/ui-style.md` (colours, outline, shape, text, states, sliders, icons, where it comes
  from); listed in `specs/system/README.md`, referenced from `menus.md`; constitution rule 7 says new UI follows `ui-style.md`.

## Out of scope
- Non-button UI: the menu backdrop (75% black), How to play text, timer, "Touch to start" hint, FPS overlay, health bar
  (stays green), death / win screens, water ripples.
- Layout changes: button positions, sizes, menu order, font sizes.
- New controller art (the A / START icons stay placeholder shapes, only recoloured).
- The scene's own hidden Play / Exit objects in `MainMenu.unity` (not shown at runtime).
- Setting up adb, Unity 2022.2.8f1 or the release key on the developer's PC (needed for the phone checks, see Details;
  machine setup, not a repo change).

## Details
- **Scenes:** start screen (`MainMenu`) and game (`GameplayScene`); both use the same code-built menu.
- **Inputs:** touch (no selection state, white buttons only); gamepad and keyboard (selection = black button / thick-outline
  slider); tilt does not touch menus. Navigation, pages and B / Esc behaviour are unchanged (constitution rule 6).
- **Edge cases:** the menu opens over the dark backdrop while paused; Update shows status text (checking, downloading)
  in the same style; buttons that are hidden (touch-only while a menu page is open) stay hidden.
- **Persistence:** no new settings. The old PlayerPrefs key `buttonStyle` is no longer read; an old value on an installed
  copy has no effect. Old `t_uiSkew` / `t_uiRough` / `t_uiDry` / `t_uiSeed` values are ignored.
- **Audience:** players (all buttons) and developer mode (dev / debug / tuning pages, UI tweak panel) alike.
- **Constitution:** rule 7 gets one sentence pointing to `ui-style.md`. Changing the constitution needs the project owner's
  approval of this spec.
- **Phone checks are required** (developer's decision). They need, on the developer's PC: adb (platform-tools) with the phone
  connected (USB debugging), Unity 2022.2.8f1 with the Android modules (the installed 2022.3.49f1 would upgrade the project),
  and the release key (`~/keys/serenity-release.jks`). Until then the feature can be implemented and editor-checked, but not
  set to `verified`.

## Acceptance criteria
| ID | Criterion | Check | Result |
|----|-----------|-------|--------|
| AC1 | Every menu page button (main, settings, controls, dev, debug) is a white rounded rectangle with a black outline and black text | screenshot (phone) + editor | **pass**: editor (shots + log: every unselected button white sprite, black text) and phone Pixel 8 Pro (main, settings, pause menus) |
| AC2 | Start screen: Play / Exit (pad mode), Menu, touch Exit and Update use the same style; Menu and Exit are visible on their halves of the screen | screenshot (phone) + editor | **pass** for Play/Exit, Menu, touch Exit (editor + Pixel 8 Pro; Exit outline visible on the white half, Menu on the black half). Update: **not tested on screen** (no newer release, so it never shows); made by the same `MakeButton`, so styled by review |
| AC3 | In game: II uses the same style | screenshot (phone) + editor | **pass**: editor + Pixel 8 Pro (II white, black outline) |
| AC4 | Small buttons (slider Default, How to play Close, tuning Reset all / Back) use the same style | screenshot (phone) + editor | **pass**: editor (Default, Close, tuning Reset all / Back) + Pixel 8 Pro (Default, Close) |
| AC5 | With a pad or keyboard, exactly the selected button is black with a white outline and white text and pulses; moving away turns it white again | editor (keyboard) + device (phone, gamepad if available) | **pass** with keyboard (editor) and adb pad key events (Pixel 8 Pro: DPAD/A/B/Start): only the selected button black, pulsing. Found and fixed on the phone: a button selected when the menu closed stayed black on reopening (`SelectFx.OnDisable`); retested pass. Real gamepad: **pass on PC** (Xbox 360 pad, Unity editor, checked by the developer); real gamepad on the phone **not tested** |
| AC6 | With touch only, no button is ever shown black (no selection state) | device (phone) | **pass**: Pixel 8 Pro, real taps (Menu, long press on Settings): no button left black |
| AC7 | A pressed button darkens briefly and then returns to white | editor + device (phone) | **pass**: editor (tint C7C7C7) + Pixel 8 Pro real long press (Settings grey while held, then white) |
| AC8 | Sliders: black track with white outline, white fill, white handle with black outline; a pad-selected slider has a thicker outline and a pulsing handle | screenshot (phone) + editor | **pass**: editor + Pixel 8 Pro (black track, white outline, white fill and handle; selected: thicker outline, handle pulses). The thicker outline is subtle (2 -> 5 units) |
| AC9 | "A" disc is white with a black letter and black outline; "START" pill has a black outline | screenshot (phone, controller connected) or editor (keyboard as pad) | **pass**: editor + Pixel 8 Pro in pad mode via adb keys (A disc white, black letter and outline; START pill black outline). Real controller: **pass on PC** (Xbox 360 pad, editor, developer); on the phone **not tested** |
| AC10 | Settings has no "Button style" item; the UI tweak panel has only "Selection pulse %" and "Menu button width x"; both still work | editor + menu-check | **pass**: editor (no Button style; tweak panel has 2 sliders, width x1.3 widens the column) + menu-check on Pixel 8 Pro (Build b13 reached right after the slider) |
| AC11 | Menu navigation is unchanged: `tools/menu-check.sh` passes (`MENU page=... sel=...` lines) | menu-check (phone) | **pass**: `tools/menu-check.sh` all ok on Pixel 8 Pro (twice, before and after the fix) |
| AC12 | `specs/system/ui-style.md` exists and describes the style (colours, outline, shape, text, states, sliders, icons, 2023 origin); listed in `specs/system/README.md`, referenced from `menus.md`; constitution rule 7 points to it | review | **pass** (review: `ui-style.md`, README row, menus.md rule, constitution rule 7) |
| AC13 | No other UI changed (backdrop, help text, timer, hint, health bar, FPS overlay) | screenshot (phone) + review | **pass**: review (diff touches no backdrop / help / timer / hint / health bar / FPS code) + Pixel 8 Pro shots (timer, hint, health bar as before) |

## Open questions
- (none)

## Verification notes (2026-11-27)
- Compile: Unity 2022.2.8f1 (Roslyn, Assembly-CSharp, with and without `UNITY_ANDROID`): 0 errors. Signed release APK built on this PC.
- Editor: visible play mode driven by a temporary script (deleted, not committed), 18 screenshots + per-button log.
- Phone: Pixel 8 Pro (wireless adb), real taps and adb key events; `menu-check.sh` all ok.
- Real gamepad: Xbox 360 pad on the PC in the Unity editor, checked by the developer: works. Not tested: real gamepad on the phone (AC5, AC9), Update button on screen (AC2).
- Found, not in scope (need their own folder): `Haptics.cs` uses `Handheld` outside `#if UNITY_ANDROID`, so a Windows build fails;
  `tools/ui-check.sh` taps positions of an old menu layout (2400x1080) and no longer walks the current menus.

## System specs affected
- `specs/system/ui-style.md` (new)
- `specs/system/README.md`
- `specs/system/menus.md` (button style line, settings page)
- `specs/system/dev-mode.md` (UI tweak panel sliders)
- `specs/system/settings.md` (`buttonStyle` key removed)
- `specs/constitution.md` (rule 7)
