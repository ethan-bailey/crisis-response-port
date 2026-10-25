# Crisis Response Port — current handoff

## Durable checkpoint

- Development branch: `develop/wasm`.
- Current tested source and live branch checkpoint: `859db220925af2d542a62f055f2e647a2622a936` (`feat: package and deliver source dependencies incrementally`).
- This checkout was fetched from `origin/develop/wasm` at that exact SHA and was clean before this handoff update.
- No open pull requests were returned by the GitHub connector at the prior session start; no PR was opened here.
- Site v39 remains the last owner-accepted iPhone Safari build. Nothing was deployed.

## Completed source delivery system

The preceding checkpoint (`dc6843cb778b843900911e258075386af6503585`)
added the verified incremental source store and WASM bridge. This checkpoint
adds `scripts/package_incremental_source.py`, optional incremental profile
packaging and integrity checks in `scripts/package_wasm_site.py`, sequential
browser delivery in `scene-data/scene-package-loader.mjs`, and the opt-in
`?source-profile-smoke` page path. Default startup is unchanged.

Measured from recovered source input SHA-256
`6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`:

- Global Context + Interior Mode UI + Interaction Menu + Operator: 1,229
  assets; 36,969,948 compressed asset bytes; 77,801,511 asset JSON bytes; max
  asset 3,742,886 JSON bytes / 2,822,414 gzip bytes.
- Adding Mission 01: 1,918 assets; 97,766,376 compressed asset bytes;
  205,509,197 asset JSON bytes; max asset 14,858,177 JSON bytes / 3,108,968
  gzip bytes.
- Incremental delivery verifies compressed and decompressed size/hash and
  sends one asset at a time. The WASM store still retains delivered JSON
  documents, so these profiles are not phone-startup ready. Actual browser/WASM
  heap was not measured because no browser runtime is available here.

Details: `docs/evidence/INCREMENTAL_SOURCE_DELIVERY_2026-10-25.md` and
`docs/evidence/INCREMENTAL_SOURCE_PACKAGE_STORE_2026-10-25.md`.

## Verification

On checkpoint `859db220925af2d542a62f055f2e647a2622a936`:

- `python3 -m unittest discover -s scripts -p package_wasm_site_test.py -v` —
  passed 14/14, including profile generation, package inclusion, and per-asset
  integrity verification.
- `node --test scripts/scene_package_loader_test.mjs` — passed 4/4.
- `node --check scene-data/scene-package-loader.mjs` — passed.
- `python3 -m py_compile scripts/package_incremental_source.py
  scripts/package_wasm_site.py` — passed.
- Recovered-source Global/UI/Operator profile: 1,229 assets generated and
  independently verified; adding Mission 01 was measured at 1,918 assets.
- `git diff --check` — passed.

The active Mission UI actor contract last passed on source checkpoint
`dc6843cb778b843900911e258075386af6503585` with input package
`/tmp/cpr-scenes-full-source.json` (same recovered-source hash above). The
subsequent changes at `859db22` affect packaging and page delivery only; the
Mission UI contract was not rerun on this exact SHA. Desktop browser transfer,
peak browser memory, and iPhone Safari acceptance remain open.

## One next action

Reduce retained WASM memory or narrow the pre-input dependency closure based on
the measured 77.8 MB / 205.5 MB profiles. Then run the opt-in profile transfer
in a real browser, measure peak WASM/browser memory and bytes before input, and
exercise the original Mission UI actor in one RuntimeWorld before connecting
page input, PhysX, rendering, and weapon callbacks. Keep default startup and
publication unchanged until that gate passes. Do not deploy without the
owner-authorized release gates.
