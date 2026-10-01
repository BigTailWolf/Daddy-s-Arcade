# Local development

Open the repository root in Unity 6000.6.0f1. Input System 1.20.0 is installed
through Package Manager; Active Input Handling uses the new Input System.
After updating an already open editor, restart Unity to activate the backend.
Choose Daddy's Arcade > Open Home and press Play.

## Controller controls

- D-pad or left stick left/right: move; holding repeats after 250 ms.
- A: clockwise rotation. B: counterclockwise rotation during play.
- Hold down: soft drop, one step every 50 ms. Kid lock grace still applies.
- Y: hard drop and lock immediately, including Kid mode.
- Menu: pause. Any active player's Menu pauses every board.
- Menus: D-pad/stick to select, A confirms, B goes back/resumes.

Keyboard fallback: Parent/single player uses arrows, X or Enter (clockwise),
Z (counterclockwise), Space (hard drop), P or Escape (pause).
Player 2 uses WASD, E (clockwise), Q (counterclockwise), F (hard drop).
Player 3 uses IJKL, O (clockwise), U (counterclockwise), H (hard drop).
Keyboard and controller input can coexist for local testing.

## Modes and family help

Dad's Blocks Main Menu offers five modes:

| Mode | Controllers | Interaction |
| --- | --- | --- |
| Kid | 1: Kid | Gentle solo play |
| Parent | 1: Parent | Faster solo play |
| Parent + Child | 1: Parent, 2: Kid | Parent helps Kid |
| PK | 1 and 2: Parents | Parents attack each other |
| Whole Family | 1 and 2: Parents, 3: Kid | Parents attack each other and both help Kid |

Each board has a NEXT panel showing its next piece in the starting orientation
and color. The preview advances when the current piece locks; Parent and Child
have independent previews. PK opponents use the same seeded piece sequence for
fairness, advancing independently; attack randomness does not alter their pieces.
Controller assignments remain fixed
through a round. Connect controllers before starting when possible. Disconnecting
an assigned controller pauses play; reconnect it to resume, or restart to assign
currently connected devices. Losing window focus also pauses play.

Family mode has independent boards and scores. Parent uses 700 ms gravity;
Child uses 1400 ms gravity and an 800 ms grounded adjustment period.
When Parent locks a piece and clears exactly 3 or 4 lines at once, Child's
bottom 1 or 2 rows are removed, respectively, only if Child's settled stack is
strictly taller than 8 rows at that instant. Height is measured from the highest
occupied settled cell to the floor, including gaps; the falling piece is excluded.
Rows above collapse downward. Assistance earns no child score and does not chain
into another assistance event. Either player's game over ends the shared round.

In PK, clearing 3 / 4 lines queues 1 / 2 garbage rows for the opponent, with no
height threshold. Each gray row has one gap. Rows in one attack share a gap;
different attacks choose seeded random gaps. Pending attacks are shown beside
the board and rise after the recipient locks their current piece and clears
their own lines, before spawning the next piece. Clearing still awards points
and removes the player's own full lines, but never cancels pending attacks.
Overflowing the top or failing to spawn ends the round; the other Parent wins.

Whole Family combines that PK rule with the existing help rule: either Parent's
3 / 4-line clear also helps Kid if Kid is above 8 rows. Kid neither sends nor
receives attacks. A Parent losing ends the round with the other Parent winning;
Kid topping out ends the shared family round. Restart resets all boards and queues.

All gameplay pause menus use ArcadePauseMenu with Resume, Main Menu (Dad's
Blocks mode selection), and Exit to Arcade (the arcade launcher). Game options
append to these standard entries; Dad's Blocks adds Restart Round. Exiting to
menus or restarting discards the current round. All boards freeze while paused.

## Background music

Two original synthesized 16-bar loops play automatically: Welcome Home (96 BPM)
in the arcade/game menus and Side by Side (112 BPM) during a round. They use soft
melody, chords, bass and light percussion, with crossfades between contexts.
Pause/game over lowers the volume; losing window focus fades music to silence.
Menu > Music: On/Off toggles music and saves the preference across launches.
Music stays continuous across restarts and needs no downloaded audio files.
Unity's built-in Audio module is required and included in Packages/manifest.json.

## Automated checks

Daddy's Arcade > Run Core Checks runs deterministic game rules and pause routing
checks. Batch mode additionally runs virtual Input System gamepads to check button
mapping, held inputs, and player isolation. Example:

    Unity.exe -batchmode -nographics -projectPath <project> -executeMethod CoreChecks.RunBatch -logFile <log>

Expected success messages: "Daddy's Arcade virtual controller checks passed."
and "Daddy's Arcade core checks passed." Real three-controller comfort, physical
reconnection, and console deployment still require hardware testing.

Validated on Windows / Unity 6000.6.0f1: project compilation, deterministic rules,
pause routing, five-mode attack/help rules, and virtual three-controller checks
passed (exit code 0). Physical three-controller play remains unverified.
