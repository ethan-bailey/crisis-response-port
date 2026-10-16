# Original source closure continuation — 2026-10-16

## Why this step

The existing selected-source WASM slice compiles: exact SDK 8 checkpoint
`bb60e3adf1368c7156b45ac9d5a6385de5841301`, successful run `38849201407`,
private complete site artifact `12024269115`. Packaging review is draft PR #19.
Repeating that compile would not add missing game classes or scenes.

This work advances the dependency inventory required by roadmap Gates 0–2 and
issue #14. Baseline browser branch SHA:
`da842fb9b9f9457ff41d040bcd5874c881dca8f2`. Isolated branch:
`codex/full-source-closure-audit`. Original C#, runtime/renderer/gameplay,
independent Unity branch, main and hosting stay unchanged.

## Files and exact boundaries

- `scripts/audit_wasm_source_closure.py`: standard-library, read-only audit of
  **every recovered .unity scene**, recursive external serialized GUID/fileID
  reference tuples, reachable asset hashes, dummy shader flags, opaque graphs,
  missing metadata/assets and ambiguous GUIDs. A `.meta` without its file is not
  resolved. Unity built-ins stay explicit engine dependencies, including fileID.
  Unresolved Unity UI package GUIDs are not silently suppressed.
- The same report inventories all recovered C# source hashes, the current
  `runtime/Original` and `runtime/Engine` compile source files, and verifies raw
  original source hash claims recorded by the existing presentation, weapon,
  ballistics and base source-provenance manifests. Method hashes are **not**
  independently re-extracted; this is not a blanket source-body parity check.
- Lexical candidates identify source paths/lines involving native plugins,
  Windows registry, legacy networking, threading, filesystem namespaces,
  dynamic Resources loads, scene transitions, particles and navigation. These
  can match comments/strings; confirm code before implementing adapters.
- `scripts/audit_wasm_source_closure_test.py`: ten synthetic reference/provenance
  tests including cycles, all-scene enumeration, read-only operation, ambiguous
  GUIDs, missing source assets, dummy shader/binary graph boundaries, original
  hash drift, path escape and UTF-8 BOM handling. Fixtures are not game systems.
- `.github/workflows/audit-full-wasm-source-closure.yml`: one scoped private
  metadata audit; no .NET/Unity build, browser installation or deployment.
  Verifies preserved archive SHA256 before extraction and uploads metadata only.

Original input: private release `recovery-preservation-2026-10-15`, asset
`crisis-response-recovery-direct-v3-1.zip` (101,082,158 bytes), SHA256
`cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
Only `export/ExportedProject/Assets/` and `ProjectSettings/` are read.

**This audit cannot establish complete closure:** local fileID resolution,
prefab override semantics, binary asset dependencies, runtime Resources strings,
C# symbol resolution, Unity engine behavior, actual native-vs-WASM comparison
and device execution remain separate gates. Missing dependencies are reported,
not fabricated. A successful audit means the inventory ran with valid original
source hashes; it does not mean the unresolved-reference count is zero.

## Validation and reproduction

```sh
python3 -m unittest discover -s scripts -p audit_wasm_source_closure_test.py -v
python3 scripts/audit_wasm_source_closure.py <recovered-ExportedProject> --repo . --output <new-audit-directory>
```

Local targeted tests: **10/10 PASS**. Cloud original-data evidence and next
bounded implementation priorities will be appended after the audit finishes.
`source-closure.json` is the complete machine-readable graph/hash inventory;
`source-closure.md` is its compact summary. The CLI fails on stale/missing raw
source hashes; graph limitations remain explicit in the JSON scope object.

## Rollback and continuation

This branch adds only this document, one audit script, its tests and one scoped
workflow. Selectively remove those additions in new commits after checking
later dependencies; never reset shared branches or modify original recovery.
The verified SDK-8 compiled archive and original private live Site v8 remain
separate checkpoints. No new playable deployment is claimed by this source audit.

## First preserved-original run and parser correction

First metadata run `38889154540`, SHA `ba1f5d11806f99596b4811eb6884297a5f067ccb`,
failed closed on 11 weapon source claims. Evidence artifact `12040548645`.
The original files were not damaged: that manifest stores **bare filenames**,
unlike the other manifests' project-relative paths. The initial audit wrongly
looked for them at project root. It still inventoried all six scenes and
verified the other 149 raw source claims. The correction resolves a bare name
only when unique among recovered C# files, then checks the exact SHA256; an
ambiguous name still fails. A new regression covers both unique and ambiguous
names. No expected original hash was changed.

The exact 101,082,158-byte original archive was also brought into this workspace
and SHA256 verified against the preserved release before extraction. All 11
weapon source claims were independently matched locally to unique original
files and verified. Recovered source count is **1,333 Assembly-CSharp files**,
**86 Assembly-CSharp-firstpass files** and **one AssetRipper patch**, not a
claim that all 1,420 are the game's own code or are compiled into WASM.

## Verified original-data checkpoint

- [Corrected source audit run 38889549595](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38889549595)
  **SUCCESS** at code/workflow SHA `3ceeb1ea99a258453fb7212b106e67e510266b6d`.
- [Private metadata artifact 12040833766](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38889549595/artifacts/12040833766),
  `original-six-scene-source-closure`, contains the complete JSON graph, compact
  report, archive verification, input commit and **10/10** passing targeted tests.
  Current expiry: November 15, 2026. Scripts remain in git to reproduce it.
- **160/160 raw source hash claims verified**, covering 152 distinct original
  source/assets/settings paths; **zero** original hash mismatches and zero
  ambiguous GUIDs among **3,512** indexed GUIDs.
- **1,194** unique scene-reachable assets; full inventory contains **2,412**
  hashed asset/script records, including 1,333 main game C# source files,
  86 first-pass files and one exporter patch. The current runtime includes
  35 selected original C# files and 19 engine adapter files. These are file
  inventories, not a completion percentage or proof of whole-class execution.
- Exact complete `source-closure.json` SHA256:
  `0790ea1312ae7d88cebb377dcd155cf83dd0bd78f0783ded78d366b5ecd3ca6a`.
- Independent local reproduction used the SHA-verified original archive, all
  54 current C# source inputs fetched at the baseline SHA and all four identical
  provenance manifests. Every input hash was checked first. The complete local
  report matched the downloaded cloud report **byte for byte**, including the
  SHA above. No additional cloud audit/build is needed for documentation updates.

| Scene | Reached assets | Unresolved links | Built-in links | Dummy shader exports |
| --- | ---: | ---: | ---: | ---: |
| Benchmark | 432 | 128 | 81 | 19 |
| Disclaimer | 5 | 6 | 1 | 0 |
| Marksman range | 417 | 123 | 79 | 14 |
| Mission 01 | 960 | 158 | 169 | 34 |
| Title | 262 | 164 | 52 | 2 |
| Transition | 5 | 7 | 1 | 0 |

Each link is a source-asset/GUID/fileID tuple, not an individual repeated field
occurrence. Assets are shared between scenes, so do not sum columns as unique
asset totals. No opaque serialized YAML graphs were encountered in this scan;
that does not resolve binary assets' own hidden dependencies.

## Concrete blockers and next bounded steps

1. **Unity UI script identity:** GUID `f5f67c52d1564df4a8936ccd202a3bd8` appears
   with **24 distinct component fileIDs** and has no exported `.meta` asset.
   The original archive *does* preserve
   `export/AuxiliaryFiles/GameAssemblies/UnityEngine.UI.dll` (241,152 bytes,
   SHA256 `30cf854b3db333ba8a3ce47c2091ea11387509861654d8209c1879f7bcb27eeb`).
   Next derive the exact native MonoScript/assembly type bindings from the
   original assembly/exporter rules before importing UI component behavior.
   Do not suppress those IDs or create replacement menu/UI behavior.
2. **Missing .308 casing mesh:** GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`,
   fileID `4300000`, is the only other unresolved external GUID. It is referenced
   by `Assets/Resources/weapons/ammunition/casings/Casing .308.prefab` and the
   two `ammunition/marksman/Marksman round .308 Winchester FMJ.prefab` and
   `Marksman round .308 Winchester SP.prefab` assets. There is no matching
   recovered `.meta`; preserve this missing-source flag until original geometry
   is found or an explicitly approved substitution is documented separately.
3. **Real shader behavior:** **35 unique reachable shader exports** carry the
   `DummyShaderTextExporter` marker, including 34 in Mission 01's graph. The
   blocker extends beyond the two previously audited window shaders to original
   materials and post effects. Copying these dummy sources cannot prove parity.
4. **Full C# and dynamic resource closure:** lexical candidates include 40
   Resources calls, 64 navigation references, 208 particle references, nine
   scene-transition calls, 22 filesystem namespace sites, two registry sites,
   two threading sites and two legacy-networking sites. Full paths/lines are
   in the JSON. Resolve mandatory source symbol/lifecycle dependencies before
   adding original classes to the current compile; do not use fake stubs to
   turn an incomplete game into a green compile.

The two missing external GUIDs comprise **25 distinct GUID/fileID pairs**.
Built-in engine links remain separate, explicitly unimplemented dependencies.
The native game assembly is also preserved as
`export/AuxiliaryFiles/GameAssemblies/Assembly-CSharp.dll` (1,482,240 bytes,
SHA256 `f5b73879f976e082ce91c238fd3443675ecc18dda00e51e6046b75303656a0cd`).
Use original metadata/reference behavior, not guessed source rewrites.

This is a verified **inventory milestone**, not a new playable build or full
six-scene source closure. Package PR #19 and its SDK-8 game archive remain the
latest actual compile checkpoint. No .NET/Unity/browser rebuild, private-site
deployment, main merge or edit to recovered gameplay occurred in this session.
