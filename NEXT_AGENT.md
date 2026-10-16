# Next agent — authoritative handoff, WASM priority confirmed 2026-10-16

## Vercel release policy — manual deploys only (2026-10-16)

**Automatic Git-triggered Vercel deployments are disabled** on `main` and every existing `codex/*` branch with root `vercel.json` → `git.deploymentEnabled=false`. Do not delete or override that setting merely to get a preview URL. New branches must inherit the root configuration. GitHub Actions CI is the normal per-commit validation path. Only create an explicit protected Vercel deployment when a specific verified WASM/browser package is ready for physical iPhone testing. Record tested SHA, artifact, deployment URL/ID, current protection, and rollback. Branch aliases retain their **last deployed** build and will not follow later commits automatically. See `docs/VERCEL_MANUAL_DEPLOYMENTS.md` for the exact branch commit ledger, manual deployment procedure, and rollback. No Vercel deploy has been initiated for this configuration change.


**User direction, 07:43 Eastern:** continue the original C# → .NET WebAssembly compile. Native Unity validation is a separate experiment, not a prerequisite, route decision or current work assignment. Prioritize source dependencies for actual missing gameplay/animation features; do not expand packaging/audits or write replacement gameplay merely to show activity.

Current bounded WASM batch: [PR #21](https://github.com/ethan-bailey/crisis-response-port/pull/21), branch `codex/wasm-original-arm-sequences`, code `256b574eca62a902e23e9ba45d2b2542d64fe445`, [run 38896212172](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38896212172). Six unchanged original arm-sequence classes, seven source prefabs / 21 frames; actual SDK-8 publish and Chromium/WebKit composition checks **passed**. Private site artifact `12043404117`, evidence `12043827958`, expire 2026-11-15; 41 selected original C# inputs (previously 35). No animation playback/full-game claim. [Batch handoff](https://github.com/ethan-bailey/crisis-response-port/blob/codex/wasm-original-arm-sequences/docs/WASM_ORIGINAL_ARM_SEQUENCES_2026-10-16.md).

## Separate native experiment — prior context, not the WASM queue

The handoff execution order was reviewed against the latest known successful native build and the separate #19/#20 PRs. That earlier queue has been superseded by the user's explicit WASM-first instruction. Native compilation is NOT a decision to abandon the still-working C#/WASM path. It tests a new uncertainty: the 2026-10-15 Unity Marksman player binaries were discarded and no live browser instance was established.

One single private native run is now [Actions #38895140830](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38895140830), original known-working Unity-build source baseline `4ed43897973d9fac607b4e57baf2cbe4102acf3d`, new isolated branch `codex/native-marksman-webgl-runtime-validation`, workflow input SHA `7f4f688cba700fe96b5c68c57d71f4d3428fad0c`. Private output retention, proper `.wasm.br` and `.data.br` MIME/encoding, actual Chromium/WebKit `createUnityInstance` resolution, canvas/input observation, screenshots and failure evidence are the explicit gates. See [native experiment checkpoint](https://github.com/ethan-bailey/crisis-response-port/blob/codex/native-marksman-webgl-runtime-validation/docs/NATIVE_MARKSMAN_RUNTIME_VALIDATION_2026-10-16.md).

**Status at the prior native-experiment documentation update: running**; no runtime browser/Unity result should be claimed until the CI job ends. Avoid duplicate Unity CI. This isolated branch is based on the earlier exact tested native commit, and is NOT an edit to independent Unity PR #7, WASM PR #9 gameplay code, or `main`. It is not a full Mission 01 native build and is not physical iPhone Safari acceptance. SDK8 compiler packaging PR #19 and source/UI audit PR #20 remain distinct, unmerged, verified metadata checkpoints. Reconsider any route change only with actual native runtime evidence.


Read this file first, then [WORKSPACE.md](WORKSPACE.md). This replaces the competing “latest” and “next” sections in the [archived handoff](docs/history/NEXT_AGENT_BEFORE_2026-10-16_REVIEW.md). The archive and dated ledger are historical evidence, not instructions to repeat completed work.

**The full game is unfinished.** Preserve the original developer's code, assets and behavior for iPhone Safari. Compilation, recovered data, browser execution, native parity, deployment and physical-device acceptance are separate milestones. Do not report a completion percentage.

## Ownership and starting checks

- Browser work: `codex/projectile-material-impacts`, [PR #9](https://github.com/ethan-bailey/crisis-response-port/pull/9). Review started at `fdce2e51b54b9da08dd6cd3db2828289c16fe11d`; this documentation commit will advance it. Read the actual HEAD before writing.
- Compiler/package tools: `codex/wasm-compile-package`, [PR #19](https://github.com/ethan-bailey/crisis-response-port/pull/19), reviewed head `bd8755a7922f302016dc0aa27b53a0e8f602563e`.
- Source/UI audit: `codex/full-source-closure-audit`, [PR #20](https://github.com/ethan-bailey/crisis-response-port/pull/20), reviewed head `2ee5dd15323f57c6319bed55715054b61796c33e`.
- These PRs are open and unmerged. Tools/docs on #19/#20 are not automatically available on the browser branch. Review their diffs and current mergeability before using or integrating them; mergeability can be transient. A recheck of #19 returned mergeable after an earlier false result.
- Independent Unity work: `codex/iphone-input-parity-foundation`, [PR #7](https://github.com/ethan-bailey/crisis-response-port/pull/7). **Do not edit/reset/merge this branch or main.** Any new native experiment must use a new isolated branch.
- User says no other agent is currently working. Still use SHA-checked writes, inspect active and queued runs before dispatch, and never force-push or reset. No in-progress Actions runs were reported during this review; that is not a promise about the next session.
- Keep original assets and builds private. Do not change visibility. Roll back with selective revert commits after checking later changes.

## Verified checkpoints — scope matters

| Evidence | Exact tested commit / run | What it proves; limits |
| --- | --- | --- |
| Browser gameplay slice | `0933f2d4f5c5047f4e4a7f7c701fb13fd0f47e8c`; [38846445335](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38846445335); artifact `12024020540` | Original C# contracts, WASM/PhysX, Chromium/WebKit movement/touch/camera/weapons/glass and particle-manifest startup passed. Not full game, native particles or iPhone acceptance. |
| Original impact-event contract | `12f292657267d0c0521f00140c4ddec1976b7cd2`; [38847238826](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38847238826); artifact `12023701591` | Isolated original SpawnEntryEffect GUID/phase/time contract. Does not establish live browser shot emissions; a browser scenario previously recorded receivedEvents=0. |
| Original arm-sequence WASM expansion | `256b574eca62a902e23e9ba45d2b2542d64fe445`; [38896212172](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38896212172); site `12043404117`, evidence `12043827958` | 41 original C# inputs, six unchanged new classes; seven prefabs / 21 frames; original composition ran in Chromium/WebKit. Not controller playback. #21 remains unmerged. |
| Earlier SDK 8 complete site package | `bb60e3adf1368c7156b45ac9d5a6385de5841301`; [38849201407](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38849201407) | SDK 8.0.426, selected-source contracts, 38 existing checks and 12 package tests; 724 files, 199 boot resources, 195 WASM binaries. No fresh browser/device test or deployment of this package. |
| All-scene/UI identity audit | `f50b00110ba29f9fffa28046cf78405b1acd66ec`; [38892941728](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38892941728); artifact `12041579699` | 18 tests, five primary hash vectors, 24/24 UI type identities, 160 raw-source claims verified. Metadata/provenance only; no UI runtime behavior. |
| Native Unity Marksman WebGL compile | `4ed43897973d9fac607b4e57baf2cbe4102acf3d`; [38800542438](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38800542438); artifact `12004975378` | Actual Unity/IL2CPP build succeeded, 18 output files / 36,117,455 bytes, static HTTP checks passed. Only logs/manifests retained; compiled player was NOT retained. No browser gameplay/device proof, and only Marksman was built. |

SDK 8 package artifact: `12024269115`; evidence: `12024274148` (expire 2026-11-15). Inner tar SHA256: `935253d36d861cd357f6ac03c4fe551cf5be7c4796749906899087770d6567e9`. Downloaded tar matched CI and independently reverified. It contains /game/ and /runtime-web/ for an origin-root mount; Three.js still uses the existing pinned external CDN imports, so do not call it offline-complete. Earlier SDK-10-selected package run is superseded.

Native logs expire 2026-10-29; UI audit artifact expires 2026-11-15. Preserve needed evidence privately before expiration. Later documentation heads are not new tested game commits.

Owner-private [Site v8](https://crisis-response-runtime.craftysnipe13.chatgpt.site) is older than the latest glass/particle-source changes and SDK 8 package. No new deployment or physical iPhone acceptance is established.

## Current content and blockers

- Browser branch baseline selects **35 original C# files plus 19 engine-adapter files**; the verified unmerged #21 batch selects **41 plus 20**, not the complete recovered game (1,333 main scripts, 86 firstpass scripts and one recovery patch). Mission01 geometry/operator and partial movement, aiming, animation and weapon behavior exist. Full animation coverage, all maps, AI, mission objectives/progression and original UI are unfinished.
- Six scenes have been inventoried: Benchmark, Disclaimer, Marksman range, Mission01, Title, Transition. The source graph indexes 3,512 GUIDs, 1,194 unique reachable assets and 2,412 hashed records. Inventory is not scene loading, complete symbol analysis or gameplay execution.
- UI GUID `f5f67c52d1564df4a8936ccd202a3bd8`: all 24 referenced IDs now map uniquely to original UnityEngine.UI.dll types. Exported metadata is absent and runtime behavior remains unimplemented. Do not repeat the identity search or synthesize original metadata.
- Remaining unidentified external GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID 4300000: .308 mesh referenced by casing and two rounds. Keep missing provenance explicit; an approved substitution is not original recovery. Built-in engine dependencies, dynamic Resources, prefab overrides, local IDs and full source-symbol closure still need work.
- **35 unique reachable shader exports are dummy exports**, 34 reachable from Mission01. The gap extends beyond two window shaders. Recover independently verified shader behavior/native references; do not treat dummy text as original shading.
- Original impact source data is recovered: 14 prefabs, 26 systems/renderers (14 billboard, 12 mesh), three meshes, and **Splinter normals already decoded and verified**. Unity particle simulation/rendering is unfinished. Investigate the real browser event path before renderer implementation; do not force original settings to make effects appear.
- 56 glass quads have adapter rendering; 12 mirrors remain unimplemented. These are not native shader-parity results.

Immutable original recovery: private release `recovery-preservation-2026-10-15`, asset `crisis-response-recovery-direct-v3-1.zip`, 101,082,158 bytes, SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. Verify before extraction. Do not regenerate or overwrite runtime/Original/ casually.

## Execution order — dependencies before features

1. **Establish the exact baseline and review existing work.** Read current refs, diffs, run status and linked handoffs. Review #19 and #20 on their own branches; resolve any real integration conflicts before relying on their tools. Preserve their tested commits/evidence. Do not rerun builds for this documentation edit, merge main, or modify the independent Unity branch.
2. **Review the verified original arm-sequence WASM batch (#21).** The existing run passed native composition contracts, actual SDK-8 WASM publish and Chromium/WebKit calls. Preserve exact source/artifact evidence, review integration into the browser branch, and do not repeat completed tests merely for documentation changes. Composition is an animation prerequisite, not controller playback.
3. **Select the next source dependency group needed for playable progress.** Inspect the original IKArmSequencer/IKArmController/ArmIKSolver and CharacterBody/weapon callback dependencies. Import a bounded executable original-code group with provenance. Do not add empty callbacks, unconditional success flags or stand-in character systems to force compilation. Keep the .NET/PhysX route; no native-Unity wait or engine switch is required.
4. **Close source and engine prerequisites incrementally on the chosen route.** Extend all-scene dependency resolution, dynamic Resources/scene references and source-symbol coverage. Implement only source-required adapters: lifecycle, object/prefab identity, serialization, scene loading, time/input, physics and animation. Add original source in bounded groups. Unresolved mandatory dependencies block the affected feature; audit totals are not full closure.
5. **Restore playable original flows in dependency order.** First scene bootstrap, original player movement/pose/animations and weapons/damage; then navigation/AI, team interactions, doors/grenades/physiology; then mission objectives, outcomes, progression and transitions. Wire original menus/UI callbacks and save/platform adapters to the real systems they invoke, rather than temporary fake actions. Extend across all required scenes. Presentation work can proceed alongside a functioning scenario using original assets and verified native references; preserve explicit shader/particle/audio blockers.
6. **Verify each substantive milestone against matching inputs.** Use cheap local/source/native-contract checks first, then one meaningful WASM/browser run for the changed flow. Capture deterministic input/time/RNG and native comparisons where possible. Test the actual packaged candidate in Chromium and WebKit after packaging; the older browser pass does not validate a new package. Do not rebuild repeatedly for metadata or documentation changes.
7. **Deploy a matching protected candidate, then accept it on iPhone.** Verify owner-only access, source/artifact hashes, base paths, MIME/compression, startup and rollback. An incomplete checkpoint must be labelled as such. Test actual iPhone Safari touch, audio, suspend/resume, memory/performance and complete required mission flows; CI WebKit is not hardware acceptance. Only call the full game finished when the roadmap's behavior, presentation and device gates pass.

Steps 2–3 expand the selected-source WASM runtime around original animation/gameplay dependencies. Native Unity evidence can inform comparisons without blocking this route. If a prerequisite is blocked, record exact evidence and continue only independent work.

## Where to get exact commands and evidence

- [Compiler/package handoff on #19](https://github.com/ethan-bailey/crisis-response-port/blob/codex/wasm-compile-package/docs/WASM_COMPILE_PACKAGE_HANDOFF.md): exact commands, SDK-selection correction, tests and rollback. Its SDK 8 pin is scoped to the new workflow; do not introduce an unsolicited root-wide compiler change.
- [Source-closure handoff on #20](https://github.com/ethan-bailey/crisis-response-port/blob/codex/full-source-closure-audit/docs/FULL_SOURCE_CLOSURE_HANDOFF_2026-10-16.md).
- [Original UI bindings handoff on #20](https://github.com/ethan-bailey/crisis-response-port/blob/codex/full-source-closure-audit/docs/ORIGINAL_UI_BINDINGS_HANDOFF_2026-10-16.md).
- [Native WebGL result review on #20](https://github.com/ethan-bailey/crisis-response-port/blob/codex/full-source-closure-audit/docs/NATIVE_WEBGL_RESULT_REVIEW_2026-10-16.md). These are branch URLs deliberately: files are not yet merged into the browser branch.
- [Complete-game acceptance roadmap](docs/COMPLETE_WASM_PORT_ROADMAP.md): gates, not a mandate to finish every asset before testing a bounded runtime scenario.
- [Chronological evidence ledger](docs/WORKSPACE_STATUS.md) and [historical handoff](docs/history/NEXT_AGENT_BEFORE_2026-10-16_REVIEW.md).

For every checkpoint record exact input SHA, changed paths, original hashes/GUIDs, commands/results, artifact IDs/expiry, remaining limits, next bounded action and selective rollback. On failure report the actual blocker; never replace original gameplay with stand-ins or imply full completion from a green build. Work occurs only during an active session; do not promise offline continuation.
