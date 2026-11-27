# Board project-serenity (2026-11-27)
Resume: print open issues (gh -R jun182/project-serenity). NEXT: user tests b12 with the real pad on the Y700; release alpha-b12 only on OK.

## Open
- **b12 built, NOT released** (apk Builds/project-serenity-b12.apk; waiting for the user's OK to release alpha-b12). Implemented per MENU-TREE.md: one page visible at a time (start-screen buttons, Menu/II hidden while a menu is open), B = back one level, Start = open/close menu (also on start screen; it no longer starts the game), START badge under Menu/II in pad mode, black/white pad icons (A disc, selection = white), smaller Play/Exit, NeverSleep while playing, `MENU page=.. sel=..` and `MENU key=JoystickButtonN` log lines, `tools/menu-check.sh` (adb key events): 18/18 ok on the Y700.
- Measured: Unity on Android reports A=JoystickButton0, B=1, **Start=10** (not 7: that was why Start never worked in b11). Not verified with the real pad, only adb key events. Sliders: left/right works by Unity default, "A = reset" NOT done (Default button is focusable instead).
- Idea from user (not decided): live-tweakable UI/design variables in the debug menu so UI changes need no rebuild.
- Nord 2T has b10 (adb): open the start screen, expect the in-app updater to offer b11; this is the live updater test (do not adb install there).
- Y700 (adb wireless): b11 installed fresh (old com.DefaultCompany.ProjectSerenity removed). Controller prompts/menu navigation to be tested there.
- Y700 controller test by user (b11): works partly; findings in MENU-TREE.md. Pixel still not adb-visible (USB id 18d1:4ee1 = MTP only) at last check; b11 NOT installed there.
- Pixel 9 Pro XL: user asked for wireless adb + fresh b11. Not done: over USB it shows only as MTP (no adb device, no mdns service). Needs USB debugging authorised / Wireless debugging on the same Wi-Fi.
- GitHub issues enabled; #1-#3 assigned to Copilot (check for PRs, review before merge), #4 #5 for us. ui-check.sh needs a 2400x1080 device (Nord 2T); not run for b11 because the Nord was left for the user's updater/controller test. Was: can we use GitHub issues + PRs to let GitHub work issues (how software-humans site was built)? Not decided; discuss, nothing posted.
- ISSUES.md stale (#3,#4,#8 resolved in code; #5 controller, #7 fps open). REVIEW.md not written. Old scene DebugPanel = removal candidate. Stale line in ~/april-claudia/running.md.
- Gotcha: new script file => first batch build may fail CS0246; rebuild once and grep "error CS" ~/unity-tools/build.log.

## Build/test
Build: see git history of this file (BuildAndroid.Build, log ~/unity-tools/build.log, apk Builds/project-serenity-release.apk). `OUT=<dir> tools/ui-check.sh [apk]`. Never push upstream.
