# Menus

All menus are built in code (`PauseMenu.cs`, `StartScreen.cs`), spawned on every load of `MainMenu` and `GameplayScene`. The scene's own Play/Exit buttons are hidden.
Replaces the former `MENU-TREE.md` (b12 design notes, in git history).

## Rules
- Only one page is visible at a time; the start-screen buttons and touch-only buttons are hidden while any menu page is open.
- B / Esc goes up exactly one level; Start closes the whole menu.
- Pad focus never lands on touch-only buttons (Menu, II, top-left Exit, Update).
- Opening the menu in game pauses (`Time.timeScale` 0); closing restores the game speed (1, or the debug game speed).
- Screen stays awake while playing with the menu closed; system sleep timeout on the start screen and while paused.
- Big buttons use the button style from Settings (Classic / Paint / Rhombus, default Rhombus).

## Start screen (`MainMenu`)
- **Touch mode:** pulsing "Touch to start" at the bottom (black on the white half, white on the black half). A tap anywhere: light
  vibration, three water-ripple rings at the finger, game starts 0.55 s later. Small Exit button top-left.
- **Pad mode:** Play and Exit buttons (centre, lower half), up/down moves, A/Enter confirms. An "A" disc icon next to Play.
- Always: Menu button top-right (touch-only, START badge in pad mode). Update button top-left when a newer release exists (`updater.md`).
- Start opens the menu. Esc / Android Back quits the app (task removed from recents).

## Menu tree
```
main      Close|Resume, Settings, Controls, How to play,
          [game only] Quit to menu, Quit game, [game + Debug panel ON] Debug...
settings  Frame rate: 30/60/90/120 (cycles), Screen shake: ON/OFF, [start screen only] Mode: Progressive/Endless,
          Obstacles (Endless mode) slider 0.5-3, Button style: Classic/Paint/Rhombus, Build bN (7 taps = dev mode),
          [dev mode] Developer..., Back -> main
dev       Debug panel: ON/OFF, UI tweak panel: ON/OFF, Disable developer mode, Back -> settings
controls  Tilt steering: ON/OFF, Invert tilt: ON/OFF, Back -> main
help      How-to-play text, Close -> main
debug     (game only) Kill me, Invincible, Difficulty level +1, Zoom test cubes, Tuning sliders..., Game speed slider, Back -> main
tuning    (from debug) one slider per tunable, Reset all to defaults, Back -> debug
```
Details of dev / debug / tuning: `dev-mode.md`. Note: B on the dev page goes to `main` (not `settings`); the Back button goes to `settings`.

## Sliders
Label shows value and "(default)" when at the default. Each has a small Default button that resets it. Values snap to the slider's step.
Pad: left/right changes the value.

## Quit
- Quit to menu: saves prefs, resets pause and game speed, loads `MainMenu`.
- Quit game / Exit: Android `finishAndRemoveTask` (no reopening from recents), `Application.Quit` elsewhere.

## Test hook
Every page or selection change logs `MENU page=<page> sel=<label>` (page is `start`, `game`, a menu page, `help` or `tuning`);
every joystick button logs `MENU key=JoystickButtonN`; dev mode logs `MENU dev=ON|OFF`. `tools/menu-check.sh` depends on these.
