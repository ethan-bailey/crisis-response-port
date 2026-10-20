# Current handoff — actual original actor prefab loading

**Live state observed on 2026-10-20, before this handoff update**
- Full-source branch: feature/full-source-characterbody-integration, head bc1f4c7bd9b81045db7d6c61c30cc365734dcd7e. The executable/runtime source remains bd2a945e3193e374d176369978476ce5ebe1aeda; subsequent commits only update the REA evidence and this handoff.
- PR #42: open draft; base develop/wasm; mergeable=false.
- Live base develop/wasm: 2fc0a8e93be97c4d99123910fad4bd25798e71a8.
- Other open PRs: #7 (Unity), #56 (projectile/limb), #57 (IK). Keep these workstreams separate while the original actor loader is incomplete.
- Exact-SHA workflow lookup found no PR-triggered workflow runs for current source bd2a945, current docs head before this update bc1f4c7, manual compile checkpoint 5ed26d8, or current base 2fc0a8e.

## Latest validated executable checkpoint

Prior handoff records a manual .NET 8 build at source checkpoint 5ed26d87ddb66eb6841f72cbfeed8ebcf3e6690d, with EngineChecks 60, FoundationChecks 120, actor ownership/migration, source inventory, and provenance checks passing. No GitHub Actions run is attached. The later executable source SHA bd2a945 is not covered by that compile evidence.

The focused runtime/Checks/Crisis.Runtime.Checks.csproj -- --prefab-only test passes a synthetic fixture only. It verifies local serialized references, delayed Awake after binding, collider/joint values, and two independent clones. It does not load an original actor prefab or execute original CharacterBody/Limb lifecycle. The broader Checks invocation stopped at WeaponContracts with AmmunitionLoader.Load: Availability; the complete suite did not pass.

## Current REA evidence

See docs/REA_ACTOR_PREFAB_STATIC_EVIDENCE_2026-10-20.md for original artifact provenance, assembly SHA/MVID, prefab hashes, script identities, unsupported serialized fields, external asset counts, and exact loader/inventory touchpoints.

Key finding: the original recovery artifact contains Assembly-CSharp.dll, contrary to the earlier handoff's statement that no assembly was available. REA 6.7.0 statically inspected it with complete metadata/CIL coverage; it did not execute gameplay. The DLL is untracked and exists only in the recovered artifact.

Operator and Occupant both reference original source scripts missing from FullSource:
- ControlRenderOrder: 2 Operator components; recovered source SHA c890c3c271dc45af942f8d9d65cd1e07f64770f0f8ab05ffb6de90a1668e792d; Unity GUID 64b7b44a6d8a556f689e2a9dcfb79ed5.
- LimbAudio: 18 Operator and 16 Occupant components; recovered source SHA 2a5ebe98de3afc9155d6c99ca82eccf5d528cabfd306175bcf4796c560a5820e; Unity GUID b7bc07d16f703334b84ae52f02709339.

Both recovered sources are present in the hash-verified artifact. Preserve exact source/GUID identity and regenerate/review docs/audit/source-dependencies.json if restoring either file. Do not create substitutes.

## Actual loader blocker

At the observed runtime source, runtime/Engine/SceneLoader.cs contains ResolveScriptType, AssignBuiltinFields, AssignSerializedFields, and ReadValue. Script resolution uses the execution package's guidPaths plus a unique FullSource type. ReadValue resolves UnityEngine.Object values from the current prefab's local fileID map only. The builtin External handler currently marks nonzero cross-asset references unresolved; it does not instantiate package assets. Completeness rejects unsupported component classes and unresolved fields.

Actual Operator/Occupant records have unsupported serialized fields for MeshRenderer (20), Animator (7), BoxCollider2D (1: m_SpriteTilingProperty), and AudioSource (15). They also contain hundreds of cross-asset references. Therefore restoring the two scripts alone cannot pass Resources.Load(... requireComplete:true) for either real actor. The synthetic fixture remains useful only for local-reference behavior.

## Next implementation gate

1. Build an actual-prefab field/reference inventory from the preserved Operator and Occupant records and the exact source package. Resolve what object types and serialized members the real actors require before touching the loader.
2. Add the exact LimbAudio and ControlRenderOrder sources with their original GUID mappings and refresh the source inventory. Validate source ownership, identity, GUID resolution, and inventory freshness.
3. Extend the package/runtime reader for required builtin serialized fields and external asset types. Continue failing closed on unknown fields, types, or missing assets.
4. Load one complete original actor graph and verify CharacterBody/Limb/physiology ownership and lifecycle with mission, masks, audio, and presentation services bound. Then test two independent actors and ordered spawn/equipment/movement/hit/physiology/destruction behavior.
5. Only after those acceptance gates, run the consolidated WASM, Chromium/WebKit, and local HTTP smoke tests at the exact tested SHA. Keep preview/deployment unchanged until then. Do not merge #42 or claim iPhone parity based on CIL inspection, compile evidence from another SHA, or the synthetic fixture.

## Guardrails

- Repository AGENTS.md requires checking live develop/wasm, all open PRs, target head, comparable CI and the current remediation doc before any edit. This session rechecked those items. Re-read them again before code changes.
- PR #42's description is stale and carries earlier checkpoint SHAs. Do not treat its body as current source or build evidence; update it after a validated source checkpoint.
- Older compiler counts refer to different source graphs and do not indicate regressions or progress without exact graph comparison.
- This handoff/evidence refresh changed docs only. No source, runtime behavior, WASM, preview, deployment, or merge changed.
