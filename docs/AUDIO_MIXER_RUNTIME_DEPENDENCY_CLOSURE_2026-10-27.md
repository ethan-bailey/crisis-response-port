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

1. Run the Mission runtime contract against the exact verified source package: all 19 groups, five snapshots, 47 values per snapshot, a transition carrying authored float values, source mixer group GUID/fileID, and exposed setter no-op behavior.
2. Run full Node/Python/runtime/engine/foundation contracts, pinned .NET Release/WASM build, source-index and complete Mission package integrity/retention, production full-profile IndexedDB and Chromium audio on one exact pushed SHA.
3. Only after those pass, use Chromium source audio output to exercise all five authored snapshots and all Mission source groups. Check pause/unpause, end/no-music transitions, source gain/pitch, virtual voice ordering and playback continuity. This is browser evidence, not iPhone/Safari output evidence.
4. Continue spatial closure separately: recover or prove the missing `SpatialBlend` default, inventory which Mission sources are 2D/3D, evaluate distance/custom rolloff and Doppler against the selected source listener, then implement browser panning/attenuation with tests. Do not fold unknown values into this mixer batch.
5. Collect device-owned iPhone Safari acceptance for physical output, pause/resume behavior, audible balance and performance. Do not deploy the new runtime until exact site archive/source crosswalk and package integrity gates pass.

## Local checkpoint

- Mixer unit and production audio tests pass locally (see current `NEXT_AGENT.md` for the exact totals and hosted run). The workspace has no `dotnet` executable; the pinned hosted build/runtime contract is mandatory before treating this batch as verified.
