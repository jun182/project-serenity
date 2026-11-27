# Gameplay

Code: `PlayerController.cs`, `GameDirector.cs`, `SpawnerScript.cs`, `ObstacleController.cs`, `SerenityBlocker*.cs`, `HideObjectsPerSide.cs`.
Values marked *tunable* come from `GameSettings.Get` (default unless developer mode is on, see `dev-mode.md`).

## World
- Scenes: `MainMenu` (start screen) and `GameplayScene`. The player moves only along x; obstacles and blockers come toward the player along -z.
- **White side** (noise): x <= 5. Obstacles fall here.
- **Black side** (Serenity): x > 5. The player heals here. The boundary is the player's x, not a trigger.
- Player x is kept inside -4 .. 14: past either edge the target speed is forced back inward at top speed.
- Objects on each side fade between light (0.9) and dark (0.1) depending on which side the player is on (`HideObjectsPerSide`).

## Movement
- Target speed = input (-1..1) x top speed 10 x *steerSpeed* (1). Actual speed eases toward it with rate 5 x *steerAccel* (1) per second.
- Input sources are described in `controls.md`. Movement does not run while paused.

## Health
- Starts at 100. Health bar at the bottom shows health / 100.
- **Hit:** touching an obstacle (tag `Obstacle`) costs *damage* (10). No damage when invincible (dev mode) or after a win.
- **Heal:** on the black side and not blocked, +*healSpeed* (2) per second, up to 100.
- **Serenity blocker** (tag `SerenityBlocker`): while touching one or more, the player is pushed left at top speed and does not heal.
  Plays the "serenity blocked" sound on contact.

## Death
- Health <= 0 (checked on both sides): player visual hidden, death sound, screen effect `_DeathFloat` ramps from -1 to 1.1 at
  speed 0.66; when it passes 1 the game loads `MainMenu`.
- Known issue on `main`: steering and healing still run after death (see open question 2).

## Obstacles
- Spawned at the spawner's y/z, x uniform in -5 .. *spawnMaxX* (5). Base interval 0.15 s divided by the rate multiplier.
  Several can spawn in one frame at high rates.
- Move at 20 x *obstacleSpeed* (1) units/s toward -z, destroyed at z <= -30.
- **Near miss:** an obstacle that passes the player's z without hitting, within *nearMissDist* (1.5) in x, while the player is alive
  and on the white side: small camera shake + slide, near-miss vibration (see `audio-and-feel.md`). Counted in `GameDirector.NearMisses`.
- Spawning stops when the player is dead or has won.

## Serenity blockers
- One spawn point (scene). At most 8 alive; next one 3-6 s after a free slot. Length (z scale) random 10-30. Speed 10 units/s, removed at z <= -50.

## Modes (`GameSettings.GameMode`, changed on the start screen only)
- **Progressive** (default): difficulty level rises continuously from 0 to 5, one level per *levelSeconds* (30 s).
  Rate multiplier per level (interpolated): 0.4, 0.65, 1 (= original rate), 1.4, 1.85, 2.3; then x *rateMult* (1).
  At level 5 a win countdown of *winSeconds* (30 s) starts; surviving it = win.
- **Endless:** fixed rate = *rateMult* x Endless obstacle slider (1, range 0.5-3). No win.

## HUD (GameplayScene)
- Survival timer `m:ss` at the top centre, drawn twice (black on the white half, white on the black half). Stops when dead.
  During the win countdown a second line "survive N".
- **Win:** big "you wonned" text in the centre; 4 s later (real time) the game loads `MainMenu`.
- Health bar: full-width strip at the bottom, 2.5 % of screen height, black background, green fill. Does not block touches.
- Camera is zoomed in 1.25x (vertical FOV) for phone screens.
