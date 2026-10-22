# PR #42 attempt 6 — final status (2026-10-22)

## Pinned inputs and result

- PR source checkpoint: a653609b62f3e3c79f73d0a17f0bf3dc89eae7b8; current handoff parent before merge: e8ecf6b02653415729e1b1665c04227608693075 (docs-only after the inventory checkpoint).
- develop/wasm checkpoint: 2fc0a8e93be97c4d99123910fad4bd25798e71a8.
- Merge commit pushed to PR #42: 27058090a91b5e5759492d884ba7d961ac9ea2f3.
- Tested merge tree: 0f885ffc326a1b2f9a084543a92b7efc1e75.
- GitHub PR status after push: open, draft, mergeable: true.

The pre-merge checklist contains 49 changed type-header/location entries and 1,169 changed or relocated member entries. The prepared resolution for every row was PR definition/file wins. The full checklist is in PR42_ATTEMPT6_CHECKLIST_TYPES_2026-10-22.md and PR42_ATTEMPT6_CHECKLIST_MEMBERS_01_2026-10-22.md through _06_...md.

## Verification

- Required full-source gate: dotnet build runtime/Crisis.Runtime.csproj --no-incremental passed on the tested tree: 0 errors, 45 warnings, 11.0 seconds.
- Calibre contract: PASS (original Calibre GUID, fileID, type, path, serialized name, and prefab lifetime).
- Weapon contract: PASS (weapons: PASS).
- Original limb skin provenance verifier: PASS (tools/verify-original-limb-skin-provenance.py). It preserves the archive hash and per-source SHA checks while the two deferred skin workflows and --original-limb-skin invocation were removed.
- The first build run failed after an incorrect file-level deletion of runtime/Engine/BallisticEngine.cs; the PR copy was restored exactly, staged-deletion audit against the PR tree found no other PR-owned deletions, and the required build was rerun successfully on the tree above.
- Named collision guardrails: PR WeaponHost.cs, OriginalArmPhysicsHost.cs, CharacterPhysics.cs (CapsuleCollider), and Foundation/ArmorClass.cs are exact PR-side files. ArmorClass.PenetrationTest occurs once. No develop OriginalCapsuleCollider.cs remains. No Rigidbody change events were added; the relevant physics/presentation files match the PR source.
- Checklist result: all 49 type-location and 1,169 member work-list entries resolved to the PR-side definitions/locations; no planned work-list deviation.

## Deferred behavior and source material

- Develop-only runtime behavior in the 106-member deferred set was excluded. This includes SourceSkinTrace, the develop skin/physics consumers, develop additions to WeaponHost and OriginalArmPhysicsHost, and the develop partial CharacterBody/Limb runtime files.
- All 36 develop-only gameplay type implementations were excluded from the runtime. Three source-only definitions (LimbLayer, LimbRenderer, and LimbTextureData) remain under source-reference/Original/LimbSkin/ as the explicitly requested recovery evidence; that directory is outside the runtime project and none of these definitions is wired into runtime behavior.
- The develop CharacterLimbRagdollState.cs file was removed in full. Its 33 other member definitions are also classified as develop-only in the inventory, so retaining them would violate the instruction to omit all 106 develop-only members and leave dependencies on deferred types. This follows the global deferral list and deviates from the parenthetical instruction to keep that file's other members; see this conflict explicitly for owner review.
- Retained limb source/mask provenance material; removed both live-limb-skin-native.yml and original-limb-skin-native.yml, and kept their exact source/hash verification in the standalone verifier above.

## Remaining acceptance items

| Acceptance item | Status | Reason |
| --- | --- | --- |
| Two-actor gameplay gate | NOT RUN in attempt 6 | This merge attempt's required gate was full-source build plus Calibre and weapon contracts. The two-actor gameplay gate remains outstanding. |
| Exact-source browser smoke gate | NOT RUN in attempt 6 | It remains a separate acceptance gate and was not part of the attempt-6 compile/contract gate. |
| Runtime Foundation Actions | NEEDS CI | Actions minutes remain exhausted. Restoring minutes or waiving under the single-line policy is the owner's decision. |

Attempt 6 stopped after the PR-branch merge push. PR #42 has not been merged into develop/wasm, and the ChatGPT Sites migration has not started.
