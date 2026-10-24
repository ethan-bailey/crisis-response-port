# FullSource browser dependency closure batch — 2026-10-24

## Scope and result

The complete recovered archive was materialized from the private Library and
verified against SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
`scripts/extract_runtime_scene.py` regenerated the FullSource package at
`/tmp/cpr-scenes-full-source.json` (SHA-256
`6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`).

The roots are Global Context, Mission 01, Operator, M4A1, 5.56-FMJ, Glock-18,
and 9-FMJ. The transitive authored package-reference closure contains **1,918
assets and 22,361 records**, with zero missing roots or package-resolved
references. Thirty-nine assets have unresolved or unsupported references;
these are preserved per asset in the generated index. This closure is too
broad to imply that every component is executable or appropriate for a
browser payload.

The reproducible indexer is `scripts/index_fullsource_runtime_closure.mjs`.
Run it with the generated FullSource package and save the JSON index outside
the browser payload. Its `sourcePackageSha256` binds the index to the exact
input. The full generated index used in this pass is
`/tmp/cpr-fullsource-browser-closure.json` and is workspace-local.

## Combined application checks

- `--prefab-lifecycle /tmp/cpr-scenes-full-source.json` — passed for original
  Operator and Occupant; 134/132 active behaviors reached Awake, OnEnable, and
  Start. The Operator's original two-arm Unarmed transition reached its
  authored Pelvis target and completed.
- `--mission-contract /tmp/cpr-scenes-full-source.json` — passed in one
  RuntimeWorld batch: Global Context and Mission 01 initialized, with 2
  Operators and 5 Occupants. The importer reported unsupported physics,
  rendering, and presentation components/fields; source startup is not browser
  rendering or physics acceptance.
- `--spawner-contract /tmp/cpr-scenes-full-source.json` — passed original
  Operator/Occupant lifecycle, M4A1/Glock ownership, original IK
  holster/activate/fire callbacks, directional aim/turn, movement/posture,
  independent physiology, unconscious/ragdoll/death timing, and PhysX command
  dispatch. Recorded force dispatch is not proof of a native physics solve.

## Decision and next integration boundary

The test supports full-closure indexing before integration and applying related
source dependencies in one headless batch. It does not justify sending all
1,918 assets to the browser. The normal page still uses its browser-side
movement/animation and transport `WeaponHost` boundary. Next, define a compact
authored actor bridge payload from the indexed closure, then make the
FullSource Operator the single gameplay owner and project its resulting poses
to rendering/physics. Keep unresolved/unsupported references explicit; do not
silently replace them. Browser acceptance remains separate.
