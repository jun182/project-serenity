# Audio and feel

Code: `MusicManagerController.cs`, `PlayerController.cs`, `Haptics.cs`, `CameraShake.cs`, `CameraZoom.cs`.

## Music (GameplayScene)
Three looping tracks start together; only one is unmuted at a time.
- Starts on **Dark**.
- Entering the black side: **Dark** + "into serenity" sound.
- Leaving it: **Light** + "into noise" sound; after *maxTimeInLight* (0 s) on the white side **Broken** replaces it.
  With the default 0 this happens on the first frame (open question 1).
- Sounds: hit (alternates two clips), serenity blocked, death.

## Haptics (Android only; amplitude waveforms from API 26, plain pattern below)
| Event | Pattern |
|-------|---------|
| Hit | 90 ms, full strength |
| Near miss | 35 ms at *nearMissBuzz* (160/255); 0 = off; at most one per 0.12 s |
| Healing heartbeat | double beat 30 ms + 22 ms, soft, every 0.9 s while healing, not blocked and alive |
| UI tap (start screen) | 30 ms |
| Pad selection move | 12 ms, very light |
Not confirmed on a device (open question 6).

## Camera (GameplayScene, all off when Screen shake is OFF or paused)
- **Hit:** strong shake (trauma +0.7). **Near miss:** small shake (+0.3) plus a quick sideways slide of *nearMissSlide* (0.35) toward
  where the player moves (away from the obstacle if standing still). Shake size scales with *shakeStrength* (1); trauma decays 1.6/s.
- **Breathing while healing:** slow roll over two beats and a zoom pulse of *breatheZoom* (1.5 %) peaking on each heartbeat; fades in/out in 0.8 s.
- **Tilt-steering lean:** in tilt mode the screen rolls with the sideways speed, up to *gyroTilt* (3 deg), hard cap 8. Leans with movement to the right. Not tested on a device.
- Base view: 1.25x zoom (`CameraZoom`), applied before shake so shake offsets the zoomed view.
