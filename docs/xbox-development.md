# Xbox development notes

## Status

Xbox deployment is **not yet validated** for this repository. This document
separates two different Microsoft paths so that a successful hobby experiment is
not mistaken for a production publishing pipeline.

## Path A: family testing on a retail Xbox in Developer Mode

Microsoft documents a UWP development path that can build and test games on a
retail Xbox switched into Developer Mode. Unity also continues to document UWP
build support.

This makes a Unity-to-UWP experiment the leading candidate for putting an early
family build on the existing console. It is still a compatibility hypothesis,
not a completed pipeline. It must be tested using the actual chosen Unity
version, Windows SDK, Visual Studio version, Xbox Series X/S console, Developer
Mode account, controller, and deployment method.

Important limitations:

- the build and Visual Studio packaging/deployment steps belong on Windows;
- UWP games are no longer accepted as new Xbox Store games;
- UWP has different runtime and resource constraints from the formal GDK path;
- success here does not prove readiness for Xbox certification;
- wireless deployment may work, but wired networking is the safer baseline for
  the first validation.

## Path B: formal Xbox release

A future Xbox Store game should be treated as a separate program:

1. Apply to ID@Xbox and submit a game concept.
2. Complete Microsoft's agreements and gain the relevant developer access.
3. Obtain the Xbox GDK/GDKx resources and appropriate development hardware or
   approved testing path.
4. Obtain the engine's Xbox platform support and required commercial license.
5. Integrate Xbox identity, lifecycle, storage, services, packaging, and
   certification requirements as needed.
6. Test, package, certify, and publish through the authorized Xbox workflow.

For Unity, current public guidance says Xbox console development requires Unity
Pro, registration as an Xbox developer, GDK for Xbox, and the Xbox platform
add-on. Some detailed materials are available only after authorization/NDA.

For Godot 4, UWP support was removed and the open-source engine does not ship
closed console SDK integration. Formal console ports therefore require an
authorized third-party provider or a maintained private port.

## Windows machine assumptions to verify

- x64 Windows 10 or Windows 11;
- enough free disk space for Visual Studio, Windows SDKs, Unity modules, and
  eventually GDK components;
- supported Visual Studio workload and toolchain;
- local-network reachability to the Xbox;
- Xbox Developer Mode activation and account access;
- exact Unity editor version installed on Mac and Windows;
- for formal development, Microsoft authorization and access to protected tools.

## First Xbox validation spike

Do this only after the Mac milestone is healthy, or earlier with a tiny sample if
Xbox-on-retail-console feasibility must be de-risked immediately.

1. Freeze a Unity editor version shared by Mac and Windows.
2. Produce the smallest UWP build with one rendered scene and controller input.
3. Generate and compile the Visual Studio solution on Windows.
4. Deploy manually to the retail Xbox in Developer Mode.
5. Record exact prerequisites, commands, settings, architecture, certificates,
   networking steps, and errors.
6. Confirm launch, rendering, focus/lifecycle, safe area, controller input,
   suspend/resume, and clean uninstall/redeploy.
7. Only then create repository automation for repeatable portions.

## Script policy

The repository may reserve these future entry points:

```text
scripts/dev
scripts/test
scripts/build-xbox
scripts/deploy-xbox
```

`scripts/build-xbox` and `scripts/deploy-xbox` must not pretend to work before
the Windows/Xbox flow has been performed manually and recorded. Until then, the
documentation is the source of truth and no fake success command should exist.

## Public references checked

- Microsoft UWP on Xbox FAQ:
  https://learn.microsoft.com/windows/uwp/xbox-apps/frequently-asked-questions
- Microsoft GDK setup overview:
  https://learn.microsoft.com/gaming/gdk/docs/gdk-dev/get-started/get-started-home
- Microsoft GDK development-PC requirements:
  https://learn.microsoft.com/gaming/gdk/docs/gdk-dev/get-started/overviews/set-up-dev-pc
- ID@Xbox onboarding:
  https://www.xbox.com/games/publish/
- Unity Xbox development overview:
  https://unity.com/solutions/xbox
- Unity UWP manual:
  https://docs.unity3d.com/6000.0/Documentation/Manual/WindowsStore.html
- Godot stable FAQ:
  https://docs.godotengine.org/en/stable/about/faq.html

These public pages are guidance, not a substitute for protected platform
documentation obtained after authorization. Recheck them when beginning the
Xbox spike because platform programs and engine requirements can change.

