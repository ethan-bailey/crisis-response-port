# FullSource actor bridge dependency batch — 2026-10-25

## Landed slice

Added a guarded browser-host bridge around an already initialized original
player actor. `FullSourceActorBridge` requires the actor and original Global
Context/InputHandler to belong to the same `RuntimeWorld`, and requires exactly
one active `FullSource.PlayerController` bound to that `FullSource.CharacterBody`.
For a complete playable context, it maps browser action levels and pointer
coordinates into Unity Input, advances the original RuntimeWorld once, and
returns the actor's root state plus the authored local pose of every owned
transform. `RuntimeBridge` exposes initialize/tick entry points for that actor.

The bridge refuses to tick if the recovered Mission's InteriorMode UI or its
InteractionMenu is absent. This is a required original `PlayerController.Update`
dependency, not a transport fallback. The normal `game/index.html` page does
not yet initialize the FullSource mission/player world or call these endpoints;
its current movement/animation and transport WeaponHost remain authoritative.
No behavior was switched on the owner-accepted page.

## Verification

Against working-tree changes based on `develop/wasm` parent
`5752f38a9c09eb4d0a6c2fffbde8aa35c4773a95`:

- Release Browser/WASM project build: passed.
- EngineChecks: 69 assertions passed.
- FoundationChecks: 121 assertions passed.
- Original Operator/Occupant prefab lifecycle: passed; 134/132 active behaviours
  completed Awake/OnEnable/Start, and original two-arm Operator IK transition
  reached both authored Pelvis targets.
- Mission 01 headless source startup: passed; Global Context initialized,
  mission started, 2 Operators and 5 Occupants spawned. The bridge returned an
  actor-owned transform snapshot and refused to tick because this test
  intentionally deactivates/suppresses the original InteriorMode UI graph.
- Original M4A1/Glock ownership, IK holster/activate/fire callbacks, movement,
  posture, physiology, incapacitation/death timing, and PhysX command dispatch:
  passed.
- Existing full-runtime compiler warnings remain; the build had zero errors.

This is source/runtime verification only. It does not verify browser rendering,
full Mission UI, native browser physics, or iPhone Safari acceptance. No deploy
or owner-device acceptance was performed.

## Remaining dependency boundary

The full authored Global Context + Mission + Operator/resource closure is 1,918
assets and 22,361 records. The recovered Mission test suppresses original UI
because parts of that graph are unsupported in the current runtime. Do not
make the 1,918-asset closure a default iPhone startup payload. First resolve and
measure a compact authored payload containing the real player UI dependencies,
then run the bridge against that complete source graph. Only after that should
the main page consume its actor snapshot, route browser controls through it,
and retire the transport actor/WeaponHost authority. Obtain owner iPhone Safari
testing when that integrated build reaches the next major playable milestone.
