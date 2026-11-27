# Workflow

- `android-port` is the **development branch**: all Android-port work lands here. Small changes can be committed directly; bigger or risky ones go on a feature branch (`feature/<short-name>`, `fix/<short-name>`) that is merged into `android-port` once it builds and is tested. Delete feature branches after merging.
- `main` is the **release branch**: it only receives tested work from `android-port` (merge, no direct commits) and is what releases (`alpha-bN` tags, picked up by the in-app updater) are cut from. Merge to `main` only when the project owner says so.
- Bump `BuildInfo.Number` for every release; the tag must be `alpha-b<that number>`.
- Debug/developer features stay hidden in release builds: developer mode is session-only, unlocked by 7 taps on the build number in Settings or the Konami code on the start screen.
- Pull requests (Copilot, other contributors) target `android-port`.
