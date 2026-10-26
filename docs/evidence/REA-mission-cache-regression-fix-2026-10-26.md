# Mission cache regression fix — 2026-10-26

## Acceptance target

The Mission's largest-first bounded delivery must retain the previously verified
device cache behavior: IndexedDB primary storage, CacheStorage fallback,
manifest-bound keys, size/SHA validation of cache hits, recovery of corrupt
records, observable quota failures, storage estimate/persistence progress, and
the version-scoped clear-cache control.

## Live-source finding

The live `develop/wasm` branch was `59cb7b876ad23a4aa6de5008f01fa5401d053fd6`.
Commits `3c17b1115190c0530f9c4112c9d2fcc1fa1ad5a3` and
`59cb7b876ad23a4aa6de5008f01fa5401d053fd6` changed delivery to prioritize
large files, but also removed IndexedDB, cache integrity recovery, quota and
storage reporting, the clear-cache export, and the tests covering those paths.
`game/index.html` still imports and calls `clearIncrementalSourceCache`, so the
removed export also prevented the game module from loading. The associated test
deletions removed acceptance coverage without replacing it with a real-browser
test.

## Implementation

Restored the cache and progress implementation from the previously verified
`cc4042f7dc7fcd42a7ff3e36e7c29e3744b706e5` source tree, then retained the live
largest-first ordering. The bounded six-request fetch groups are now sorted by
compressed size descending with path as a deterministic tie-breaker; bridge
delivery follows that order and validates the complete byte totals. No original
gameplay source was changed.

## Verification

- `node --test scripts/scene_package_loader_test.mjs`: **7/7 passed**, including
  verified cache hits, corrupt-cache recovery, quota failure visibility,
  IndexedDB reuse, version-scoped clearing, and batching.
- `npm ci --prefix game --ignore-scripts && npm run --prefix game vendor`:
  regenerated the tracked-worktree-missing PhysX adapter from its lockfile.
- `node --test scripts/*_test.mjs scripts/*_contracts.mjs`: **128/128 passed**.
- `python3 -m unittest discover -s scripts -p '*test*.py' -v`: **43/43 passed**.
- `python3 scripts/inventory_source_dependencies.py --check`: passed; 810
  sources, 1,710 symbols, 1,384 reachable files, 127 ambiguous names requiring
  ownership review.
- Pinned SDK preflight with `LD_PRELOAD=/tmp/libprocshim.so`:
  `/tmp/codex-dotnet-8.0.426/dotnet --version` printed **8.0.426**.
- `dotnet restore` and `dotnet run` for EngineChecks and FoundationChecks, and
  `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release
  -o /tmp/cpr-cache-fix-publish`: completed. Release publish warns that the
  optional `wasm-tools` workload is not installed; it still emitted the Blazor
  output.
- Initial whole Node-suite attempt failed only because a fresh worktree lacked
  generated `game/vendor/physx-js-webidl.mjs`; the documented lockfile/vendor
  recovery above corrected that runner prerequisite, after which all Node tests
  passed.

## Scope and remaining acceptance

This proves loader contracts and .NET package compilation, not browser IndexedDB
transactions on Safari. Actual fresh/second browser launch, quota/eviction,
interrupted download, manifest invalidation, load timing and memory still need
Chromium/WebKit runs and owner iPhone Safari acceptance. This fix does not close
the broader source/package, authored actor lifecycle, combat, rendering or
end-to-end gates.

Runner availability was checked with `command -v chromium chromium-browser
google-chrome firefox` and a bounded `find` under `/usr`, `/opt`, and
`/root/.cache` for Chrome/Chromium/WebKitWebDriver/Playwright; no browser
binary was present. Browser-backed acceptance is therefore deferred to a
runner with those binaries or the owner's phone, while source and package work
continues. The CUA cloud Chrome surface was also tried against the verified
package served locally on port 8877. Workspace-local `urllib` returned 200 for
`/game/` and the 5,347,727-byte WASM with the expected hash; cloud Chrome
returned `net::ERR_CONNECTION_REFUSED` for `http://127.0.0.1:8877/game/`.
That browser cannot use this runner's loopback, so no interactive result is
claimed. Do not expose/publish the package merely to work around this; use a
browser installed in the workspace or an authorized non-publishing test path.

## Exact pushed commit and package evidence

- GitHub connector commit: `12d61245431b3e3a026fbd82a8199f6e983a8590`, tree
  `7dd6c19db9afcf0d02077a11c5d106744b641b74`, parent `59cb7b8...`.
- Actions run **#22**, ID `40263692936`, completed **success** for exact SHA
  `12d61245431b3e3a026fbd82a8199f6e983a8590`.
- The repo-local initial commit `6d4f45c...` had the same Git tree; package was
  rebuilt after fetching the connector commit so its manifest records exact
  remote SHA `12d612...`.
- `python3 scripts/package_wasm_site.py build --repo . --published
  /tmp/cpr-cache-fix-publish/wwwroot --output
  /tmp/cpr-cache-fix-package-remote --toolchain
  /tmp/cpr-inherit-toolchain.json --incremental-profile-package
  /tmp/cpr-v70-profile/global-ui-mission-operator` completed. Verification:
  `status: verified`, 2,684 files, 199 Blazor resources, 195 WASM files,
  1,919 assets (97,767,549 compressed; 205,514,345 source bytes),
  `sourceCommit=12d612...`.
- HTTP smoke against that package: `/`, `/game/`, `/runtime-web/` returned
  200 `text/html`; `/game/vendor/physx-js-webidl.wasm` returned 200
  `application/wasm`, 5,347,727 bytes, SHA-256
  `7d4696e6273166308ce1f0db5dd5aee96c843b6e3722d96db7e4422aa1fbee1f`.
- GitHub branch subsequently advanced through docs-only handoff commits
  `41c4e535...`, `54d170fc...`, and `a3e8bda...`. The 12d code commit is their
  ancestor; refresh the live branch before the next source change.

## Spark provenance check

The mounted recovered profile has `Assets/Texture2D/Spark.png` metadata with
GUID `43907091bffd53b478c985dca2762559` and source metadata declaring 1,539
bytes and SHA-256 `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344`.
The available exported PNG at
`/tmp/cpr-recovery-b184be9/export/ExportedProject/Assets/Texture2D/Spark.png`
is 1,014 bytes and hashes to `b33ee840f7cc744655e99b138a413f72036a92bd89e237d8ed8cdf412b4a2929`.
It is not byte-identical to the declared source and is not substituted. Search
covered the mounted `/tmp` and workspace for `Spark.png`; no 1,539-byte
matching source was found. Exact raw texture recovery and any claims of
visual identity remain pending the source owner's artifact or archive.

A further authorized recovery attempt downloaded prior GitHub Actions artifact
`11903811738` with `github_download_workflow_artifact`. The archive is
101,082,158 bytes, SHA-256
`cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. Its
`export/ExportedProject/Assets/Texture2D/Spark.png` is the same 1,014-byte
`b33ee840...` exported image; its `.meta` matches SHA-256
`980e0c84ccde638ef5ade55ddc2d9249f20e61aa4c6cfafaa1c25a066b2ad363`. The
artifact does not contain the required original source bytes. Next recovery
input is the original project export/archive or a source owner copy whose
1,539 bytes match the expected SHA; importing the exported 1,014-byte texture
would be a provenance-changing substitute and remains disallowed.

## Next source-backed particle slice

The matching source profile's 14 impact prefabs contain 14 active
`ClampVelocityModule`s in three configurations. All use `separateAxis=0`,
`inWorldSpace=0`, drag scalar 0, magnitude scalar 0, and x/y/z scalar 1, with
dampen values 0.98 (11 systems), 0.99 (1), and 0.65 (2). Unity's module
documentation describes velocity limiting plus damping/drag and the space
selection, but does not resolve the exact Unity 2017.2 serialized field mapping
for this profile. See the [Unity 2018.1 module API](https://docs.unity3d.com/2018.1/Documentation/ScriptReference/ParticleSystem.LimitVelocityOverLifetimeModule.html)
and [Unity 2018.4 Limit Velocity module manual](https://docs.unity3d.com/2018.4/Manual/PartSysLimitVelOverLifeModule.html)
for context. The latter documents that the `Space` choice applies only when
separate axes are enabled. No browser behavior was added from the later API
description.
Next, establish whether zero serialized magnitude means an unbounded limit or a
zero limit in this Unity 2017.2 source/runtime, and verify local-space
transforms and damping order with a native source capture or version-matched
fixture. Then add the smallest contract and runtime implementation for the
observed behavior.

## Reload audio source-path checks

Read the authoritative `runtime/Original/FullSource/WeaponBase.cs` and
`runtime/Original/CharacterBehavior/CharacterBehavior/ChangeMagazine.cs` paths.
Added an audio runtime contract that locks these source behaviors: a null
magazine insert exits before audio; successful insertion invokes the source
magazine event before its insert sound; detachable-magazine extraction is
conditional on magazine ownership; the fire-from-magazine extraction sound
precedes the empty-chamber early return; detachable drop/ownership stays source
controlled; quick/full reload select the original removal/insertion calls; the
internal-magazine path uses `ReturnMagazine`; and task end clears its state
without inventing completion audio. Weapon switching and late-reload fire retain
the original interruption path.

- Focused `node --test scripts/weapon_audio_runtime_test.mjs`: **7/7 passed**.
- Full `node --test scripts/*_test.mjs scripts/*_contracts.mjs`: **129/129
  passed**.
- The first new test attempt asserted `weapon.InsertMagazine(...)`; the original
  code calls `self.ActiveWeapon.InsertMagazine(...)`. Corrected the expectation
  to match source, reran the focused test and full Node suite, both passed.
- This static source contract does not execute reload state transitions or
  prove audible timing on the browser device. Those lifecycle and audio
  acceptance cases remain open. No C# gameplay source was changed.
