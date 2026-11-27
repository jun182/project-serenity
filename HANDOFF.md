# Board project-serenity (2026-11-27)
Resume: print open issues, then check b11 on devices (see Open).

## Open
- **b11 released** (alpha-b11, jun182/project-serenity, pushed android-port 282a523): shared menu (PauseMenu.cs BuildMenu: pages main/settings/controls/debug; start screen has a Menu button top-right), game speed slider (debug), controller nav (stick/A/B/Start, SelectFx highlight when pad used). Built, 0 compile errors. ui-check NOT run, menu NOT looked at on any screen.
- Nord 2T has b10 (adb): open the start screen, expect the in-app updater to offer b11; this is the live updater test (do not adb install there).
- Y700 (adb wireless): b11 installed fresh (old com.DefaultCompany.ProjectSerenity removed). Controller prompts/menu navigation to be tested there.
- Pixel 9 Pro XL: user asked for wireless adb + fresh b11. Not done: over USB it shows only as MTP (no adb device, no mdns service). Needs USB debugging authorised / Wireless debugging on the same Wi-Fi.
- User asked: can we use GitHub issues + PRs to let GitHub work issues (how software-humans site was built)? Not decided; discuss, nothing posted.
- ISSUES.md stale (#3,#4,#8 resolved in code; #5 controller, #7 fps open). REVIEW.md not written. Old scene DebugPanel = removal candidate. Stale line in ~/april-claudia/running.md.
- Gotcha: new script file => first batch build may fail CS0246; rebuild once and grep "error CS" ~/unity-tools/build.log.

## Build/test
Build: see git history of this file (BuildAndroid.Build, log ~/unity-tools/build.log, apk Builds/project-serenity-release.apk). `OUT=<dir> tools/ui-check.sh [apk]`. Never push upstream.
