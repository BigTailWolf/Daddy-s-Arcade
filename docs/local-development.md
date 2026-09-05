# Local prototype

Editor version: 6000.6.0f1 (Apple Silicon). This is the installed supported
release, not the originally proposed LTS selection.

Add the repository root as a project in Unity Hub and open it. The editor setup
creates Assets/Arcade/Home.unity on first import. Open that scene and press Play.
The runtime bootstrap creates the Home Menu; Enter or the Play button enters
Dad's Blocks. Escape requests a return Home and Enter confirms.

Keyboard: arrows move/drop/rotate, Space rotates, P pauses, R restarts.

Run automated rule checks through Daddy's Arcade > Run Core Checks. Exceptions
indicate failure; a Console success message indicates completion.

Current validation status: source whitespace checked only. The automation host
cannot initialize Unity's UDS shared memory, so editor compilation, rule checks,
visual verification, and real controller testing are still pending. Controller
buttons are preliminary; directional controller mapping is not implemented yet.
The image in README is a design concept, not a screenshot of this prototype.

Current rules: 10 x 20 board, seven geometric pieces, uniform random selection,
700 ms gravity, immediate locking on a blocked descent, 100 points per cleared
line, clockwise rotation without wall kicks. Future kid-friendly lock delay
and richer controls remain planned.
