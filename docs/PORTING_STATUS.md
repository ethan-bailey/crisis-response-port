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
| Decompile C# assemblies | Blocked in current environment | No ILSpy/monodis/.NET runtime available and package download is unavailable. |
| Export Unity assets/scenes | Not started | Requires an asset extraction tool such as AssetRipper or UnityPy and inspection of recovered output. |
| Reconstruct Unity project | Not started | Compiled player archive lacks original `Assets/` and `ProjectSettings/` project structure. |
| Produce WebGL build | Not started | Requires a compatible Unity Editor and reconstructed project; no verified build currently exists. |
| iPhone Safari testing | Not started | Requires a WebGL build and actual browser/device test pass. |
| Behavioral parity tests | Not started | Must be based on observed original-game behavior and linked to specific systems. |

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

**Preferred:** reconstruct a Unity project around recovered original assets and decompiled C# behavior, then build for WebGL with the closest practical Unity version and adapt only platform-specific code. The exact Unity version and project-reconstruction feasibility must be tested before committing to this route.

**Fallback:** if a complete Unity/WebGL reconstruction proves infeasible, evaluate a minimal compatibility implementation subsystem by subsystem, explicitly marking reconstructed behavior and gaps. Do not replace the whole game with an invented HTML game and call it a port.

**Rust:** defer unless profiling or a specific platform dependency demonstrates a reason to introduce a Rust/WASM component. A wholesale rewrite would increase parity risk.

## Asset/scene reconnaissance

The Unity serialized files identify themselves as Unity `2017.2.0f3`. A preliminary printable-string scan of the scene files surfaced UI/object labels such as `Disclaimer text`, `Loading text`, `Player open door`, `Scene loader`, `Rooms checked`, and `Resource counters`. These are clues only: the scene files have not yet been parsed into a reliable hierarchy, and labels are not sufficient to reconstruct scenes.

The current official [AssetRipper documentation](https://assetripper.github.io/AssetRipper/docs/index.html) says it supports Unity versions from 3.5.0 through 6000.4.x, with version-dependent support quality. Unity asset export is therefore a reasonable next tool-assisted step once a runnable build of AssetRipper is available.

## iPhone web-runtime feasibility note

The current Unity 6.0 system requirements list iOS Safari 15+ as a supported browser target ([Unity 6.0 system requirements](https://docs.unity3d.com/6000.0/Documentation/Manual/system-requirements.html)). That establishes a possible modern Web runtime target, **not** a guarantee that this recovered Unity 2017 game will compile or perform acceptably on iPhone. The old project must be reconstructed and migrated as needed, then tested for startup memory, frame time, touch controls, audio, and browser lifecycle behavior.

## Immediate blockers

- The original C# source and editable Unity project are absent from the archive.
- The current workspace lacks a C# decompiler/.NET runtime and a Unity Editor.
- A faithful WebGL build cannot be honestly claimed until project reconstruction, compilation, runtime testing, and subsystem comparisons succeed.
