# Audio spatial runtime dependency closure (2026-10-27)

## Exact source inventory

- Reproducible scanner: `scripts/inventory_profile_audio_sources.py`; tests: `scripts/inventory_profile_audio_sources_test.py`.
- Input is the verified `global-ui-mission-operator` profile, `sourcePackageSha256=ea3592ff5bdd39534bb905cd0abac74f456578b46eb4b12cdd0cb79fe8520cf7`, 1,919 assets. The scanner verifies every descriptor against the profile manifest and each recovered Unity source-text SHA-256 before counting components.
- The profile contains 231 serialized AudioSource component records across Mission scene and prefab assets. This is a source-definition count, not a claim that 231 are instantiated at once. All 231 have serialized `DopplerLevel`, `MinDistance`, `MaxDistance`, `Pan2D`, and `rolloffMode`; all 231 omit a serialized `SpatialBlend` field. `Pan2D` is zero on all 231.
- Compressed per-component inventory SHA-256: `28f16496f7233e81144e346bcfe31bfbc37fdbfd0d345fe39afcf6542ebb4118`. Reproduce with `python3 scripts/inventory_profile_audio_sources.py <verified-profile-dir> /tmp/mission-audio-sources.json && gzip -n -9 -c /tmp/mission-audio-sources.json > docs/audio/MISSION_SOURCE_AUDIO_SOURCES_2026-10-27.json.gz`.
- Doppler is explicitly disabled (`0`) on 110 components and enabled at `1` on 121. Rolloff mode values are 0:79, 1:150, 2:2. The profile's serialized distance histogram is recorded in `docs/audio/MISSION_SOURCE_AUDIO_SOURCES_2026-10-27.json.gz`: `MinDistance` is 1 on 218 and has six other distinct authored values; `MaxDistance` ranges from 1 to 1000 with 11 distinct authored values.
- The selected Mission profile includes four AudioListener component records (two in `Mission 01.unity`, one in Interior mode prefab, one in Marksman mode prefab) and eight AudioReverbZone records in `Mission 01.unity`. The live bridge selects a listener only when exactly one enabled listener is active in hierarchy; it transports listener/source transform position, forward and rotation. It does not transport either transform's velocity.

## Source-backed meaning versus unresolved default

Unity's 2018.4 Audio Source manual states that 3D sound settings apply proportionally to Spatial Blend, describes the Doppler, distance and rolloff controls, and defines the relationship between the blend and 3D calculations. The versioned `AudioSource.spatialBlend` API describes 0 as full 2D and 1 as full 3D. Those references establish property meaning, but do not establish the default value used when this project's serialized component omits the property.

This recovered project snapshot has no `ProjectSettings/ProjectVersion.txt` or native Unity editor fixture. Therefore the serialized absence is not sufficient evidence to set the browser adapter to 0 or 1. Keep SpatialBlend unknown; do not mistake `Spatialize` (a plugin toggle) for the blend. The custom runtime `AudioSource` currently has no `spatialBlend` property, so no source setting is currently delivered to the browser.

## Complete implementation dependency set

1. Establish the source project's exact Unity editor version and absent-field default from a version-matched native Unity fixture or authoritative version-matched serialized source. Record the fixture and output hash.
2. Preserve SpatialBlend as an explicit source property with an `unknown` state for fields whose default is not proven. Do not change default behavior by treating every missing field as zero.
3. Resolve the one active listener and source Transform hierarchy/pose in the same update used for the source audio event. Inventory parent movement, Rigidbody/Rigidbody2D, CharacterController and transform-driven motion; current metadata includes position/orientation only.
4. Evaluate only semantics supported by source fields: 2D/3D blend; selected listener; `AudioManager` rolloff scale; Logarithmic/Linear/Custom rolloff and full authored custom curve; min/max distance; `DopplerLevel`; 2D pan and pan-level curve; source spread and custom curve; reverb-zone mix/custom curve; enabled/bypass flags; mixer group and authored snapshot gain.
5. Inventory the eight Mission AudioReverbZones (enabled state, owner transform, min/max distances, preset and room/decay/reflection fields), plus listener effects and source bypass flags. The current WebAudio output has no equivalent Unity reverb-zone/effect graph; do not claim those effects from a source gain-only implementation.
6. Define deterministic behavior for missing listener, multiple listeners, zero velocity, source/listener destruction, pause, and transitions between active scenes. Add numeric tests for 2D, 3D, mixed blend, left/right panning, near/far rolloff, zero/nonzero Doppler, custom curves, reverb-zone boundaries and voice-ranking audibility.
7. Validate exact Mission source package and production browser output on one pushed SHA; separately obtain physical iPhone Safari/audio acceptance. Chromium does not prove iPhone spatial output.

## Current implementation and next action

The production source event includes authored source position/orientation, selected listener position/orientation, Doppler level, min/max distance, rolloff mode/curve, pan fields, spread, reverb mix, and mixer group GUID/fileID. The browser currently applies source/group gain and pitch but deliberately does not apply distance, panning, Doppler, spatial blend or reverb-zone effects. The missing SpatialBlend default is the first dependency to close; do not implement guessed attenuation while it is unknown. Next independently trace movement/velocity provenance for Mission AudioSource owners and parent transforms, then locate a version-matched Unity editor/default fixture or record that access as the concrete blocker while continuing camera/render/particle/physics closures.

## Versioned references

- Unity 2018.4 Audio Source manual: https://docs.unity3d.com/2018.4/Documentation/Manual/class-AudioSource.html
- Unity `AudioSource.spatialBlend` API: https://docs.unity3d.com/2018.4/Documentation/ScriptReference/AudioSource-spatialBlend.html
