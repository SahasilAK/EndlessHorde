# EndlessHorde/

## Project Overview
Unity 6.6 2D top-down zombie survival game. Gameplay is implemented in `Assets/Scripts/` and starts from `Assets/Scenes/Game.unity`; package dependencies and editor/player configuration live in `Packages/` and `ProjectSettings/`.
See `README.md` for player controls, gameplay features, and project setup.

## Subfolders
- `Assets/` — scenes, gameplay code, art, audio, and Unity assets. See [`Assets/KNOWLEDGE.md`](Assets/KNOWLEDGE.md).
- `Packages/` — Unity package manifest. See [`Packages/KNOWLEDGE.md`](Packages/KNOWLEDGE.md).
- `ProjectSettings/` — project, editor, and build configuration. See [`ProjectSettings/KNOWLEDGE.md`](ProjectSettings/KNOWLEDGE.md).

## Files
### `.gitattributes`
Git line-ending and text/binary handling rules for project files.

### `.gitignore`
Excludes Unity-generated caches, builds, editor files, and other local artifacts from Git.

### `.kbignore`
Excludes Unity-generated folders, IDE project files, metadata, and the package lockfile from knowledge-base scans.

### `README.md`
Player-facing project guide covering setup, controls, wave progression, upgrades, and asset locations.

### `ROADMAP.md`
Ordered feature roadmap and acceptance prompts for waves, scoring, variants, menus, art/audio, and the Windows build.
