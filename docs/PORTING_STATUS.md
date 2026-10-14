# Porting status and parity gates

## Current verified status

| Work item | Status | Evidence / remaining work |
|---|---|---|
| Locate original archive | Complete | `Crisis Response.rar` was found in the user's ChatGPT Library. |
| Extract original files | Complete | 415 regular files / 204,344,165 bytes extracted without reported errors. |
| Record original hashes | Complete locally | Archive SHA-256 and per-file SHA-256 manifest generated; binary assets are not committed to Git. |
| Confirm engine and player | Complete | Unity 2017.2.0f3; Windows x86 Mono player. |
| Inspect managed assembly metadata | Complete | 1,534 types; 8,640 methods; 9,025 fields; 1,112 properties in `Assembly-CSharp.dll`. |
| Map gameplay symbols | Initial pass complete | Type and method names are present in metadata/string heaps. This is not source decompilation. |
| Decompile C# assemblies | Complete in cloud runner | ILSpy produced 1,333 C# files from the gameplay assembly whose SHA-256 matches the user's original archive. Decompiled source is included in the recovery artifact. |
| Export Unity assets/scenes | Complete | AssetRipper CLI exported 7,231 files (about 169 MiB) including six scenes, scripts, assets, and project settings. |
| Reconstruct Unity project | Initial recovery complete | AssetRipper reconstructed an editable Unity project with `Assets/`, `ProjectSettings/`, `Packages/manifest.json`, and scenes. It may need repairs and is not yet verified to compile. |
| Static WebGL compatibility audit | Complete (static scan only) | `docs/WEBGL_COMPATIBILITY_AUDIT_2026-10-14.md` records likely blockers: threaded `System.Net.WebRequest` calls, Windows Registry access, desktop file-browser/filesystem assumptions, and touch input work. These are candidates to test, not confirmed compile errors. |
| Produce Unity WebGL build | Optional / skipped | The Unity Editor build requires valid Unity activation, so this route is not a prerequisite. The selected primary route is a license-free browser runtime using recovered original C# through .NET WebAssembly plus a Unity API compatibility layer. See `docs/UNITY_BUILD_GATE.md`. |
| iPhone Safari testing | Not started | Requires a WebGL build and actual browser/device test pass. |
| Behavioral parity tests | Not started | Must be based on observed original-game behavior and linked to specific systems. |
| Missing .308 casing mesh GUID | Investigated; unresolved | Full recovery-artifact scan found six serialized references but no `.meta` owner for GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`. No candidate was substituted. See `docs/MISSING_CASING_MESH_GUID_SEARCH_2026-10-14.md`; CI now generates a repeatable search report. |

## Subsystem parity matrix

No subsystem is marked as implemented or parity-verified merely because its symbols exist in the assembly.

| Subsystem | Evidence found in assembly | Parity evidence required |
|---|---|---|
| Character locomotion, animation, ragdolls | `CharacterBody` (311 methods), `Limb`, `IKArmController` | Matched scenarios, movement/animation state transitions, collision and ragdoll outcomes |
| AI and team behavior | `CharacterBehavior`, `TeamOperator`, `TeamController` (`CommandMoveTo`, `CommandMoveOut`, `CommandHoldPosition`, `IssueRulesOfEngagement`), `Blackboard` | Decision traces for visibility, threat response, commands, cover, restraint, and team coordination |
| Weapons, aiming, reloads, malfunctions | `WeaponBase` (141 methods), `Firearm`, `WeaponAimer`, `CyclingBolt` | Weapon configuration, input sequences, ammo transitions, recoil, malfunction and reload outcomes |
| Ballistics and damage | `Projectile` (126 methods), `BallisticMaterial` (`HandleProjectile`, `EnergyToPenetrateSurface`, `DeflectProjectile`), `ProjectileLimbEvent` | Reproducible shots across materials, penetration/deflection, damage and hit-location results |
| Doors and traps | `DoorController` (`SetOpen`, `SetClosed`, `ForceOpen`, `ForceClose`), `DoorTrap`, `DoorExplosive` | Lock/open/breach/trap scenarios and corresponding AI path/decision behavior |
| Grenades and explosions | `Grenade` (`PullPin`, `StartFuse`, `Throw`, `Detonate`), `FragmentationGrenade` | Fuse timing, blast effects, stun/fragmentation outcomes, door/room interactions |
| Physiology, wounds, bleeding | `Physiology` (119 methods), `LimbPhysiology` (`CreateWound`, `UpdateHemorrhaging`, `InflictVitalTrauma`), `VitalFunctions`, `Wound` | Injury progression, blood loss, consciousness, treatment, vital state transitions |
| Missions, campaigns, scoring, saves | `Mission` (128 methods), `CampaignManager` (`ProgressCampaign`, `StartMission`, `ContinueCampaign`, `SetMissionCompleted`), `ScoreKeeper` (92 methods) | Mission completion/failure, score calculations, persistence and load/resume |
| UI, menus, input/settings | `MainMenu`, `PauseMenu`, `InputHandler` (`StartBinding`, `UpdateBinding`, `SetNewKeyCode`, `SetNewKeyAxis`), `InputMapping` | Screen flows, bindings, menu behavior, settings persistence and touch mapping |
| Audio, particles, decals, rendering | `BloodStream`, Unity audio/particle/render references | Asset mapping, effect timing, visual output and mobile performance |

## Port strategy gate

**Updated 2026-10-14:** Unity WebGL compilation is no longer the sole path. See [Browser port strategy pivot](BROWSER_PORT_STRATEGY_PIVOT_2026-10-14.md).

**Primary route (license-free):** a browser runtime using recovered original C# compiled to .NET WebAssembly, with a compatibility layer for Unity APIs and a browser renderer/input host. This route requires no Unity license key or Unity account credentials. The compatibility layer is not yet complete.

**Secondary route:** retain Unity WebGL as an optional route only if a valid license is already available. Do not make activation or Unity secrets a prerequisite.

**Not selected as primary:** browser-hosted WINE/PROTON for the original Windows player. Its Unity 2017/Mono/3D graphics and iPhone Safari compatibility is unverified and its Windows API dependencies make it a poor first path.

Do not replace the whole game with an invented HTML game and call it a port. Defer a wholesale Rust rewrite unless a concrete subsystem justifies it.

## WebAssembly replacement work — 2026-10-14

The source-preserving engine replacement is the explicit goal; see [the WebAssembly engine replacement plan](WEBASSEMBLY_ENGINE_REPLACEMENT_PLAN_2026-10-14.md), the [runtime architecture decision](WEBASSEMBLY_RUNTIME_ARCHITECTURE_DECISION_2026-10-14.md), and [issue #4](https://github.com/ethan-bailey/crisis-response-port/issues/4).

The cloud audit at [run 38587729815](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38587729815) scanned 1,420 C# files and recorded 67 recognized Unity API tokens with 9,625 lexical occurrences. These are source inventories, not compiler errors.

The corrected scene-reference audit reached 417 readable serialized assets and 708 dependency edges. It now reports one unresolved serialized asset GUID, `b4b69dc6c74eeb54cbd22d28e37f9a19`, referenced by .308 casing/projectile prefabs. See [run 38589347410](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38589347410). No replacement asset has been silently substituted.

A meaningful browser geometry gate passed in [run 38589625850](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38589625850): the original combined mesh was decoded from Unity's serialized vertex/index buffers (14,187 source vertices, 9,455 triangles, 274 submeshes) and rendered in headless Chromium through WebGL. The browser reported 274 mesh groups, 9,455 triangles, a 1280×800 canvas, and zero page/console errors. This validates only one static original mesh; it does not execute Unity gameplay and is not yet a WebAssembly game engine. See [the browser geometry/WASM status report](WASM_RUNTIME_SPIKE_2026-10-14.md).

The first runtime gate passed in [run 38590382729](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38590382729): original `Target.cs` compiled to .NET WebAssembly and its `Awake`/`GetComponent` behavior passed in Chromium. The expanded gate adds original `BallisticMaterialProperties.cs`; see [the Unity API compatibility matrix](UNITY_API_COMPATIBILITY_MATRIX_WASM_2026-10-14.md) for verified coverage and remaining services. The expanded original-source math/ballistics feasibility gate passed at [run 38593586937](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38593586937). It compiles selected original C# files and checks selected behaviors in a test harness, including vector/quaternion/transform math and ballistic-material branches. The harness still substitutes test fixtures for Unity physics, projectile energy, effects, timing, and other engine services; it is not a playable slice. The hierarchy-aware scene/geometry preflight also passed at [run 38594581681](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38594581681). Full scene loading, Unity lifecycle, materials/shaders, physics, animation, audio, UI, input, serialization, and game-specific systems remain to be implemented and validated. Unity's own WebGL build remains skipped, by design, because the primary route must not require Unity license secrets.

## Asset/scene reconnaissance

The Unity serialized files identify themselves as Unity `2017.2.0f3`. A preliminary printable-string scan of the scene files surfaced UI/object labels such as `Disclaimer text`, `Loading text`, `Player open door`, `Scene loader`, `Rooms checked`, and `Resource counters`. These are clues only: the scene files have not yet been parsed into a reliable hierarchy, and labels are not sufficient to reconstruct scenes.

The current official [AssetRipper documentation](https://assetripper.github.io/AssetRipper/docs/index.html) says it supports Unity versions from 3.5.0 through 6000.4.x, with version-dependent support quality. Unity asset export is therefore a reasonable next tool-assisted step once a runnable build of AssetRipper is available.

## iPhone web-runtime feasibility note

The current Unity 6.0 system requirements list iOS Safari 15+ as a supported browser target ([Unity 6.0 system requirements](https://docs.unity3d.com/6000.0/Documentation/Manual/system-requirements.html)). That establishes a possible modern Web runtime target, **not** a guarantee that this recovered Unity 2017 game will compile or perform acceptably on iPhone. The old project must be reconstructed and migrated as needed, then tested for startup memory, frame time, touch controls, audio, and browser lifecycle behavior.

## Immediate blockers

- The original C# source and editable Unity project are absent from the archive; ILSpy and AssetRipper have recovered decompiled C# and a reconstructed Unity project in a cloud runner.
- The license-free runtime still needs a production-grade Unity API compatibility layer; the current WASM harness is only a narrow test fixture. Unity Editor activation is not a prerequisite for this route.
- A faithful WebGL build cannot be honestly claimed until the recovered project compiles, runs in Safari, and passes subsystem comparisons.
