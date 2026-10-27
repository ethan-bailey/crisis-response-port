# Audio mixer runtime dependency closure (2026-10-27)

## Authoritative inputs inventoried before mixer runtime work

- Recovered Unity source: `source-reference/Original/Assets/AudioMixerController/Master mixer.mixer`, SHA-256 `73854ffc3e2e052fc521e3cee96310c561d039c21660664b92c25b66734a1250`.
- The controller owns 19 groups, five snapshots and five exposed float parameters. Each authored snapshot contains 47 parameter GUID/value pairs. Snapshot fileIDs: Title `24500082`, Mission `24500084`, Paused `24500086`, Ended `24500088`, No music `24500090`.
- Group fileIDs and parent-child paths, in source order:
  - `24300000` Master → Music, Realtime, Voice over, Interface.
  - `24300030` Music → Title music, Mission music, Ended music.
  - `24300050` Realtime → Communication, Ambience, Effects.
  - `24300046` Ambience → Interior ambiance, Exterior ambiance.
  - `24300034` Effects → Interior, Exterior.
  - `24300070` Interior → Unsuppressed fire, Suppressed fire, Non-explosive gun audio, Explosions.
  - All remaining groups above are leaves. The precise fileID edges are preserved in the original `.mixer` source and are now hydrated together by `SerializedAssetResolver`.
- Group volume and pitch are GUID-backed mixer parameters. The snapshots provide the authored dB/linear values for those GUIDs. AudioSource references use one mixer GUID with different group fileIDs; the bridge therefore sends both identities.
- Snapshot transitions originate in recovered `Mixer`, `InteriorMode`, and `NegotiationMode` callers. Transition duration is carried unchanged. The mixer transition clock uses `Time.realtimeSinceStartup`, since Unity audio transitions are expected to continue while scaled gameplay time is paused.
- The five recovered exposed names are `ptsMjOH`, `WovWKVN`, `SOprRTH`, `ipmjNUH`, and `qJOQoiH`. Original C# calls use different strings (`EffectsVolume`, `VoiceVolume`, `AmbienceVolume`, `InterfaceVolume`, `RealtimePitch`). Preserve the source API result `false`; do not add aliases without source evidence.
- The mission audio closure and unresolved dependencies are also inventoried in `AUDIO_VOICE_AND_MIXER_DEPENDENCY_CLOSURE_2026-10-26.md`: the recovered export lacks `SpatialBlend`; distance attenuation, Doppler, send/effect DSP, native mixer execution, and iPhone speaker output remain separate work/evidence.

## Implemented in the current local batch

- On the first source mixer reference, `SerializedAssetResolver` hydrates all 19 groups and all five snapshots from the same verified mixer asset records, retaining original fields, GUID, fileID, names and mixer ownership. This makes the runtime graph complete before any one source's group controls routing.
- The browser bridge emits the exact group/snapshot fields once the 19/5 graph is present, re-emits an incomplete graph rather than silently locking an incomplete configuration, transports snapshot transitions with raw authored values and realtime timestamps, and attaches mixer group GUID **and fileID** to source state/events.
- A call-order audit found a hidden startup edge: `InitializeIncrementalEntryTeamMission` returns the one-time graph and startup transitions in `session.snapshot`, before the next frame (which has no graph). The page now consumes that payload before enabling the Mission frame loop, with a regression assertion for ordering.
- `source-audio-mixer.mjs` interpolates each snapshot's authored parameter values, traverses group parents, applies volume dB and pitch values, and uses the resulting gain in both physical output and source-voice ranking. Unsupported/unknown group references remain neutral and are counted; malformed or incomplete configuration is rejected atomically. The existing master ceiling remains in place.
- Source `SetFloat` calls that don't match the exposed list still return `false`. SpatialBlend and rolloff remain unapplied, so current voice ranking still is not Unity's full post-spatial audibility calculation. Mixer sends, effect DSP, native output and device acoustic proof are not claimed.

## Required exact-SHA verification and next dependencies

1. **Passed on exact source SHA `42e90ef66f75f253fa4f421efe1310cf03a1e0fe`, Actions run #115 (`40434187934`).** Main job `121958819953` and independent Chromium job `121958820128` succeeded. The pinned .NET SDK 8.0.426 build, full Node/Python/runtime/engine/foundation contracts, Mission contract for all 19 groups/five snapshots/47 values, source group GUID/fileID, setter false behavior, Release/WASM, source index and full Mission package verifier/retention passed.
2. Package verifier reported `status=verified`, exact source commit `42e90ef...`, 2,688 files / 199 Blazor resources / 195 WASM files; Mission profile 1,919 assets / 22,365 records / 97,767,549 compressed bytes / 205,514,345 source bytes. Source-index SHA-256 `9d866276582031d440c316c8e60d69d252506cf67a8ae9d03dbdbe0d91980f92`; recovery archive SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. Retained artifact `12703765727`, 138,599,551 bytes, SHA-256 `69e5c44c0e202503e11927c14015b38778f640da54242e7a4dbc2d5e25ab23a1`, expires 2026-11-10.
3. Full-profile Chromium IndexedDB persisted 1,919/1,919 assets (13,552 ms cold, 10,333 ms warm; 1,919 initial requests, zero warm requests, 1,919 cache hits). Production Chromium audio decoded seven source clips and measured peak analyser RMS `0.16220188828971008`. These model the browser/Blazor path, not iPhone/Safari output. Site v85 remains deployed.
4. Continue spatial closure separately: recover or prove the missing `SpatialBlend` default, inventory source velocities/listener hierarchy, evaluate distance/custom rolloff and Doppler, then implement browser panning/attenuation/reverb-zone behavior only when the full dependency set is known. See `AUDIO_SPATIAL_RUNTIME_DEPENDENCY_CLOSURE_2026-10-27.md`.
5. Collect device-owned iPhone Safari acceptance for physical output, pause/resume behavior, audible balance and performance. Do not deploy the new runtime until exact Site archive/source crosswalk and package integrity gates pass.

## Local checkpoint

- Mixer unit and production audio tests pass locally (Node 164/164, Python 49/49 at the mixer commit). The workspace has no `dotnet` executable; exact hosted run #115 passed the pinned build/runtime/package gates and closes this mixer batch at SHA `42e90ef66f75f253fa4f421efe1310cf03a1e0fe`.

## Hidden startup adapter gap — Actions #136 (2026-10-27)

The exact packaged Mission path passed mixer/source/archive preparation, verified
all 1,919 profile assets, and then failed before enabling the Mission frame loop.
The source startup snapshot correctly carried the mixer configuration and
transitions, but the browser `createWeaponEffects` facade forwarded
`updateSourceAudioMixer` and `updateSourceAudioStates` to an `audioStats` object
that only implemented `snapshot`. The production `createWeaponAudioRuntime`
already owns both methods. `createWeaponAudioStats` now delegates all three API
calls, and the regression exercises the mixer configuration/event/source-time
arguments, active state/time arguments, and return values while separately
asserting that the production weapon adapter instantiates the facade.

Focused mixer/audio tests pass 31/31, full Node contracts pass 169/169 after
materializing the pinned PhysX package, and Python contracts pass 53/53 on the
local source checkout based on `55648848ce427b272708f25fc32c8226da048b05`.
The exact Release/WASM build, complete Mission package, and packaged browser
startup for the repair are pending hosted CI on the pushed repair SHA. The
previous Actions #136 / run `40485228497` failed specifically at
`TypeError: audioStats.updateSourceAudioMixer is not a function`; its separate
Chromium IndexedDB/audio job passed. This adapter repair does not establish
Mission visual, iPhone speaker, spatial mixer, or native Unity audio parity.
