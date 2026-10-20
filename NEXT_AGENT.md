# Current handoff — actual original actor prefab loading

**Observed at 2026-10-20, immediately before this handoff refresh:**
- Full-source branch: feature/full-source-characterbody-integration, head f1898971cc90043b74874346dcaa04f60353c97a. This head contains only documentation updates after executable source head bd2a945e3193e374d176369978476ce5ebe1aeda.
- PR: #42, open draft, base develop/wasm, mergeable=false.
- Live base: develop/wasm at 2fc0a8e93be97c4d99123910fad4bd25798e71a8.
- Other open workstreams: #7 Unity, #56 projectile/limb, #57 IK. Keep them separate through actor migration.
- Exact-SHA workflow lookup returned no PR-triggered runs for executable head bd2a945, manual compile checkpoint 5ed26d8, or current base 2fc0a8e. Do not present another graph's CI as verification for #42.

## Current validated checkpoint

Per the prior branch handoff, executable source checkpoint 5ed26d87ddb66eb6841f72cbfeed8ebcf3e6690d passed manual .NET 8 build, EngineChecks (60), FoundationChecks (120), actor ownership and migration gates, source inventory and provenance checks. No workflow run is attached to that manual compile. Current source head bd2a945 is later and must not inherit that compile claim without revalidation.

Focused runtime/Checks/Crisis.Runtime.Checks.csproj -- --prefab-only passes only a synthetic fixture: local serialized references, delayed Awake after binding, preserved collider/joint values, and two independent clones. It does not load original actor prefabs or execute original CharacterBody/Limb lifecycle. The broader Checks run stops at WeaponContracts: AmmunitionLoader.Load: Availability; it did not pass as a whole.

The original recovery artifact also contains an untracked Assembly-CSharp.dll; this was established by REA static analysis in this session. See docs/REA_ACTOR_PREFAB_STATIC_EVIDENCE_2026-10-20.md. The earlier handoff statement that no assembly was available to inspect is superseded. The assembly is in the recovery artifact, not tracked in the repo; REA does not execute it.

## REA evidence and actual-prefab gaps

REA 6.7.0 reports complete metadata/CIL coverage for Unity Mono assembly SHA-256 f5b73879f976e082ce91c238fd3443675ecc18dda00e51e6046b75303656a0cd, MVID 277b46fb-f585-4670-973c-6eedbc68ece9. The preserved Operator and Occupant prefabs reference missing FullSource scripts ControlRenderOrder (Operator 2 components) and LimbAudio (Operator 18, Occupant 16). The original recovered source copies exist in the archive. Integrate them as source, preserving original behaviors and inventory/GUID identity; do not substitute stubs.

Static field-binding comparison identifies unconsumed serialized fields for MeshRenderer (20), Animator (7), BoxCollider2D (1: m_SpriteTilingProperty) and AudioSource (15). The real prefabs also reference package assets through serialized cross-asset references; the loader currently resolves local prefab fileIDs only. External asset GUID paths resolve in the recovery archive, but runtime mesh/material/texture/audio/controller/prefab instantiation is not established. Thus the current requireComplete:true actual-prefab gate is expected to fail until built-in fields and asset references are handled.

## Next acceptance gate

1. Extend the generated package/runtime loader for the actual preserved Operator and Occupant records. Resolve all external assets and serialized built-in fields, report every unsupported field/type/reference, and fail closed on leftovers.
2. Add the exact recovered LimbAudio and ControlRenderOrder source to the FullSource closure. Refresh the dependency inventory and run existing source identity and ownership checks. Preserve audio/collision and render-order destruction behavior; no placeholder component.
3. Load one complete actor and verify original CharacterBody/Limb/physiology ownership and lifecycle after mission, mask, audio, and presentation services are bound.
4. Only after a complete one-actor graph, test two independently loaded actors and complete spawn/equipment/movement/hit/physiology/destruction flows. Then run the consolidated WASM, Chromium/WebKit, and local HTTP smoke gate at the exact source SHA. Keep the current preview unchanged until those checks pass; do not merge #42 or claim iPhone gameplay parity before required evidence.

## Constraints and stale status

- Repository remediation instructions require live-state checks before edits; on this pass, current base, all open PRs, #42 head, exact-SHA workflow lookups and docs/REPOSITORY_REMEDIATION_2026-10-18.md were re-read.
- PR #42's description still carries older checkpoint SHAs and reports its synthetic prefab test as newer than the actual current runtime head. Treat this handoff and code at the observed SHA as authoritative; refresh PR prose separately after the next validated source checkpoint.
- Earlier failed compiler counts (including the 144-error run) belong to different source graphs and are superseded for this branch. Never compare counts without matching exact source graphs.
- No runtime code, generated WASM, preview, deployment, or merge changed in this update. This session only added the REA evidence record and refreshed this handoff.
