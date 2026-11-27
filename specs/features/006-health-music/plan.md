# 006 Plan

## Approach
The three music tracks already start together and loop in sync, so the crossfade is done with `AudioSource.volume`; no
audio mixer or new assets.

- **`MusicManagerController`** gets a white-side mix value `brokenMix` (0 = only Light, 1 = only Broken) with a target:
  - `PlayTheme(Theme.Dark)`: as now (only Dark unmuted).
  - `Theme.Light` is renamed `Theme.LightBrokenMix` (developer's request, 2026-11-27) and means "white side": Dark muted, Light and
    Broken both unmuted, and the mix **snaps** to the current target (side switch stays an instant cut).
  - `SetBrokenTarget(float t)`: sets the target. `Update()` eases the current value toward it with
    `Mathf.SmoothDamp(..., smoothTime 0.15 s)`, which settles in about 0.5 s. It uses `Time.deltaTime`, so it is frozen
    while paused (timeScale 0).
  - Volumes, equal-power: Light = cos(mix * pi/2), Broken = sin(mix * pi/2) (at mix 0.5 both are 0.707).
  - `Theme.Broken` is removed (nothing else uses it).
  - Public `LightVolume` / `BrokenVolume` for the overlay.
- **`PlayerController`**: on the white side, every frame computes
  `t = start > only ? InverseLerp(start, only, health) : (health <= only ? 1 : 0)` from the tunables `musicBrokenStart` (60)
  and `musicBrokenOnly` (30), and calls `SetBrokenTarget(t)`. The `timeInLight` / `maxTimeInLight` code and fields are
  removed. **Game start:** a "side not set yet" flag makes the first Update pick the theme for the starting side (no
  sound effect). Today, a player who starts on the white side keeps the Dark track until the first side switch. That counts
  as part of this change: spec Details "Game start".
- **`GameSettings.Tunables`**: remove `maxTimeInLight`; add `musicBrokenStart` "Broken starts (HP)" 60 (0-100, step 1) and
  `musicBrokenOnly` "Only broken (HP)" 30 (0-100, step 1).
- **`PauseMenu` overlay** (already shown only when `DebugOn`, which needs dev mode): append `mus L 0.71 B 0.71`.
  `DiagTheme` text "Light(out)" becomes "White(out)".

Rejected: an AudioMixer snapshot blend (needs scene/asset edits for the same result); a linear `MoveTowards` glide (a
10-HP hit would finish in under 0.2 s, not about 0.5 s).

## Files
- Changed: `Assets/MusicManagerController.cs`, `Assets/Scripts/PlayerController.cs`, `Assets/Scripts/GameSettings.cs`,
  `Assets/Scripts/PauseMenu.cs` (one overlay line).
- Docs: `specs/system/audio-and-feel.md`, `specs/system/dev-mode.md`, `specs/system/README.md`, `CHANGELOG.md`, `HANDOFF.md`.
- No scene or prefab edits. The serialized `maxTimeInLight` value in the scene becomes unused data (Unity ignores it).

## Constitution check
- Updater, app id, signing: not touched.
- Dev-mode hiding: the new tunables go through `GameSettings.Get` (defaults while locked); the overlay is already behind `DebugOn`.
- Input coverage: no input involved. Look: unchanged.

## Risks
- Unity starts every track at volume 1. Until the first Update after Start, the white-side tracks are muted, so there is no blip.
- An old PlayerPrefs `t_maxTimeInLight` value stays on devices, unused. Harmless.
- Can't listen headlessly, so the audible checks (AC1, AC3-AC5) need the developer on a phone.

## Build and test
- Headless release build (`specs/system/build-test-release.md`), `grep "error CS"` in the log (AC8).
- Code review of the formula and the threshold edge cases (AC2, AC4, AC6).
- Device (developer): install the APK, unlock dev mode, turn on the debug overlay, and read `mus L/B` at full health, after
  4-6 hits and after 7+ hits; listen; cross to the black side and back (AC1-AC5, AC7). Tunables page: check the two new
  sliders and that "Plain track time" is gone (AC6). If adb is available: `tools/ui-check.sh` screenshot of the overlay.
