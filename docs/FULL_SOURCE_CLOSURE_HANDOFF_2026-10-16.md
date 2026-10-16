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
