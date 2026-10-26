# REA: authored FireAlarm scene AudioSource — 2026-10-26

## Report and cause

When the owner returned to the open Mission page, a delayed callback threw
`NullReferenceException` in `FullSource.FireAlarm.SetState(true)`. The
recovered Mission 01 scene stores `AudioSource` class ID 82 directly on its
`Smoke detector` GameObject. `SceneLoader` had only imported that component
for prefab records, leaving the source callback without the same-GameObject
AudioSource it expects. The serialized clip reference was also omitted.

The browser audio queue is asynchronous: it can request a clip while browser
decoding is still pending. Browser-backed `AudioSource.Play` previously
rejected that request before the queue could handle it.

## Changes

- Import class ID 82 on scene objects as well as prefabs and retain its
  serialized fields, including the original clip reference.
- Mark browser-backed audio as deferred so both `Play` and `PlayOneShot`
  enqueue playback requests for not-yet-decoded clips. Other audio drivers
  continue to reject unloaded clips.
- Add a Mission 01 source contract that activates the authored FireAlarm at
  zero delay, ticks the original callback, and checks that the original
  `fire-alarm.ogg` event is queued. Add a foundation contract for deferred
  browser `Play` and `PlayOneShot`.

## Verification on the working trees

- Dev `develop/wasm` before commit: focused full-source Mission contract
  passed. It found Mission 01 / Smoke detector / AudioSource Smoke detector /
  `Assets/AudioClip/fire-alarm.ogg`; delayed callback reached alarm state and
  queued the playback request.
- Site source `main` before commit: same focused Mission contract passed.
  The clip remained `decoded=false` at dispatch time, confirming that the
  browser queue accepts the request during asynchronous decoding.
- Dev FoundationChecks passed: 122 assertions. Site FoundationChecks passed:
  123 assertions. Site Release Runtime build passed with 0 errors and 48
  existing warnings. Browser/WASM Release publish passed.
- The earlier full Browser EntryTeam contract reached a later, separate
  particle identity assertion after this alarm/audio path no longer threw.
  That contract remains failed; the particle acceptance is not claimed here.

## Release and remaining acceptance

Site v83 is deployed from exact Site source commit
`9c875e649f113d1c9fe91fbf6b34b59b091401ee` at the existing owner-private URL.
Saved version ID:
`appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_79849b3c7700819191cfa83dc9a264b6`.
Deployment ID: `appgdep_6adfc00174208191b605e69abf497e5a`.

The package verifier passed with 2,686 files, 199 Blazor resources, 195 WASM
files, and 1,919 Mission assets / 22,365 records. The profile contains
95,091,064 compressed asset bytes and 180,193,684 source bytes; source package
SHA-256 `b2332a1c325c3cee0e2a42e26b05affd3282696a3e840bc7ad134f05dbb2a04b`.
An independent sequential audit of the exact Site archive matched every
manifest file hash and size, the build manifest, and the project hosting
identity: 2,688 regular files total, no missing or extra files, and zero hash
mismatches. Local archive SHA-256
`9c95a47fef6d9409083c9e5c16b947b7e2719662e6db735d82ab04c1c839aace`; Sites
archive metadata reports
`sha256:38923e48beded1d0822c1d7e6bea5da3dafa36e2f86ec51b6129e598461e359a`.

Public run #89 on commit `3aecf75ed4c19f55828bbb837caca4c2da63209d` failed
in the full-profile mission-outcome check because that headless fixture had
not bound its silent audio driver at world scope before scene-created
AudioSources activated; it also rejected an undecoded scene light-hum clip.
The independent Chromium cache/audio job passed. The fixture now binds the
deferred silent driver before loading the scene. Local
`--mission-outcome-contract` passed after the correction, including the
authored MissionEndMenu camera transition. Exact-source CI for this follow-up
is pending.

Owner iPhone Safari frame rate, operator pose/grip alignment, visual/audio
output, particle rendering, and full game parity remain open. These headless
source contracts do not measure device performance or visible hand placement.
