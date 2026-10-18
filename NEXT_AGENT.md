# CURRENT HANDOFF — Crisis Response Port

**Branch:** `chore/live-state-guard-20261018` (documentation-only safeguards). **Integration:** `develop/wasm`. This file completely replaces old chronological handoffs.

## Live-state gate
Before source edits, recheck live `develop/wasm`, PR #42/#56/#57, recently merged PRs #63–#66 and exact narrow/full CI. Read `AGENTS.md` §0. Do not infer currency from older audit, chat, GitHub commit search or a dated handoff.

## Post-restructure baseline
PR #66 was merged into PR #42's foundation branch, **not** the playable integration branch. `docs/REPOSITORY_REMEDIATION_2026-10-18.md` on the foundation branch is the latest recorded restructuring report: actual complete-graph compile **144 errors / 60 missing names**, SHA `407c66d026784b72c96490eabae6085dd87c22e6`, run 39091701268. Narrow remediation CI passed at `389d8e8dcc3cc49d4327cc55588d874271dc4271`, run 39093133256. The old **30/21** checkpoint is superseded (different graph).

Restructuring inventories 1,384 recovered C# files, classifies the 60 names, freezes legacy actor ownership, protects complete-source compilation and makes expensive repeated workflows manual-only. These are foundational controls, **not** proof of completed original CharacterBody or live NPC AI.

## Workstream ownership
- `develop/wasm`: playable current integrated browser baseline; inspected SHA `ba12245122a5fc256a4bbb6c3f815050d25bcc20`; verify it again before writing.
- PR #42: original full CharacterBody, engine source dependencies, post-remediation actor migration. Diverged (70 ahead, 225 behind) at inspection: **do not merge wholesale**.
- PR #56: original projectile/physiology/Occupant bridge, not production live NPC AI; source actor ownership must be reconciled.
- PR #57: original IK callbacks; migrate away from transport actor.
- PR #63–#65: source reference and Occupant spawn extraction; verify merge state before duplicate recovery.
- PR #7: legacy Unity route.
  
## Next implementation gates
1. Refresh live branch heads and CI and inspect `docs/audit/compiler-baseline.json` and `docs/REPOSITORY_REMEDIATION_2026-10-18.md` on PR #42.
2. Select and recover complete original transitive dependencies from the pinned inventory, reviewing 72 ambiguous source names. Verify original archive hashes; no fake stubs.
3. Make `Crisis.Runtime.FullSource.CharacterBody` the only mutable actor with the original limbs, physiology, equipment, weapon and blackboard. Rebind #56 projectile and #57 IK consumers in reviewed slices, not by wholesale merge.
4. Use narrow engine/foundation/actor-ownership tests; run one explicit full compile per cohesive batch with stable baseline diagnostic comparison. Never disguise a failing full compile as passing.
5. Once the real graph compiles, validate two real actors and source hit/wound/circulation/ragdoll lifecycle, then consolidate .NET WASM/PhysX Chromium/WebKit before any requested manual protected iPhone preview.

## Preservation
Private original assets, no invented gameplay or duplicate health. Preserve approved analog joystick, icon controls and chamber-inclusive magazine count without separate chamber display. Keep 19 NPC limb collision queries gated until hit-preflight/two-actor proof. No automatic deployments. On next handoff, replace this whole file, never append history.
