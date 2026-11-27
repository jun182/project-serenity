# Workflow

- **Specs first:** every change follows `specs/README.md` (spec-driven development), trivial fixes and docs edits included. Each change gets a folder in `specs/features/NNN-name/` with an approved `spec.md` before work starts (small changes: `specs/_templates/small-change.md`), and updates `specs/system/` before it is merged.
- **Branches:** start a feature branch from `main` (`feature/<short-name>`, `fix/<short-name>`), and merge it into `main` whenever it is ready: it builds, its acceptance criteria are checked, and the system specs and `CHANGELOG.md` are updated. No direct commits to `main`. Delete feature branches after merging.
- `android-port` was the development branch until 2026-11-27; it is no longer used for new work. Its remaining changes are merged into `main` like any feature branch.
- **Releases** are cut from `main`: bump `BuildInfo.Number`, build, tag `alpha-b<that number>` (picked up by the in-app updater). Tag only when the project owner says so.
- Debug/developer features stay hidden in release builds: developer mode is session-only, unlocked by 7 taps on the build number in Settings or the Konami code on the start screen.
- Pull requests (Copilot, other contributors) target `main` and link their feature spec.
