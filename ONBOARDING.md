# Hand-off for a new contributor (and their AI agent)

Welcome. Start here, then read `specs/README.md` (how we work), `specs/constitution.md` (what must not break), `specs/system/` (how the game works today), `CLAUDE.md` (rules for agents), `CONTRIBUTING.md` (branches), `HANDOFF.md` (what is going on right now) and `CHANGELOG.md`.

- **What this is:** an Android port of the original Serenity game (this fork: `jun182/project-serenity`, original: `NightAnimal333/project-serenity`). Touch, tilt and gamepad steering, a code-built menu, an in-app updater that installs new GitHub releases.
- **How we work:** spec-driven, for every change (even a typo fix). An idea (GitHub issue, `BACKLOG.md`, or a request) becomes `specs/features/NNN-name/spec.md`; once its acceptance criteria are approved, plan, build, verify each criterion, update `specs/system/`. In Claude Code: `/spec`, `/plan`, `/tasks`, `/verify`.
- **Branches:** a feature branch from `main`, merged into `main` when ready. Releases (tags) only on the owner's OK.
- **Set up:** Unity 2022.2.8f1 with the Android modules. Open the project, or use the headless build in `specs/system/build-test-release.md`. Test on a phone with `tools/menu-check.sh` (adb key events). Real-device testing is needed for gyro and gamepad.
- **Don't break:** the in-app updater (`Updater.cs`, tag format `alpha-bN`, `BuildInfo.Number`), the app id `se.vibing.serenity` and the signing key (kept outside the repo; ask the owner, never commit it).
- **Hidden debug:** debug tools must stay hidden from players. Developer mode = 7 taps on the build number in Settings, or the Konami code on the start screen.
- **Open work:** `gh issue list -R jun182/project-serenity`, `BACKLOG.md`, `ISSUES.md`, and the open questions in `specs/system/README.md`. Pick something, tell the owner, write the spec.
- **Before you finish a session:** update `HANDOFF.md` and `CHANGELOG.md`, commit, push.
