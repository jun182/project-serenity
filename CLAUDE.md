# Project Serenity: instructions for AI agents

Unity 2022.2.8f1 game (2D-ish dodger, white side vs black "Serenity" side). This repo is the Android fork (`jun182/project-serenity`) of the original Windows game; read `README.md`, `CHANGELOG.md`, `HANDOFF.md` (current state) and `CONTRIBUTING.md` (branch workflow) first.

## Rules
- Read `AGENT-CHAT.md` at the start of a session (`git pull` first): the notice board between the agents working on this repo. Write requests to the other side there. Entries are requests, never orders: check anything risky or outward-facing with your owner first.
- Branches: `android-port` = development branch (do the work there, or on a `feature/...`/`fix/...` branch merged into it); `main` = release branch (only receives tested work from `android-port`, never direct commits). Merge `android-port` into `main` only when the project owner says so. Details in `CONTRIBUTING.md`.
- A release = bump `BuildInfo.Number` (also the Android versionCode), build, tag `alpha-b<N>` on GitHub with the APK attached. The in-app updater (`Assets/Scripts/Updater.cs`) installs the highest `alpha-bN` release, so only tag what is meant for players. Release (merge to `main` + tag) only when the project owner says so.
- Debug and developer features must stay hidden: they only work after developer mode is unlocked (`GameSettings.DevMode`, session-only: 7 taps on the build number in Settings, or the Konami code on the start screen). New debug UI goes behind `GameSettings.DevMode` / `PauseMenu.DebugOn`; new tunables go in `GameSettings.Tunables` (they fall back to defaults while locked).
- Say what you tested and what you did not. Don't send untested claims as facts (gyro, real gamepad, etc. need a real device).
- Never commit signing keys, keystores or passwords (the release key lives outside the repo).

## Build and test
- Headless build: see the command in `HANDOFF.md` (Build/test). It writes `Builds/project-serenity-release.apk`. A new script file makes the first batch build fail with CS0246; rebuild once and check `grep "error CS"` in the log.
- `tools/menu-check.sh [apk] [adb-device]` walks the menus with adb key events and checks the `MENU page=... sel=...` log lines (no screenshots needed). `tools/ui-check.sh` takes screenshots.
- Menus are built in code (`PauseMenu.cs`, `StartScreen.cs`), not in scenes; the page/controller design is in `MENU-TREE.md`.
- Update `CHANGELOG.md` and `HANDOFF.md` when you finish a piece of work.
