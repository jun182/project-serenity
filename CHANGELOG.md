# Changelog: Android port (fork) vs. the original game

Original: NightAnimal333/project-serenity (Windows build). This fork: development on branch `android-port`, releases from `main` (see CONTRIBUTING.md).
Builds are numbered b1, b2, ...; the in-game debug overlay shows the build number.

## Platform
- Android build (arm64, IL2CPP), built headless with `Assets/Editor/BuildAndroid.cs`. 32-bit dropped (modern phones are 64-bit only).
- Frame rate setting 30/60/90/120 (Android otherwise caps at 30). Camera zoomed in 1.25x for phone screens.
- "Quit game" closes the app fully (no reopening from recents).

## b13 (2026-11-27)
- Debug is hidden: the Debug panel / UI tweak panel / tuning sliders / invincible / zoom cubes only work after developer mode is unlocked, and developer mode resets on every app start. Unlock: tap the build number in Settings 7 times (hint shows only for the last 2 taps), or the Konami code (up up down down left right left right B A) on the start screen (keyboard or pad). While locked, all tunables use their defaults.
- Settings: "Build bN" line; "Developer..." page (Debug panel, UI tweak panel, Disable developer mode) appears after unlock.
- Cached per-frame PlayerPrefs reads (gyro on/invert, screen shake, mode, debug flag): reimplemented from Copilot PR #6 on top of the dev-mode gate. Dead player no longer moves or heals during the fade-out (from PR #8, not runtime-tested). Material leak fix in HideObjectsPerSide (PR #7).
- Fix (issue #9, not runtime-tested): `PlayerController.SteerNorm` was computed as per-frame speed / (top * current frame time), so frame-time variation made the tilt-steering camera roll jitter when changing direction. The steering velocity is now smoothed in normalised form (same feel, frame-time independent). Keyboard/touch/pad were not found to move the camera; the roll stays tilt-only.
- Start screen: no more "Touch to start" text; Play is a button in touch mode as well (tapping empty screen does nothing; the water ripples are removed). Space also starts on PC/Web. Request from Artūrs ("touch to start" reads as a mobile game). No PC/Web build exists yet (only Android is built; Windows/WebGL modules not installed here).
- `tools/menu-check.sh` also checks locked by default, 7-tap unlock, Konami unlock, and re-lock after restart.

## b9 (2026-11-27)
- First release with the in-app updater (b8 = same code, not released). Verified b8 -> b9 on a phone.

## b8 (2026-11-27)
- Permanent app identity `se.vibing.serenity`, signed with a release key (kept outside the repo); release build. Everyone must uninstall the old `com.DefaultCompany.ProjectSerenity` once.
- In-app updater: at every start of the start screen the app checks the GitHub releases (`alpha-bN`) and shows "Update available: bN"; tap = download + system installer. Silent when offline.
- Build number in one place (`BuildInfo.cs`), also the Android versionCode. Font moved to Resources (UiFont), interim timer.

## Controls
- Touch: hold the left/right half of the screen to steer.
- Optional tilt steering (phone accelerometer), with an invert option. Remembered between runs.
- Pause menu (II button top-left, Esc, controller Start): resume, options, quit to menu, quit game.
- "How to play" screen in the start screen and pause menu (b6).

## Gameplay
- Two modes (b5): **Progressive** (default): difficulty 0-5 rises over time by spawning more obstacles
  (level 2 = the original rate); at level 5 survive a countdown to win ("you wonned").
  **Endless**: the original fixed rate, with an obstacle amount slider (0.5x-3x).
- Survival timer at the top of the screen (b5).
- Obstacles now spawn over the whole left side (b5). Originally `chanceToSpawnInMiddle` was 0, so x 2..5 never got
  obstacles and the player could hide at the border.
- Health bar at the bottom (b4). The original "Balance" text showed a static 999% and was hidden.

## Feel
- Haptics: vibration on hit, soft heartbeat while healing in Serenity.
- Screen shake (option, default ON, b5): strong on hit, small on a near miss; gentle rocking and zoom pulse in time
  with the heartbeat while healing.
- Near miss (b7): short vibration (strength tunable, default 160/255) and a subtle sideways camera slide in the
  direction the player is moving (follows the Screen shake option).

## Bug fixes vs. the original
- Hits, damage and music changes never happened on Android: the Player's MusicManager reference was empty in the scene
  (also in the original), so every sound call threw. Now looked up at start (b3).
- Serenity side is decided by the player's x position (> 5); the trigger exit never fired on the phone (b2).
- Death check runs on both sides (b4).
- Pause no longer lets the player slide (movement ran while paused).

## Debug tools (pause menu, Debug panel ON)
- Overlay: fps, build, music/hit/side counters, dead flag, last runtime error, mode, level, spawn rate, near misses.
- Kill me, Invincible, Difficulty +1, zoom test cubes, and a tuning page with sliders (seconds per level, win
  countdown, spawn rate, obstacle speed, spawn range, damage, heal speed, shake strength, near-miss distance,
  breathing zoom, plain-track time). Stored on the phone, no rebuild needed.
- Tuning page b7: three columns, sliders snap to round steps, a Default button per slider plus "Reset all",
  new sliders: steer speed, steer response, near-miss vibration, near-miss slide (negative = flip direction).
