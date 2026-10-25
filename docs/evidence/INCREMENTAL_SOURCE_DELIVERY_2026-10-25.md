# Incremental source delivery profile — 2026-10-25

## Delivered

- `scripts/package_incremental_source.py` walks the original package reference
  index from an explicit set of root assets, preserves the package metadata and
  source asset records, and emits one gzip JSON file per asset plus a hash and
  byte-count manifest.
- `scripts/package_wasm_site.py build` accepts an optional
  `--incremental-source-package` and includes the generated profile in its
  integrity manifest. Site verification checks the compressed and decoded
  size and SHA-256 for metadata and every asset. Existing builds remain
  unchanged when this option is absent.
- `scene-data/scene-package-loader.mjs` verifies and delivers the profile to
  the WASM store sequentially, reporting compressed/source bytes and largest
  asset. `?source-profile-smoke` exposes this transfer path without starting
  actor input. It remains opt-in; default page behavior has not changed.

## Measured recovered-source closure

Input `/tmp/cpr-scenes-full-source.json` SHA-256:
`6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`.

| Profile roots | Assets | Asset bytes compressed | Asset JSON bytes | Metadata gzip / JSON | Largest asset JSON / gzip |
|---|---:|---:|---:|---:|---:|
| Global Context + Interior Mode UI + Interaction Menu + Operator | 1,229 | 36,969,948 | 77,801,511 | 620,503 / 4,682,799 | 3,742,886 / 2,822,414 |
| Above plus Mission 01 | 1,918 | 97,766,376 | 205,509,197 | 767,191 / 6,702,550 | 14,858,177 / 3,108,968 |

Every profile asset and metadata document in the generated Global/UI/Operator
bundle was independently decompressed and verified against its manifest.
These totals are still too large to describe as phone-startup ready. Transfer
is sequential, so the browser JavaScript layer handles only one compressed and
decompressed asset at a time, but the current WASM store retains the delivered
JSON documents. Actual peak browser/WASM heap was not measured because this
environment has no browser runtime.

## Verification

- `python3 -m unittest discover -s scripts -p package_wasm_site_test.py -v` —
  passed 14/14, including generated-profile packaging and full per-file
  integrity verification.
- `node --test scripts/scene_package_loader_test.mjs` — passed 4/4, including
  sequential delivery and invocation order against a fake WASM bridge.
- `python3 -m py_compile scripts/package_incremental_source.py
  scripts/package_wasm_site.py` — passed.
- Real browser transfer, runtime heap measurement, and iPhone Safari acceptance
  were not run. Nothing was deployed.

## Next gate

Reduce retained WASM memory for the measured closure or narrow the authored
pre-input dependency set. Then run `?source-profile-smoke` on the target
runtime, load the original Mission UI and actor in one RuntimeWorld, and record
real device memory and pre-input bytes before wiring page movement, PhysX,
rendering, and weapon callbacks through that actor.
