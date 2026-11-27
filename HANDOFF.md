# Board project-serenity (2026-11-27)
Resume order: (1) ask the user for b7 test results, (2) tick off open issues + write REVIEW.md, (3) menu rework (b8).

## State
- b5/b6 features tested OK by the user on the Pixel (modes, difficulty, win, shake, near miss, help, debug page).
- **b7** (tuning + near-miss feel, user request 2026-11-27): see CHANGELOG "b7". Sent to the Pixel as `project-serenity-b7.apk`.
  Untested on the phone. Ask the user:
  - Which tuning slider "didn't work"? The code reads every tunable (checked 2026-11-27); my guess was that the near-miss effect was
    too small to notice (tiny shake, no vibration). If one really has no effect, find out which one.
  - Near-miss vibration strong enough (slider 0-255, default 160)? Slide too subtle/strong (default 0.35), right direction
    (negative = flip)? Slide + near-miss shake are off when Screen shake is OFF; vibration is not.
  - Do the tuning page (3 columns) and the per-slider Default buttons fit/work on the Pixel? Labels auto-shrink (best fit).
- **Tester install failure (2026-11-27): CLOSED, tester's side.** Pixel 8 tester got "problem with the app file" on b7; with
  `alpha-b7-rebuild1` (GitHub pre-release on jun182/project-serenity, always pass `-R`) it worked; they don't know what changed.
  Not reproduced: b7-rebuild1 (hash verified vs GitHub download) installs + launches on the Y700 (Android 16) and the Nord 2T
  (Android 14, arm64). Menu/gameplay untested on the Nord 2T. All builds share one debug key (CN=Android Debug, made on this laptop);
  a build from another machine would need uninstall first. Testers are told to open GitHub issues for problems.
- **Nord 2T** (CPH2399, USB serial `INBY4PTGT8NJROFY`): wireless adb via `adb tcpip 5555` over USB, then
  `adb connect 100.64.3.98:5555` (Tailscale `johola2t`). Lasts until reboot.

- **Release signing (2026-11-27, user approved):** keystore `~/keys/serenity-release.jks` + `~/keys/serenity-release.pass` (alias `serenity`, outside any repo;
  the user must back both up). `BuildAndroid.cs` now sets package **`se.vibing.serenity`**, release signing (refuses to build without the key), release build
  (no "Development Build" bar), output `Builds/project-serenity-release.apk`. First build was running when this was written: verify with
  `apksigner verify --print-certs` (not the debug key), install on the Nord 2T next to the old app (different package), run `tools/ui-check.sh`.
  Everyone must uninstall the old `com.DefaultCompany.ProjectSerenity` once (settings reset). Overlay still says "build b7" (hard-coded in PauseMenu.cs ~206).
- **In-app updater (user approved 2026-11-27):** check GitHub at EVERY start, fork `jun182/project-serenity`, fixed tags `alpha-bN`, in-app download + install.
  Design: `GET api.github.com/repos/jun182/project-serenity/releases` (list endpoint: `/latest` skips pre-releases), highest `alpha-bN` vs current build number;
  one build-number source (`BuildInfo`), also used as `bundleVersionCode` (Android refuses updates with a lower code). "Update available" button on the start
  screen → download APK to app files dir with progress → `ACTION_VIEW` install intent. Needs: INTERNET permission (`forceInternetPermission`),
  `REQUEST_INSTALL_PACKAGES`, a custom manifest in `Assets/Plugins/Android/`, `canRequestPackageInstalls()` check (else open "Install unknown apps" settings for the app).
  **No androidx in the generated Gradle project, so no `FileProvider`:** write a minimal Java `ContentProvider` (serves the one APK, read-only) in `Assets/Plugins/Android/`.
  Failures must stay silent (offline = no button). Test on the Nord 2T by publishing a higher-numbered test release. Updates only install over builds with the release key.
- **User's instructions (2026-11-27, end of session):** build the updater, then commit + push ALL uncommitted work (explicit OK given for that, after the updater is in),
  test on the Nord 2T and report whether it works; if it does, cut a new release (`alpha-bN`, fork `jun182/project-serenity`, pass `-R`), then update this board and tell the user to /clear.
  Signing build `Builds/project-serenity-release.apk` was started in the background (check `pgrep -x Unity`, `~/unity-tools/build.log`); verify it first.
- **Uncommitted:** font change, interim timer, tools/ui-check.sh, signing changes. The auto-mode classifier blocked my commit/push (reason unclear): the user commits or says go explicitly.

## Next 1: tick off open issues
- ISSUES.md is stale: #1-#4, #8, #9 look resolved by user tests (install, touch, pause, tilt, death, music): confirm with the user, move to Done.
  Still open: #5 controller (untested), #7 frame rate 90 → ~80 fps.
- **REVIEW.md**: both reviews are in `../project-serenity-review/` (codex.md, gemini.md; task.md = brief). Check each finding against the
  current code, write verified findings + feature ideas into `REVIEW.md` in the repo. Already fixed in b6 (from Codex): null pause panel on
  start screen, double timeInLight, blocker overlap count, FindObjectOfType per frame, hit log spam, spawnMaxX cap 5.
  Gemini's first finding (Back/Esc NRE on MainMenu) is probably the same as the b6 null-panel fix: verify.

## Next 2: menu rework (b8), approved 2026-11-27
- Start screen (MainMenu scene): title + "Touch to start", or "Press Start" if `Input.GetJoystickNames()` has a non-empty name; no buttons.
  **Android Back on the start screen = quit the app** (user's choice 2026-11-27).
- Then ONE shared menu (same code as the pause menu): Play/Resume, Mode (start only), **Settings** (frame rate, screen shake,
  Endless obstacle slider), **Controls** (tilt on/off, invert tilt, how-to-play text), Debug toggle + debug tools (when on), Quit.
  All UI is built in code in `Assets/Scripts/PauseMenu.cs` (scene UI from the original main menu may need hiding).
- User finds the pause menu cluttered (2026-11-27). Proposed (not yet approved): top level = big highlighted Resume, Settings,
  Controls, How to play, Quit to menu (bottom, muted), Debug page button only when Debug is ON (5 tools + tuning sliders).
  Endless slider only in Endless mode. "Quit game" off the pause menu (start screen + Back). Up to 15 controls → 5-6.
- After each build: send, commit + push to android-port (user OK'd unattended send/commit/push 2026-11-27), update CHANGELOG.md.

## Notes
- Zoom test cubes: debug toggle (default OFF). Music: plain-track time is a debug slider (default 0 = original).
- Scene DebugPanel (old sliders: speed, max health, drag...) drives unused PlayerController fields; hidden unless Debug is ON.
  Not touched in b7. Candidate to remove in b8.
- BACKLOG.md: tilt recentre, target SDK warning, controller stick.

## How to rebuild (laptop, CachyOS)
`cd ~/Desktop/_claude-projects/project-serenity && LD_LIBRARY_PATH=$HOME/unity-tools/libxml2-legacy/usr/lib ~/Unity/Hub/Editor/2022.2.8f1/Editor/Unity -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod BuildAndroid.Build -logFile ~/unity-tools/build.log`
(background; ~8 min; output `Builds/project-serenity-debug.apk`, copy to `Builds/project-serenity-bN.apk`).
Send: `tailscale file cp <apk> pixel-9-pro-xl-1:`. Stop Unity with `pkill -x Unity` (never -f).
Environment quirks: Unity CLI ~/.local/bin/unity 1.1.0, legacy libxml2 via LD_LIBRARY_PATH, licensing client from Hub 1.19.0. No adb to the Pixel.

## Parked
- Ask the original team before publishing a port. Never push to upstream; work on branch android-port (origin = jun182 fork).
