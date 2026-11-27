# Changelog: Android port (fork) vs. the original game

Original: NightAnimal333/project-serenity (Windows build). This fork: branch `android-port`.
Builds are numbered b1, b2, ...; the in-game debug overlay shows the build number.

## Platform
- Android build (arm64, IL2CPP), built headless with `Assets/Editor/BuildAndroid.cs`. 32-bit dropped (modern phones are 64-bit only).
- Frame rate setting 30/60/90/120 (Android otherwise caps at 30). Camera zoomed in 1.25x for phone screens.
- "Quit game" closes the app fully (no reopening from recents).

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
