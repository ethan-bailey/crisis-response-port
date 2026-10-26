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

1. Inventory every `AudioClip` reference reachable from Mission actors and runtime-spawned prefabs: `CharacterAudio` hurt/snore/cough/impact/squirt/equipment/footstep arrays, `AudioSurface` step/bounce lists, weapon channels, `AudioPlayer`, communication, and scene one-shots. Include source path, GUID/fileID, format, byte/hash, owner prefab, and whether its owner can be instantiated after startup.
2. Define a browser decode adapter for the original package loader and Web Audio. It must load verified original WAV/OGG bytes from the already persisted source package, decode through `AudioContext.decodeAudioData`, deduplicate concurrent decodes, bound memory, and report `Loaded` only after decode succeeds. Preserve source gain, priority, pitch, time, and stop semantics.
3. Resolve timing: either preload and decode the full actor audio closure before Mission input is enabled, or model pending source events with a documented age limit so late clips cannot play out of sync. Include clips used by NPCs spawned after entry.
4. Add exact-package contracts for the full inventory, a runtime-created `CharacterAudio` owner, `hurt-07.ogg` decode success, playback after trusted audio unlock, decode failure, concurrent request coalescing, and stale-event handling. Assert no unreported dropped or unmapped original clip events.
5. Run full-profile browser storage and production-audio acceptance on Chromium, then owner Safari cold/warm Mission acceptance. Measure peak decoded-audio memory and frame time on phone hardware; the owner also reports overall performance as poor.

## Current status

The source clip is in the package, but the dynamic decode/readiness/browser-dispatch path is not closed. No audio workaround is applied here. Do not claim the reported error fixed, do not claim complete source audio, and do not publish based on this finding alone.
