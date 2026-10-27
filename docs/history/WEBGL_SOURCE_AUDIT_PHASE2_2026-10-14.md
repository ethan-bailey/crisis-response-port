# WebGL source audit — phase 2 (2026-10-14)

## Scope and confidence

This is a static inspection of the recovered AssetRipper Unity project at Unity 2017.2.0f3. It is not a compile, browser execution, or gameplay test. The inspected local project is the recovered artifact whose gameplay assembly hash was previously matched to the original archive. Keep the untouched recovery artifact as the baseline; do not overwrite it while applying port fixes.

## Newly verified source-level findings

### 1. Server requests are not just a generic networking concern

Files:
- `Assets/Scripts/Assembly-CSharp/ServerConnection.cs`
- `Assets/Scripts/Assembly-CSharp/HTTPRequest.cs`
- `Assets/Scripts/Assembly-CSharp/Threading/ThreadedJob.cs`
- `Assets/Scripts/Assembly-CSharp/HTTPGet.cs`
- `Assets/Scripts/Assembly-CSharp/HTTPPost.cs`
- `Assets/Scripts/Assembly-CSharp/Usage/Statistics.cs`
- `Assets/Scripts/Assembly-CSharp/Usage/PostFeedback.cs` and related Usage request classes

Observed facts:
- The live and staging server URLs in `ServerConnection` use plain `http://`, not HTTPS.
- `HTTPRequest` runs through `ThreadedJob`, which starts a managed `Thread` and calls `Thread.Abort` on timeout.
- GET/POST implementations use `System.Net.WebRequest`.
- Call sites include session/client statistics, version checks, and feedback submission.

Impact:
- A game hosted on an HTTPS browser page cannot freely call these HTTP endpoints: browser mixed-content policy is a blocker, independently of Unity compilation.
- The threaded request implementation is not a safe WebGL implementation.
- Moving request code into browser-compatible requests may still fail if the server does not support HTTPS and the necessary CORS headers. This cannot be solved by changing Unity code alone if server configuration is unavailable.
- The POST implementation contains hard-coded request-key constants. Treat those as public client-side values, not secrets; never copy them into additional artifacts or assume they provide secure authentication.

Recommended decision:
- First classify each call site as gameplay-critical or ancillary. The inspected call sites suggest session telemetry, client registration, version notification, and feedback rather than the core mission simulation.
- For WebGL, disable ancillary communication by default behind a platform guard unless a compatible HTTPS endpoint and CORS behavior can be verified. Preserve desktop behavior.
- If a browser-compatible backend is required, use HTTPS + explicit CORS and an authentication design that does not depend on a secret embedded in downloadable client code. Do not silently redirect HTTP to HTTPS without checking that the endpoint actually serves HTTPS.

### 2. Registry access is isolated to the settings-reset path

File: `Assets/Scripts/Assembly-CSharp/GlobalContext.cs`

Observed facts:
- `Microsoft.Win32` is imported for `Registry.CurrentUser.DeleteSubKey("Software\\Antimony\\Crisis Response")`.
- This is called by `ClearExitVideoSettings`; ordinary startup calls `Settings.Load()` and `Apply()`.

Recommended decision:
- Guard only the registry deletion for non-Windows/browser targets; retain the settings-file reset path. This is a narrow platform-specific fix, not a reason to replace the settings system wholesale.
- Confirm all related call paths before editing because this method also clears startup video settings.

### 3. Persistence has distinct feature classes that need separate treatment

Files:
- `Serializer.cs`
- `EngagementRules.cs`
- `SerializableCampaign.cs`
- `FileBrowser.cs`

Observed facts:
- Settings serialization writes encrypted JSON text wrapped in `BinaryFormatter` to a filesystem path.
- Campaign save/load uses `BinaryFormatter` and accepts an arbitrary file path.
- `FileBrowser` enumerates directories/files and normalizes paths with Windows backslashes.
- `GlobalContext.ClearAllSettings` enumerates and deletes files in the persistent directory.

Recommended decision:
- Do not remove persistence as a blanket workaround. Separate internal settings persistence from user-managed campaign import/export.
- On WebGL, keep internal settings only if the selected Unity version supports the required persistent-data behavior and verify flush/reload in Safari. Treat arbitrary campaign browsing as unsupported unless redesigned around browser file input/download APIs.
- Add tests for first run, save, reload, clear settings, and browser tab restart. A successful call to `File.Create` on desktop does not establish WebGL persistence parity.

### 4. iPhone controls require a designed input layer, not just a build-target toggle

Files:
- `InputHandler.cs`
- `InputMapping.cs`
- `PlayerController.cs`
- `Marksman.cs`
- `MarksmanCamera.cs`
- `WeaponPointer.cs`

Observed facts:
- Core input polling uses legacy `Input.GetKey`, `Input.GetAxis`, mouse buttons, mouse X/Y, and cursor lock/confine operations.
- Default mappings include keyboard keys, mouse buttons, and scroll wheel.
- CrossPlatformInput/TouchPad scripts exist in Unity Standard Assets, but the inspected gameplay input code does not show a touch implementation wired into `InputHandler`.

Impact:
- A WebGL build could conceivably be playable with keyboard/mouse on desktop and still be unusable on iPhone. Browser mouse emulation does not provide a faithful replacement for a multi-control FPS interface, and iOS Safari does not provide a conventional mouse pointer/cursor lock experience.

Recommended decision:
- Keep the original input path for desktop. Add a separate touch-control adapter that invokes the same existing gameplay actions rather than rewriting gameplay logic.
- Prototype a small real gameplay scene first: movement/look, fire/aim, reload, interact/door, mode switching, and command actions. Test screen aspect ratio, touch capture, gesture conflicts, Safari browser chrome, and background/resume before building the entire game.

### 5. Build settings confirm WebGL is not configured as a verified target

Files: `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/EditorBuildSettings.asset`, `Packages/manifest.json`

Observed facts:
- The six enabled scenes are Disclaimer, Title, Transition, Mission 01, Marksman range, and Benchmark.
- The project version is Unity 2017.2.0f3.
- WebGL-related serialized settings are present but use old defaults, including `webGLUseWasm: 0`; the scripting-backend settings are not populated as modern per-platform settings.
- The project includes Unity modules such as `unitywebrequest`, but module presence does not prove a WebGL build succeeds.

Recommended decision:
- Do not hand-edit legacy serialized settings as a substitute for a real Editor build. First make a copy/branch, open with the intended Editor version, capture upgrade warnings, then use a supported WebGL toolchain and save a separate migrated project.
- The existing workflow's Unity activation gate remains the immediate blocker. A successful asset export or source scan is not a successful player build.

## Recommended next gates, in order

1. Preserve the recovered project unchanged and make a separate port branch/copy.
2. Resolve a valid, legitimate Unity Editor build environment/activation. Do not bypass licensing.
3. Attempt a first WebGL build and save the complete Editor/compiler log as an artifact.
4. Fix only proven compile errors first. For the first compatibility pass, isolate Registry calls and disable ancillary server calls on WebGL; do not alter mission/AI/weapon behavior.
5. Produce a tiny playable scene build and test it on iPhone 16 Safari before attempting full-game touch controls.
6. Only after the first scene runs, expand the input adapter, persistence, campaign file flows, audio, rendering/performance, and lifecycle tests.
7. Maintain a parity matrix with observed evidence. No subsystem is parity-verified merely because it compiles.

## Current conclusion

The source audit reveals a concrete browser blocker beyond compiler compatibility: the game's configured backend URLs are HTTP while normal hosted browser delivery should use HTTPS, and browser security policies may block the requests. The likely lowest-risk route to a playable browser build is to preserve the recovered Unity gameplay/assets, isolate desktop-only integration (registry and arbitrary filesystem browsing), disable or replace ancillary server features for WebGL, and add a deliberate iPhone touch layer. This remains a plan grounded in static source inspection, not a claim that a build has been produced or tested.
