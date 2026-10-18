# NEXT AGENT — Crisis Response Port (current handoff)
**Prepared:** 2026-10-17 · **This branch:** `maintenance/foundation-first-master-plan-20261017` (documentation-only, not gameplay) · **Canonical integration:** `develop/wasm`.

## Execute the adopted plan, not chronological handoff logs
[Foundation-first completion execution plan](docs/FOUNDATION_FIRST_COMPLETION_PLAN_2026-10-17.md) specifies dependency order, owners, precise implementation and acceptance gates (0–8). The older [full-game inventory](docs/COMPLETE_WASM_PORT_ROADMAP.md) remains the exhaustive parity checklist. Read [AGENTS.md](AGENTS.md) and [operations audit](docs/REPOSITORY_OPERATIONS_2026-10-17.md); treat older recorded SHAs as historical. Every future handoff **replaces this entire file**, no appended status diaries.

## Actual current lane state
| Lane | Inspected head | Readiness and merge decision |
|---|---|---|
| `develop/wasm` | `ec4414358e1b31beb8cb540ac11aa421147b132f` before this maintenance proposal | Canonical playable WASM baseline, not complete game. Keep verified rollback. |
| [PR #56](https://github.com/ethan-bailey/crisis-response-port/pull/56) original projectile/limb + Occupant/spawn | `fb96e92a848409014b85f6e094d995a9376fb731` | Draft. 157 commits / 163 changed files ahead of develop at audit; substantial source work, controlled browser fixtures and later focused CI, but real automatic NPC mission startup, source AI, production hit effects, full ragdoll and live NPC collider enablement absent. **Do not bulk-merge.** |
| [PR #57](https://github.com/ethan-bailey/crisis-response-port/pull/57) IK callbacks | `2b73044c107b875d8545d03541bf6b2d35f64654` | Draft, focused earlier callback check, full IK action closure/browser integration pending. |
| [PR #42](https://github.com/ethan-bailey/crisis-response-port/pull/42) full original CharacterBody | `ebf6ef01de3172b72fbab321b4941c1decb3643e` | Draft, 2,454-line recovered class and supporting source, entire original dependency graph not compiling as one live class. Do not merge wholesale. |
| [PR #7](https://github.com/ethan-bailey/crisis-response-port/pull/7) older Unity/iPhone | `ff2eed647e4eeb3446453b304bb4cbc4b6d31349` | Preserved historic alternative; not the primary C#→WASM integration workstream. |

## What is genuinely verified
- #56 latest focused original Occupant mission-bridge regression [39080683868](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39080683868) **SUCCESS** and fast source/package [39080683913](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39080683913) **SUCCESS**, both on head `fb96e92a848409014b85f6e094d995a9376fb731`. Tests verify source record integrity, not actual live mission NPC AI.
- Prior successful full WASM/browser milestone [39056615029](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39056615029) on gameplay source `a41b49cb6182327f792bb0118d51910f197c5d43`: source record actor bridge, original Occupant visual and physical graph, browser checks. NPC creation was test-invoked, not automatic author mission startup.
- More recent [39079729107](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39079729107) reported `compile-package` job **SUCCESS** when inspected; distinguish its exact source and full browser coverage before calling it a complete game run. Current #56 head has only the two aforementioned focused checks confirmed here.
- Protected older iPhone preview [stable URL](https://crisis-response-port-git-preview-w-6bd30d-ethanbaileymarsh-8332.vercel.app/game/) had Vercel READY but remote SSO/protection returned 403 and **physical Safari never validated**. Manual deployments only; do not quietly promote latest branch code or alter privacy settings.

## Immediate work order and tests
1. **Gate 0:** Build an explicit original `CharacterBody` dependency/callsite matrix from [PR #42](https://github.com/ethan-bailey/crisis-response-port/pull/42), `source-reference/Original/CharacterDependencies/`, current `runtime/Original/`, original NPC Behavior Designer/task sources and preserved assets. Inventory source vs engine shim vs unbound vs tested. Compile the **actual** recovered complete class, record concrete errors, and fix one dependency family at a time. No placeholder implementations or bypassed methods.
2. **Gate 1:** Finish shared actor `CharacterBody` physics, lifecycle, animation and physiology first; preserve existing source movement and approved iPhone proportional analog input. Test source update order and two independent original characters.
3. **Gate 2:** Close #57 original arm IK/action/equipment dependencies and merge validated reusable slices; avoid duplicates with #42/#56.
4. **Gate 3:** Complete original hit mission/audio/time/weapon-drop and body-state graph, source exit wounds/RNG/render dependencies, and real non-owner projectile collisions; keep all **19 NPC limb query colliders disabled** until a complete production hit dependency preflight and live two-actor test succeeds.
5. **Gates 4–5:** Bind `game/source-occupant-spawner.mjs`, `game/source-occupant-mission-integration.mjs`, Global.Context/OccupantSettings/EquipmentList, real RNG/raycast to normal `game/index.html` startup and compile original source AI Behavior Designer trees/tasks for navigation/perception/combat. Create dedicated AI PR after foundations; no synthetic AI.
6. **Gates 6–8:** Full missions, original asset presentation, source parity/reference tests, native then fixed-SHA WASM/PhysX/Chromium/WebKit; manual protected preview, physical iPhone test, reviewed small PR merges to `develop/wasm`.

## Source and safety invariants
The repository and source assets stay private. Recovery archive SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`; private preservation release `recovery-preservation-2026-10-15`. No original game logic substitutions, placeholder AI, fake targets, parallel health state, or unjustified parity claims. Preserve approved round-inclusive ammo UI/no extra chamber display, icon controls, circular iPhone joystick with 16% dead zone and 65% walk→sprint breakpoint, native full-speed keyboard. Retain screenshot/build artifacts with exact tested SHA. One heavy build per milestone; no auto Vercel deployments.

## Before handing off again
Re-query branch heads, all active PRs, focused/full CI, exact deployed manifest and current blocker status. Replace this entire file (not append). Commit and read back both first section and total length. Preserve history in Git, not in this document.
