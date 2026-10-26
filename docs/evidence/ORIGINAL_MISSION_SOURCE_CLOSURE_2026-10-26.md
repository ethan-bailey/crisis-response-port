# Original Mission source dependency closure — 2026-10-26

## Result

The earlier “original bytes missing” blocker is resolved for the Mission startup roots. The canonical complete Unity export is available as the GitHub Actions artifact `crisis-response-recovery-direct-v3-1`, run `38582072502`, artifact `11903811738`. Artifact metadata lists it as unexpired at 2026-10-26T06:32Z and expiring 2026-10-28T14:38:58Z.

The archive restored at `/workspace/scratch/original-recovery-artifact.zip` is **101,082,158 bytes** with SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. This exactly matches the pinned hash recorded in source-reference provenance. `unzip -t` passed and listed **8,575** entries. The earlier `crisis-response-recovery-direct-v3-1.zip` scratch file with a different digest is corrupt/truncated and is not used.

## Rebuilt full-source index and six-root closure

Using extractor source checkpoint `f06edeffd2f9c88d45b2bf06950cf9f60db8cbae`:

```sh
python3 scripts/extract_runtime_scene.py \
  /tmp/cpr-recovery-full-20261026/export/ExportedProject \
  /tmp/cpr-full-source-regenerated.json
sha256sum /tmp/cpr-full-source-regenerated.json
```

Result: **2,060 assets**, **40,139 records**, **24,856 object references**, **776 binary assets**, **6 index-level unresolved GUIDs**, **251,585,827 bytes**. The regenerated JSON SHA-256 is `657b93c8a385f5f94e337eb9d49f9ee10d7b581f080bf43c13c25cbe04343c88`. This is a newly reproduced index identity; it must not be conflated with older temporary index hashes in prior evidence.

The complete runtime startup closure was regenerated using these six roots:

1. `Assets/Resources/Global context.prefab`
2. `Assets/GameObject/Interior mode UI.prefab`
3. `Assets/Resources/ui/Interaction menu.prefab`
4. `Assets/Resources/characters/Operator.prefab`
5. `Assets/Resources/Version.prefab`
6. `Assets/Scenes/Mission 01.unity`

Command:

```sh
python3 scripts/package_incremental_source.py \
  /tmp/cpr-full-source-regenerated.json \
  /tmp/cpr-mission-profile-exact-f638 \
  --profile global-ui-mission-operator \
  --root 'Assets/Resources/Global context.prefab' \
  --root 'Assets/GameObject/Interior mode UI.prefab' \
  --root 'Assets/Resources/ui/Interaction menu.prefab' \
  --root 'Assets/Resources/characters/Operator.prefab' \
  --root 'Assets/Resources/Version.prefab' \
  --root 'Assets/Scenes/Mission 01.unity'
```

The closure contains **1,919 assets / 22,365 records**, **97,767,549 compressed asset bytes**, **205,514,345 decoded asset bytes**, and no unresolved GUID references among the selected roots. Largest decoded asset: **14,858,177 bytes**. Profile source index identity: `657b93c8a385f5f94e337eb9d49f9ee10d7b581f080bf43c13c25cbe04343c88`. Metadata JSON SHA-256: `757ec05e3fb7802a75f9d00c14087ba36bd97a48cf979bde41b320af0d18b768`; gzip SHA-256: `23a8fd369b6525472990c4761feb54290fbfa98e8e8fbe57357195960b6973d8`; decoded/gzip sizes are 6,912,902 / 849,982 bytes.

A local independent pass decompressed all **1,919/1,919** asset entries and checked compressed and decoded length plus SHA-256 against the manifest; metadata compressed and decoded length/hashes also passed. No Mission-specific .NET contract ran locally because `dotnet` is not installed in this executor.

## CI and package boundary

Workflow commit `f012fa85de9bb7928c78dfc9ae846837a1f2c77c` passed Actions run **#44** (`40281343536`) on 2026-10-26. Contracts/Release WASM job `121435564389` and Chromium IndexedDB job `121435564761` both succeeded. At that exact SHA, CI reported Node **138/138**, Python **44/44**, the Runtime/Engine/Foundation checks, Release WASM output verification, seven Chromium IndexedDB checks, and the full-profile `--mission-ui-incremental-contract`.

The CI source package was **1,919 assets / 22,365 records**, **97,767,549 compressed asset bytes**, **205,514,345 decoded source bytes**, and zero unresolved GUID references in the selected six-root closure. The package verifier reported **2,684 files**, **199 Blazor resources**, **195 WASM files**, and exact source commit `f012fa85de9bb7928c78dfc9ae846837a1f2c77c`.

The retained GitHub artifact `crisis-response-site-package-f012fa85de9bb7928c78dfc9ae846837a1f2c77c`, artifact ID `12632421032`, is **138,552,515 bytes** with API digest `sha256:f54b020670a9207a6752d5b64c64e00a934684731da83351478b4d032835bbf6`. It was downloaded independently; the ZIP SHA matched the API digest. Its inner deterministic `cpr-site-package.tar.gz` SHA-256 is `36e492ed43f71787a16f79f862e87af4fcbc0bdbd403c77a77e212476187340f`. A second independent manifest walk verified **all 2,684 files**, **175,099,212 expanded bytes**, with zero missing files or hash/length mismatches. The matching refreshed source snapshot artifact, ID `12632426021`, is **99,141,041 bytes**, API digest `sha256:f89c27e17123c9bd74d47cf8fcebf541de4d860882e99a30f8f8c4b2127e2809`; the workflow revalidated its checksums and canonical original-archive provenance.

Run #44 predates the new `Chunk`/`LowPolySphere` decoded mesh assets and their CI regeneration comparison. This run therefore establishes the full-source/package baseline at `f012fa8`; the next commit will run all updated tests and recreate those mesh JSON files from the selected artifact before package verification. It does **not** establish full gameplay parity, WebGL browser gameplay, cold/warm phone memory behavior, iPhone Safari acceptance, or the v71 Site-source crosswalk. No Site was deployed.

## Next dependency batch

1. Push the hash-locked `Chunk` and `LowPolySphere` source geometry, revised lifecycle assertions, dependency-version reconciliation, and CI regeneration comparison. Confirm the exact pushed-SHA Actions run passes before treating these files as build inputs.
2. Keep source-variant matching explicit. All 17 particle materials, textures and meshes are present by path/GUID in the complete export, but all 17 byte hashes differ from the historical particle inventory. The new snapshot's files cannot be silently substituted into the older runtime maps; reconcile that version boundary before rendering them.
3. Close renderer lifecycle for generic Wound `Emit`/`Simulate`/`Play` source commands, and add source-hash-bound Blood material/texture records. Keep actual visual acceptance open until the same-version source mapping and native/browser comparison pass.
4. Continue the source inventory through tracer LineRenderer material, tracer owners/projectile hooks, other original actors and scenes, then batch the related runtime implementation. These owner prefabs are in the full six-root profile, so inventory from that profile rather than treating them as absent.
5. Keep `develop/wasm` source and the full package committed/pushed and reproducible. Do not publish a Site build without the exact Site source crosswalk, package verification, and owner-authorized release.
