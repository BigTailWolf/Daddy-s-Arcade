# Product brief

## Purpose

Daddy's Arcade is a personal, family-first collection of small games that a
parent and child can enjoy together on a television. It aims to preserve the
approachability of classic arcade play while using original names, assets,
audio, interface design, tuning, and content.

This is not initially a commercial product. The architecture should preserve a
credible path to formal distribution without making the first family build pay
the cost of a production publishing program.

## Audience and setting

- Primary player: a child learning game controls and rules.
- Secondary player: a parent who also enjoys a more traditional challenge.
- Primary setting: Xbox Series X/S, television, and Xbox controller.
- Primary development setting: macOS, with an Xbox controller when useful.
- Packaging setting: a Windows x64 machine when Xbox tooling requires it.

## First game: Dad's Blocks

Dad's Blocks is an original falling-block puzzle game built from common gameplay
ideas. Its first version is deliberately small and uses geometric placeholder
visuals.

The minimum playable loop is:

1. Spawn a piece at the top of the board.
2. Let the player move and rotate it while gravity moves it downward.
3. Lock it when it can no longer descend.
4. Remove completed horizontal lines and award points.
5. Continue until a new piece cannot enter the board.
6. Allow the player to restart.

The first milestone also includes pause, keyboard input, Xbox controller input,
and automated tests for the core rules.

## Daddy's Arcade Home Menu

The first playable application must establish Daddy's Arcade as a collection,
even though only Dad's Blocks is implemented initially.

On launch, the player sees a controller-first Home Menu containing:

- Daddy's Arcade identity and a clear game-selection area;
- one enabled Dad's Blocks tile or card;
- room in the visual system for future game tiles without showing misleading
  playable content;
- clear focus, selection, and launch feedback;
- a Settings entry only if it provides real first-release settings;
- a safe way to quit on desktop where appropriate.

Launching Dad's Blocks happens inside the same application. The player can
return to the Home Menu without relaunching it. The menu should preserve a clean
boundary between the arcade shell and each game's own state and presentation.

## Experience principles

- **Readable from the sofa:** large shapes, high contrast, and safe TV margins.
- **Controller first:** the Home Menu and every game can be operated without a
  pointer or keyboard.
- **Kind by default:** mistakes should be understandable, feedback immediate,
  and restarts painless.
- **Depth without clutter:** advanced information belongs in Dad Mode rather
  than complicating the child's first experience.
- **Fast iteration:** a gameplay change should normally be testable on the Mac
  within seconds.

## Modes

Kid, Parent, Parent + Child, PK, and Whole Family are implemented in the local prototype.
The advanced features below remain future directions where noted.

### Kid Mode

- slower gravity;
- generous lock delay and input forgiveness;
- simple interface;
- strong visual and audio feedback;
- optional assistance that does not shame or punish the player.

### Dad Mode

- more traditional speed progression;
- score and level systems;
- next-piece preview (implemented for every mode);
- ghost piece;
- hold.

### Parent-child play

Local cooperative play uses independent Parent and Child boards. Parent clears
of 3 or 4 lines remove the Child board's bottom 1 or 2 rows only when its settled
stack is strictly taller than 8 rows. Controller 1 controls Parent; controller 2
controls Child. Either game over ends the round. See local-development.md for
precise input, assistance, and validation details.

### PK and Whole Family

PK sends 1 / 2 garbage rows to the opponent for 3 / 4-line clears, without
cancelling incoming rows. Whole Family has two competing Parents who also both
help one Kid; Kid is protected from attacks. See local-development.md for role
assignments, timing, and round-ending rules.

### Shared pause menu

Menu pauses the game for every player. Every game uses the
standard Resume, Main Menu (the current game's menu), and Exit to Arcade routes.
Games may append their own options; Dad's Blocks includes Restart Round.

## Out of scope for the first milestone

- additional games;
- elaborate launcher features such as profiles, downloads, online catalogs, or
  animated cabinet environments;
- online services, accounts, achievements, or leaderboards;
- final art, music, branding, and monetization;
- Xbox Store submission;
- production-grade Xbox build and deployment automation;

## Intellectual-property boundary

The project may implement unprotected gameplay concepts, but it must not copy a
third party's protected branding or creative expression. In particular, it will
not use the Tetris name or logos, reproduce official art or music, trace a
recognizable branded interface, copy proprietary levels or text, or market
itself as an official or compatible Tetris product.

Before any public or commercial release, names, store presentation, audiovisual
assets, and the complete player experience should receive a dedicated IP review.
