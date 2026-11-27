# 005 Start screen docs after the android-port merge

- **Status:** done
- **Source:** found while working on 004 (merging `origin/main` before the push), request from NightAnimal333 (chat, 2026-11-27)
- **Approved:** 2026-11-27, NightAnimal333 (chat: "merge, fix ui-style and menus, retest quickly, push")
- **Branch:** `fix/start-screen-docs`

## Problem
jun182 merged the android-port work into `main` (start screen: Play button in touch mode, no "Touch to start" text, no ripples,
Space starts on PC / Web). `specs/system/menus.md` still describes the old touch start screen, and 004's new `ui-style.md` names the
"Touch to start" text as an example. `controls.md` still says the pad selection is white with black text (missed in 004).

## Change
Docs only, no code:
- `specs/system/menus.md`: start screen touch mode = Play button + small Exit; Space starts on PC / Web.
- `specs/system/ui-style.md`: split-text example is the timer only.
- `specs/system/controls.md`: pad selection = black, white outline, white text (`ui-style.md`).
Out of scope: the rest of the android-port work in `specs/system/` (PlayerPrefs cache, dead-player freeze, material leak, README
baseline / open questions); stays an open item in `HANDOFF.md`.

## Acceptance criteria
| ID | Criterion | Check | Result |
|----|-----------|-------|--------|
| AC1 | `menus.md` describes the touch start screen as it is after the merge (Play button, Exit, no hint, no ripples, Space on PC / Web) | review (`StartScreen.cs`) + device (Pixel 8 Pro screenshot) | **pass** (code read; Pixel 8 Pro: Play + Exit + Menu, no hint). Space on PC: **not tested** (no PC build) |
| AC2 | No `specs/system/` file mentions "Touch to start" or ripples as current behaviour | review (grep) | **pass** (only as removed, and in the README baseline note) |
| AC3 | `controls.md` pad highlight matches `ui-style.md` | review | **pass** |
| AC4 | The merged build shows the touch-mode Play button in the 004 style and menu navigation still works | device (Pixel 8 Pro screenshot) + menu-check | **pass** (Pixel 8 Pro: touch Play white, pad Play black with A disc, back to white after a tap; menu-check all ok) |

## System specs affected
- `specs/system/menus.md`, `specs/system/ui-style.md`, `specs/system/controls.md`
