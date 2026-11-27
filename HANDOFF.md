# Board project-serenity (2026-11-27)
Resume: `git pull`, read AGENT-CHAT.md, then `gh issue list -R jun182/project-serenity`. Everything is on `main` now (android-port merged 2026-11-27 and retired). Release alpha-b13 ONLY on the user's OK (tag + APK `Builds/project-serenity-release.apk`). Workflow: spec-driven (specs/README.md, `/create <idea>`), feature branch -> main (CONTRIBUTING.md). Agent instructions: CLAUDE.md. b12 is installed on the Y700 (adb 192.168.18.155:42501, reconnect with `adb connect`).

## Open
- **SDD trial (user decided 2026-11-27: try Arthur's workflow, see what we learn):** specs/ (process, constitution, system specs, feature folders 001-003), `.claude/commands/` `/spec` `/plan` `/tasks` `/verify` `/create`. Every change, even trivial, gets a folder in specs/features/. Note what helps and what is overhead. **specs/system/ was written from main @ 7807e69 and does not yet describe the merged android-port work (start screen Play button/no ripples, PlayerPrefs cache, dead-player freeze, material leak): update it.** Open questions: specs/system/README.md.
- **b13 (built 2026-11-27, NOT released):** hidden session-only developer mode (7 taps on build number / Konami), cached PlayerPrefs reads + dead-player freeze (PR #6/#8 reimplemented, #7 merged; freeze NOT runtime-tested), start screen: Play button in touch mode, no "Touch to start", no ripples (Space starts on PC/Web, untested; no PC/Web build exists). `tools/menu-check.sh` 27/27 ok on the Nord 2T. Real pad d-pad Konami not tested.
- **Issue #9** (screen jitter on A/D direction change): draft PR #10 (branch `copilot/screen-jitters-steering-direction`, SteerNorm made frame-time independent; reviewed by reading, not built/run). Parked in BACKLOG by the user. Targets old android-port base: retarget to main when picked up.
- Issues #1/#2 look done (b13 / PR #7) but are still open; #3 open until the freeze is runtime-tested.
- Not tested: gyro tilt (can't be driven over adb), real pad, Rhombus buttons + UI tweak panel on screen, A = reset on sliders not done.
- Open GitHub issues #4 (pad menu test, needs human), #5 (90 fps shows ~80). ISSUES.md stale; REVIEW.md not written; old scene DebugPanel = removal candidate.
- `NightAnimal333` (Arthur) is a collaborator with push (verified 2026-11-27). Agents talk via AGENT-CHAT.md.
- **OPEN (user, later):** add `mbpa1706@hamnesjo.se` to the GitHub account (github.com/settings/emails, then click the verification mail) so the older commits made with it link to jun182. The gh token lacks the `user` scope.
- Gotchas: Nord 2T (`adb connect 100.64.3.98:5555`, may need reconnect) refuses `pm clear`; Nord 2T has b10 (live updater test: do not adb install there). New script file => first batch build fails CS0246, rebuild once, `grep "error CS" ~/unity-tools/build.log`.

## Parked
- Pixel 9 Pro XL wireless adb + fresh install (BACKLOG.md). Windows/WebGL build (modules not installed). Smooth the gyro lean if it feels jittery.

## Build/test
`LD_LIBRARY_PATH=$HOME/unity-tools/libxml2-legacy/usr/lib ~/Unity/Hub/Editor/2022.2.8f1/Editor/Unity -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod BuildAndroid.Build -logFile ~/unity-tools/build.log` -> Builds/project-serenity-release.apk. `tools/menu-check.sh [apk] [device]`, `OUT=<dir> tools/ui-check.sh [apk]`. Never push upstream.
