# Current handoff — 2026-10-25

## Current release

- `develop/wasm` runtime source checkpoint: `c15c91f662d67cc1f640c527c17efa88c5b80be7` (a final handoff-only commit may follow).
- Crisis Response Runtime Site version **47** is deployed at `https://crisis-response-runtime.craftysnipe13.chatgpt.site` and remains owner-only. The Site source commit is `b87a46983edca40b4fc317db1a81fda9123c3d45`; its manifest identifies runtime source `c15c91f`.
- The Mission source loader sends compressed assets as base64 JSON text to the existing .NET `byte[]` JS interop method. This addresses the iPhone `ByteArrayJsonConverter` exception.
- Mission loading now shows asset and MiB progress outside the hidden play HUD, provides Cancel via `AbortController`, and restores the start control after cancellation for retry. A touchend guard blocks rapid double-tap zoom on the game canvas.
- Runtime errors retain the scrollable, copyable details panel with iOS selection fallback and retry action.

## Verification

- Pinned .NET 8.0.426 Release WebAssembly publish passed at the loader-fix source checkpoint; no C# code changed afterward.
- Loader and UI contract checks passed **6/6**, including already-aborted source delivery. Extracted game module syntax and `git diff --check` passed.
- Final package integrity verified **2,673 files**, 199 Blazor resources, 195 WASM files, and the 1,919-asset Mission profile. The optional 220 MB-uncompressed scene-smoke archive remains excluded.
- Site version 47 reports deployment `succeeded`; archive manifest, game progress/cancel UI, base64 loader, and `.openai/hosting.json` were verified before publication.

## Remaining acceptance and limits

- Playwright and iPhone Safari acceptance are still open. Ask the owner to reload the live Site, tap **Start original Mission · 99 MB**, confirm progress advances, and try movement, aim, and fire after Mission ready.
- If progress stops, tap **Cancel**, retry once, and if it errors tap **Copy details** and paste the result into ChatGPT. On clipboard failure, long-press the selected stack and choose **Copy**.
- Rapid double-tap zoom suppression is implemented on the game canvas but has not been manually verified on iPhone Safari.
- The original Mission browser audio adapter is silent. Some Unity components, NPC physics, and gameplay scripts remain unsupported or unverified. Do not claim full Unity parity.

## Next action

Have the owner test Site version 47 on iPhone Safari, especially download progress, cancellation/retry, double-tap zoom, movement, aim, and fire.
