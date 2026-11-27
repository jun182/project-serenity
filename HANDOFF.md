# Board project-serenity (2026-11-27)
Resume: menu rework (b7), user-approved: start screen + one shared menu (details below). First finish the b6 hand-off steps.

## In flight when the session was cleared
- **b6 build** (help screen + Codex fixes) was running in the background. Check `~/unity-tools/build.log` for `Build result: Succeeded`
  (early `error CS` lines about new classes are a harmless first compile pass). Then `cp Builds/project-serenity-debug.apk Builds/project-serenity-b6.apk`,
  `tailscale file cp Builds/project-serenity-b6.apk pixel-9-pro-xl-1:`, delete the b6 line in `~/april-claudia/running.md`.
  If Unity isn't running and there's no result, rebuild (command below).
- **Gemini review** (agy, gemini-4-neon-high) was running, writing to `/tmp/claude-1000/-home-johola-Desktop--claude-projects/44d0b20f-4eab-4121-9580-c54628560ab7/scratchpad/review/gemini.md` (brief: `task.md`).
  Codex review is copied to `../project-serenity-review/codex.md`. Copy gemini.md there too. Then write `REVIEW.md` in the repo:
  verified findings (check each against the code) + feature ideas from both, for the user. Codex findings already fixed in b6:
  null pause panel on start screen, double timeInLight, blocker overlap count, FindObjectOfType per frame, hit log spam, spawnMaxX cap 5.

## Next: menu rework (b7), approved 2026-11-27
- Start screen (MainMenu scene): title + "Touch to start", or "Press Start" if `Input.GetJoystickNames()` has a non-empty name; no buttons.
- Then ONE shared menu (same code as the pause menu): Play/Resume, Mode (start only), **Settings** (frame rate, screen shake,
  Endless obstacle slider), **Controls** (tilt on/off, invert tilt, how-to-play text), Debug toggle + debug tools (when on), Quit.
  All UI is built in code in `Assets/Scripts/PauseMenu.cs` (scene UI from the original main menu may need hiding).
- After b7: build, send, commit to android-port, update CHANGELOG.md.

## Open
- Tested OK by the user: death (b4), health bar, tilt, pause slide fix. b5/b6 features untested: modes, difficulty, win, shake, near miss, help, debug tuning page.
- Zoom test cubes now a debug toggle (default OFF). Music: plain-track time is a debug slider (default 0 = original).
- Scene DebugPanel sliders (Damage etc.) no longer affect damage/heal (now GameSettings tunables).
- ISSUES.md #1-5, #7; BACKLOG.md (tilt recentre, target SDK warning, controller stick).

## How to rebuild (laptop, CachyOS)
`cd ~/Desktop/_claude-projects/project-serenity && LD_LIBRARY_PATH=$HOME/unity-tools/libxml2-legacy/usr/lib ~/Unity/Hub/Editor/2022.2.8f1/Editor/Unity -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod BuildAndroid.Build -logFile ~/unity-tools/build.log`
(background; ~8 min). Send: `tailscale file cp <apk> pixel-9-pro-xl-1:`. Stop Unity with `pkill -x Unity` (never -f).
Environment quirks: Unity CLI ~/.local/bin/unity 1.1.0, legacy libxml2 via LD_LIBRARY_PATH, licensing client from Hub 1.19.0. No adb to the Pixel.

## Parked
- Ask the original team before publishing a port. Never push to upstream; work on branch android-port (origin = jun182 fork).
