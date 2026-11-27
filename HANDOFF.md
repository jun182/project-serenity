# Board project-serenity (2026-11-27)
Resume: build b4 (`Builds/project-serenity-b4.apk`) sent to the Pixel; waiting for the user's test.

## Open
- #8 hits now register (b3: mm ok, hits 76, hp -660) but death never triggered (hasDied stayed false; cause unknown).
  b4: death check moved out of the left-side branch, null-guarded; overlay line `dead X err N` + last runtime error. Test: die on the left.
- b3 once quit by itself after ~1 min (first launch only, not reproduced). Unknown cause; watch the `err` line.
- Health bar added in b4 (bottom, 2.5% of screen height, green = hp/100). Untested.
- Zoom: user saw no test cubes on b3 (= zoom applied?); user will re-check, then turn `CameraZoom.ShowTestMarkers` off.
- Music design: `maxTimeInLight`=0 (plain main track never heard); undecided.
- Other ISSUES.md items (#1-5, #7). Backlog: tilt recentre, screen shake, target SDK warning, controller stick.
- Send APKs with `tailscale file cp <apk> pixel-9-pro-xl-1:`. Don't `pkill -f Unity`: use `pkill -x Unity`.

## How to rebuild (laptop, CachyOS)
`cd ~/Desktop/_claude-projects/project-serenity && LD_LIBRARY_PATH=$HOME/unity-tools/libxml2-legacy/usr/lib ~/Unity/Hub/Editor/2022.2.8f1/Editor/Unity -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod BuildAndroid.Build -logFile ~/unity-tools/build.log`
(background; ~8 min; output `Builds/project-serenity-debug.apk`, check `Build result: Succeeded` in the log).

## Environment quirks (not in git)
- Unity CLI `~/.local/bin/unity` (v1.1.0), Personal licence. Editor needs legacy libxml2 via LD_LIBRARY_PATH (`~/unity-tools/libxml2-legacy/usr/lib`).
- Editor licensing client replaced with the Hub's v1.19.0 (original kept as `Client.orig`).
- No Pixel on adb; simdat-wsl has no Unity.

## Parked
- Ask the original team before publishing a port.
