# Original glass-window geometry and texture transport — 2026-10-15

## Provenance and boundaries

Working branch: `codex/projectile-material-impacts`, draft PR #9. Prior fully verified WASM/browser checkpoint: source/workflow commit `455a017cc2900bace19a3eb47cf0276aa71db8cf`; native build run `38803362416`, full browser run `38803647712`. The independent Unity WebGL branch and `main` are untouched. **No edits were made to the recovered `runtime/Original/` algorithms.**

Two read-only GitHub Actions audits were run against the private original recovery archive:

- [Original window component/mesh audit, run 38808368685](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38808368685), private metadata artifact `12007372045`.
- [Source glass/mirror material and shader audit, run 38808663563](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38808663563), exact source-scene SHA256 `c43b36c415b4df14ef6223eaced686a8ddcead6c46f7504346aecb1a03c5743e`. Read the run artifact for complete per-window provenance.

**Confirmed:** 68 original Mission 01 `BallisticWindow` components; each owns one `MeshFilter` with Unity built-in `Quad` (`fileID 10210`, GUID `0000000000000000e000000000000000`), one MeshRenderer and one BoxCollider. The scene uses 56 `Window glass.mat` references (`e064a5da6daba5b48993500f56ce5f51`) and 12 `Window mirror glass.mat` references (`1ad2ae92a0af3fe4db5c98def8d33295`). The regular glass material has custom shader GUID `c5c4e0da9b5e8fa4cb7c2f0db89a9c36`, queue 3000 and serialized `_Opacity=0.25`; mirror glass uses custom shader GUID `fb32b7a5134a45a41997e05b8b5dc9e2`, queue 2000 and serialized `_Opacity=0.22`. Neither proprietary shader has been ported.

## Implemented integration (not shader parity)

- `scripts/audit_original_window_visuals.py`: source-only inventory of component identities, hierarchy, original mesh refs, material shader settings, and transforms. It creates **no** replacement mesh or effects.
- `.github/workflows/audit-original-window-visuals.yml`: scoped-on-itself workflow to preserve the audit privately, never a per-commit expensive build.
- `runtime/Engine/BallisticRuntimeHost.cs`: exposes `WindowVisuals()` through the existing recovered scene graph. It serializes exact per-window Unity world matrices (including nested nonuniform scale/shear), original source object/component IDs, source texture GUIDs, and active state. It forwards original `Window.Reset()` texture replacements as `windowReset` events and leaves `windowStamp` from `BallisticWindow.StampAlpha` untouched.
- `runtime/Browser/Program.cs`: read-only `OriginalWindowVisuals` WASM bridge.
- `game/original-window-visuals.mjs`: validates original scene SHA and unique Unity Quad IDs, constructs one 1×1 source-geometry Quad for each original regular-glass window, converts source world matrices into Three.js coordinates without lossy decomposition, and creates per-pane GPU textures using the exact recovered original `Texture2D` RGBA pixels. Source `windowStamp` applies original per-pixel patches with bottom-row-first ordering; `windowReset` restores original pixels. Mirror shaders are intentionally omitted rather than invented. Renderer material `MeshBasicMaterial` implements the source `_Opacity=0.25` as a **browser adapter**, **not** the unrecovered custom shader.
- `game/weapon-runtime.mjs`, `game/presentation.mjs`, `game/index.html`: connect original window effects to the visual scene and expose diagnostics.
- `runtime/Checks/BallisticContracts.cs`: asserts exact audit hash/68 windows, world matrix translations against original live Transform, and reset events.
- `scripts/weapon_smoke.mjs`: tests 56 visible regular-glass quads, 12 skipped mirrors, source matrix conversion, pixel stamping, reset, disposal in Chromium/WebKit.
- `.github/workflows/game-movement-smoke.yml`: one explicitly triggered complete native/WASM and browser verification run for the milestone.

**Limitations:** Custom Unity window shaders (transparent regular glass, mirrored glass), exact reflection/refraction/lighting, composition against the original combined gray environment mesh, moving-window parent matrices, and final visible glass-hole occlusion have **not** been verified for 1:1 visual parity. The source alpha collisions remain authoritative in C#, and the overlays must not be mistaken for full original game restoration. There are also still unported particle systems, fragments, dynamic impulses, full physiology, and two unresolved built-in mesh colliders. Actual iPhone Safari validation is still required.

## Verification, rollback and publication

The one-shot integrated [run 38809439277](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38809439277) was launched at checkpoint code/workflow commit `ed3d80d89f5e4e797e0370d9152bcc828bd8becb`. **VERIFIED SUCCESS, October 15, 2026 21:30 UTC.** All native C# contracts, complete .NET/PhysX WASM game build, landscape/touch input, source scene/character movement, camera and arm presentation, and Chromium/WebKit M4 plus original window visualization regressions passed. The final weapon-smoke logs explicitly report `[{"engine":"chromium","passed":true},{"engine":"webkit","passed":true}]`. Private artifact **`12007339085`** (`crisis-response-mission01-game-smoke`, about 28.9 MB) includes compiled WASM game runtime and browser screenshots. The exact tested code/workflow SHA is `ed3d80d89f5e4e797e0370d9152bcc828bd8becb`. Documentation-only edits after the tested code do not change that result; later code edits require fresh checks. No physical iPhone device/browser parity proof or new owner-private Site publication occurred. Ordinary documentation/source commits do not re-trigger this expensive workflow; only workflow-file changes do.

Selectively reverse only this window milestone; do not force-push or reset the unrelated work. Baseline before these commits: `13f22b4286e7f13a30ced752b96ab4ad15ff735f`. Revert in reverse order after reviewing any later edits, or restore only these touched files with a new rollback commit:

```text
scripts/audit_original_window_visuals.py        [added]
.github/workflows/audit-original-window-visuals.yml [added]
runtime/Engine/BallisticRuntimeHost.cs
runtime/Browser/Program.cs
game/original-window-visuals.mjs                [added]
game/weapon-runtime.mjs
game/presentation.mjs
game/index.html
runtime/Checks/BallisticContracts.cs
scripts/weapon_smoke.mjs
.github/workflows/game-movement-smoke.yml
docs/WASM_ORIGINAL_WINDOW_VISUALS_2026-10-15.md [added]
NEXT_AGENT.md (handoff)
```

Undo the C# bridge and its JS consumer **together**; do not revert the earlier source-time, native material, original Wood exit-hole, or game-movement fixes. Existing owner-private live Site version 8 remains unchanged until a separately verified private deployment.
