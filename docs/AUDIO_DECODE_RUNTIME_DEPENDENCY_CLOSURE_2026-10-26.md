# Runtime source-audio decode dependency closure (2026-10-26)

## Newly reported failure

An owner test reported this source-runtime exception after a delayed hurt callback:

```text
System.InvalidOperationException: Audio sample Assets/AudioClip/hurt-07.ogg is not decoded
  at UnityEngine.AudioSource.PlayOneShot(...)
  at Crisis.Runtime.FullSource.CharacterAudio.PlayHurt()
  at RuntimeWorld.Tick(...)
  at RuntimeBridge.TickFullSourceActorBridge(...)
```

The complete Mission profile used for local checks contains `Assets/AudioClip/hurt-07.ogg`: `manifest.json` records the serialized descriptor as 27,302 bytes with SHA-256 `49abb20dda09db23004a1600a48f6d970ec04d341663b210484c68307f9ad060`; its gzip is 20,160 bytes (`a218aa1f58ba54848af56d269b3089640c1b83622faad9cbc91dea6f7a0714f1`). The descriptor embeds the actual OGG as 20,334 bytes, SHA-256 `2f4203c0903402ea6a4c00a8b728d6e7f6e396caf620bccb895500a0852ae1cc`. These are three distinct identities; only the embedded byte SHA is the decoder source identity. The source asset itself is therefore present; this is a runtime decode/readiness gap, not a missing package file.

## Code path confirmed by inspection

- `CharacterAudio.PlayHurt` selects from the recovered `hurtSamples` list and calls `communicationAudio.PlayOneShot` from the delayed source callback.
- `BrowserEntryTeamMission.Start` marks clips currently in `RuntimeWorld.Objects` as `Loaded` after loading Global Context and again after the Mission scene. Actor or prefab assets resolved later can register additional `AudioClip` objects after that pass.
- `AudioSource.PlayOneShot` correctly refuses to treat `AudioClip.SourceData` or source-package presence as successful decode and throws while `loadState != Loaded`.
- The current browser source-event dispatcher maps only the small `weapon-runtime.json` audio map. `hurt-07.ogg` is not mapped there, so merely relaxing the .NET readiness exception would convert a visible crash into a dropped/unmapped sound.

## Dependency-ordered closure before implementation

1. **Done for the selected Mission profile.** `scripts/inventory_profile_audio.py` generated [`MISSION_SOURCE_AUDIO_INVENTORY_2026-10-26.json.gz`](audio/MISSION_SOURCE_AUDIO_INVENTORY_2026-10-26.json.gz) from `global-ui-mission-operator` profile `ea3592ff5bdd39534bb905cd0abac74f456578b46eb4b12cdd0cb79fe8520cf7`. All 325 `.wav`/`.ogg` entries have a GUID, source hash/size, and at least one serialized owner; the inventory records all 1,030 source asset/component/field references and flags 945 references whose owner is a prefab that can be instantiated after startup. Formats: 295 WAV / 30 OGG; 28,904,172 embedded audio bytes, 38,603,354 descriptor JSON bytes, and 24,345,810 compressed descriptor bytes. Corrected schema-2 inventory gzip SHA-256: `be008e00ebc0c420fa2aa3305317c10eadf1cabeb2e65ad74585d55752b39630`. The superseded schema-1 inventory must not be used for raw-audio sizes or hashes. Regenerate with `python3 scripts/inventory_profile_audio.py <verified-profile-dir> /tmp/mission-audio-inventory.json`, then `gzip -n -9 -c /tmp/mission-audio-inventory.json > docs/audio/MISSION_SOURCE_AUDIO_INVENTORY_2026-10-26.json.gz`. The runtime-spawn flag is derived from prefab ownership and is a potential, not proof every such prefab is spawned in every mission.
2. **Implemented in the current local source batch; exact-source CI pending.** The browser adapter fetches embedded bytes through the verified source package, independently SHA-checks them in JS, calls `AudioContext.decodeAudioData`, coalesces requests, caps decoded-source buffers at 48 MiB and reports `Loaded` only after decode succeeds. Browser audio events now carry source IDs for Stop/loop ownership and preserve event time, gain, pitch, loop and mute fields. Priority is transported but has no priority-based voice stealing yet; spatialization/mixer routing are also not represented by the current WebAudio graph.
3. **Timing implemented with two paths.** The currently instantiated Mission AudioClip set is predecoded before controls are enabled. A later dynamically instantiated prefab clip is fetched on demand and its source event waits only while it remains within the 120 ms age window; late or failed clips are counted in diagnostics. This does not preload clips in prefabs that are not instantiated by Mission entry.
4. **Local implementation contracts added; exact package CI pending.** Node tests cover original-byte hash verification, decode success/failure, concurrent request coalescing, stale events, source identity and stop/loop state. C# source-event contract covers source ID, volume and Stop ordering. The active Mission clip set is preloaded, and the new package contract instantiates the Occupant prefab, deterministically selects its original `hurt-07.ogg` sample, invokes `CharacterAudio.Hurt()`, and asserts a source-ID-bound deferred event. The new C# contract and resulting browser decode/playback are not yet verified by exact-SHA CI. A guarantee that every decoded Mission event reached an unlocked device output also remains unestablished. No count of unreported drops is claimed until exact CI and runtime diagnostics are checked.
5. **Open acceptance.** Run exact-source build/package checks and full-profile Chromium audio acceptance; inspect decode/drop/peak-memory diagnostics. Then owner Safari cold/warm Mission, phone speaker and frame-time/memory acceptance; phone hardware is not available to this runner. Owner also reports overall performance as poor.

## Exact source-package checkpoint (2026-10-26)

Corrected inventory source SHA `11b1464fcf03573defd327f72a49837b8d087a55` passed exact Actions run #94 (`40406748532`): Contracts/Release/WASM/package/full-profile job `121866360699` and Chromium IndexedDB/audio job `121866360273`. The corrected inventory generator/tests and full Mission source package passed; retained artifact `12690358200` is 138,553,351 bytes with SHA-256 `86a2d3fbc828b21bb4c5a553bc9ad97dec1e47feb3e61aea700596a1157442b1` and expires 2026-11-09. This validates the corrected inventory identity and package on that commit only. The independent Chromium audio smoke still exercises the existing seven WAV weapon clips; it does not yet exercise all 325 source-audio assets or the delayed `CharacterAudio` path.

## Current status

The source clip is in the package. The dynamic decode/readiness/browser-dispatch implementation and a new packaged runtime-created Occupant `CharacterAudio.Hurt()` integration contract are present locally but have not passed exact-SHA source compilation/package validation. Local suites pass Node 147/147 and Python 49/49. Do not claim the reported error fixed until exact CI, that exact Occupant contract, and active Mission runtime checks pass. Full source audio parity remains open for priority voice management, spatial/mixer parity and owner device acceptance; do not publish before the exact Site build and archive integrity gate.
