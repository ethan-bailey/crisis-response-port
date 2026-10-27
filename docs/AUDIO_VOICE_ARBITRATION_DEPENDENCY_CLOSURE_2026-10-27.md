# Audio voice arbitration dependency closure (2026-10-27)

## Source facts and implementation boundary

The recovered `ProjectSettings/AudioManager.asset` sets `m_RealVoiceCount: 64`, `m_VirtualVoiceCount: 512`, and `m_VirtualizeEffects: 1`. The full recovered prefab/scene export contains 256 serialized `AudioSource` components (not a count of simultaneous Mission voices). Their serialized priority distribution is 128:135, 5:39, 150:30, 40:20, 100:18, 0:4, 15:4, 160:2, 135:2, 210:2. Defaulting every source to 128 would therefore lose authored ordering.

Version-matched Unity 2018.4 `AudioSource.priority` states that the lowest-priority and least-audible sources are virtualized first, with priority values 0–255 and 0 highest: <https://docs.unity3d.com/2018.4/Documentation/ScriptReference/AudioSource-priority.html>. The Audio Manager manual says the real-voice limit is the number that can play simultaneously and that the loudest voice is selected each frame; it also describes the virtual voice pool and effect virtualization: <https://docs.unity3d.com/2018.4/Documentation/Manual/class-AudioManager.html>. Together these sources support ordering candidates by lower numeric priority, then higher audibility. They do not prescribe a tie order for equal priority and equal audibility; the port's stable enqueue order is a deterministic implementation choice, not claimed Unity behavior.

## Inputs that exist and inputs still missing

| Dependency | Recovered/source state | Browser state | Required closure |
| --- | --- | --- | --- |
| Priority | Imported into `AudioSource.priority`; values range 0–210 in the full export | Carried on `BrowserAudioEventQueue` events | Rank by priority, then evaluated audibility |
| Event gain / mute | Source volume, one-shot scale, mute and pitch are imported | Gain and mute are carried | Include effective gain in audibility |
| Distance/rolloff | `dopplerLevel`, `minDistance`, `maxDistance`, rolloff mode/custom curve are imported; AudioManager rolloff scale is 1 | #112 carries distance/rolloff/curve fields with source events | Evaluate against the selected listener only after 2D/3D blend is resolved |
| Spatial blend | `SpatialBlend` is absent in recovered serialized export | No 2D/3D distinction | Recover the Unity default or keep distance audibility explicitly unknown; do not infer it from `Spatialize` |
| Mixer routing | Original output group is imported; 19-group tree and five snapshots are inventoried | Group GUID carried, but group gain/snapshot values are not applied | Consume group GUID, snapshot changes, attenuation, and resulting gain before final audibility ranking |
| Listener/source motion | One active EntryTeam listener is asserted; event-time source/listener transforms are carried | #112 streams active source/listener pose state each FullSourceActorBridge frame and validates/caches the latest set | Still no velocity samples, PannerNode consumption, or runtime attenuation |
| Real/virtual state | 64 real voices, 512 virtual voices, effects virtualization enabled | Every decoded event currently creates a WebAudio node | Keep virtual playback time/state and promote/demote as rank changes; record pool pressure |
| Lifecycle/state | `isPlaying`, source-scoped Stop, overlap/replacement and natural end have contracts | Active decoded nodes are tracked by source ID | Preserve playback position and source state while virtual; clear on end, Stop, replacement, disable, destroy and unload |

## Current code checkpoint

`game/source-audio-voice-arbitration.mjs` defines a pure ranking/allocation contract: ascending priority, descending caller-supplied normalized audibility, then stable sequence. It splits ranked candidates into a requested real-voice set (default 64) and the remaining virtual candidates. This is not yet wired into `weapon-audio-runtime.mjs`; the production WebAudio path still starts each accepted event. The helper rejects incomplete audibility instead of silently estimating it from a partial event. #112 adds source-state transport through the event queue, active voice snapshots, FullSource actor frame and game adapter; the browser stores/validates the latest state set, but it does not yet use those values for ranking, spatialization or mixer gain.

Focused tests cover the recovered 64-real-voice boundary with 513 candidates, priority-before-loudness, audibility tie-breaking, deterministic equal ties, invalid inputs, and atomic source-state updates. C# bridge checks assert AudioSource fields/curves, active transforms, and Stop clearing; local JS verifies the consumer cache path. These tests validate the selection and transport contracts only; they do not establish browser voice virtualization or Unity equivalence.

## Dependency-ordered implementation batch

1. Run exact source/package/Chromium verification for the #112 transport batch before using it as a base.
2. Resolve and encode the source's missing SpatialBlend/default semantics with version-matched source evidence; keep uncertainty explicit until then. Calculate audibility from source gain/mute and any confirmed spatial rolloff, then add mixer/snapshot group attenuation after group routing. Do not substitute measured WebAudio output RMS for Unity's pre-mixer virtualization estimate.
3. Integrate ranked physical allocation. Keep lower-ranked voices virtual with a playback clock/offset; on each source-state update, end, Stop, replacement, or context transition, re-rank and promote/demote while preserving `isPlaying` and natural completion. Use recovered 64 real and 512 managed voices. Preserve any over-pool behavior as an explicit approximation until original-editor evidence is available.
4. Add deterministic C# transport contracts and JS/runtime tests for category priority profiles, same-priority gain tie, 65+ voices, promoted offset, virtual natural end, stop/replacement, decode-pending cancellation, pose/audibility changes, and effect virtualization. Run the exact pinned source build, package integrity, full-profile Chromium cache and production-audio acceptance on the resulting SHA.
5. Only then collect iPhone Safari playback, resume, device audibility and performance evidence. Headless Chromium is not physical-speaker proof.

## Explicit non-claims

The new ranking helper is contract scaffolding only, not production parity. The source count histogram is full-project, not Mission-only. SpatialBlend remains unknown. Mixer group gains/snapshots, ongoing transforms/velocity, WebAudio virtualization, effect virtualization, native spatial DSP and iPhone output remain open until implemented and tested.
