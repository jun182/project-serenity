# Menu tree + controller/debug design (draft 2026-11-27, for b12; not implemented)

Findings on the Y700 (b11, pad): (1) top-left button not selectable; (2) buttons overlap, cannot go back; (3) screen turns off by system timeout
(confirmed: screen_off_timeout 120000 ms and the screen was black while the game sat on the start screen).
Likely causes (unverified, read from code): the start-screen Play/Exit stay visible under the dimmed menu (alpha 0.75), so they overlap the menu pages;
B = `JoystickButton1` may not be the Y700 pad's B (Unity/Android mapping), so Back never fires. Check with logcat before assuming.

## Screens and controller flow (pad mode). A = select, B = back, Start = open/close menu, stick/d-pad = move
```
START SCREEN              [Start = Menu]  (badge top-right, not focusable)
  Play  (A)  -> GAME
  Exit  (A)  -> quit
  Start/Menu -> MENU(start)   (Play/Exit hidden while any menu page is open)

GAME                      [Start = Pause] (small badge replaces the II button focus)
  Start -> MENU(game)  (time stopped)

MENU main:  Resume|Close, Settings, Controls, How to play, [game: Quit to menu, Quit game, Debug... (Debug ON only)]
  B or Start = Resume/Close
  Settings : Frame rate, Screen shake, [start only: Mode], Obstacles slider, Debug panel   | B -> main
  Controls : Tilt steering, Invert tilt                                                    | B -> main
  How to play (text page)                                                                   | A or B -> main
  Debug (game only): Kill, Invincible, Level +1, Zoom markers, Tuning sliders..., Game speed slider | B -> main
    Tuning sliders (page)                                                                   | B -> Debug
```
Rules: only one page visible at a time (hide everything else, incl. start-screen buttons); B always goes up exactly one level;
sliders: left/right changes the value, A = reset to default; the focus never lands on touch-only buttons (II / Menu / Exit-top-left).

## 1. Start badge (item 1)
Touch buttons (II, Menu, top-left Exit) stay touch-only. In pad mode show a small badge "[START] Menu" beside them (placeholder icon,
real controller art later). Open on Start. Unclear which top-left button you meant (in-game II or start-screen Exit): confirm.

## 3. Keep the screen on (item 3)
Usual practice (Unity docs / Android games): `Screen.sleepTimeout = SleepTimeout.NeverSleep` while playing (also with a pad, no touch for long
stretches), and `SleepTimeout.SystemSetting` on menus/pause so the phone can sleep if left alone. Plan: NeverSleep in GameplayScene when not paused,
SystemSetting on start screen/pause. Not verified on device yet.

## 4. Debugging without a pad (item 4): better than a virtual gamepad
`adb shell input keyevent KEYCODE_DPAD_UP|DOWN|LEFT|RIGHT|BUTTON_A|BUTTON_B|BUTTON_START` injects real key events, no new code needed
if the menu also accepts arrow keys/Enter/Escape (it should anyway; keyboard users).
Verification without screenshots: on every page/selection change `Debug.Log("MENU page=<name> sel=<button>")`; read with
`adb logcat -d -s Unity | grep MENU` (text, cheap, device independent). Optional tiny text at screen corner (0,0) only while Debug is ON, for a cropped screenshot.
Script `tools/menu-check.sh`: steps from the tree above ("down down A -> expect page=settings") and fails on mismatch.
