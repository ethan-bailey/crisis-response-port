# Crisis Response Port — current handoff

## Current checkpoint

- Development branch: `develop/wasm`.
- Live source checkpoint: `fd44090d513eb4cbe40dffacb8ef3d9cbc955df9`
  (`fix: prevent stale loud weapon audio on iOS`).
- Repository was fetched after the GitHub write; local `develop/wasm` matches
  the remote at that checkpoint. No open pull requests were found.
- Nothing was deployed. The separate owner-private memory diagnostic Site is
  still version 1 and does not contain this audio change.

## iPhone source memory check

The owner completed the private iPhone Safari check on 2026-10-25. It ran for
270,623 ms (about 4m31s), delivered all 1,230 assets, and reported no final
error. Peak managed allocation was 68,869,128 bytes (about 65.7 MiB); managed
heap peak was 74,776,576 bytes (about 71.3 MiB). JavaScript heap and device
memory were unavailable, so this is not total Safari or device memory.

The delivery metadata reported 37,646,794 compressed bytes and 82,624,418
source bytes. The final per-sample counters reported 36,971,121 compressed
bytes and 77,806,659 source bytes for 1,230 assets. The report does not explain
these mismatched totals. Its first sample, at 15 ms, ran before incremental
source initialization; subsequent samples succeeded and the run completed.
Full result and instructions: [phone source memory check](docs/evidence/PHONE_SOURCE_MEMORY_CHECK_2026-10-25.md).

## Gunshot audio fix

At the original sample playback site, the browser could call `start()` while
iOS had suspended the AudioContext. That can defer audible playback until the
context resumes, making a shot late. The runtime now drops one-frame audio
events when the context is not running or the decoded clip is not ready, uses
a quieter shared output gain plus a compressor, and reports play/drop/decode
counters in the presentation snapshot. This is a source-level diagnosis; the
patched audio has not been packaged or accepted on iPhone yet.

Checks run against the exact source content in `fd44090`: Node syntax checks
for `game/weapon-runtime.mjs` and `game/presentation.mjs`, and
`git diff --check` all passed. No runtime browser smoke or iPhone audio check
was run. Do not describe the audio fix as accepted or deploy the package yet.

## Remaining system work

The FullSource actor movement force and physics state round trip passed the
active Mission UI contract in the prior checkpoint. The normal page still uses
browser-side movement and the transport weapon host. The memory profile has
now completed on the owner’s iPhone, but the report’s byte totals need
reconciliation before claiming exact delivered size. Page input, physics,
rendering, and weapon callbacks still need to be wired through the same actor.
Playwright/browser binaries were not available in the earlier environment;
iPhone Safari remains the target acceptance device.

## One next action

Build and integrity-check a package from exact source
`fd44090d513eb4cbe40dffacb8ef3d9cbc955df9`, then prepare an iPhone Safari
gunshot check that exercises the first shot, sustained firing, and returning
from a suspended/background state. Keep deployment and audio acceptance open
until the owner tests that exact build.
