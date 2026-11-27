# In-app updater

Code: `Updater.cs`, `BuildInfo.cs`, `Assets/Plugins/Android/UpdateProvider.java` (+ `SerenityUpdater.androidlib`). Constitution rules 1 and 2.

## Check
- Every time the start screen loads: GET `https://api.github.com/repos/jun182/project-serenity/releases?per_page=30`, timeout 15 s.
- Tag format `alpha-b<N>`: N = the leading digits (max 6), so `alpha-b7-rebuild1` counts as 7. Other tags are ignored.
- Picks the highest N > `BuildInfo.Number` among non-draft releases that have an asset ending in `.apk`.
- Found: button top-left "Update available: bN" (touch-only). Not found, offline or any error: nothing shown, no message.

## Install (tap)
1. Android 8+: if the app may not install packages, open the "install unknown apps" settings for the app and show
   "Allow installs, then tap again".
2. Download the APK to the provider's path, label "Downloading N %". Failure: "Download failed, tap to retry".
3. "Installing...", open the system installer through the FileProvider URI. Failure: "Install failed, tap to retry".
- Android only installs it if it is signed with the same key and has a higher versionCode (= `BuildInfo.Number`).
- Verified b8 -> b9 on a phone (CHANGELOG).
