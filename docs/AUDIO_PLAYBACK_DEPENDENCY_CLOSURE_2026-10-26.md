# Browser weapon-audio dependency closure — 2026-10-26

## Gate and acceptance scope

This is the pre-implementation inventory for audible weapon playback. The target is a deterministic headless Chromium proof that original extracted weapon clips are fetched, decoded, mapped from source event paths, unlocked by a real browser gesture, and produce nonzero samples through the production WebAudio graph. This does not assert that a phone speaker is audible, that iOS Safari uses the same autoplay policy, or that volume is comfortable on every device. Those require device acceptance.

## Inventory to close before coding

| Dependency | Source of truth / package item | Required verification |
|---|---|---|
| Exact runtime source and contract | game/weapon-audio-runtime.mjs; game/weapon-runtime.mjs; scripts/weapon_audio_runtime_test.mjs | Reuse production createWeaponAudioRuntime, preloadWeaponAudio, createSourceWeaponAudioDispatcher, and runtime event source mapping. Unit fakes are not evidence of audible output. |
| Authored audio event manifest | game/assets/weapon-runtime.json.gz, gzip-decoded with browser DecompressionStream; audio records carry source and url | Load the actual compressed manifest from localhost, require source+URL and unique mappings. Required keys: audioFire, audioExtractMagazine, audioInsertMagazine, audioBoltReturn. Dispatch one-shot events using each authored source path and the production dispatcher. |
| Original clip bytes | game/assets/weapon-runtime/audioFire.wav, audioExtractMagazine.wav, audioInsertMagazine.wav, audioBoltReturn.wav; remaining source-mapped clip files under the same directory | Serve files from the checkout, preload every authored audio entry and require each fetch/decode to succeed. Assert those four source keys point to the expected WAV basenames. Do not fabricate or synthesize audio. |
| URL/package resolution | Production manifest URL fields and the same static game/assets/weapon-runtime directory shipped in the package | Browser network requests must return the actual file bytes; no mocked fetchAudio, response, or decoded buffer. Fail on HTTP errors or missing package files. |
| WebAudio graph | Production runtime: AudioContext → event GainNode → master GainNode → DynamicsCompressor → destination | Use real Chromium AudioContext, decodeAudioData, AudioBufferSourceNode, gain and compressor nodes. Assert context is running, all source buffers loaded, master gain equals the fixed 0.16 ceiling, and each dispatched event increments played with no stale/suspended/unloaded/unmapped drops. |
| Gesture lifecycle | Runtime registers capture-phase pointerdown and invokes context.resume() | Create the runtime before a Playwright user click; click a visible fixture button using Playwright's trusted browser input. Await running and require no unlock failure. A direct JS call to unlock() is not a substitute for the gesture path. |
| Event time/mapping | Source dispatcher maps event.sourcePath and passes sourceTime - event.time | Use fresh source one-shot events at age 0. Keep freshness contract bounded by current production classifier (−0.05 to +0.12 seconds). Never use stale queued audio to hide timing errors. |
| Audible graph evidence | Actual decoded clips through actual source/gain/limiter nodes | In test instrumentation only, tap an AnalyserNode onto the output edge while preserving the real destination connection. After firing all four source-mapped events, sample time-domain data and require nonzero deviation from digital silence. Keep this separate from any music/audio backend. |
| Browser and workflow | Pinned playwright@1.64.1; Chromium installed in GitHub Actions; .github/workflows/source-contracts.yml | Add an independent Chromium audio playback job step. It must run after npm ci and Chromium install. Retain test output with decoded clip names, context state, per-event play counts, loaded buffers, gains and analyser RMS. |
| Boundaries / unproven claims | iPhone Safari, device output route, mute switch, hardware volume, power-saving behavior, mission-level shot cadence | Mark headless acceptance as browser graph playback only. Keep physical iPhone/Safari and speaker audibility open for the exact deployed version. Do not deploy a test-only change. |

## Dependency order

1. Confirm the production audio module, compressed authored manifest and all four named original WAV files are in the exact branch/package; the current branch contains them.
2. Implement one Playwright smoke script that serves the working tree, obtains the real manifest and audio files over HTTP, and imports the production runtime.
3. From a trusted click, prove the registered unlock path resumes the real AudioContext.
4. Decode/preload every source clip; dispatch the four required original source events; inspect playback/drop counters and an analyser tap on the output graph.
5. Wire the script into the pinned Chromium CI job. Run exact source tests, Release WASM build, complete source profile/package verifier and IndexedDB browser test for the same commit SHA.
6. Record exact workflow/job IDs, source SHA, package profile, inner tar.gz and artifact checksums. Only then mark the headless audio subgate passed. Physical device acceptance remains separate.

## Hidden-gap protocol

If the browser test reveals an undocumented prerequisite—such as the source key differing from the event source path, an incorrect relative URL, gesture registration after the first pointerdown, missing decoded data, or graph silence—record the observed error and update this closure and the gate index before modifying production code. Do not paper over failure by synthesizing sound, faking decoded buffers, direct-calling unlock, increasing gain beyond the source cap, or weakening the actual-event mapping assertion.

## Current status

Dependency inventory complete; no browser implementation change in this documentation checkpoint. The known source keys, WAV files and production API are present on develop/wasm. The next action is the Chromium smoke implementation above. Site v73 remains live; no deployment is needed for this test-only batch.


## Hidden dependency found — run #64

The initial browser implementation decoded the clips and reached the gesture-state assertion, but GitHub Actions Chromium had already started the AudioContext in running state before any click. The failure was exact: expected suspended, got running. The browser default therefore does not enforce the user-gesture prerequisite this acceptance intends to verify. Before the next source edit, this is recorded as a harness dependency: launch Chromium with the explicit --autoplay-policy=user-gesture-required flag; keep the runtime constructed before the trusted click, require suspended state before it and running state afterward, then rerun the full exact-source release/package and IndexedDB gates. The #64 IndexedDB subtest passed; the audio test did not reach event playback or output analysis, so no audio-playback result is claimed.


## Harness correction in verification

Commit 2b55d5f4905e7a14eddb0fd029f8941e80d237b1 adds Playwright's --autoplay-policy=user-gesture-required launch argument. The full exact-source Release/profile/package and Chromium run #65 / ID 40321027816 is in progress; no audio playback pass is claimed until its browser job records the context transition and analyser output.
