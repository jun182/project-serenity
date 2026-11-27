# Board project-serenity (2026-11-27)
Resume: (1) ask the user for b9 test results, (2) input-adaptive menu + menu rework (b10), (3) tick off ISSUES.md + write REVIEW.md.

## Open
- **b9 released** (GitHub pre-release `alpha-b9` on jun182/project-serenity, always pass `-R`; package `se.vibing.serenity`, release-signed with `~/keys/serenity-release.jks`
  + `.pass`, user must back both up). In-app updater verified on the Nord 2T (CPH2399, wireless adb `100.64.3.98:5555`, see memory nord-2t-test-device):
  b8 -> b9 via start-screen button, install prompt, versionCode 9 (lastUpdateTime 16:47:58). One earlier attempt failed with
  `INSTALL_FAILED_VERIFICATION_FAILURE` while a Play Protect dialog flashed; the next attempt succeeded. Cause not found: watch for it with testers.
  Needs "Install unknown apps" allowed for the app once (button opens that page). Keep the phone awake (it pauses the download when the screen sleeps).
- Everyone must uninstall the old `com.DefaultCompany.ProjectSerenity` once. Old b7 testers have no updater: they install b9 by hand.
- Build number lives in `Assets/Scripts/BuildInfo.cs` (also versionCode). Bump for every release, tag `alpha-bN`, asset must be the `.apk`.
  Manifest additions live in `Assets/Plugins/Android/SerenityUpdater.androidlib/` (NOT a loose AndroidManifest.xml: that replaces Unity's own and the app has no launcher activity).
- Still untested by the user on a phone: b7 tuning/near-miss feel (vibration strength 0-255 default 160, slide default 0.35), 3-column tuning page.
- ISSUES.md stale: #1-#4, #8, #9 look resolved, confirm with the user. Open: #5 controller, #7 frame rate 90 -> ~80 fps.
- REVIEW.md: reviews in `../project-serenity-review/` (codex.md, gemini.md). Verify findings against code, write verified ones + ideas into REVIEW.md.
  Already fixed in b6: null pause panel, double timeInLight, blocker overlap, FindObjectOfType per frame, hit log spam, spawnMaxX cap.

## Next: menu rework (b10), approved 2026-11-27, see BACKLOG.md "Input-adaptive start menu" (user request, do next)
- Start screen: touch mode = "Touch to start" + droplet-ripple + vibration at the touch point, Exit as small top-left button; controller mode = Play/Exit
  visible, stick/d-pad select, A/Start confirm, space for controller icons (placeholder). Switch dynamically by last input. Back on start screen = quit.
- Then ONE shared menu (pause code): Resume, Settings, Controls, How to play, Quit to menu; Debug page only when Debug on. Debug menu: game speed slider (BACKLOG).
  UI is built in code in `Assets/Scripts/PauseMenu.cs`. Old scene DebugPanel (unused fields) is a removal candidate.

## How to rebuild / test
Build: `cd ~/Desktop/_claude-projects/project-serenity && LD_LIBRARY_PATH=$HOME/unity-tools/libxml2-legacy/usr/lib ~/Unity/Hub/Editor/2022.2.8f1/Editor/Unity -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod BuildAndroid.Build -logFile ~/unity-tools/build.log`
(background, ~8 min, output `Builds/project-serenity-release.apk`; the log may show CS0246 errors for new scripts yet say Succeeded: check the APK with `aapt dump badging`).
Test: `tools/ui-check.sh [apk]` (Nord 2T, screenshots + sheet; set OUT=dir). Release: `gh release create alpha-bN <apk> -R jun182/project-serenity --prerelease --target android-port`.
Stop Unity with `pkill -x Unity` (never -f). Never push to upstream; work on android-port (origin = jun182 fork).

## Parked
- Ask the original team before publishing a port. BACKLOG.md: tilt recentre, target SDK warning (targetSdk 32), controller stick.
