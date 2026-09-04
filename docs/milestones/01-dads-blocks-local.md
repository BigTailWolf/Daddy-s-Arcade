# Milestone 1: Dad's Blocks local playable

## Outcome

A complete minimal game can be launched from the Daddy's Arcade Home Menu and
played on macOS with either the keyboard or an Xbox controller. Its core rules
run under automated tests.

Xbox packaging and deployment are not part of this milestone.

## Acceptance criteria

### Gameplay

- The application starts at the Daddy's Arcade Home Menu.
- Selecting the Dad's Blocks tile starts a fresh game session.
- The player can return from Dad's Blocks to the Home Menu without relaunching
  the application.
- A visible board starts a new game with an active falling piece.
- Pieces descend automatically according to gravity.
- The player can move left and right and rotate.
- Pieces cannot cross board boundaries or occupied cells.
- A piece locks when the selected lock rule is satisfied.
- Full horizontal lines are removed and the board collapses correctly.
- The player receives a visible basic score.
- The game ends when a new piece cannot spawn.
- The player can restart after game over.
- The player can pause and resume.

### Input

- A documented keyboard layout controls the Home Menu and all game actions.
- An Xbox controller connected to the Mac controls the Home Menu and the same
  game actions.
- Gameplay consumes abstract actions, not raw keys or button numbers.
- Disconnecting or reconnecting a controller does not corrupt game state.

### Visuals and usability

- Placeholder geometry is sufficient; no final art is required.
- Board, active piece, settled cells, score, pause, and game-over states are
  visually distinguishable.
- The interface remains legible at a typical television aspect ratio.
- Restarting takes one clear action and does not require relaunching the game.

### Engineering

- Core rules do not depend on engine scene or platform APIs.
- Core tests cover collision, movement, rotation, locking, line clearing,
  scoring, game over, pause, deterministic piece input, and restart.
- The documented local development and test commands work on the Mac.
- The repository contains no unlicensed third-party game assets.
- Work is split into small, reviewable commits.

## Suggested implementation slices

1. Engine project, application shell, and automated-test harness.
2. Home Menu with one registered Dad's Blocks entry and scene transitions.
3. Pure board and piece model with collision tests.
4. Movement, rotation, gravity, and locking with tests.
5. Line clear, score, game over, and restart with tests.
6. Minimal game scene and geometric rendering.
7. Keyboard action mapping across menu and game.
8. Xbox controller mapping and on-Mac verification across menu and game.
9. Pause, return-Home behavior, UI states, and milestone acceptance pass.

## Exit evidence

- automated test output from the Mac;
- a locally playable build or editor-run procedure;
- a completed manual keyboard checklist;
- a completed manual Xbox-controller-on-Mac checklist;
- known issues recorded in the repository.
