# Settings and persistence

All persistent state is in Unity `PlayerPrefs` (survives restarts, cleared by uninstall / `pm clear`). Developer mode itself is not saved.

| Key | Type | Default | Set from | Meaning |
|-----|------|---------|----------|---------|
| gameMode | int | 0 | Settings (start screen) | 0 Progressive, 1 Endless |
| screenShake | int | 1 | Settings | camera shake, breathing, near-miss slide on/off |
| targetFps | int | 60 | Settings | 30/60/90/120; vSync off, applied at app start and on change |
| t_endlessDensity | float | 1 | Settings slider | Endless obstacle amount 0.5-3 (player setting, works without dev mode) |
| gyroEnabled | int | 0 | Controls | tilt steering |
| gyroInvert | int | 0 | Controls | invert tilt |
| debugPanelVisible | int | 0 | dev page / P key | Debug panel (only with dev mode) |
| uiPanel | int | 0 | dev page | UI tweak panel (only with dev mode) |
| invincible | int | 0 | debug page | only with dev mode |
| zoomMarkers | int | 0 | debug page | only with dev mode, applies next run |
| t_<tunable> | float | see `dev-mode.md` | tuning page / UI tweak panel | only read with dev mode |

Toggles save immediately. Sliders write on every change and save when a sub-page or the menu closes, or on quit to menu.
Tunable values are cached in memory after the first read.
No longer read (left on installed copies, harmless): `buttonStyle` and `t_uiSkew` / `t_uiRough` / `t_uiDry` / `t_uiSeed` (removed in 004).
