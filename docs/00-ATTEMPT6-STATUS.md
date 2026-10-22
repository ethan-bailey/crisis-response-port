# PR #42 merge probe attempt 6 — blocked preflight handoff

## Checkpoint verification

- PR branch: `feature/full-source-characterbody-integration` at `861df48cd203e23fc22b57378b83a3cda688369c` when checked.
- The handoff commit is docs-only: `861df48cd203e23fc22b57378b83a3cda688369c` → parent `23ea98113c5d537d750939c1bd952debf243b0ee` → source checkpoint `a653609b62f3e3c79f73d0a17f0bf3dc89eae7b8`. The two intervening commits changed documentation only.
- develop/wasm inventory checkpoint: `2fc0a8e93be97c4d99123910fad4bd25798e71a8`.
- PR #42 remains open and draft; GitHub reported `mergeable: false` at preflight.

## Attempt 6 work list prepared before any merge

The checklist filters shared inventory entries by comparing normalized full C# member declarations/bodies and their file locations, and type declaration headers and declaration file sets. It contains **49 type header/location entries** and **1,169 shared member entries** (1,218 planned resolution entries total). Every listed row plans to retain the PR definition and path(s). Shared definitions with identical normalized source and matching paths are omitted as file-level no-ops. The inventory’s broader collision total remains 199 shared types plus 2,599 shared member signatures.

The explicit plan also requires PR whole-file wins for `runtime/Engine/WeaponHost.cs`, `runtime/Engine/OriginalArmPhysicsHost.cs`, and `runtime/Original/ArmorClass.cs`; PR `CapsuleCollider` from `runtime/Engine/CharacterPhysics.cs`; and PR Animator accessors. The merged `ArmorClass.cs` would need a direct inspection to ensure only the PR `PenetrationTest` remains.

## Gate disposition

**No merge was started.** The required isolated Crisis Response worktree is unavailable in this runtime: the current directory is not a Git checkout; the only discovered worktree is an unrelated Sites checkout. `dotnet` is not installed or discoverable. Therefore the exact-head reset/clean-status check, merge, fresh `dotnet build runtime/Crisis.Runtime.csproj --no-incremental`, Calibre contract, weapon contract, and merged-tree acceptance gates could not be run. No merge commit or source edit was created. This docs-only update leaves the PR source at its verified head.

| Gate | Status | Reason |
|---|---|---|
| Fresh full-source build | **NOT RUN** | No Crisis Response worktree and no `dotnet` executable. No build time or build result exists for a merged tree. |
| Calibre contract | **NOT RUN** | No merged checkout/build available. |
| Weapon contract | **NOT RUN** | No merged checkout/build available. |
| Two-actor gameplay gate | **NOT RUN** | No merged tree; cannot claim attempt-6 acceptance. Existing PR body reports native original-scene and spawner-driven scene contracts did not pass, with scene dependencies unresolved. |
| Exact-source browser smoke gate | **FAIL / NOT PASSED (existing PR status); NOT RERUN** | Existing PR body says exact-source browser smoke did not pass; recovered exact Mission 01 built-in Sprite references remain unresolved. No merged tree was available to rerun it. |
| Runtime Foundation Actions | **NEEDS CI / OWNER DECISION** | No Actions run or retry was started. Owner decides whether to restore minutes or waive under the single-line policy. |

The inventory self-test **passed** for Animator accessors, `CapsuleCollider.Raycast`, `ArmorClass.PenetrationTest`, `WeaponHost`, and `OriginalArmPhysicsHost`. This attempt does not claim the resulting resolution plan was applied.

## Deferred behavior and exclusions planned

- Defer all **106 develop-only members** in shared types and all **36 develop-only type identities**. Names/locations are in the inventory’s deferred report.
- Exclude develop `OriginalLimbSkinTrace` and nested types, the `SourceSkinTrace` field only in `runtime/Original/Weapons/CharacterLimbRagdollState.cs` while preserving that file’s other members, and the `--original-limb-skin` invocation/workflows.
- Retain provenance/hash checks and Limb/LimbRenderer source and mask recovery material. Before any future deletion, classify the file against PR definitions and references.
- Do not add Rigidbody change events unless remaining merged code requires them; none were added or evaluated because no merge occurred.

## Artifacts

- [Attempt 6 type-header/location checklist](PR42_ATTEMPT6_CHECKLIST_TYPES_2026-10-22.md)
- [Attempt 6 member checklist, part 1](PR42_ATTEMPT6_CHECKLIST_MEMBERS_01_2026-10-22.md)
- [Part 2](PR42_ATTEMPT6_CHECKLIST_MEMBERS_02_2026-10-22.md) · [Part 3](PR42_ATTEMPT6_CHECKLIST_MEMBERS_03_2026-10-22.md) · [Part 4](PR42_ATTEMPT6_CHECKLIST_MEMBERS_04_2026-10-22.md) · [Part 5](PR42_ATTEMPT6_CHECKLIST_MEMBERS_05_2026-10-22.md) · [Part 6](PR42_ATTEMPT6_CHECKLIST_MEMBERS_06_2026-10-22.md)
- The existing [two-tree inventory index](PR42_TWO_TREE_TYPE_MEMBER_INVENTORY_2026-10-22.md) and [deferred behavior report](PR42_TWO_TREE_DEFERRED_2026-10-22.md) provide all 199 shared types, 2,599 common signatures, all 106 deferred members, and all 36 deferred types.

No final merge into `develop/wasm` and no ChatGPT Sites migration were started.
