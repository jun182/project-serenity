# Board project-serenity (2026-11-27)
Resume: FIRST print the open issues (ISSUES.md open items + the Open list below, short) for the user to read, THEN continue: (1) check b10 start screen on the Nord 2T (see Open), commit; (2) shared pause-menu rework (rest of b10); (3) ISSUES.md + REVIEW.md.

## Open
- **b9 is released** (GitHub pre-release `alpha-b9`, jun182/project-serenity, always `-R`; asset `project-serenity-b9.apk`; in-app updater verified Nord 2T b8->b9).
  Details unchanged: package `se.vibing.serenity`, keystore `~/keys/serenity-release.jks`+`.pass` (user must back up), uninstall old `com.DefaultCompany.ProjectSerenity` once,
  build number in `Assets/Scripts/BuildInfo.cs` (= versionCode), manifest additions in `Assets/Plugins/Android/SerenityUpdater.androidlib/`. Stray `INSTALL_FAILED_VERIFICATION_FAILURE` once, cause unknown.
- **b10 in progress, UNCOMMITTED on android-port** (BuildInfo=10): new `Assets/Scripts/StartScreen.cs` (+ .meta; touch mode "Touch to start" + ripple + `Haptics.Tap()`, small Exit top-left;
  controller mode Play/Exit, stick/d-pad + A/Start, placeholder "A" icon; Back = close help, then quit), PauseMenu wiring (Update button moved right), PauseMenu FPS overlay `raycastTarget=false`,
  PlayerController `timeInLight` only reset on serenity side (plain-track slider now works).
  Phone-tested on Nord 2T (versionCode 10): touch mode OK (Exit, ripple, tap starts game). Hint text was white on the white half: now timer-style (two clipped halves, black/white + outline), build started 17:05 (APK `Builds/project-serenity-release.apk`): install it, run `OUT=<dir> tools/ui-check.sh Builds/project-serenity-release.apk`, check the hint is readable on both halves. Source is committed locally.
  **Controller mode untested** (no pad attached). Not pushed, not released (release only with the user's OK: `gh release create alpha-b10 <apk> -R jun182/project-serenity --prerelease --target android-port`).
- **Gotcha:** a NEW script file makes the first batch build fail CS0246 (meta created mid-build) yet log says Succeeded: rebuild once, then check `grep -c "error CS" ~/unity-tools/build.log` and that the APK contains the type
  (`unzip -p apk assets/bin/Data/Managed/Metadata/global-metadata.dat | strings | grep Name`). No `aapt` on this laptop: use `adb shell dumpsys package se.vibing.serenity | grep versionCode`.
  `tools/ui-check.sh` now creates OUT and has a ripple shot (3x3 sheet).
- ISSUES.md stale: #3, #4, #8 resolved in code (Gemini check); #1/#2/#9 need the user's phone confirmation; open: #5 controller, #7 frame rate 90 -> ~80 fps.
- **REVIEW.md not written yet.** Gemini checked both reviews: `../project-serenity-review/gemini-verification-2026-11-27.md` (I spot-checked: #2 timeInLight and #9 FPS overlay real and FIXED in b10;
  #3 "frame-rate dependent movement" is WRONG; #20 simultaneous touches cancel = design choice). Unverified by me and worth doing later: per-frame PlayerPrefs reads, obstacle pooling/burst cap,
  Material leak in HideObjectsPerSide, dead player still moves/heals, win timer uses unscaledTime, safe area, DrawFullscreenPass RT leak. Ideas list is at the end of that file.
- Efficiency question (user: context fills fast): separate session opened in `~/Desktop/_claude-projects/efficiency-review/` (HANDOFF.md there; it also asks Codex for a second review). Result: its `REPORT.md`.
- `~/april-claudia/running.md` has a stale line for the b10 build (delete it).

## Next: rest of b10 = shared menu (approved), see BACKLOG.md
- ONE menu (pause code, `PauseMenu.cs`): Resume, Settings, Controls, How to play, Quit to menu; Debug page only when Debug on + game speed slider (BACKLOG, Time.timeScale 0.5-5x, reset on leave).
  Old scene DebugPanel (unused fields) is a removal candidate. Then build, `tools/ui-check.sh`, commit, tell the user; push/release only on OK.

## How to rebuild / test
Build: `cd ~/Desktop/_claude-projects/project-serenity && LD_LIBRARY_PATH=$HOME/unity-tools/libxml2-legacy/usr/lib ~/Unity/Hub/Editor/2022.2.8f1/Editor/Unity -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod BuildAndroid.Build -logFile ~/unity-tools/build.log`
(background, ~1-8 min, output `Builds/project-serenity-release.apk`). Test: `OUT=<existing dir> tools/ui-check.sh [apk]` (Nord 2T wireless adb `100.64.3.98:5555`, memory nord-2t-test-device).
Stop Unity with `pkill -x Unity` (never -f). Never push to upstream; work on android-port (origin = jun182 fork).

## Parked
- Ask the original team before publishing a port. BACKLOG.md: tilt recentre, target SDK warning (targetSdk 32), controller stick, run summary/retry and other ideas from the Gemini file.
