# EndlessHorde

EndlessHorde is a 2D top-down zombie survival game built with Unity 6.6. Fight
through increasingly difficult waves, collect upgrades, and survive as long as
you can.

## Play

Open the project with **Unity 6000.6.3f1**, open `Assets/Scenes/Game.unity`, and
press **Play**. Start a run from the main menu.

### Controls

| Input | Action |
| --- | --- |
| W, A, S, D | Move |
| Mouse | Aim |
| Left mouse button | Fire |
| Escape | Pause or resume |

## Gameplay

- The first wave has five zombies; each new wave adds one more and increases
  their speed.
- Fast and tough zombie variants appear alongside standard zombies.
- Every fifth wave includes a boss with health that scales with the wave.
- Clear a wave to fully heal and choose one of three randomly offered upgrades.
  Upgrades improve firing speed, damage, movement, health, weapon modes,
  piercing, regeneration, boss damage, healing on kills, damage protection, or
  pickup drops.
- Zombies can drop health and temporary rapid-fire pickups.
- Score and the high score are shown in game; the high score is saved locally
  with Unity PlayerPrefs.

## Project

- Main scene: `Assets/Scenes/Game.unity`
- Gameplay code: `Assets/Scripts/`
- Reusable gameplay prefabs: `Assets/Prefabs/`
- Original retro pixel art: `Assets/Art/RetroPixel/`
- Runtime pickup sprites: `Assets/Resources/RetroPixel/`
- Sound, music, and asset credits: `Assets/Audio/Credits.txt`

Unity packages are declared in `Packages/manifest.json`. The project uses the
Universal Render Pipeline, the Input System, TextMesh Pro, and Unity UI. See
`ROADMAP.md` for the original feature plan.
