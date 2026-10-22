# NEXT AGENT — Crisis Response WASM port

## Checkpoint

- `develop/wasm` received PR #42 as merge commit `09987a75d671d778287605322394c474da1ed74a`, with prior develop parent `2fc0a8e93be97c4d99123910fad4bd25798e71a8`.
- This handoff is a docs-only update directly on `develop/wasm`, following that merge commit. The branch head is the commit containing this handoff; GitHub commit metadata records its exact SHA.
- PR #42 is merged. The source branch `feature/full-source-characterbody-integration` is retained at `b835196201650e7ef0be6efd9ca038b46e34259e`.
- The merge commit tree is `7e3be9753b909d4b1210484c4a84e9270aed424b`. Compared with tested attempt-6 tree `0f885ffc326a1b2f9a084543a92b7efc1e75`, the only pre-handoff difference is the documentation-only update to `docs/00-ATTEMPT6-STATUS.md`; no source file differs.
- Attempt-6 verification on the tested tree: `dotnet build runtime/Crisis.Runtime.csproj --no-incremental` passed (0 errors, 45 warnings, 11 seconds); Calibre, weapon, and original limb-skin provenance/hash contracts passed. No Actions run occurred at the merge SHA; see the owner Actions-disabled decision below.

## Owner decisions

1. **Accepted:** remove `runtime/Original/Weapons/CharacterLimbRagdollState.cs` in full. The inventory classifies its other 33 members as develop-only and dependent on deferred types; the tested merged tree passed the full gate without the file.
2. **GitHub Actions disabled by owner decision for this project.** Outcome was (c): no Actions run exists at merge SHA `09987a75d671d778287605322394c474da1ed74a` (`total_count: 0`). `.github/workflows/runtime-foundation.yml` declares `workflow_dispatch` but is absent from the default branch `main`, so it cannot be dispatched normally. The minutes balance is unverified; the owner will not restore or rely on Actions regardless. Actions workflows were removed in a follow-up commit; local verification (fresh full-source build, relevant contracts, and provenance checks recorded against exact SHAs) is the standing landing gate.
3. **The two-actor gameplay gate and exact-source browser smoke gate do not block the merge.** Both remain outstanding and are the next gameplay acceptance tasks.

## Deferred behavior applied

- No develop-only behavior was carried over: all 106 develop-only members in shared types and all 36 develop-only type identities remain deferred.
- This includes develop `OriginalLimbSkinTrace`, the `SourceSkinTrace` field, develop additions to `WeaponHost` and `OriginalArmPhysicsHost`, and the develop-only limb/skin runtime consumers.
- The develop `CharacterLimbRagdollState.cs` was removed in full as accepted above.
- `LimbLayer`, `LimbRenderer`, and `LimbTextureData` remain only as source-reference recovery evidence, outside runtime behavior. Retain the provenance/hash checks and Limb/LimbRenderer source and mask recovery material. The `--original-limb-skin` invocation and its workflows remain removed.
- The inventory and attempt-6 checklists remain in `docs/PR42_TWO_TREE_*` and `docs/PR42_ATTEMPT6_CHECKLIST_*`.

## Ordered next work

1. Run the two-actor gameplay gate on develop.
2. Run the exact-source browser smoke gate on develop.

The ChatGPT Sites publish is not gated on Actions. It remains gated on the local and live smoke tests specified in the Sites brief.

Do not start the ChatGPT Sites migration in this session. It has its own brief and starts only after the owner confirms. PRs #7, #56, and #57 remain outside this handoff's scope.
