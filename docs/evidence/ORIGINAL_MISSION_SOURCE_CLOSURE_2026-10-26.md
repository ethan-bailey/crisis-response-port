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

## CI and release boundary

On 2026-10-26, workflow source commit `f638ac74ca3a9fa58cecb5973884dabbb4136430` changed `.github/workflows/source-contracts.yml` to download the pinned recovery artifact, verify its archive SHA, regenerate the complete index and profile, assert all six roots and exact closure totals, build a Site package from the exact GitHub SHA, verify the package, and retain the deterministic archive. Actions run **#42**, ID `40280454837`, is still running as of this evidence update. Its outcome is not yet claimed.

This closes the previously documented missing-source blocker for this six-root startup profile. It does **not** establish complete game parity, dynamic mission/actor behavior, WebGL/browser gameplay acceptance, phone memory suitability, the v71 Site-source crosswalk, or iPhone Safari acceptance. The 97.8 MB asset payload remains large for cold phone startup. No Site was deployed in this batch.

## Next actions

1. Collect exact-SHA Actions run `40280454837` and fix its first concrete failure, if any. Require the full package's independently verified manifest and artifact SHA before treating the build gate as passed.
2. Add the full-profile `--mission-ui-incremental-contract` invocation to CI and run it against the generated profile. The one-off local environment lacks .NET; do not infer that contract from the successful Python byte-integrity pass.
3. Preserve the original source/profile through an authorized durable private source path before the pinned Actions artifact expires on 2026-10-28. Do not place the original archive into a public source tree.
4. Then close remaining interactive mission, control, combat, rendering/audio, full-package phone startup/cache, Site-source crosswalk, and owner iPhone Safari gates. Do not publish a new Site version until its exact current source commit and package archive are bound and verified.
