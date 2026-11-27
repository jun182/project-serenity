# Issues: Project Serenity Android port (fork of NightAnimal333/project-serenity, jun182/project-serenity)

Status key: open / in progress / done / parked. Dates are 2026-11-27 unless stated.

## Open
1. **Install + test the 64-bit APK on the Pixel** (open): sent via `tailscale file cp` as `project-serenity-arm64.apk` (42 MB, arm64-v8a only, same file as `Builds/project-serenity-debug.apk`). Receipt not confirmed.
   The first APK (32-bit armeabi-v7a only) failed with "App not installed": Pixel 9 Pro XL is 64-bit only. Fix: IL2CPP + ARM64 set in `Assets/Editor/BuildAndroid.cs`. 32-bit dropped on purpose (user's decision).
2. **Test the first APK** (open): does it install, start, and are the touch controls (hold left/right half of screen) usable? The invert/black-and-white shader (`Invert Blit Shader.shadergraph`) is the main risk on Android: not tested.
3. **Pause menu** (built 2026-11-27, untested on phone): Resume, gyro toggle, Quit to menu. Open with controller Start, Esc, or an on-screen button. Build buttons in code, not scene YAML.
4. **Gyro tilt toggle** (built, untested; has Invert button): main menu button + pause menu button, remembered with PlayerPrefs, dead zone + sensitivity. `Input.acceleration.x`. Needs a real phone to tune.
5. **Controller support** (open): left stick = move (`Input.GetAxis("Horizontal")`), Start = pause menu.
6. **Ask the original team before publishing the port** (parked): the user is part of the team; the fork is public (GitHub forks of public repos cannot be private).
7. **Frame rate setting does not match the counter** (open, observed 2026-11-27 on the Pixel 9 Pro XL): 90 setting shows ~80 fps, 120 setting shows 110-120, 60 not reported as a problem. Unverified guess: display runs fixed refresh steps (60/120 Hz) and Android frame pacing makes 90 uneven. Keep the 90 option for now (user's decision); investigate later (e.g. `Application.targetFrameRate` vs Android display modes / Swappy frame pacing).
8. **Cannot die** (open; b3 fixed hit registration: hits 76, hp -660, but death never triggered; b4 moves the death check + shows `dead`/`err` on the overlay, observed 2026-11-27 on `build b2`, Pixel 9 Pro XL): on the left side the overlay reads `heal False x -3.7 hits 0 serIn 1 serOut 1 Light(out)`, hp stays 100 after the user steered into obstacles on purpose. Not yet known whether the player visibly overlapped an obstacle. Obstacles spawn at x -5..5 (`SpawnerScript`), fall along -z, have a trigger BoxCollider; the player's `OnTriggerEnter` counts `Obstacle`-tagged hits. Next: log trigger events / check the player's collider+Rigidbody and z overlap. **Likely cause (found 2026-11-27, static reading, untested):** the scene's Player has an empty `musicManagerController` reference (upstream too), so `PlaySound`/`PlayTheme` throw before the damage/theme code runs. Build b3 adds a `FindObjectOfType` fallback and overlay fields `mm ok|NULL`, `trig N`.
9. **Music never leaves the serenity track on the left side** (open, same observation): the user hears only the serene music although `PlayerController` reports Light(out). Likely `maxTimeInLight`=0 in the scene, so Broken is requested every frame outside Serenity (`PlayTheme(Broken)` overrides Light). Unverified which track is audible; check the three AudioSources' clips/mute state. **Likely cause (found 2026-11-27, static reading, untested):** the scene's Player has an empty `musicManagerController` reference (upstream too), so `PlaySound`/`PlayTheme` throw before the damage/theme code runs. Build b3 adds a `FindObjectOfType` fallback and overlay fields `mm ok|NULL`, `trig N`.

## Done
- Fork + clone (`origin` = jun182 fork, `upstream` = NightAnimal333).
- Unity Hub AppImage + Unity CLI 1.1.0 + Editor 2022.2.8f1 with Android modules, Unity Personal licence activated (user accepted the terms).
- Touch controls in `Assets/Scripts/PlayerController.cs`; headless build script `Assets/Editor/BuildAndroid.cs`.
- First debug APK built (package `com.DefaultCompany.ProjectSerenity`, min SDK 22).

## Fixed 2026-11-27
- Serenity side now decided by player x > 5 (`PlayerController`), not trigger enter/exit (exit never fired). Confirmed on phone: serIn/serOut +1 per crossing, `heal` flips.
- Quit game left the task in recents (Back reopened the app): `PauseMenu.QuitApp()` uses `finishAndRemoveTask`. Confirmed on phone.

## Commits
Android port work committed to the fork (origin) 2026-11-27 after b4; never push to upstream.
