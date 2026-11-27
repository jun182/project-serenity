# Constitution

Rules every spec, plan and change must respect. Changing one of these needs its own spec, approved by the project owner.

1. **The in-app updater keeps working.** Release tags are `alpha-b<N>`, N = `BuildInfo.Number`, which is also the Android
   versionCode. Every release bumps it. Only tag what is meant for players: installed copies pick up the highest tag. See `system/updater.md`.
2. **App identity and signing never change.** App id `se.vibing.serenity`, signed with the release key. The keystore and its password
   live outside the repo and are never committed. A different key or id means no installed copy can update.
3. **Debug stays hidden from players.** Debug and developer features only work after developer mode is unlocked (session-only).
   New debug UI goes behind `GameSettings.DevMode` / `PauseMenu.DebugOn`; new tunables go in `GameSettings.Tunables`, and while
   locked they behave as their defaults. See `system/dev-mode.md`.
4. **Releases on the owner's OK only.** Merging to `main` is fine when work is ready; creating a release tag needs the project owner.
5. **Honest testing.** Every acceptance criterion is reported as pass, fail or not tested. Gyro/tilt, real gamepads, haptics and
   sound need a real device; don't report them as tested otherwise.
6. **Every input works.** Menus and gameplay must work with touch, a gamepad (A/B/Start, stick/d-pad) and a keyboard. Pad focus never
   lands on touch-only buttons. One menu page is visible at a time; B goes back exactly one level.
7. **The look.** Black and white: the white side (noise, obstacles) and the black side (Serenity, healing). New UI follows it and the UI style in `system/ui-style.md`
   (white buttons, black outline, black text; one style for every button).
8. **Specs first, every time.** Every change, including trivial fixes and docs edits, has a folder in `features/` with an approved
   spec before work starts, and `system/` is updated before the branch is merged.
