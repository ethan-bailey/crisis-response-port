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

The complete Mission profile used for local checks contains `Assets/AudioClip/hurt-07.ogg`: `manifest.json` records 27,302 source bytes, SHA-256 `49abb20dda09db23004a1600a48f6d970ec04d341663b210484c68307f9ad060`, and compressed JSON SHA-256 `a218aa1f58ba54848af56d269b3089640c1b83622faad9cbc91dea6f7a0714f1`. The source asset itself is therefore present; this is a runtime decode/readiness gap, not a missing package file.

## Code path confirmed by inspection

- `CharacterAudio.PlayHurt` selects from the recovered `hurtSamples` list and calls `communicationAudio.PlayOneShot` from the delayed source callback.
- `BrowserEntryTeamMission.Start` marks clips currently in `RuntimeWorld.Objects` as `Loaded` after loading Global Context and again after the Mission scene. Actor or prefab assets resolved later can register additional `AudioClip` objects after that pass.
- `AudioSource.PlayOneShot` correctly refuses to treat `AudioClip.SourceData` or source-package presence as successful decode and throws while `loadState != Loaded`.
- The current browser source-event dispatcher maps only the small `weapon-runtime.json` audio map. `hurt-07.ogg` is not mapped there, so merely relaxing the .NET readiness exception would convert a visible crash into a dropped/unmapped sound.

## Dependency-ordered closure before implementation

1. **Done for the selected Mission profile.** `scripts/inventory_profile_audio.py` generated [`MISSION_SOURCE_AUDIO_INVENTORY_2026-10-26.json.gz`](audio/MISSION_SOURCE_AUDIO_INVENTORY_2026-10-26.json.gz) from `global-ui-mission-operator` profile `ea3592ff5bdd39534bb905cd0abac74f456578b46eb4b12cdd0cb79fe8520cf7`. All 325 `.wav`/`.ogg` entries have a GUID, source hash/size, and at least one serialized owner; the inventory records all 1,030 source asset/component/field references and flags 945 references whose owner is a prefab that can be instantiated after startup. Formats: 295 WAV / 30 OGG; 38,603,354 source bytes / 24,345,810 compressed source-record bytes. Inventory gzip SHA-256: `265c9ebb8b4ccbdac4bc3cf6e4353cf65309d27efeb69dce9e6086ce08b0b7cc`. Regenerate with `python3 scripts/inventory_profile_audio.py <verified-profile-dir> /tmp/mission-audio-inventory.json`, then `gzip -n -9 -c /tmp/mission-audio-inventory.json > docs/audio/MISSION_SOURCE_AUDIO_INVENTORY_2026-10-26.json.gz`. The runtime-spawn flag is derived from prefab ownership and is a potential, not proof every such prefab is spawned in every mission.
2. Define a browser decode adapter for the original package loader and Web Audio. It must load verified original WAV/OGG bytes from the already persisted source package, decode through `AudioContext.decodeAudioData`, deduplicate concurrent decodes, bound memory, and report `Loaded` only after decode succeeds. Preserve source gain, priority, pitch, time, and stop semantics.
3. Resolve timing: either preload and decode the full actor audio closure before Mission input is enabled, or model pending source events with a documented age limit so late clips cannot play out of sync. Include clips used by NPCs spawned after entry.
4. Add exact-package contracts for the full inventory, a runtime-created `CharacterAudio` owner, `hurt-07.ogg` decode success, playback after trusted audio unlock, decode failure, concurrent request coalescing, and stale-event handling. Assert no unreported dropped or unmapped original clip events.
5. Run full-profile browser storage and production-audio acceptance on Chromium, then owner Safari cold/warm Mission acceptance. Measure peak decoded-audio memory and frame time on phone hardware; the owner also reports overall performance as poor.

## Exact source-package checkpoint (2026-10-26)

Actions run #92 (`40403481591`) passed on source SHA `143c4fd711c4da7fe2a8a13dcc95424c4ecdb6a0`, including Release/WASM, complete Mission source-package verification, retained artifact, and full-profile Chromium IndexedDB. The package selected 1,919 assets / 22,365 records. Independent Chromium weapon-audio acceptance passed, but it covers the existing seven WAV weapon clips; it does not exercise these 325 source-audio assets or the delayed `CharacterAudio` path.

## Current status

The source clip is in the package, but the dynamic decode/readiness/browser-dispatch path is not closed. No audio workaround is applied here. Do not claim the reported error fixed, do not claim complete source audio, and do not publish based on this finding alone.
