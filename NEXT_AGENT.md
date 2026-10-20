# Current handoff — original actor prefab dependency closure

**Observed on 2026-10-20, immediately before this handoff refresh**
- Full-source branch: feature/full-source-characterbody-integration at e27d7fc442a2d14cfdde1ffbdac636b47c12cf00. This is documentation/evidence only after executable source head bd2a945e3193e374d176369978476ce5ebe1aeda.
- PR #42 remains open, draft, mergeable=false. Live target develop/wasm is 2fc0a8e93be97c4d99123910fad4bd25798e71a8.
- Other open PRs: #7, #56 and #57; keep their workstreams separate from the real actor loader.
- Exact-SHA Actions lookup has no PR-triggered runs on bd2a945, current docs head e27d7fc, prior manual compile checkpoint 5ed26d8, or current base 2fc0a8e.

## Milestone completed this session: actor dependency closure audited

See docs/REA_ACTOR_PREFAB_STATIC_EVIDENCE_2026-10-20.md for the exact artifact provenance, REA method evidence, source hashes/GUIDs, unsupported serialized fields, and transitive reference tables.

A recursive static scan over serialized YAML assets currently present in scene-execution.json reached 81 assets / 943 records for Operator and 64 / 850 for Occupant. Within those records, 346 Operator and 306 Occupant asset references point to paths that are indexed by GUID but absent from the generated execution package. Missing packaged targets include WAV/OGG audio, PNG textures, shaders, animation clips, physics materials and mixers. The extractor scripts/extract_runtime_scene.py only queues .asset/.prefab/.unity/.mat/.controller/.overrideController targets; it records GUID-to-path for other targets without carrying those assets into scene-execution.json.

The nested asset records also contain 87 references per actor to unresolved script GUID f5f67c52d1564df4a8936ccd202a3bd8. This is not a direct actor script GUID; identify its component and source/package from the original project or Unity package metadata before mapping it. Unity built-in GUID references e000... and f000... appear separately.

Direct actor components also reference missing original FullSource scripts:
- ControlRenderOrder: 2 on Operator, 0 on Occupant; original source hash c890c3c271dc45af942f8d9d65cd1e07f64770f0f8ab05ffb6de90a1668e792d; Unity GUID 64b7b44a6d8a556f689e2a9dcfb79ed5.
- LimbAudio: 18 on Operator, 16 on Occupant; source hash 2a5ebe98de3afc9155d6c99ca82eccf5d528cabfd306175bcf4796c560a5820e; Unity GUID b7bc07d16f703334b84ae52f02709339.

Keep exact recovered component behavior and identity. No stubs.

## Validation state and superseded evidence

Prior handoff records manual .NET 8 build of source SHA 5ed26d87ddb66eb6841f72cbfeed8ebcf3e6690d, EngineChecks 60, FoundationChecks 120, ownership/migration, source inventory and provenance passing. No Actions run is attached. These results do not validate later executable source SHA bd2a945.

The --prefab-only runtime check passes only a synthetic local-reference fixture. It is not an original actor load. The broader Checks run stops at WeaponContracts with AmmunitionLoader.Load: Availability. No complete test suite pass is established.

The original recovery ZIP contains Assembly-CSharp.dll; the earlier handoff claim that there was no assembly to inspect is superseded. REA 6.7.0 statically analyzed that binary with complete metadata/CIL coverage, but did not execute it.

## Required next implementation sequence

1. Update package generation so the generated execution package carries every referenced asset needed by the transitive actor graph, or a verified external asset bundle with integrity and stable GUID/fileID identity. Include audio, textures, shaders, animations, physics materials and mixers. Map the nested unresolved script GUID and builtin Unity IDs explicitly; fail closed on unknown references.
2. Extend SceneLoader for the complete required set of serialized built-in fields and external asset object types. Preserve the Unity serialized values; do not silently discard unsupported renderer, animator, collider or audio fields.
3. Restore exact ControlRenderOrder and LimbAudio source with GUID mapping. Regenerate/review docs/audit/source-dependencies.json and pass source identity/ownership checks.
4. Load the actual Operator prefab completely and verify original CharacterBody/Limb/physiology ownership and lifecycle with mission, mask, audio and presentation services bound.
5. Then load Occupant and prove two independent original actor graphs and ordered spawn/equipment/movement/hit/physiology/destruction flows. Run consolidated WASM, Chromium/WebKit, and local HTTP smoke tests at the exact source SHA before changing the preview or merging.

## Guardrails

- Recheck live develop/wasm, all open PRs, target head, comparable CI and docs/REPOSITORY_REMEDIATION_2026-10-18.md before any code or doc edit.
- PR #42 description still contains stale checkpoint SHAs; use this handoff, repository files and exact source evidence, then refresh PR prose only after a validated executable checkpoint.
- Compiler error totals from different source graphs cannot be compared as progress/regression.
- This session changed documentation only. Runtime code, generated WASM, preview, deployment and merge are unchanged.
