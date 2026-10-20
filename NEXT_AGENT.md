# Live continuation update — 2026-10-20

**This update supersedes dated workspace snapshots below where they conflict.**

- PR #42 was rechecked before work: open, draft, base `develop/wasm`. At test time its executable/source HEAD was `b5ac90849b5140fe96d49bb21c3e85af9b5b7a23`; the Codespaces working tree had 13 pre-existing dirty paths, all preserved. The `docs/REA_ACTOR_PREFAB_STATIC_EVIDENCE_2026-10-20.md` continuation records exact commands, results, and Codespaces log paths.
- Local native checks passed on that dirty tree: runtime checks project build (44 warnings, zero errors); EngineChecks (69 isolated graphics/source API assertions); FoundationChecks (120 assertions); original arm sequence/source, arm-controller source, arm physics, and hand-runtime scripts (each exit 0). These do not establish full gameplay. Probe output explicitly reports full IK/ArmController playback and native shared parity false.
- `git diff --check` failed (exit 2) on an EOF blank line at `runtime/Engine/SerializedAssetResolver.cs:76`; it was not modified or cleaned.
- Actual `--prefab-only` loading failed closed on missing serialized `fileID 2800000` for `Assets/Texture2D/black pelvis.png`; available package image bytes lack the corresponding serialized Texture2D/importer record. One bounded search found no other copy under `/workspaces` or in repository GitHub code search. Do not fabricate the missing object or import settings.
- No GitHub Actions workflow ran, so there is no workflow ID. The build was from the dirty working tree and is not exact-commit reproducibility evidence.
- The PR description and REA evidence were refreshed with this checkpoint. The REA update is a documentation-only commit; no runtime code was pushed by that action. The Codespaces worktree was not synchronized, reset, or discarded.

## Ordered next gates

1. Recover the original serialized Texture2D object/import metadata and any other missing transitive actor asset records from the authoritative recovery source; preserve GUID/fileID and source provenance.
2. Resolve the full transitive asset closure for the actual Operator and Occupant prefabs, retaining all unknown values and failing closed.
3. Pass actual Operator prefab construction and lifecycle/ownership checks.
4. Pass actual Occupant prefab construction and prove two independently owned actor graphs.
5. Verify original spawn, equipment, movement, hit, physiology and destruction flow for both actors.
6. Recheck open PRs and live `develop/wasm`; only once implementation is ready for final verification, integrate with a normal merge commit (no rebase/cherry-pick).
7. Build from a recorded exact source SHA and record any workflow ID/artifact.
8. Run Chromium and WebKit gameplay checks against that build, then local HTTP smoke.
9. Refresh REA evidence and this handoff with exact tested SHA, commands, logs and results; refresh PR #42 description while keeping it open/draft.

Do not advance past gate 1 by substituting synthetic assets. Preserve all existing pending Codespaces changes and leave the preview untouched.

---

# Current handoff — original actor prefab dependency closure

**Observed on 2026-10-20, before this handoff update**
- Full-source branch: feature/full-source-characterbody-integration at 4842fe3c1e47336d6db016cf4af00ab6c019d5b5. This commit updates documentation/evidence only after executable source head bd2a945e3193e374d176369978476ce5ebe1aeda.
- PR #42 remains open and draft; its recorded base snapshot is 5d83c4ef627baa6dc9009ef9a585c0b70e0d9435, while live develop/wasm is 2fc0a8e93be97c4d99123910fad4bd25798e71a8. The branches diverge: 141 commits ahead, 226 behind, mergeable=false (observed comparison; refresh before integration).
- Other open PRs: #7, #56 and #57; keep their workstreams separate from the real actor loader until identity/ownership conflicts are resolved.
- Exact-SHA Actions lookup has no PR-triggered runs on executable source bd2a945, current docs head 4842fe3, prior manual compile checkpoint 5ed26d8, or current base 2fc0a8e.

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

## Integrate the live target branch before final verification

Do not integrate develop/wasm into PR #42 during the actual-prefab implementation phase. First complete the package/reference/serialized-field path, load the real Operator and Occupant prefabs, and finish their original two-actor lifecycle flow. Then, immediately before the final verification phase, refresh the live target head, PR heads/open PRs, relevant workflow evidence, and local worktree status. Preserve all existing uncommitted work; do not discard or overwrite it.

Integrate the then-current develop/wasm into feature/full-source-characterbody-integration with a normal merge commit so the published feature history is preserved. Do not rebase or cherry-pick only the latest documentation commit. Resolve conflicts against original source identity, serialized values, and the single FullSource actor ownership graph; if a conflict cannot be resolved without weakening those constraints, stop and document it. This is a branch integration step, not a merge of PR #42 into develop/wasm.

After integration, record the exact resulting source SHA, then run the complete requested verification phase once on that SHA: focused source/provenance and real-prefab checks, full runtime checks, exact-SHA WASM build, Chromium/WebKit gameplay checks, and local HTTP smoke test. Update evidence and PR prose with the tested SHA and actual results. Do not change the preview or merge PR #42 until all required gates pass.

The 141-ahead/226-behind comparison above is a dated snapshot, not a permanent command. Always recompute the relationship at the time of integration.

## Guardrails

- Recheck live develop/wasm, all open PRs, target head, comparable CI and docs/REPOSITORY_REMEDIATION_2026-10-18.md before any code or doc edit.
- PR #42 description still contains stale checkpoint SHAs; use this handoff, repository files and exact source evidence, then refresh PR prose only after a validated executable checkpoint.
- Compiler error totals from different source graphs cannot be compared as progress/regression.
- This session changed documentation only. Runtime code, generated WASM, preview, deployment and merge are unchanged.


### October 20 continuation — checksum-pinned actor archive recovered; runtime gate still fails

The prior note that the actor texture/import metadata was missing is superseded by this run's verified archive result. Release asset `crisis-response-recovery-direct-v3-1.zip` matched pinned SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. `scripts/recover_character_actor_prefabs.py --archive <archive> --check` passed for the exact Operator/Occupant prefab and .meta files. Original `black pelvis.png` and .meta are also in the archive; the PNG SHA-256 is `80a395d3a90e9566e438ac9a09b98be29e38be27274dd36677b023efd6b2a19c`, .meta SHA-256 `0859109e8a7cd6d6c6048d4089e909e98ea51ff5d6b73ebabf1d81811707d773`, GUID `38b42807fa7576a44a0d6ea90b08db63`. Operator references fileID `2800000`.

**Immediate next implementation gate:** determine how the package generator and runtime represent original imported binary assets and Unity fileIDs, then preserve this texture's exact identity/import semantics and resolve the full dependency closure without synthetic IDs or fake fields. A regenerated package from the pinned extracted project reports 1,382 assets, 21,366 records, 14,249 object-reference occurrences, 508 binary assets, and 11 unresolved GUIDs. Actual prefab loading still fails closed: `KeyNotFoundException: Package asset Assets/Texture2D/black pelvis.png has no serialized fileID 2800000` (`/tmp/run-actual-prefab.log`). Do not mark lifecycle complete until package closure and actual prefabs pass.

This run's source probes/build/test results and logs are appended in `docs/REA_ACTOR_PREFAB_STATIC_EVIDENCE_2026-10-20.md`. Codespaces HEAD was `b5ac90849b5140fe96d49bb21c3e85af9b5b7a23` with 13 pre-existing pending changes; checks were dirty-worktree checks, not exact-SHA acceptance. EngineChecks 69 and FoundationChecks 120 passed; native Checks build passed with 44 warnings. No Actions workflow ID, full two-actor run, Chromium/WebKit gameplay check, or HTTP smoke test was completed. Preserve the current dirty worktree. PR #42 remains draft/open; do not merge until immediately before final verification.
