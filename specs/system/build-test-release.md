# Build, test, release

## Build settings (`Assets/Editor/BuildAndroid.cs`)
- Unity 2022.2.8f1 with Android modules. IL2CPP, ARM64 only. Landscape left/right, no portrait.
- App id `se.vibing.serenity`; versionCode = `BuildInfo.Number`, versionName `0.1.<N>`. Release build (no development watermark).
- Release keystore: `$SERENITY_KEYSTORE` or `~/keys/serenity-release.jks`, alias `serenity`; password `$SERENITY_KEYPASS` or
  `~/keys/serenity-release.pass`. Missing key = build refuses (exit 1) instead of using the debug key.
- Scenes: `MainMenu`, `GameplayScene`. Output `Builds/project-serenity-release.apk`.
- Min SDK 22; target SDK low, which gives the "built for an older version of Android" install warning (BACKLOG).

## Headless build
```
LD_LIBRARY_PATH=$HOME/unity-tools/libxml2-legacy/usr/lib ~/Unity/Hub/Editor/2022.2.8f1/Editor/Unity -batchmode -nographics -quit \
  -projectPath "$PWD" -buildTarget Android -executeMethod BuildAndroid.Build -logFile ~/unity-tools/build.log
```
(Linux paths of the machine it was set up on.) A new script file makes the first batch build fail with CS0246: build once more, then
`grep "error CS" ~/unity-tools/build.log`.

## Test tools
- `tools/menu-check.sh [apk] [adb-device]`: installs (optional), clears app data, walks the menu tree with adb key events and compares the
  `MENU page=... sel=...` log lines; also checks dev mode off by default, 7-tap unlock, dev page, and locked again after restart. Exit 1 on mismatch.
- `OUT=<dir> tools/ui-check.sh [apk]`: screenshots (laid out for a 2400x1080 screen).
- Needs a real device: tilt/gyro, real gamepad, haptics, sound, frame rate.

## Release
Only on the project owner's OK (constitution 4):
1. Bump `BuildInfo.Number`. 2. Build. 3. Merge to `main`. 4. Create GitHub release tag `alpha-b<N>` with the APK attached.
Installed copies then offer the update (`updater.md`).
