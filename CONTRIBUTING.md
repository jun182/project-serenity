# Workflow

- `main` is the release branch: it always builds and is what releases (`alpha-bN` tags, picked up by the in-app updater) are cut from.
- All work happens on feature branches (`feature/<short-name>`, `fix/<short-name>`) and is merged into `main` (pull request or merge) once it builds and is tested.
- Don't commit straight to `main`; don't leave long-lived side branches (delete after merging).
- Bump `BuildInfo.Number` for every release; the tag must be `alpha-b<that number>`.
- Debug/developer features stay hidden in release builds: developer mode is session-only, unlocked by 7 taps on the build number in Settings or the Konami code on the start screen.
