# 006 White-side music follows health (Light -> Broken crossfade)

- **Status:** done (listening checks left to the developer)
- **Source:** request from NightAnimal333 (chat, 2026-11-27); answers `specs/system/README.md` open question 1
- **Approved:** 2026-11-27, NightAnimal333 (chat)
- **Branch:** `feature/health-music`

## Problem
The game has three looping music tracks that start together and stay in sync: **Light**, **Broken** (a broken version of
Light) and **Dark** (the black / serenity side). The intent is that the white side sounds clean when the player is healthy and
broken when they are hurt. Today health plays no part: on the white side the Light track is switched on and then replaced by
Broken once the player has been there for *maxTimeInLight* seconds. That tunable defaults to 0, so Broken takes over on the
first frame and players never hear the Light track.

## Goal
On the white side the music tells the player how hurt they are: Light at good health, a smooth crossfade into Broken as health
falls, only Broken when health is low. The black side keeps playing Dark.

## Scope
- **White side, by health** (health 0-100, same value as the health bar):
  - health >= *Broken starts* (default 60): only Light.
  - health between *Only broken* (default 30) and *Broken starts*: both tracks play, crossfaded **equal-power** (overall loudness
    stays even; e.g. at 45 both are at about 71%).
  - health <= *Only broken*: only Broken.
- **Glide:** when health jumps (a hit takes 10 at once, healing adds it back), the volumes ease to the new mix over about
  **0.5 s** instead of jumping.
- **Black side:** unchanged. Only Dark plays, whatever the health.
- **Side switch:** unchanged instant cut with the "into serenity" / "into noise" sounds. Leaving the black side jumps straight
  to the white-side mix for the current health (no glide on the cut itself).
- **Old timer removed:** the *maxTimeInLight* behaviour and its tunable ("Plain track time (s)") are removed.
- **Two new tunables** on the developer-mode Tunables page; while dev mode is locked they use their defaults:
  - "Broken starts (HP)": default 60, range 0-100, step 1.
  - "Only broken (HP)": default 30, range 0-100, step 1.
- **Developer overlay** (dev mode only): the existing top-right diagnostics line also shows the current Light / Broken volumes,
  so the mix can be checked without listening.

## Out of scope
- Crossfading between sides (Dark <-> white side mix).
- Any change to the Dark track, the sound effects (hit, blocked, death, into serenity / noise) or their volumes.
- New music or a master / music volume setting.
- The start screen (it has no gameplay music).

## Details
- **Scene:** GameplayScene only. **Inputs:** none; this follows health, so touch, tilt, pad and keyboard play the same.
- **Audience:** players (the music); developer mode (the two tunables and the overlay values).
- **Game start:** whichever side the player starts on, that side's music plays from the first frames (white side: the mix for
  the current health, which is Light at full health).
- **Paused:** the mix does not change while paused (game time is stopped); it continues on resume.
- **Death:** health is 0 or below, so the white-side mix is only Broken; the death sound plays as now.
- **Invincible (debug):** health never drops, so the white side stays on Light.
- **Win:** nothing special; the mix keeps following health.
- **Tunables set the wrong way round** (*Only broken* >= *Broken starts*): no blend. Only Light above *Only broken*, only
  Broken at or below it.
- **Persistence:** the two tunables are stored like the other tunables (PlayerPrefs, dev mode only). The old `maxTimeInLight`
  value is no longer read.
- **Constitution:** new tunables live in `GameSettings.Tunables` and the overlay addition is behind dev mode (rule on debug
  features). No conflict.

## Acceptance criteria
| ID | Criterion | Check | Result |
|----|-----------|-------|--------|
| AC1 | White side at full health: only Light is audible (Broken volume 0). | device (overlay values + listen) || pass (overlay): Pixel 8 Pro, HP 100/80/60 on the white side -> `mus L 1.00 B 0.00`. Listening: not tested, developer to listen. |
| AC2 | White side with health between 60 and 30: both play, equal-power (e.g. ~0.71 / ~0.71 at 45). | device overlay after hits + review of the formula || pass: HP 40 -> `L 0.50 B 0.86` = cos/sin(2/3 x 90deg), settled. 45 not reachable on the phone (hits came in steps of 20, see note). Formula reviewed. |
| AC3 | White side at health 30 or below: only Broken (Light volume 0). | device overlay + listen || pass (overlay): HP 20 and 0 -> `L 0.00 B 1.00`. Listening: not tested. |
| AC4 | After a hit the volumes glide to the new mix over about 0.5 s, no hard step. | device (listen) + review || pass (overlay + review): one frame caught mid-glide (`L 0.01` right after HP 40 -> 20); SmoothDamp 0.15 s. How it sounds: not tested, developer to listen. |
| AC5 | Black side: only Dark plays at any health; side switches still cut instantly with their sounds. | device (listen) || pass (overlay + review): start on the black side shows `Dark(in) L 0.00 B 0.00`, `serIn 0` (no switch sound at start); leaving it shows `White(out)` with the mix at once. Side-switch code path otherwise unchanged. Listening: not tested. |
| AC6 | "Plain track time (s)" is gone; "Broken starts (HP)" (60) and "Only broken (HP)" (30) are on the Tunables page and change the thresholds; with dev mode locked the defaults apply. | device / editor (dev mode) + review || pass: tuning page on the phone shows "Broken starts (HP): 60 (default)" and no "Plain track time"; the "Only broken (HP)" label sits under the debug overlay in the screenshot (overlap was already there before this change). Threshold effect and locked defaults: by review (both read via `GameSettings.Get`). Moving the sliders on the device: not tested. |
| AC7 | The overlay shows Light / Broken volumes only in developer mode. | device / screenshot || pass (review + menu-check): the line is part of the overlay text, which only shows with `DebugOn` (dev mode + Debug panel); menu-check confirms dev mode is off by default. |
| AC8 | Release build compiles with no `error CS`. | headless build log || pass: Roslyn compile (Android + Editor defines) 0 errors; headless signed release build `Build Finished, Result: Success`, 0 `error CS`. menu-check 27/27 ok (first run lost the first key press at startup; two reruns all ok). |

Device checks need a real phone and listening; I can build and review, the developer listens.

**Note (out of scope):** on the phone each obstacle hit counted twice (hits +2, HP -20 per hit with "Damage per hit" 10). Recorded in `specs/features/007-double-hit/`.

## Open questions
- none

## System specs affected
- `specs/system/audio-and-feel.md` (music section)
- `specs/system/dev-mode.md` (tunables table, overlay)
- `specs/system/README.md` (close open question 1)
