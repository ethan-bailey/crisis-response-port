# Current handoff — 2026-10-25

## Released error recovery

- Runtime source release: `develop/wasm` code checkpoint `044012ace4ca15eb6554566975db5d41d4201f2a`; the handoff note may be one documentation-only commit ahead.
- The Mission source loader now passes each compressed asset as a base64 JSON string to the existing .NET `byte[]` bridge. The previous `Uint8Array` JSON payload triggered Blazor's `ByteArrayJsonConverter` exception on iPhone Safari.
- Runtime errors now open in a scrollable panel above the controls with **Copy details**, selectable stack text for iOS long-press copy, close, and Mission retry actions.
- Owner-private Crisis Response Runtime Site version **46** is deployed at `https://crisis-response-runtime.craftysnipe13.chatgpt.site`. Its Site source commit is `f9751de48bc78516295a7d2e1d815c3e27904699`; the packaged WASM manifest identifies source checkpoint `044012a`.
- Owner-only access remains custom/private, one allowed owner and no groups.

## Verification

- Release WebAssembly publish passed with pinned .NET SDK `8.0.426` at the loader fix checkpoint. No C# method or runtime binary change was needed; .NET's byte-array converter accepts base64 JSON strings.
- Source-loader contract test passed with a multi-chunk base64 payload; game error-panel contract and source-loader tests passed **5/5**. Extracted game module syntax and `git diff --check` passed.
- Site package integrity passed for **2,673 files**, including 199 Blazor resources, 195 WASM files, and the 1,919-asset Mission profile. Default package omits the 220 MB-uncompressed optional scene-smoke archive.
- Site version 46 deployment returned `succeeded`; source branch was fetched at `044012a` and the Site stayed owner-only.

## Remaining acceptance and limits

- The newly deployed build has not been exercised in Playwright or iPhone Safari here. The owner skipped manual testing earlier; please open the Site on iPhone, tap **Start original Mission · 99 MB**, wait for Mission ready, then try movement, aim, and fire.
- If it errors, tap **Copy details** in the error panel and paste the copied text into ChatGPT. If clipboard access is blocked, the panel selects the stack; long-press it and choose **Copy**. The Mission retry button is also inside the panel.
- The original Mission browser audio adapter is silent; gunshot audio acceptance remains open. Some Unity components, NPC physics, and gameplay scripts are unsupported or unverified. Do not claim full Unity parity.

## Next action

Ask the owner to test Site version 46 on iPhone Safari and share the copied error details if Mission startup still fails.
