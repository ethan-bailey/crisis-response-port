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
  the WASM store sequentially as compressed bytes, reporting compressed/source
  bytes and largest asset. C# verifies both compressed and decompressed size
  and hashes, then keeps the compressed copy. Source assets inflate only when
  the synchronous loader asks for them. SceneLoader and SerializedAssetResolver
  release parsed asset documents after copying/resolving their authored values;
  later access rehydrates them from the verified compressed bytes.
  `?source-profile-smoke` exposes this transfer path without starting actor
  input. It remains opt-in; default page behavior has not changed.

## Measured recovered-source closure

Input `/tmp/cpr-scenes-full-source.json` SHA-256:
`6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`.

| Profile roots | Assets | Asset bytes compressed | Asset JSON bytes | Metadata gzip / JSON | Largest asset JSON / gzip |
|---|---:|---:|---:|---:|---:|
| Global Context + Interior Mode UI + Interaction Menu + Operator | 1,229 | 36,969,948 | 77,801,511 | 675,552 / 4,817,294 | 3,742,886 / 2,822,414 |
| Above plus Mission 01 | 1,918 | 97,766,376 | 205,509,197 | 853,717 / 6,912,437 | 14,858,177 / 3,108,968 |

Every profile asset and metadata document in the generated Global/UI/Operator
bundle was independently decompressed and verified against its manifest.
These payload totals are still large for phone startup. During transfer,
browser JavaScript keeps one compressed asset at a time; WASM keeps the
compressed profile (36,969,948 or 97,766,376 asset bytes) and parsed metadata.
The store inflates one asset at a time (largest observed source asset
3,742,886 or 14,858,177 bytes), validates its source hash, and retains its
compressed bytes. Parsed source documents are released after scene/prefab or
direct-asset resolution and can be rehydrated later. Actual peak browser/WASM
heap was not measured because this environment has no browser runtime.

## Verification

- `python3 -m unittest discover -s scripts -p package_wasm_site_test.py -v` —
  passed 14/14, including generated-profile packaging and full per-file
  integrity verification.
- `node --test scripts/scene_package_loader_test.mjs` — passed 4/4, including
  sequential delivery and invocation order against a fake WASM bridge.
- `python3 -m py_compile scripts/package_incremental_source.py
  scripts/package_wasm_site.py` — passed.
- Pinned .NET Checks build — passed with 0 errors and 48 existing warnings;
  incremental-store contract passed compressed verification, lazy JSON
  hydration, post-release rehydration, same-world scene loading, fail-closed
  handling, and legacy compatibility.
- `--verify-incremental-source-profile` — passed the actual recovered Global/UI/
  Operator profile through the C# store: 1,229 assets and 7,594 records, all
  source hashes verified, parsed documents released after inspection.
- WASM Browser build — passed with 0 errors and 48 existing warnings.
- Default runtime contracts and active Mission UI actor contract passed after
  lazy parsed-document release was added. The Mission UI contract used the
  full monolithic fixture; the profile delivery itself was tested through the
  fake WASM bridge, not a real browser.
- Real browser transfer, runtime heap measurement, and iPhone Safari acceptance
  were not run. Nothing was deployed.

## Next gate

Reduce retained WASM memory for the measured closure or narrow the authored
pre-input dependency set. Then run `?source-profile-smoke` on the target
runtime, load the original Mission UI and actor in one RuntimeWorld, and record
real device memory and pre-input bytes before wiring page movement, PhysX,
rendering, and weapon callbacks through that actor.
