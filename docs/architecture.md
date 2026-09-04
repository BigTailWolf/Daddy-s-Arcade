# Architecture

## Goals

The architecture exists to protect two things:

1. a fast local development loop on macOS; and
2. the ability to add Windows and Xbox platform integration without rewriting
   the game rules.

It should stay simple enough for a small family project and clear enough for an
AI coding agent to navigate safely.

## Dependency direction

```text
Device input                 Engine clock / view
     |                              |
     v                              v
Input adapter -> UI/game actions -> Arcade shell / game session
                                      |
                                      v
                             Pure gameplay rules
                                      |
                                      v
                               State snapshots
                                      |
                                      v
                              Presentation adapter

Xbox services -> Xbox platform adapter (optional, later)
```

Dependencies point inward toward gameplay rules. Core rules must not import
Unity scene classes, macOS APIs, Windows APIs, or Xbox APIs.

## Proposed layers

### Core rules

Owns the board, pieces, collision, legal movement, rotation, gravity steps,
locking, line clearing, score events, game-over conditions, and deterministic
random-piece input.

Core behavior should be expressible as commands applied to state. Randomness and
time are supplied from outside so tests can reproduce failures exactly.

### Application/session

Coordinates a play session, translates elapsed time into rule steps, handles
pause and restart, and emits a view-friendly snapshot or events. It should not
know which physical device produced an action.

### Arcade shell

Owns application startup, the Home Menu, game registration, selection, and the
transition into and out of a game. The shell knows how to launch a registered
game, but it does not contain Dad's Blocks rules.

The first registry contains only Dad's Blocks. A small interface or data record
for title, identifier, menu art/placeholder, availability, and launch target is
enough; a plugin system, downloadable catalog, or generalized content framework
would be premature.

Each game owns its session state. Returning Home must dispose or suspend that
session according to an explicit policy, and launching a new session must not
leak state from a previous run.

### Input

Maps platform input to a small vocabulary such as:

```text
MoveLeft
MoveRight
SoftDrop
HardDrop        # later unless accepted into the first milestone
RotateClockwise
RotateCounterClockwise
Pause
Restart
Confirm
Back
OpenMenu
```

Keyboard, an Xbox controller connected to the Mac, and an Xbox controller on the
console must converge on these same actions. Device-specific button labels stay
in the input and UI layers.

### Presentation

Draws board cells, the active piece, score, pause state, and game-over state. The
first version uses simple geometry and colors. Presentation consumes state; it
does not decide whether a move or rotation is legal.

### Platform adapters

Any future Xbox lifecycle, storage, identity, achievements, or deployment
integration stays outside the core. Platform capabilities should be optional so
the game remains runnable in the Mac editor.

## Proposed repository shape

The exact engine-generated directories will be added only after the engine
decision is accepted. The intended logical shape is:

```text
docs/
  decisions/
  milestones/
  architecture.md
  product.md
  xbox-development.md
game/
  shell/
  core/
  application/
  input/
  presentation/
  platform/
tests/
  core/
scripts/
  dev
  test
  build-xbox      # only after the real toolchain is validated
  deploy-xbox     # only after the real toolchain is validated
```

With Unity, engine-required names and layout may differ, but the same boundaries
should remain visible in assemblies/namespaces and tests.

## Testing strategy

The core test suite should cover at least:

- board bounds and occupied-cell collision;
- valid and invalid translation;
- valid and invalid rotation near walls and settled blocks;
- gravity progression;
- lock timing semantics once chosen;
- one-line and multi-line clears;
- scoring for the first ruleset;
- spawn collision and game over;
- pause preventing game progression;
- deterministic piece sequences;
- restart returning to a clean initial state.

Input mapping gets focused adapter tests where practical. A small number of
engine-level play tests should confirm that scenes, rendering, and input are
wired correctly. Most rule failures should be diagnosable without opening the
editor.

Shell tests should verify that the application starts at Home, only available
games can launch, Dad's Blocks can return Home, focus remains valid after a
transition, and a new game session starts with clean state.

## Early rules decisions still required

- board dimensions;
- exact piece set and rotation definitions;
- whether wall kicks are in milestone one;
- gravity interval and lock-delay behavior;
- first scoring table;
- whether hard drop and counter-clockwise rotation are milestone-one actions;
- deterministic randomizer choice.

These should be recorded as explicit decisions rather than inherited silently
from any commercial falling-block game.
