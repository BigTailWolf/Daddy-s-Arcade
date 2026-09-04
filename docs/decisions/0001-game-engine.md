# ADR 0001: Game engine

- Status: Proposed
- Date: 2026-09-04
- Decision owners: project owner and maintainer

## Context

Daddy's Arcade needs an unusually specific combination:

- very fast 2D iteration on an Apple Silicon Mac;
- straightforward keyboard and Xbox-controller testing on macOS;
- clean, automated tests for platform-independent rules;
- a possible hobby deployment to a retail Xbox Series X/S in Developer Mode;
- a credible later path to ID@Xbox and Xbox Store distribution;
- a codebase that remains easy for a person and an AI coding agent to change.

The first game by itself does not require a large engine. The Xbox path is the
main reason the engine choice matters.

## Options considered

### Unity 6 LTS with C#

Strengths:

- mature macOS editor and 2D workflow;
- C# core rules can be kept outside scene behavior and tested quickly;
- input system can map keyboard and controller devices to shared actions;
- current UWP build documentation provides a candidate route for retail Xbox
  Developer Mode testing;
- Unity has an official commercial Xbox/GDK path for authorized developers.

Costs and risks:

- heavier editor and project structure than this first game strictly needs;
- formal Xbox support requires platform approval and currently Unity Pro;
- UWP-to-retail-Xbox compatibility must be validated with the actual toolchain;
- generated files and scene assets need discipline to keep changes reviewable.

### Godot 4 with GDScript or C#

Strengths:

- free and open source;
- small installation and quick 2D iteration;
- approachable scene system and good fit for a family-scale desktop game;
- no commercial engine subscription for ordinary desktop development.

Costs and risks:

- Godot 4 removed its former UWP platform support;
- the upstream project does not integrate proprietary console SDKs;
- Xbox deployment commonly depends on an authorized third-party porting provider
  or a privately maintained platform port;
- the hobby retail-Xbox Developer Mode goal therefore has no equally direct,
  current upstream route.

## Proposed decision

Adopt **Unity 6 LTS with C#**, provided the project owner accepts the heavier
tooling and the possibility of Unity Pro costs if formal Xbox development begins.

Use pure C# assemblies for gameplay rules and keep Unity-specific code in input,
application-host, presentation, and platform adapter layers. Do not introduce
Xbox SDK dependencies during the Mac milestone.

## Validation gates

This decision is not accepted merely by adding Unity files. Before treating Xbox
as a supported target:

1. Confirm the exact Unity LTS patch version used on both computers.
2. Verify keyboard and Xbox-controller input on the Mac.
3. Build a minimal UWP target on Windows.
4. Deploy it to the actual Xbox Series X/S in Developer Mode.
5. Record the working process before adding build/deploy scripts.

If the retail-console UWP spike fails for reasons that cannot reasonably be
resolved, revisit whether the family-TV target should use another delivery
method while retaining Unity for the formal Xbox path.

## Consequences

- The repository remains engine-neutral until this ADR is accepted.
- After acceptance, the editor version will be pinned and the repository layout
  will follow Unity conventions without weakening the documented layer
  boundaries.
- UWP hobby deployment and GDK commercial distribution remain separate tracks.
- A future change of engine would require a new ADR and migration plan.

## Acceptance

To accept this decision, change `Status: Proposed` to `Status: Accepted` in the
same commit that pins the initial Unity version.

