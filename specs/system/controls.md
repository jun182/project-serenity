# Controls

Code: `PlayerController.cs` (steering), `GyroSettings` in `PauseMenu.cs`, `PadIcons.AnyPadInput`, `StartScreen.cs`, `PauseMenu.Update`.

## Steering in game
All sources are added, then clamped to -1..1:
- **Keyboard / pad / d-pad:** Unity axis `Horizontal` (raw): A/D, arrow keys, left stick, d-pad (Android sends d-pad as arrow keys).
- **Touch** (only when tilt steering is off): each finger held on the right half = +1, left half = -1. Touches on UI (e.g. the II button) are ignored.
- **Tilt** (when Tilt steering is ON): `Input.acceleration.x` (left/right roll). Dead zone 0.06 (~3.5 deg), full speed at 0.40 (~23 deg),
  linear in between. *Invert tilt* flips the sign. Neutral is flat/level; there is no recentre. Touch steering is disabled in tilt mode.
- Not tested on a real phone: tilt direction and strength.

## Menu and system keys
| Action | Pad | Keyboard | Touch |
|--------|-----|----------|-------|
| Move selection | stick / d-pad | arrows | - |
| Select | A (JoystickButton0) | Enter | tap |
| Back one level | B (JoystickButton1) | Esc / Android Back | Back button on the page |
| Open/close menu | Start (JoystickButton10 on Android, 7 elsewhere) | - | Menu (start screen) / II (game) |
| Pause in game | Start or B | Esc / Android Back | II |
| Quit on start screen | - | Esc / Android Back | Exit (top-left) |

B on the start screen (menu closed) does nothing.

## Input mode (pad vs touch)
- The UI follows the last input: any pad or keyboard navigation input (axes > 0.5, arrows, Enter, any joystick button; not Esc) switches
  to **pad mode**; a touch or mouse click switches to **touch mode**.
- Pad mode: selected item highlighted (white with black text, slight pulse, light select vibration), START badge shown next to the
  touch-only Menu / II button.
- Touch mode: no highlight, no badge.
- On the start screen, pad mode starts on if a joystick is connected at load. The input that switches to pad mode only wakes it and does nothing else.
