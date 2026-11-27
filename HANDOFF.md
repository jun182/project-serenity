# Board project-serenity (2026-11-27)
Resume: print open issues. NEXT (b12): read MENU-TREE.md (design for the Y700 controller findings: overlap, no Back, screen sleep, start badge, adb key-event debugging), confirm the 2 open questions with the user, implement, build, run menu-check, commit, push; release only on OK.

## Open
- **b11 released** (alpha-b11, jun182/project-serenity, pushed android-port 282a523): shared menu (PauseMenu.cs BuildMenu: pages main/settings/controls/debug; start screen has a Menu button top-right), game speed slider (debug), controller nav (stick/A/B/Start, SelectFx highlight when pad used). Built, 0 compile errors. ui-check NOT run, menu NOT looked at on any screen.
- Nord 2T has b10 (adb): open the start screen, expect the in-app updater to offer b11; this is the live updater test (do not adb install there).
- Y700 (adb wireless): b11 installed fresh (old com.DefaultCompany.ProjectSerenity removed). Controller prompts/menu navigation to be tested there.
- Y700 controller test by user (b11): works partly; findings in MENU-TREE.md. Pixel still not adb-visible (USB id 18d1:4ee1 = MTP only) at last check; b11 NOT installed there.
- Pixel 9 Pro XL: user asked for wireless adb + fresh b11. Not done: over USB it shows only as MTP (no adb device, no mdns service). Needs USB debugging authorised / Wireless debugging on the same Wi-Fi.
- GitHub issues enabled; #1-#3 assigned to Copilot (check for PRs, review before merge), #4 #5 for us. ui-check.sh needs a 2400x1080 device (Nord 2T); not run for b11 because the Nord was left for the user's updater/controller test. Was: can we use GitHub issues + PRs to let GitHub work issues (how software-humans site was built)? Not decided; discuss, nothing posted.
- ISSUES.md stale (#3,#4,#8 resolved in code; #5 controller, #7 fps open). REVIEW.md not written. Old scene DebugPanel = removal candidate. Stale line in ~/april-claudia/running.md.
- Gotcha: new script file => first batch build may fail CS0246; rebuild once and grep "error CS" ~/unity-tools/build.log.

## Build/test
Build: see git history of this file (BuildAndroid.Build, log ~/unity-tools/build.log, apk Builds/project-serenity-release.apk). `OUT=<dir> tools/ui-check.sh [apk]`. Never push upstream.
