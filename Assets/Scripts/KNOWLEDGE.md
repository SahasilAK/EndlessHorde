# Scripts/

Gameplay behaviours for player control, zombie combat, waves, scoring, health, and pickups.

## Files
### `Bullet.cs`
`Bullet` launches upgradeable Rigidbody2D projectiles with damage, piercing, boss bonus damage, and kill healing.

### `GameManager.cs`
`GameManager` owns menu/pause/restart flow, wave-clear skill choice UI, score and PlayerPrefs high score, HUD updates, audio playback, and player-death handling.
- Depends on: `Health`, `PlayerController`, TextMesh Pro, Unity UI.

### `Health.cs`
`Health` tracks current/max health and raises change/death events; supports damage, healing, and runtime max-health configuration.

### `Pickup.cs`
`Pickup` applies health or timed rapid-fire effects and loads the matching retro sprite when collected by the player.
- Depends on: `Health`, `PlayerController`, `Resources/RetroPixel`.

### `PlayerController.cs`
`PlayerController` handles keyboard movement, mouse aiming, upgradeable weapon modes and stats, regeneration, camera shake, and cooldown-protected contact damage.
- Depends on: `Bullet`, `GameManager`, Unity Input System.

### `WaveSpawner.cs`
`WaveSpawner` displays and scales waves, spawns a boss every fifth wave, and grants a full heal plus three skill choices after clearing a wave.
- Depends on: `ZombieAI`.

### `ZombieAI.cs`
`ZombieAI` configures zombie variants and scaled bosses, pursues and damages the player, displays a health bar, flashes on hits, awards score, plays audio, and drops pickups.
- Depends on: `Health`, `GameManager`, `Pickup`.
