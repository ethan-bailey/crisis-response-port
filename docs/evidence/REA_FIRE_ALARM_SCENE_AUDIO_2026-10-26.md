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

No release provenance is recorded until the changed source is pushed and the
exact Site source SHA is rebuilt, packaged, and independently verified.
Recheck the production deployment after that exact archive is saved.

Owner iPhone Safari frame rate, operator pose/grip alignment, visual/audio
output, particle rendering, and full game parity remain open. These headless
source contracts do not measure device performance or visible hand placement.
