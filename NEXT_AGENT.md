# CURRENT HANDOFF — Crisis Response Port

**Prepared:** 2026-10-17 (America/New_York; 2026-10-18 UTC). **Worktree:** `feature/full-source-characterbody-integration` (PR #42). **Canonical game integration:** `develop/wasm`. This file replaces, rather than appends to, earlier handoffs. It is branch-local; do not mistake it for the handoff on `develop/wasm`.

## First action: verify LIVE repo state before editing

Read `AGENTS.md` §0 and `docs/REPOSITORY_REMEDIATION_2026-10-18.md` on this branch. Re-query the exact heads of `develop/wasm`, PR #42, #56 and #57; recent merged PRs (especially #63–#66); and relevant CI. Check whether the target branch moved after this handoff. Do not select tasks from a dated audit or the largest-numbered PR without comparing merges, source state, and acceptance gates. The current handoff itself is not proof of recency.

**Do not revive the obsolete 30-error / 21-name compiler checkpoint.** The more complete graph yielded **144 errors / 60 unresolved names** at SHA `407c66d026784b72c96490eabae6085dd87c22e6`, full compile run [39091701268](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39091701268). Different graph scopes make naive count comparison invalid. PR #66 remediation merged into PR #42 as `6c4415e62cef53646818c8c08412d5604282a740`; narrow CI [39093133256](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39093133256) passed at `389d8e8dcc3cc49d4327cc55588d874271dc4271`. Verify fresher evidence before claiming these are still latest.

## What the restructure did — not gameplay parity

PR #66 made the expensive full-closure and historical recovery workflows manual-only; installed repeatable diagnostic diffs and a fixed compiler baseline; indexed 1,384 original exported C# files and classified all 60 unresolved names; guarded the seven legacy transport-owner files and prohibited new split actor ownership; extracted Unity engine primitives without rewriting rule bodies; and added 13 isolated engine assertions, 90 original foundation assertions and narrow CI. **Full original CharacterBody still does not compile, and live NPC AI is not done.** Source recovery and fixture tests are not automatic mission startup or playable character verification.

## Current boundaries

- **PR #42:** original full actor and foundation compiler/engine dependencies. Actor destination is `Crisis.Runtime.FullSource.CharacterBody`; do not add alternate mutable actor owners.
- **PR #56:** source projectile/limb injuries, physiology, Occupant bridge and mission spawning; draft, live source actor/NPC collision integration unfinished. Preserve 19 disabled NPC limb query colliders until production hit preflight and two-actor tests pass.
- **PR #57:** source IKArmController callbacks; transport-character constructor must be migrated to the unified original actor. Do not merge as complete IK parity.
- **PR #63–#65:** reviewed extractions of original mission/audio/settings/Occupant sources and independently testable spawner slices; check merged status/head before duplicating them.
- **PR #7:** historical alternative Unity workstream, not the active original C#→WASM dependency path.
- **`develop/wasm`:** current game/preview baseline, separate from PR #42. Read its latest commit; do not overwrite game input or auto-deploy.

## Actual next development sequence

1. Read `docs/audit/compiler-baseline.json`, indexed original source inventory and `docs/REPOSITORY_REMEDIATION_2026-10-18.md`. Start from a **complete transitive original dependency family**, inspecting 72 ambiguous names and archive hashes; do not add class stubs to force a green compile.
2. Finish cohesive missing Unity engine graphics/physics service contracts through isolated native tests. Avoid duplicate compiler-reporting tooling: the remediation has already added a baseline and stable diagnostic diffs.
3. Consolidate original actor/limb/blackboard/equipment/weapon identity across PR #42, #56, #57; preserve original source method order, non-owner hit accounting, and single mutable physiology owner.
4. Bind original mission/world/audio/scoring services without invented gameplay. After a significant batch, run one explicit full closure build and compare diagnostics against fixed baseline.
5. Once full source graph compiles, validate two real actors with original equipment, injury, events, state and destruction; only then run exact-SHA .NET WASM, PhysX, Chromium/WebKit integration and manual protected preview gate.

## Preservation, evidence and rollback

Original proprietary source and recovery ZIP remain private. No invented NPC AI, fabricated gameplay actor, shadow vitals, placeholder equipment or source-effect substitutions. Preserve approved circular analog iPhone joystick (16% dead zone, 65% walk→sprint breakpoint), keyboard speed, icon controls, and chamber-inclusive magazine count **without** separate chamber widget. No automatic Vercel deployment or bulk merge of unfinished PRs. Existing protected preview is not evidence of the newest feature changes or physical Safari acceptance.

For a handoff, re-read live heads, tested SHA, failures and preview manifest. **Replace this entire file** with the new compact current state. Historical steps belong in Git/docs, not appended here.
