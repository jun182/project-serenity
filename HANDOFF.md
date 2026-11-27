# Board project-serenity (2026-11-27)
Resume: print open issues (`gh issue list -R jun182/project-serenity`). b13 (b12 + hidden developer mode) is on android-port and main; b12 is installed on the Y700 (adb 192.168.18.155:42501, reconnect with `adb connect`); user tests it. Release alpha-b13 (includes b12) ONLY on the user's OK. Workflow: spec-driven (specs/README.md), feature branch -> main when ready (CONTRIBUTING.md). Agent instructions: CLAUDE.md.

## Open
- **SDD (2026-11-27, branch `feature/sdd`):** specs/ added (process, constitution, templates, system specs of main @ 7807e69, feature 001). Branch model changed: feature branch -> main, android-port retired. Since 002 every change (even trivial) needs a feature folder. 003 (branch `feature/sdd-commands`): `/create <idea>` runs the whole flow; command list in the quick start of specs/README.md. Merged into local main; not pushed yet. Tell jun182 before merging. android-port still has unmerged work (start screen, PlayerPrefs cache, dead-player fix, material leak): merge it into main and update specs/system/. Open questions: specs/system/README.md.
- **b13 (built 2026-11-27, NOT released):** developer mode hidden + session-only (7 taps on build number / Konami). `tools/menu-check.sh` all ok on the Nord 2T (pad via adb keys). Real pad d-pad Konami not tested. UI tweak panel now needs the dev unlock (not "Debug ON").
- GitHub: `NightAnimal333` (original developer) invited as collaborator (write; GitHub has no higher level on a personal repo); invite must be accepted.
- **b12 (built, pushed 2026-11-27, NOT released):** Menu tree (now specs/system/menus.md) implemented (one page at a time, B back, Start = menu, START badge, black/white pad icons, pulse + select haptics, NeverSleep in play). `tools/menu-check.sh` 18/18 ok on Y700 (adb key events; Unity Android: A=JoystickButton0, B=1, Start=10).
- **004 one button style (2026-11-27, branch `feature/button-style`):** all buttons white / black outline / black text, pad selection black; sliders and pad icons restyled; Button style setting + Brush.cs removed; style in `specs/system/ui-style.md`. Verified in the editor and on a Pixel 8 Pro (wireless adb; Windows PC: Unity 2022.2.8f1 at `G:/Unity` with its bundled adb, release key in `~/keys`); menu-check all ok. Real gamepad: works on PC (Xbox 360 pad, editor, developer-checked); not tested on a phone. Not released.
- Found during 004, need their own folders: `Haptics.cs` breaks Windows builds (`Handheld` outside `#if UNITY_ANDROID`); `tools/ui-check.sh` taps an old menu layout.
- Pad steering fixed: stick/d-pad/arrows via `GetAxisRaw("Horizontal")` (checked by adb long-press: player moves). Real pad not tested.
- Gyro screen tilt (CameraShake, tunable `gyroTilt` 3 deg default, slider max 8 = hard cap, proportional to `PlayerController.SteerNorm`; leans +z when moving right): NOT tested, gyro can't be driven over adb. Sign/strength may need tuning.
- Not done: A = reset on sliders (Default button is focusable instead). II badge sits right of II (no room left).
- Open GitHub issues #1-#5 (#1-#3 assigned to Copilot: check PRs; #3 maybe fixed in code). ISSUES.md stale; REVIEW.md not written; old scene DebugPanel = removal candidate.
- Nord 2T has b10: expect the in-app updater to offer the next release (live updater test; do not adb install there).
- Gotcha: new script file => first batch build fails CS0246; rebuild once, `grep "error CS" ~/unity-tools/build.log`.

## Parked
- Pixel 9 Pro XL wireless adb + fresh install (BACKLOG.md). Live-tweak idea => done via UiTunables, extend there.

## Build/test
`LD_LIBRARY_PATH=$HOME/unity-tools/libxml2-legacy/usr/lib ~/Unity/Hub/Editor/2022.2.8f1/Editor/Unity -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod BuildAndroid.Build -logFile ~/unity-tools/build.log` -> Builds/project-serenity-release.apk. `tools/menu-check.sh [apk]`, `OUT=<dir> tools/ui-check.sh [apk]` (2400x1080 Nord). Never push upstream.
