# System specs: how the game works today

**Baseline:** `main` @ 7807e69, build b13 (2026-11-27). Written from the code, the GameplayScene and the prefabs.
`android-port` had newer, unmerged work at that time (start screen Play button in touch mode and no ripples, cached PlayerPrefs reads,
dead player frozen, per-renderer material destroyed). Update these files when that work reaches `main`.

These files describe behaviour that exists. A change to behaviour updates them in the same branch (see `../README.md`, step 8).

| File | Covers |
|------|--------|
| [gameplay.md](gameplay.md) | sides, health, obstacles, blockers, modes, difficulty, win/death |
| [controls.md](controls.md) | touch, tilt, gamepad, keyboard; input-mode switching |
| [menus.md](menus.md) | start screen, shared menu, pages, pad navigation, HUD |
| [ui-style.md](ui-style.md) | the one look for buttons, sliders, controller icons (colours, outline, states) |
| [dev-mode.md](dev-mode.md) | unlock, debug panel, debug page, tunables, UI tweak panel |
| [settings.md](settings.md) | every PlayerPrefs key, default, where it is set |
| [audio-and-feel.md](audio-and-feel.md) | music tracks, sounds, haptics, camera shake/breathing/tilt |
| [updater.md](updater.md) | in-app update check, download and install |
| [build-test-release.md](build-test-release.md) | build settings, headless build, test tools, release steps |

## Open questions (found while writing the baseline)
Not fixed; each needs a decision and, if it changes behaviour, a feature spec.

1. ~~**Music on the white side.**~~ Done in feature 006: the white side crossfades Light -> Broken by health (`audio-and-feel.md`);
   `maxTimeInLight` removed.
2. **Dead player still steers and heals** on `main` (gh #3). Fixed on `android-port`, not merged.
3. **PlayerPrefs read every frame** during play (tilt on/off, screen shake, Invincible; gh #1). Cached on `android-port`, not merged.
4. **Blocker spawner values are swapped** in the scene (`minTimeBetweenSpawns` 6, `maxTimeBetweenSpawns` 3). Works (Unity's `Random.Range` accepts either order) but reads wrong.
5. **Legacy scene objects/scripts:** `HealthUI` (never called), `ExitButton` (empty), `MainMenuController` (scene Play/Exit, hidden at runtime),
   the old scene `DebugPanel` with `DebugPanelController` (sliders write unused PlayerController fields). Removal candidates.
6. **Haptics** code comment says "untested on the phone" while the changelog lists it as a feature. Has it been felt on a device?
7. **Win text** is "you wonned". Intentional?
8. **Updater source** is hard-coded to `jun182/project-serenity` releases. Right repo long-term?
9. **Endless slider** ("Obstacles (Endless mode)") also shows in Progressive mode, where it does nothing.
10. **Help text** says "hold the left or right half", but in tilt mode touch steering is off entirely; the text mentions tilt only in brackets.
