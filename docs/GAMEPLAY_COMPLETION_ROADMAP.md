# Gameplay Completion Roadmap

## Authority and working line

AGENT_INSTRUCTIONS.md, AGENTS.md, and the latest NEXT_AGENT.md govern execution. Use develop/wasm as the only development line. Prefer small, verified fast-forward commits directly on that branch; use one short-lived PR only when review or isolation materially helps, with at most one open at a time. Keep GitHub Actions enabled as documented by AGENT_INSTRUCTIONS.md. Do not save or deploy a Site version until every required local gate passes on the exact final source SHA and the owner's publication instructions authorize it.

Older PR #42, Codespaces-only, checkpoint, and Actions workflow directions in historical notes are superseded. The Work Mode checkout is the normal environment. Use an existing Codespace only when a required gate cannot run in the current workspace; preserve and verify its state if recovery is needed.

## Completion standard

Parity means source-backed behavior in all six original scenes, not code presence, inventories, compilation, or synthetic tests alone. Keep rules derived from recovered original C# and Unity assets. Capture native reference behavior and compare source and port traces with justified numeric tolerances. At major playable milestones and before release, require exact-SHA local build, HTTP smoke, source/Node checks, package integrity, and owner manual iPhone 16 Safari checks for full-mission play, performance, audio, and background-resume. Desktop Chromium and WebKit gameplay automation is optional diagnostic coverage; the actual iPhone Safari result is required for the supported target.

## Ordered gates

1. Close original scene and asset dependencies. Resolve prefab overrides, GUID/fileID/local references, built-ins, Resources, materials, shaders, colliders, and scene transitions for all six scenes. Locate the exact original .308 casing mesh GUID b4b69dc6c74eeb54cbd22d28e37f9a19 / fileID 4300000 and required built-ins from original evidence; do not substitute an unverified asset. Capture native scene-flow behavior.
2. Complete Unity compatibility and source compile: lifecycle, identity/destruction, prefab remapping, transforms, time/coroutines/events, physics/collision order, animation, audio, UI, resources, and scene management. Compile the required original C# graph with pinned .NET SDK 8.0.426. Distinguish the selected runtime graph from the entire recovered source tree.
3. Use original actors end to end. Make Crisis.Runtime.FullSource.CharacterBody the sole actor owner. Load authored Operator.prefab and Occupant.prefab, preserve original lifecycle and all 19 limbs, and remove competing transport ownership such as WeaponHost. Prove two independent actors through spawn, equipment, movement, hit, injury, death/ragdoll, and destruction with real Mission and global dependencies. Synthetic prefab-only tests do not pass.
4. Route controls through the original actor: keyboard and touch movement, proportional joystick walk and rim-triggered sprint, aim, hold-to-fire, reload, stance, pause, cancellation, and lifecycle reset.
5. Run original IK and physical constraints. Execute IKArmController, IKArmSequencer, ArmController, and ArmIKSolver on source-owned limbs. Complete collision, joints, muscle/limb constraints, ragdoll, recovery, and self-hit filtering.
6. Complete original weapon and combat paths: WeaponBase, Firearm, WeaponAimer, CyclingBolt, magazine/chamber/reload, recoil, grenades, Projectile, BallisticMaterial, ProjectileLimbEvent, penetration/deflection, callbacks, effects, and weapon drops.
7. Complete physiology: wound placement, bleeding, circulation, oxygen, pain, shock, consciousness, treatment, death, and UI/AI effects through LimbPhysiology, Physiology, VitalFunctions, and Wound.
8. Complete mission and progression. Connect CharacterBehavior, TeamOperator, TeamController, Blackboard, Mission, CampaignManager, and ScoreKeeper for navigation, cover, commands, doors, traps, objectives, scoring, win/loss, restart, transitions, and progression.
9. Close presentation and platform parity: original UI/settings, audio/mixer/spatial behavior, particles, shaders/materials, mirror/glass, camera/light/render order, saves/load/resume, browser storage, and mobile lifecycle. Inventories and dummy shader exports are not implementation.
10. Run original-vs-port trace comparison across every scene/system with justified tolerances, then complete the exact-SHA source/package and owner iPhone Safari gates above. Desktop browser automation is optional.

## Current evidence (2026-10-23)

At develop/wasm SHA a75983409c8aeeab76f53afeab5c0e7a41c428c8, dotnet build runtime/Crisis.Runtime.csproj --no-incremental passed with 0 errors and 45 warnings under SDK 8.0.426. dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -c Release passed its emitted contracts, but explicitly reports fullGameParity=false, fullUnityParity=false, and two unresolved built-ins. This establishes selected-graph compilation and contract behavior only.

The available curated root source-reference/Original yielded 4 assets, 3 dependency edges, and 123 unresolved GUIDs in scripts/scene_dependency_audit.py; it is not the full six-scene source project. The casing mesh GUID remains unresolved in available original evidence. A valid complete scene package was not found for the scene-flow runner. Rendered Marksman gameplay and in-scene FIRE behavior, and real Operator/Occupant lifecycle with Mission/global dependencies, remain unverified.

Resume from the earliest unfinished gate using the current handoff. Record evidence and blockers once per bounded batch; never mark a gate complete based on compilation or inventory alone.
