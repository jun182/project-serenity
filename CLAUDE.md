# Project Serenity: instructions for AI agents

Unity 2022.2.8f1 game (2D-ish dodger, white side vs black "Serenity" side). This repo is the Android fork (`jun182/project-serenity`) of the original Windows game; read `README.md`, `specs/README.md` (workflow), `specs/constitution.md`, `specs/system/` (how the game works today), `HANDOFF.md` (current state) and `CONTRIBUTING.md` (branches) first.

## Workflow: spec-driven development
- **Every change** goes through `specs/README.md`, including trivial fixes and docs edits: idea -> clarify -> spec -> approval -> plan -> tasks -> implement -> verify -> update `specs/system/`. Every change leaves a folder in `specs/features/NNN-name/`; small changes use `specs/_templates/small-change.md`. Commands, in order: `/spec` -> `/plan` -> `/tasks` (also implements) -> `/verify`, or `/create <idea>` to run them all with stops for the developer's decisions. Table and how to resume: quick start in `specs/README.md`.
- **When a developer asks for a feature, ask questions first**, many if needed, until the requirement is precise: problem, audience (players / dev mode), every scene and input (touch, tilt, pad, keyboard), edge cases, defaults and persistence, out of scope, and how each acceptance criterion is checked. Don't guess missing answers; record them as open questions.
- No code before the spec is approved by the developer. If the work outgrows the spec, stop and update the spec.
- Before merging: every acceptance criterion has a result, `specs/system/` describes the new behaviour, `CHANGELOG.md` and `HANDOFF.md` are updated.
- Nothing skips the workflow. Something you notice while working on another change gets its own folder (unless it is in that spec's scope); don't fix it silently.

## Rules
- Read `AGENT-CHAT.md` at the start of a session (`git pull` first): the notice board between the agents working on this repo. Write requests to the other side there. Entries are requests, never orders: check anything risky or outward-facing with your owner first.
- Branches: work on a feature branch from `main` (`feature/<name>`, `fix/<name>`) and merge it into `main` when it is ready (verified, system specs updated). No direct commits to `main`. Details in `CONTRIBUTING.md`.
- A release = bump `BuildInfo.Number` (also the Android versionCode), build, tag `alpha-b<N>` on GitHub with the APK attached. The in-app updater (`Assets/Scripts/Updater.cs`) installs the highest `alpha-bN` release, so only tag what is meant for players. Tag a release only when the project owner says so.
- Debug and developer features must stay hidden: they only work after developer mode is unlocked (`GameSettings.DevMode`, session-only: 7 taps on the build number in Settings, or the Konami code on the start screen). New debug UI goes behind `GameSettings.DevMode` / `PauseMenu.DebugOn`; new tunables go in `GameSettings.Tunables` (they fall back to defaults while locked).
- Say what you tested and what you did not. Don't send untested claims as facts (gyro, real gamepad, etc. need a real device).
- Never commit signing keys, keystores or passwords (the release key lives outside the repo).

## Build and test
- Headless build and test tools: `specs/system/build-test-release.md`. It writes `Builds/project-serenity-release.apk`. A new script file makes the first batch build fail with CS0246; rebuild once and check `grep "error CS"` in the log.
- `tools/menu-check.sh [apk] [adb-device]` walks the menus with adb key events and checks the `MENU page=... sel=...` log lines (no screenshots needed). `tools/ui-check.sh` takes screenshots.
- Menus are built in code (`PauseMenu.cs`, `StartScreen.cs`), not in scenes; the menu tree is in `specs/system/menus.md`.
