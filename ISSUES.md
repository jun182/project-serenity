# Issues: Project Serenity Android port (fork of NightAnimal333/project-serenity, jun182/project-serenity)

Status key: open / in progress / done / parked. Dates are 2026-11-27 unless stated.

## Open
Tracked on GitHub now (issues enabled on jun182/project-serenity, 2026-11-27): https://github.com/jun182/project-serenity/issues
- gh #1 cache per-frame PlayerPrefs reads, #2 Material leak in HideObjectsPerSide, #3 dead player still moves/heals: assigned to Copilot (review claims, unverified; PRs against main).
- gh #4 controller: test b11 menu navigation with a pad (needs a human). Old #5.
- gh #5 frame rate 90 shows ~80 fps (needs device). Old #7.
- Needs phone confirmation: music leaves the serene track on the left side (old #9), cannot die (old #8, believed fixed in code), pause menu / tilt (old #3/#4, built).
- Pixel 9 Pro XL: not reachable over adb yet (USB shows MTP only); owner must authorise USB debugging.
- Ask the original team before publishing the port (parked).
- REVIEW.md not written; old scene DebugPanel is a removal candidate.

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
