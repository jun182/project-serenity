# 004 Tasks

- [x] Branch `feature/button-style` from `main`
- [x] `UiStyle.cs` (+ `.meta`): colours, generated sliced sprites, `Button`, `Select`, `Slider`, `SelectSlider`, width helpers
- [x] `PauseMenu.cs`: `MakeButton` / `MakeSlider` use `UiStyle`; remove "Button style" item, `CycleStyle`, `styleLabel`; width helpers
- [x] `PadIcons.cs`: `Paint`, `SelectFx` (buttons and sliders), `Disc`, `Pill` use `UiStyle`
- [x] `StartScreen.cs`: `Brush.Register` -> `UiStyle.RegisterWidth`
- [x] `GameSettings.cs`: `UiTunables` keeps `uiPulse`, `uiBtnScale`
- [x] Delete `Brush.cs` (+ `.meta`); grep for leftover `Brush.`
- [x] `tools/menu-check.sh`: one DPAD_DOWN fewer to reach "Build bN"
- [x] `specs/system/ui-style.md` (new), `README.md`, `menus.md`, `dev-mode.md`, `settings.md`; constitution rule 7
- [x] Compile check: editor is open (batch mode locked out), so Unity 2022.2.8f1 Roslyn with the Bee `Assembly-CSharp.rsp`, with and without `UNITY_ANDROID`: 0 errors
- [x] Editor checks (visible play mode, temporary screenshot script, deleted)
- [x] Phone: signed APK, Pixel 8 Pro over wireless adb, screenshots with real taps + adb keys, `menu-check.sh` all ok (`ui-check.sh` is outdated, not used)
- [x] Fix found on the phone: `PadIcons.SelectFx.OnDisable` resets a button hidden while selected; rebuilt, retested
- [x] Verify each acceptance criterion, fill in the Result column in `spec.md`
- [x] `CHANGELOG.md`, `HANDOFF.md`
- [ ] Status `done`, merge into `main` (on the developer's OK)
