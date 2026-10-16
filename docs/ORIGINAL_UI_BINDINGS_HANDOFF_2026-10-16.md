# Original UI component identities — 2026-10-16

## What is now identified

The 24 previously unidentified script fileIDs under original GUID
`f5f67c52d1564df4a8936ccd202a3bd8` all match **unique actual TypeDef entries**
in the preserved original `UnityEngine.UI.dll`. Original scene/prefab YAML,
GUIDs, fileIDs, recovered gameplay, renderer and runtime code are unchanged.

**This is type-identity recovery, not finished UI or finished game features.**
Menus, layout/event execution, full animations, all maps, AI, missions and
progression remain unfinished. The runtime still selects 35 original source
files; latest actual compile remains package PR #19 / run `38849201407`.

The dependency audit now records these links separately as
`originalAssemblyTypeIdentified`, with `exportedMetaPresent=false` and
`runtimeBehaviorImplemented=false`. It does not suppress them or pretend
that a browser UI implementation exists. The remaining unidentified external
GUID is the known .308 casing mesh; built-in engines and other gates remain.

## Original inputs and reference rules

- Immutable archive SHA256:
  `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
- Original assembly path inside that archive:
  `export/AuxiliaryFiles/GameAssemblies/UnityEngine.UI.dll`.
- Original assembly: **241,152 bytes**, SHA256
  `30cf854b3db333ba8a3ce47c2091ea11387509861654d8209c1879f7bcb27eeb`.
- Base six-scene report SHA256:
  `0790ea1312ae7d88cebb377dcd155cf83dd0bd78f0783ded78d366b5ecd3ca6a`.
- Type names/namespaces and metadata tokens are read from **187 original
  TypeDef rows**, not inferred from UI fields or a list of guessed controls.
- FileIDs use UTF-8 namespace/name bytes, the four-byte MonoScript prefix,
  MD4 and signed little-endian Int32. The helper validates all five independent
  [AssetRipper reference vectors](https://github.com/AssetRipper/AssetRipper/blob/beeb50ee1c223e856c860caa338e5309404956a3/Source/AssetRipper.Tests/ScriptHashingTests.cs)
  before matching original scene references. [Pinned algorithm source](https://github.com/AssetRipper/AssetRipper/blob/beeb50ee1c223e856c860caa338e5309404956a3/Source/AssetRipper.Export.UnityProjects/Scripts/ScriptHashing.cs).
- The binary reader follows [ECMA-335 Partition II metadata layout](https://ecma-international.org/publications-and-standards/standards/ecma-335/).
  It checks file/section/heap bounds and never loads or executes the DLL.
  OpenSSL's legacy MD4 provider performs the identity hash; no third-party
  Python package or invented cryptographic implementation is added.
- The GUID is observed in original serialized references. It is **not**
  recomputed as MD5 of the DLL filename; Unity extension GUIDs are not that
  exporter fallback. All 24 required IDs uniquely match the pinned assembly.

## Source-derived binding table

| Original fileID | Original type |
| ---: | --- |
| -2095666955 | UnityEngine.UI.GridLayoutGroup |
| -2061169968 | UnityEngine.UI.Scrollbar |
| -1862395651 | UnityEngine.EventSystems.EventTrigger |
| -1254083943 | UnityEngine.UI.AspectRatioFitter |
| -1200242548 | UnityEngine.UI.Mask |
| -1184210157 | UnityEngine.UI.ToggleGroup |
| -765806418 | UnityEngine.UI.Image |
| -619905303 | UnityEngine.EventSystems.EventSystem |
| -405508275 | UnityEngine.UI.HorizontalLayoutGroup |
| -113659843 | UnityEngine.UI.Slider |
| -98529514 | UnityEngine.UI.RawImage |
| 575553740 | UnityEngine.UI.InputField |
| 708705254 | UnityEngine.UI.Text |
| 853051423 | UnityEngine.UI.Dropdown |
| 1077351063 | UnityEngine.EventSystems.StandaloneInputModule |
| 1297475563 | UnityEngine.UI.VerticalLayoutGroup |
| 1301386320 | UnityEngine.UI.GraphicRaycaster |
| 1367256648 | UnityEngine.UI.ScrollRect |
| 1392445389 | UnityEngine.UI.Button |
| 1679637790 | UnityEngine.UI.LayoutElement |
| 1741964061 | UnityEngine.UI.ContentSizeFitter |
| 1980459831 | UnityEngine.UI.CanvasScaler |
| 1997211142 | UnityEngine.EventSystems.TouchInputModule |
| 2109663825 | UnityEngine.UI.Toggle |

## Implementation and checks

- `scripts/original_ui_bindings.py`: verified-archive extraction, bounded PE/CLI
  metadata reader, reference-vector verification, unique file-ID/type matching,
  identity manifest and report annotation. Rejects archive/DLL hash mismatch,
  unknown IDs, ambiguous matches, malformed metadata and nested-type guesses.
- `scripts/original_ui_bindings_test.py`: eight tests covering PE32/PE32+, heap
  widths, truncation, heap escape, nested types, five independent reference
  vectors, explicit unsupported UI state and unchanged non-UI references.
- `scripts/audit_wasm_source_closure.py`: optional `--recovery-archive` enables
  this source-backed identity pass and saves `original-ui-bindings.json` plus
  the complete annotated scene graph. The existing raw audit still works.
- Existing scoped source-audit workflow adds the eight tests and checks all
  24 identities against the immutable original archive. No .NET/Unity/browser
  game compile or deployment is triggered by this metadata-only milestone.

Local targeted tests: **18/18 PASS** (ten prior graph tests plus eight new
metadata/binding tests). Actual-original local integration: **24/24** unique
type identities, **160/160** raw source hashes verified, one unidentified
external GUID remaining. Original UI links remain explicit dependencies:

| Scene | Unidentified links | Identified UI assembly links |
| --- | ---: | ---: |
| Benchmark | 3 | 125 |
| Disclaimer | 0 | 6 |
| Marksman range | 3 | 120 |
| Mission 01 | 3 | 155 |
| Title | 0 | 164 |
| Transition | 0 | 7 |

Counts are source-asset/GUID/fileID tuples, not individual repeated fields.
An empty unidentified-link column does not mean the scene is implemented.
Cloud evidence will be appended after the original-data check completes.

## Reproduction

```sh
python3 -m unittest discover -s scripts -p audit_wasm_source_closure_test.py -v
python3 -m unittest discover -s scripts -p original_ui_bindings_test.py -v
python3 scripts/audit_wasm_source_closure.py <ExportedProject> --repo . --recovery-archive <verified-original.zip> --output <new-report-directory>
```

The helper can also read an existing unannotated `source-closure.json`:

```sh
python3 scripts/original_ui_bindings.py <verified-original.zip> <raw-source-closure.json> <bindings-output.json>
```

## Next source-preserving work and rollback

Use these identities to extract each original UI component's serialized fields,
RectTransform ancestry, Canvas settings, persistent event targets and original
callback methods. Then resolve original lifecycle/layout/event dependencies
before extending C# compilation and browser execution. Compare behavior against
native Unity; no replacement menus or fabricated button callbacks. Merely
referencing the native DLL in the WASM project would not implement its required
Unity engine calls and must not be used to imply a working UI.

Undo only the new helper/tests and optional audit/workflow/doc changes in new
selective commits after checking newer dependencies. Preserve prior raw graph,
original archives, source hashes, compiled package and gameplay. No original
asset mutation or hosting rollback is involved.
