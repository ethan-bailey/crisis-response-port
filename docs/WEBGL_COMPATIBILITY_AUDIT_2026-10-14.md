# WebGL compatibility audit — 2026-10-14

## Scope and method

Static inspection of the recovered AssetRipper project in the verified GitHub Actions artifact `crisis-response-recovery-direct-v3-1`. This is not a Unity compilation or runtime test. No original archive or game binaries were committed to the repository.

## Confirmed recovery contents

- Unity project version: `2017.2.0f3`.
- Six enabled build scenes: Disclaimer, Title, Transition, Mission 01, Marksman range, Benchmark.
- 1,420 C# files under the recovered project's Assets tree (including Unity Standard Assets and third-party task scripts).
- Project contains Assets, ProjectSettings, Packages/manifest.json and recovered scene files.
- The original gameplay DLL hash matches the one extracted from the user's archive, as documented in RECOVERY_RUN_2026-10-14.md.

## Potential WebGL blockers found by static scan

These are evidence-backed candidates for repair, not confirmed compiler errors until a Unity build is run.

1. **Threaded HTTP requests**
   - `Assets/Scripts/Assembly-CSharp/Threading/ThreadedJob.cs` starts a `System.Threading.Thread` and uses `Thread.Abort`.
   - `HTTPRequest.cs`, `HTTPGet.cs`, and `HTTPPost.cs` inherit/use this threading pattern and `System.Net.WebRequest`.
   - Unity WebGL does not support general-purpose managed threads in the same way as desktop builds, and browser networking must respect browser APIs/CORS. Replace with coroutine/asynchronous `UnityWebRequest` paths where the feature is required; preserve offline gameplay if these requests are telemetry/version checks.

2. **Windows Registry dependency**
   - `GlobalContext.cs` imports `Microsoft.Win32` and calls `Registry.CurrentUser.DeleteSubKey` in the video-settings reset path.
   - Guard this code for desktop-only builds or provide a WebGL-safe implementation. WebGL must not attempt Windows registry access.

3. **Desktop file browser and file persistence**
   - `FileBrowser.cs` uses `System.IO` directory enumeration, attributes, directory creation, and Windows-style path normalization.
   - `CampaignManager.cs`, `EngagementRules.cs`, `SerializableCampaign.cs`, and `Serializer.cs` use filesystem persistence/serialization.
   - WebGL storage is sandboxed and does not expose arbitrary desktop paths. Adapt saves to supported persistent storage/IndexedDB-compatible Unity mechanisms and disable or replace arbitrary file browsing/import/export flows.

4. **Input assumptions**
   - Recovered code includes many mouse/keyboard input reads. They may remain useful for desktop browsers, but iPhone play requires explicit touch/UI mapping and a test pass for gestures, scrolling, text input, and menus.

5. **Legacy Unity version**
   - The project targets Unity 2017.2.0f3. The project may require migration and script/API repairs to use a supported WebGL toolchain. Do not silently upgrade serialized project files before preserving a copy of the recovered artifact.

## Scan results

- No `DllImport` or `LibraryImport` matches were found in the recovered Assets C# files during this scan.
- No direct `Process.Start` matches were found.
- Found one direct `Microsoft.Win32.Registry` usage.
- Found a threaded HTTP request path using `Thread`, `Thread.Abort`, and `System.Net.WebRequest`.
- Found multiple filesystem operations and `BinaryFormatter` save/load paths requiring WebGL storage review.

## Next executable gates

1. Obtain a valid Unity Editor activation for the cloud build. The current Actions run confirms the Unity build step is skipped because `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD` secrets are not configured.
2. Run the WebGL build using the recovered project and preserve the complete compiler log.
3. Fix only errors proven by the compiler log, starting with the platform-specific blockers above.
4. Publish a build artifact only after successful compilation.
5. Test on iPhone 16 Safari: startup, scene transitions, menu input, mission entry, movement, weapon controls, audio, save/load, background/resume, memory, and frame rate.
6. Compare observed results against the original Windows game and keep an explicit parity/gap list.

## Current conclusion

The recovered project is a substantial and useful starting point, not yet a playable browser game. The immediate hard gate is Unity activation; after that, compile logs will determine the exact repair order. This audit must not be interpreted as a successful WebGL build.
