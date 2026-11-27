# Hand-off for a new contributor (and their AI agent)

Welcome. Start here, then read `CLAUDE.md` (rules for agents), `CONTRIBUTING.md` (branches), `HANDOFF.md` (what is going on right now), `CHANGELOG.md` and `MENU-TREE.md`.

- **What this is:** an Android port of the original Serenity game (this fork: `jun182/project-serenity`, original: `NightAnimal333/project-serenity`). Touch, tilt and gamepad steering, a code-built menu, an in-app updater that installs new GitHub releases.
- **Branches:** work on `android-port` (or a feature branch merged into it). `main` is only for releases; don't commit there.
- **Set up:** Unity 2022.2.8f1 with the Android modules. Open the project, or use the headless build command in `HANDOFF.md`. Test on a phone with `tools/menu-check.sh` (adb key events). Real-device testing is needed for gyro and gamepad.
- **Don't break:** the in-app updater (`Updater.cs`, tag format `alpha-bN`, `BuildInfo.Number`), the app id `se.vibing.serenity` and the signing key (kept outside the repo; ask the owner, never commit it).
- **Hidden debug:** debug tools must stay hidden from players. Developer mode = 7 taps on the build number in Settings, or the Konami code on the start screen.
- **Open work:** `gh issue list -R jun182/project-serenity`, `BACKLOG.md`, `ISSUES.md`. Pick something, tell the owner, branch, build, test, merge into `android-port`.
- **Before you finish a session:** update `HANDOFF.md` and `CHANGELOG.md`, commit, push.
