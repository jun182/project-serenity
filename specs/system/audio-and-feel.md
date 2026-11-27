# Audio and feel

Code: `Assets/MusicManagerController.cs`, `PlayerController.cs`, `Haptics.cs`, `CameraShake.cs`, `CameraZoom.cs`.

## Music (GameplayScene)
Three looping tracks (**Dark**, **Light**, **Broken** = a broken version of Light) start together and stay in sync.
- **Black side:** only **Dark**, at any health.
- **White side** (`Theme.LightBrokenMix`): Light and Broken crossfaded by health (0-100, same as the health bar):
  - health >= *musicBrokenStart* (60): only Light;
  - between *musicBrokenOnly* (30) and *musicBrokenStart*: both, **equal-power** (Light = cos(m * 90deg), Broken = sin(m * 90deg),
    m = 0 at 60 to 1 at 30; at 45 both are 0.71);
  - health <= *musicBrokenOnly*: only Broken. Thresholds the wrong way round (only >= start): hard switch at *musicBrokenOnly*.
  - When health changes (a hit, healing) the mix glides there (SmoothDamp 0.15 s, settles in about 0.5 s). Frozen while paused.
- **Side switch:** instant cut, with the "into serenity" (entering black) / "into noise" (leaving black) sound. Leaving the black
  side starts directly at the mix for the current health (no glide).
- **Game start:** the first frame picks the music for the side the player is on (no side-switch sound).
- Death: health <= 0, so the white side is only Broken; plus the death sound.
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
