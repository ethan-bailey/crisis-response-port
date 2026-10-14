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
| Character locomotion, animation, ragdolls | `CharacterBody`, `Limb`, `IKArmController` | Matched scenarios, movement/animation state transitions, collision and ragdoll outcomes |
| AI and team behavior | `CharacterBehavior`, `TeamOperator`, `TeamController`, `Blackboard` | Decision traces for visibility, threat response, commands, cover, restraint, and team coordination |
| Weapons, aiming, reloads, malfunctions | `WeaponBase`, `Firearm`, `WeaponAimer`, `CyclingBolt` | Weapon configuration, input sequences, ammo transitions, recoil, malfunction and reload outcomes |
| Ballistics and damage | `Projectile`, `BallisticMaterial`, `ProjectileLimbEvent` | Reproducible shots across materials, penetration/deflection, damage and hit-location results |
| Doors and traps | `DoorController`, `DoorTrap`, `DoorExplosive` | Lock/open/breach/trap scenarios and corresponding AI path/decision behavior |
| Grenades and explosions | `Grenade`, `FragmentationGrenade` | Fuse timing, blast effects, stun/fragmentation outcomes, door/room interactions |
| Physiology, wounds, bleeding | `Physiology`, `LimbPhysiology`, `VitalFunctions`, `Wound` | Injury progression, blood loss, consciousness, treatment, vital state transitions |
| Missions, campaigns, scoring, saves | `Mission`, `CampaignManager`, `ScoreKeeper` | Mission completion/failure, score calculations, persistence and load/resume |
| UI, menus, input/settings | `MainMenu`, `PauseMenu`, `InputHandler`, `InputMapping` | Screen flows, bindings, menu behavior, settings persistence and touch mapping |
| Audio, particles, decals, rendering | `BloodStream`, Unity audio/particle/render references | Asset mapping, effect timing, visual output and mobile performance |

## Port strategy gate

**Preferred:** reconstruct a Unity project around recovered original assets and decompiled C# behavior, then build for WebGL with the closest practical Unity version and adapt only platform-specific code. The exact Unity version and project-reconstruction feasibility must be tested before committing to this route.

**Fallback:** if a complete Unity/WebGL reconstruction proves infeasible, evaluate a minimal compatibility implementation subsystem by subsystem, explicitly marking reconstructed behavior and gaps. Do not replace the whole game with an invented HTML game and call it a port.

**Rust:** defer unless profiling or a specific platform dependency demonstrates a reason to introduce a Rust/WASM component. A wholesale rewrite would increase parity risk.

## Immediate blockers

- The original C# source and editable Unity project are absent from the archive.
- The current workspace lacks a C# decompiler/.NET runtime and a Unity Editor.
- A faithful WebGL build cannot be honestly claimed until project reconstruction, compilation, runtime testing, and subsystem comparisons succeed.
