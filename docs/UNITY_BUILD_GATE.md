# Unity build gate and license-free port strategy

## Decision: the target browser port must not require a Unity license

The project will proceed on a license-free path that does **not** require Unity Editor, Unity Hub activation, or Unity license secrets. The primary approach is to execute recovered original C# gameplay code through a .NET WebAssembly runtime, provide browser-side compatibility implementations for the Unity APIs that code actually uses, and render/input through browser technologies.

This preserves original code wherever feasible without depending on a Unity Editor build. It is not a Unity WebGL player, and it is not yet a complete or playable game. The compatibility layer must be implemented and validated incrementally against the original project.

## Current Unity WebGL workflow status

The GitHub Actions workflow checks whether Unity license secrets exist. They are currently absent, so the Unity Editor build step is skipped. That skip is expected and does not block the license-free runtime experiments or static recovery/audit steps.

The Unity build route is optional only. Do not ask the user to create Unity account credentials, provide passwords, activate Unity on another machine, or configure license secrets as a prerequisite for this browser port. Do not bypass Unity licensing checks.

## Existing evidence for the alternative route

- The recovered project and decompiled C# source are available from the verified recovery artifact.
- Selected original source files, including `Target.cs` and ballistic-material logic, have compiled and executed in a browser WebAssembly test harness.
- A mesh extracted from the original Unity assets has rendered in a headless browser using WebGL.
- These are focused feasibility tests, not proof of a complete game runtime or behavioral parity.

## Remaining implementation and validation

The license-free path still needs the Unity scene/object lifecycle, serialized component and asset loading, materials/shaders, physics and projectile collision behavior, animation, touch controls, UI, audio, mission transitions, persistence, and iPhone Safari validation. Test shims used in the current proof of concept are not production implementations and must not be mistaken for parity.

A complete result must load and play the original game content in iPhone Safari without Unity license secrets. Record any missing or irrecoverable original assets explicitly rather than silently substituting new content.
