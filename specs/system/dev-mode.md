# Developer mode and debug tools

Code: `GameSettings.cs`, `PauseMenu.cs`, `DebugPanelController.cs`, `CameraZoom.cs`. Constitution rule 3 applies.

## Unlock (session-only)
- `GameSettings.DevMode` is a static flag: off at every app start, never saved.
- **7 taps** on "Build bN" in Settings. The label shows "(2 taps left)" / "(1 tap left)" for the last two; then "Build bN (developer)".
  Changing page resets the tap count.
- **Konami code** on the start screen with the menu closed: up up down down left right left right B A (arrow keys / d-pad / stick, B/A keys
  or pad buttons). The final A does not also press Play.
- **Disable developer mode** on the dev page turns it off (and resets game speed to 1).

## While locked
Every tunable returns its default (except the player's Endless obstacle slider), Invincible and Zoom markers read as off,
the Debug panel, debug page, UI tweak panel and FPS overlay are hidden, whatever PlayerPrefs says.

## Debug panel (dev page toggle, pref `debugPanelVisible`)
"Debug ON" = dev mode AND this pref. Shows:
- top-right overlay: fps, build, music manager status, trigger count, hp, healing, x, hits, serenity enter/exit counts, theme,
  white-side music volumes (`mus L <light> B <broken>`),
  dead, error count, shake, mode, level, rate, spawned, near misses, last error message.
- the old scene DebugPanel (P toggles it in dev mode).
- the **Debug...** entry on the in-game main menu page.

## Debug page (in game)
Kill me (health 0, closes menu) · Invincible ON/OFF · Difficulty level +1 (Progressive only; jumps to the next level start) ·
Zoom test cubes (next run): cubes at the edges of the unzoomed view, invisible if the zoom works · Tuning sliders... ·
Game speed 0.5-5x step 0.5 (`Time.timeScale`; back to 1 on quit to menu, leaving the scene, or disabling dev mode).

## Tunables (`GameSettings.Tunables`, tuning page, stored as `t_<key>`)
| Key | Label | Default | Min | Max | Step |
|-----|-------|---------|-----|-----|------|
| levelSeconds | Seconds per level | 30 | 3 | 60 | 1 |
| winSeconds | Win countdown (s) | 30 | 3 | 60 | 1 |
| rateMult | Spawn rate x (all modes) | 1 | 0.25 | 3 | 0.05 |
| obstacleSpeed | Obstacle speed x | 1 | 0.5 | 2 | 0.05 |
| spawnMaxX | Spawn max x (border=5) | 5 | 2 | 5 | 0.1 |
| damage | Damage per hit | 10 | 0 | 50 | 1 |
| healSpeed | Heal per second | 2 | 0 | 20 | 0.5 |
| steerSpeed | Steer speed x | 1 | 0.5 | 2 | 0.05 |
| steerAccel | Steer response x | 1 | 0.5 | 3 | 0.1 |
| gyroTilt | Gyro screen tilt (max deg) | 3 | 0 | 8 | 0.5 |
| shakeStrength | Shake strength | 1 | 0 | 3 | 0.1 |
| breatheZoom | Breathing zoom % | 1.5 | 0 | 6 | 0.1 |
| musicBrokenStart | Broken starts (HP) | 60 | 0 | 100 | 1 |
| musicBrokenOnly | Only broken (HP) | 30 | 0 | 100 | 1 |
| nearMissDist | Near-miss distance | 1.5 | 0.5 | 4 | 0.1 |
| nearMissBuzz | Near-miss vibration (0-255) | 160 | 0 | 255 | 5 |
| nearMissSlide | Near-miss slide (minus = flip) | 0.35 | -1.5 | 1.5 | 0.05 |

"Reset all to defaults" deletes all `t_` keys of this table. Slider changes are saved when a sub-page or the menu closes.

## UI tweak panel (dev page toggle, pref `uiPanel`)
Shown to the right of the menu while it is open; changes apply live to the big buttons (`UiStyle.cs`, `PadIcons.cs`).
The four shape sliders (slant, roughness, dry-brush, stroke variant) went with the Paint / Rhombus shapes (feature 004).
| Key | Label | Default | Min | Max | Step |
|-----|-------|---------|-----|-----|------|
| uiPulse | Selection pulse % | 3 | 0 | 10 | 0.5 |
| uiBtnScale | Menu button width x | 1 | 0.6 | 1.4 | 0.05 |

## Adding a new debug feature
Put UI behind `GameSettings.DevMode` / `PauseMenu.DebugOn`; add numbers to `Tunables` (or `UiTunables`), never read PlayerPrefs directly.
