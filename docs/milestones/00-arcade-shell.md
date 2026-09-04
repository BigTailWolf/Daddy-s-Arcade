# Foundation: Daddy's Arcade shell and Home Menu

## Outcome

Daddy's Arcade launches as a collection with a real Home Menu. Dad's Blocks is
the only playable title at first, but it is entered through the same selection
flow future games will use.

This is a product foundation included in the first playable milestone, not a
separate promise to implement multiple games.

## First-release Home Menu

The Home Menu should contain only elements that work:

- Daddy's Arcade title/identity;
- a focused game-selection region;
- one enabled Dad's Blocks card;
- controller and keyboard navigation hints appropriate to the active device;
- visible focus and activation feedback;
- desktop quit behavior where the platform expects it.

Future games may be suggested through restrained visual space or an unlabeled
layout continuation, but the menu must not present fake playable cards or dates.

## Navigation model

- Application launch always enters Home.
- Directional actions change focus.
- Confirm launches the focused available game.
- Back from a game opens a confirmation or returns Home according to the final
  UX decision; accidental progress loss should be avoided.
- Back at the Home root does not silently discard state or quit on consoles.
- Mouse support is welcome on Mac but is not required for complete navigation.

## Game registration boundary

The shell needs a small, explicit registry for available games. Each entry may
provide:

- a stable internal identifier;
- display name;
- menu visual or placeholder theme;
- availability state;
- launch target/factory.

The registry must remain local and simple. Do not build a dynamic plugin loader,
remote catalog, download manager, account system, or content service.

## Lifecycle boundary

The shell owns navigation; the game owns gameplay state.

For the first release, returning Home may end the current Dad's Blocks session,
but the player should receive protection from accidental loss when appropriate.
Starting Dad's Blocks again creates a clean session. Save/resume is a later
feature unless explicitly promoted.

## Acceptance criteria

- The application opens at Home rather than directly in Dad's Blocks.
- Keyboard and Xbox controller can focus and activate Dad's Blocks.
- Focus is always visible and cannot move to a nonexistent item.
- Dad's Blocks launches without opening a second application.
- The player can return Home and launch a clean new session.
- Menu and game use the same abstract input-action layer.
- A future second game can be registered without changing Dad's Blocks code.
- Automated or engine-level tests cover the critical Home/game transitions.
- The menu is legible at a television viewing distance and respects safe areas.

## Deferred shell features

- multiple implemented games;
- profiles and parental accounts;
- cloud saves and Xbox identity;
- achievements and leaderboards;
- online catalogs, downloads, or updates;
- elaborate 3D arcade-room presentation;
- final artwork, animation, music, and sound design.

