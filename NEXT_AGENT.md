# NEXT AGENT — Crisis Response hosting

## Checkpoint

- Branch: develop/wasm. Tested source SHA: 35f077f293305694f5a279bc073f23e159aba0fe. The current handoff is one docs-only commit on top of that tested source; read the branch head from GitHub before continuing.
- PR #42 merged as 09987a75d671d778287605322394c474da1ed74a; feature branch retained.
- Current open draft PRs at inspection: #56 and #57 target develop/wasm; #7 targets main. PR #42 is merged and is not open.
- GitHub Actions remains disabled by owner decision. No Actions workflow was used in this hosting attempt.
- Deferred behavior from PR #42 remains applied: 106 develop-only members and 36 develop-only types are deferred. LimbLayer, LimbRenderer, and LimbTextureData remain only as source-reference evidence. CharacterLimbRagdollState.cs removal was accepted; OriginalLimbSkinTrace, SourceSkinTrace, OriginalArmPhysicsHost develop-only additions, and the associated consumers remain deferred.

## Hosting attempt — blocked by local smoke

The finish line was a public ChatGPT Site running the current develop/wasm build. This attempt did not reach publication because the hard local smoke gate failed. No public Site or Vercel deployment was created.

### Build and gameplay gates

- Fresh full-source command: dotnet build runtime/Crisis.Runtime.csproj --no-incremental
- Tested source SHA: 35f077f293305694f5a279bc073f23e159aba0fe
- Result: PASS, 0 errors, 45 warnings. MSBuild reported 24.6 seconds; terminal wall time was 27.759 seconds.
- Browser runtime publish: dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release
- Result: succeeded, 45 warnings, 0 errors, 106.1 seconds. Output: runtime/Browser/bin/Release/net8.0/publish/.
- Two-actor command: dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -- --prefab-lifecycle /workspaces/crisis-response-pr42-9479dc6/work/runtime/scene-package.json
- Result: FAIL, exit 134. RuntimeWorld activation failed for CharacterBody.Awake on Operator(Clone); inner exception was System.NullReferenceException: Object reference not set to an instance of an object. The verifier stopped at Operator; a complete Operator/Occupant lifecycle pass was not established. The exact null member was not identified by the thrown exception.

### Package and integrity manifest

A staging package was assembled from the compiled browser publish wwwroot, game/assets, scene-data/marksman-range.json, marksman-combined.obj, and marksman-combined.mesh-report.json. It contains 719 payload files plus integrity-manifest.sha256: 720 files total and 63,613,523 bytes. sha256sum -c verified all 719 manifest entries. The package was temporary at /tmp/crisis-sites-package-35f077f; it was not uploaded or committed.

### Local browser smoke — FAIL (hard gate)

- Served the staged package with python3 -m http.server 8001 --directory /tmp/crisis-sites-package-35f077f in the Codespace and opened the forwarded port in a real Chrome session.
- The page loaded and displayed the runtime heading and LEFT/RIGHT/SPRINT/FIRE/KNEEL/STAND/RELOAD/SWITCH controls.
- The page then displayed “Runtime error. Reload to reset.” Framework .wasm requests returned HTTP 200, but browser-input.mjs returned HTTP 404. Clicking FIRE did not produce an input-state update.
- No Marksman scene loaded. The page itself says scene loading uses extracted data and that full gameplay is incomplete.
- One bounded packaging/server-path attempt staged the compiled output together with the available game assets and Marksman data and repeated the smoke; the same runtime error remained. The input module is absent from the develop checkpoint’s published output. Do not copy an untracked helper from another checkout or alter game code in this hosting-only task.
- Result: local smoke FAIL. The package is not ready for publication. Sites feasibility, public access, and live smoke were NOT RUN because local smoke failed. Vercel fallback was NOT RUN because Sites was not attempted; Sites hosting feasibility is only evaluated after a locally passing package exists.

## Hosting policy and next actions

ChatGPT Sites is the primary host. Preserve the verified package byte-for-byte, set public access only under the owner’s authorization, and require local and live smoke. Vercel is manual fallback only if Sites cannot host a package that already passes local smoke; automatic Vercel deployments remain disabled in vercel.json.

1. Resolve the browser runtime’s missing browser-input.mjs dependency from tracked develop/wasm sources and rerun the local smoke, including actual Marksman scene loading and responsive input.
2. Resolve the CharacterBody.Awake null reference and rerun the two-actor gameplay lifecycle gate for both actors.
3. After the local smoke and relevant gameplay gate pass, resume the Sites deployment, verify public access and asset/WASM serving, and perform fresh-session live smoke. Use the same verified bytes for any manual Vercel fallback.
4. Keep the exact-source browser smoke gate open until it passes against the intended package; do not report a URL as proof of a boot.

Sites remains the primary host; this handoff changes no runtime or game source. The next session should start from the live develop/wasm head and this failure evidence.