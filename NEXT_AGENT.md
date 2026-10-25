# Crisis Response Port — current handoff

## Durable checkpoint

- Development branch: `develop/wasm`.
- Exact tested source and live branch head: `6a091b76b4c12dda55c662d5c16fd8a357a97450` (`feat: retain compressed incremental source assets`).
- This checkout matches `origin/develop/wasm` at that SHA and was clean after the push.
- No deployment was made. Site v39 remains the last owner-accepted iPhone Safari build.
- This was committed directly to `develop/wasm`; no PR was opened.

## Completed major system: compressed incremental source store

The incremental source pipeline now transfers each gzip asset as bytes from the
browser into WASM. The manifest and metadata carry raw and compressed sizes and
SHA-256 values. C# checks both representations, limits a source asset to 32 MiB
and its compressed transfer to 25 MiB, and stores the verified compressed copy.
The synchronous scene and prefab loaders hydrate JSON on demand, then release
parsed documents after use; later reads can rehydrate from the retained gzip.
The default page startup is unchanged. The opt-in `?source-profile-smoke` route
exercises profile delivery without starting actor input.

Evidence and profile measurements:

- [`docs/evidence/INCREMENTAL_SOURCE_DELIVERY_2026-10-25.md`](docs/evidence/INCREMENTAL_SOURCE_DELIVERY_2026-10-25.md)
- Recovered input SHA-256: `6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`.
- Global Context + Interior Mode UI + Interaction Menu + Operator: 1,229
  assets; 36,969,948 compressed asset bytes and 77,801,511 raw JSON bytes.
- Adding Mission 01: 1,918 assets; 97,766,376 compressed asset bytes and
  205,509,197 raw JSON bytes.
- These closures still need a real target-device heap measurement before phone
  startup is accepted. Desktop browser automation and Playwright were not
  available in this workspace.

## Verification at `6a091b76b4c12dda55c662d5c16fd8a357a97450`

- Pinned .NET SDK 8.0.426 Checks build — passed, 0 errors and 48 existing
  warnings.
- Checks default runtime contracts — passed.
- `--source-package-store-contract` — passed compressed validation, duplicate
  retry, lazy hydration/release, scene load in the same RuntimeWorld,
  fail-closed hash/missing-asset behavior, and legacy compatibility.
- `--verify-incremental-source-profile /tmp/cpr-incremental-compressed-profile-v2`
  — passed all 1,229 profile assets and 7,594 records; parsed documents were
  released after inspection.
- `--mission-ui-contract /tmp/cpr-scenes-full-source.json` — passed startup and
  the FullSource actor snapshot, EntryTeam UI initialization, and original left
  press/release. This used the monolithic fixture; the incremental profile was
  independently verified through the C# store and fake browser bridge, not
  loaded as a Mission UI in a real browser.
- Browser WASM project build — passed, 0 errors and 48 existing warnings.
- `node --test scripts/scene_package_loader_test.mjs` — passed 4/4;
  `node --check scene-data/scene-package-loader.mjs` — passed.
- `python3 -m unittest discover -s scripts -p package_wasm_site_test.py -v` —
  passed 14/14; package generator Python compilation and `git diff --check`
  passed.
- Real browser transfer, peak browser/WASM heap, iPhone Safari acceptance, and
  deployment were not performed.

## One next action

Run the opt-in source-profile smoke on iPhone Safari with the measured
Global/UI/Operator profile; record transfer completion and peak memory. If it
fits, load the original Mission UI and actor through the incremental store in
the same RuntimeWorld before wiring page movement, PhysX, rendering, and weapon
callbacks through that actor. Keep publication unchanged until the owner’s
device acceptance gate passes.
