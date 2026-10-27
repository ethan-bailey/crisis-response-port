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
| Mixer routing | Original output group is imported; recovered `.mixer` contains 19 groups and five 47-value snapshots | Local batch recovers all sibling records, transports GUID+fileID and snapshot transitions, and applies parent-chain gain/pitch | Exact hosted package contract pending; then spatial blend/rolloff and DSP remain |
| Listener/source motion | One active EntryTeam listener is asserted; event-time source/listener transforms are carried | #112 streams active source/listener pose state each FullSourceActorBridge frame and validates/caches the latest set | Still no velocity samples, PannerNode consumption, or runtime attenuation |
| Real/virtual state | 64 real voices, 512 virtual voices, effects virtualization enabled | Production allocator caps 64 physical nodes and 512 logical voices; offsets, promotion, natural completion and `isPlaying` are preserved | Exact hosted package contract passes at run #114; next mixer batch is local/unpushed |
| Lifecycle/state | `isPlaying`, source-scoped Stop, overlap/replacement and natural end have contracts | Active decoded nodes are tracked by source ID | Preserve playback position and source state while virtual; clear on end, Stop, replacement, disable, destroy and unload |

The verified Mission package has its own serialized source inventory: 231 AudioSource component definitions in profile `ea3592ff5bdd39534bb905cd0abac74f456578b46eb4b12cdd0cb79fe8520cf7`; none serialize `SpatialBlend`. The complete component/field crosswalk and 8 Mission AudioReverbZones are documented in `docs/AUDIO_SPATIAL_RUNTIME_DEPENDENCY_CLOSURE_2026-10-27.md` and `docs/audio/MISSION_SOURCE_AUDIO_SOURCES_2026-10-27.json.gz`. Do not treat the full-project count above as Mission instance count.

## Current code checkpoint

`game/source-audio-voice-arbitration.mjs` defines ranking by ascending priority, descending normalized audibility, then stable sequence. Production `weapon-audio-runtime.mjs` caps physical nodes at 64 and logical virtual records at 512; active source frames update priority/gain, virtual voices preserve playback offset on promotion, report `isPlaying`, and expire naturally. Run #114 on exact source SHA `c8d4db1b85072c9c9f6ba54fdee9ab402c61b268` passed the pinned Release/WASM, recovered source/index, complete Mission package/integrity/retention, full-profile IndexedDB, and production Chromium audio gates. Mixer source code is pushed at `42e90ef66f75f253fa4f421efe1310cf03a1e0fe` and run #115 is checking its exact source/package/Chromium gates. Local dependency inventory now confirms that SpatialBlend remains unproven.

Focused tests cover the recovered 64-real/512-virtual boundary with 513+ voices, source priority, gain tie-breaking, promotion/offset, virtual natural end, source state updates, and mixer transition gain/pitch. The mixer workspace full Node suite passes 164/164 and Python suite 49/49; the added AudioSource inventory scanner and fixture tests bring Python to 51/51. `git diff --check` passes. C# exact-source and package checks for the mixer batch must run hosted because no local .NET SDK is installed.

## Dependency-ordered implementation batch

1. Run #115 on exact pushed SHA `42e90ef66f75f253fa4f421efe1310cf03a1e0fe`; require the Mission contract for 19 groups, five snapshots and all 47 values, source group GUID/fileID, exact transition times, setter false/no-op behavior, source/package/index/artifact integrity and both Chromium jobs. Repair failures and rerun the repaired SHA.
2. Establish the exact Unity editor version and missing-field default for SpatialBlend; the profile omits it on all 231 source definitions, and the recovered project does not include ProjectVersion or a native editor fixture. Keep uncertainty explicit until evidence closes it.
3. Trace velocity provenance for source/listener transforms through parent hierarchy, then evaluate confirmed spatial rolloff and Doppler. Only after the full closure is known implement panning/attenuation and add browser tests.
4. Validate snapshots (Title/Mission/Paused/Ended/No music), audio transition timing across pause, all Mission source groups, mixer gain and voice priority in Chromium. Do not substitute browser RMS for Unity audibility.
5. Collect iPhone Safari playback, resume, speaker balance and performance evidence. Headless Chromium is not physical-speaker proof.

## Explicit non-claims

The ranking and capped voice lifecycle passed exact hosted validation at #114. Mixer gain/pitch and group/source transport are pushed at `42e90ef66f75f253fa4f421efe1310cf03a1e0fe`; exact run #115 remains in progress. The 256-source histogram above is full-project; the Mission profile has 231 serialized AudioSource definitions. SpatialBlend remains unknown. Ongoing velocity, distance rolloff, actual WebAudio spatial panning, native spatial DSP, effect/send processing and iPhone output remain open. Do not present this work as complete Unity audibility parity.
