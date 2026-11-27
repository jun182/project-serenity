# 007 One obstacle hit counts twice

- **Status:** draft (bug recorded, not yet specified; to be picked up later)
- **Source:** found while verifying 006 on the Pixel 8 Pro (2026-11-27); developer asked to record it and fix it later
- **Approved:** -
- **Branch:** `fix/double-hit` (not created yet)

## Bug description
On the phone, every obstacle that hits the player is counted twice:
- the debug overlay's `hits` counter goes up by **2** per obstacle;
- health drops by **20** per obstacle, while the "Damage per hit" tunable is **10** (default; dev mode was on, tunables at default).

Observed with build b13 + feature 006 on a Pixel 8 Pro (wireless adb), Progressive mode, player standing still at x -2.5 on
the white side. Sequence on the overlay: HP 100 -> 80 -> 60 -> 40 -> 20 -> 0, `hits` 10 -> 12 -> 14 -> 16 -> 18 -> 20
(`hits` is static and carried 10 over from the previous run), `trig` also +2 per hit.

Effect for players: you take double damage, so a run on the white side lasts half as long as the tuning says (5 hits to die
instead of 10). It also makes the 006 music blend coarser (steps of 20 HP).

## What is known so far
- Damage is applied in `PlayerController.OnTriggerEnter` (tag `Obstacle`), once per call; `DiagTrig` and `DiagHits` both
  counted 2 per obstacle, so `OnTriggerEnter` itself runs twice.
- `Obstacle.prefab` and `Player.prefab` each have one trigger collider, so the cause is not obvious from the prefabs.
  Candidates (unchecked): a second collider on the player in `GameplayScene` (scene override / child object), an obstacle
  with two colliders at runtime, or two contacts per obstacle (e.g. re-entering after the near-miss slide / camera shake).
- Not known: whether the original Windows game (or the editor) does the same, and since which build.

## Open questions (to clarify before writing the real spec)
- Intended damage per obstacle: 10 (so 10 hits to die), or is the current feel (20) what the game has been balanced on?
- If the fix halves the damage, should *damage* default or the heal speed change to keep the difficulty?
- How to check it: overlay `hits` / `hp` on a phone, and in the editor.

## System specs affected
- `specs/system/gameplay.md` (damage per hit), possibly `dev-mode.md`.
