# Board project-serenity (2026-11-27)
Resume: `git pull`, read AGENT-CHAT.md, then `gh issue list -R jun182/project-serenity`. Dev branch `android-port` (b13, pushed), `main` = release (still at the b12-era merge + docs; b13 NOT merged to main, NOT released). Release alpha-b13 ONLY on the user's OK (merge android-port -> main, tag, attach APK `Builds/project-serenity-release.apk`).

## Open
- **b13 on android-port:** hidden session-only developer mode (7 taps on build number / Konami), cached PlayerPrefs reads + dead-player freeze (PR #6/#8 reimplemented, #7 merged; freeze NOT runtime-tested), start screen: Play button in touch mode, no "Touch to start", no ripples (Space starts on PC/Web, untested; no PC/Web build exists). `tools/menu-check.sh` 27/27 ok on the Nord 2T.
- **Issue #9** (screen jitter on A/D direction change) assigned to Copilot, base android-port: review its PR. Known: camera lean is gyro-only (CameraShake, SteerNorm); jitter cause on other inputs unknown.
- Not tested: gyro tilt, real pad Konami/d-pad, Rhombus buttons + UI tweak panel on screen, A = reset on sliders not done.
- Open GitHub issues #4 (pad menu test, needs human), #5 (90 fps shows ~80). ISSUES.md stale; REVIEW.md not written; old scene DebugPanel = removal candidate.
- `NightAnimal333` (Artur, original dev) invited as collaborator (write only, personal repo): invite must be accepted. Agents talk via AGENT-CHAT.md; his agent starts at ONBOARDING.md / CLAUDE.md.
- Git identity is now global `jun182` + noreply email (older commits show as unlinked author).
- Gotchas: Nord 2T (`adb connect 100.64.3.98:5555`, may need reconnect) refuses `pm clear` (prefs persist between runs). New script file => first batch build fails CS0246, rebuild once.

## Parked
- Pixel 9 Pro XL wireless adb + fresh install (BACKLOG.md). Windows/WebGL build (modules not installed). Smooth the gyro lean if it feels jittery.

## Build/test
`LD_LIBRARY_PATH=$HOME/unity-tools/libxml2-legacy/usr/lib ~/Unity/Hub/Editor/2022.2.8f1/Editor/Unity -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod BuildAndroid.Build -logFile ~/unity-tools/build.log` -> Builds/project-serenity-release.apk. `tools/menu-check.sh [apk] [device]`, `OUT=<dir> tools/ui-check.sh [apk]`. Never push upstream.
