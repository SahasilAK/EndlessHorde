# Scripts/

Gameplay behaviours for player control, zombie combat, waves, scoring, health, and pickups.

## Files
### `Bullet.cs`
`Bullet` launches a Rigidbody2D projectile and damages zombie health on trigger contact.

### `GameManager.cs`
`GameManager` owns menu/pause/restart flow, score and PlayerPrefs high score, HUD updates, audio playback, and player-death handling.
- Depends on: `Health`, `PlayerController`, TextMesh Pro, Unity UI.

### `Health.cs`
`Health` tracks current/max health and raises change/death events; supports damage, healing, and runtime max-health configuration.

### `Pickup.cs`
`Pickup` applies health or timed rapid-fire effects when collected by the player.
- Depends on: `Health`, `PlayerController`.

### `PlayerController.cs`
`PlayerController` handles keyboard movement, mouse aiming/shooting, rapid fire, and camera shake.
- Depends on: `Bullet`, `GameManager`, Unity Input System.

### `WaveSpawner.cs`
`WaveSpawner` displays wave numbers, spawns increasing groups outside the camera, and selects zombie variants.
- Depends on: `ZombieAI`.

### `ZombieAI.cs`
`ZombieAI` configures zombie variants, pursues and damages the player, flashes on hits, awards score, plays audio, and drops pickups.
- Depends on: `Health`, `GameManager`, `Pickup`.
