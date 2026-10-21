# NEXT AGENT — Crisis Response WASM port

Updated 2026-10-20 after the PR #42 local verification and Actions-usage review.

## Current repository state

- PR #42 targets develop/wasm and remains open, draft, unmerged, and not mergeable. Keep it draft/open. Do not merge until all local acceptance gates pass and required CI is available.
- Feature branch: feature/full-source-characterbody-integration. At the start of this final handoff refresh, Codespaces HEAD and remote feature head were 61e64b27f010204fb1cb0e29e0e27000bc11b959. The working tree was clean. The current fetched origin/develop/wasm SHA was 2fc0a8e93be97c4d99123910fad4bd25798e71a8.
- No merge, rebase, deploy, preview change, reset, or clean was performed. PR #7, #56, and #57 were not touched.
- The runtime and original-source trees are unchanged from implementation SHA 26b07b660e892b9e97025bf2b8ebe700b7753764 through 61e64b27; that was confirmed with git diff --quiet over runtime, game, and source-reference/Original. The inventory commit adds source identities only.
- The original Codespaces stashes remain untouched: stash@{0} preserves the staged inventory before fast-forward; stash@{1} preserves the earlier pending Codespaces edits. The source inventory in stash@{1} is byte-identical to the committed 659-source inventory. Do not drop either stash while reconciling user changes.

## Gates verified in this continuation

- Dependency inventory: python3 scripts/inventory_source_dependencies.py --check passed. Output: 659 sources, 1,695 symbols, 1,329 reachable files, 119 ambiguous names requiring ownership review. The five recovered ammunition source identities are in commit 61e64b27; Calibre and UIAmmoRenderItem remain visibly flagged for ambiguous ownership.
- Runtime project compilation: at source SHA 61e64b27f010204fb1cb0e29e0e27000bc11b959, command dotnet build runtime/Crisis.Runtime.csproj -v:q exited 0; Build succeeded and compiler_error_count=0. Log: /tmp/pr42-runtime-build-61e64b2.log; SHA-256 c8f868835dc96319240f92812e2ef7832455120bae6c8a8d56a04ab771b97cb. This is a .NET runtime compile only, not a WASM gameplay build.
- Actor prefab provenance: source-reference/Original/CharacterDependencies/ActorPrefabs/Operator.prefab and its .meta match the recovered export byte-for-byte. Prefab SHA-256 94bcda5de74f45d83f4bbf88e80874b8f77bf0339d307294187b085c03170536; meta SHA-256 8d01cf639201add925686afbe85b7b1fd8a7bd8ec6c41b6749d76b568ef63b0d; GUID 2e00922aa35d9a8438c52f1ec69254605.
- source-reference/Original/CharacterDependencies/ActorPrefabs/Occupant.prefab and its .meta match the recovered export byte-for-byte. Prefab SHA-256 9094b840c49d6c53f1fa077c274561ab5cbeedb20f85bdaae023ad67cb8c419; meta SHA-256 3e39f95ea92613c4c4eb1c2c2c82775b9ed075cdf7a925d980ef5b3eca9c5737; GUID 8363ad318a650364ebdaa8c237f65bfe.
- Provenance comparison command used the preserved source-reference prefabs against /tmp/cpr-recovery-artifact-11903811738/export/ExportedProject/Assets/Resources/characters/{Operator,Occupant}.prefab and corresponding .meta files; all four cmp checks passed. This proves source identity and serialized bytes, not Unity lifecycle or runtime ownership.
- The earlier focused Calibre contract passed with exact output: PASS: original Calibre GUID, fileID, type, path, serialized name, and prefab lifetime. That pass came from the full scan at local HEAD 5833ab50742e8103cc1e2db4c80dfb59cd2c797c, using implementation SHA 26b07b660e892b9e97025bf2b8ebe700b7753764. Runtime/original-source files have not changed since that implementation SHA.

## Full scan and remaining blockers

The one consolidated Codespaces scan was bash /tmp/pr42-local-scan.sh at 5833ab50742e8103cc1e2db4c80dfb59cd2c797c. Sixteen of 17 gate commands exited 0; runtime_contracts exited 134 after the Calibre pass. Logs are in /tmp/pr42-local-scan-20261020/*.log and the summary is /tmp/pr42-local-scan-20261020/summary.txt. The committed complete triage is docs/evidence/logs/triage-2026-10-20-pr42.md.

Runtime contract failure:

System.NotSupportedException: Unity built-in object 0000000000000000f000000000000000:10303 has no source-faithful runtime mapping for expected Material; refusing a synthetic substitute.

Other unresolved source/dependency gates from the consolidated scan:

| Gate | Evidence | Required source or disposition |
| --- | --- | --- |
| Built-in Material and Shader | Material fileID 10303 stops the runtime contract; Shader fileID 46 is also unresolved. | Exact-version Unity built-in mappings, or human-approved deferment. Never synthesize. |
| Imported Sprite and texture objects | Sprite GUID 8464cbc48daa246479808d64b86fb8c2, fileID 21300000 lacks a demonstrated imported-object mapping. Earlier Operator texture failure was at SHA 3979faa642053d0f584d2f91982c1b88cbe0eb62, head 5.png, Texture2D fileID 2800000, textureFormat 12; it was not rerun in the latest full scan. | Exact Unity importer/object records, or human-approved deferment. Do not substitute PNG pixels. |
| .308 Mesh | GUID b4b69dc6c74eeb54cbd22d28e37f9a19, Mesh fileID 4300000, absent from the recovered export and repository metadata. | Original Mesh and matching .meta, or human-approved deferment. |
| Mixer sub-assets | Five GUID identities referenced by Master mixer.mixer are absent from the recovered export and repository metadata. | Original mixer dependencies and metadata, or human-approved deferment. |
| Binary object records | Earlier scan found 452 audio/image binary references without serialized object records; file presence alone is not proof of Unity object identity. | Recover original Unity records, or human-approved exception/deferment. |
| Actor lifecycle and ownership | Actual Operator/Occupant lifecycle, unknown serialized fields/components, and independent physiology ownership have not passed runtime checks. | Resolve original components/imports and test actual prefabs; keep unknowns visible and fail closed. |
| Controller playback | Audit reports controllerPlaybackImplemented=false. | Implement original callback playback from source and verify. |
| Two-actor gameplay | No full original spawn/equip/movement/hit/physiology/destruction flow has passed. | Complete after source-faithful actor lifecycle/controller work. No synthetic actors. |
| Browser/game acceptance | Previous Chromium/WebKit evidence tested startup and input only. No full gameplay test on the exact current source, final WASM build, or final local HTTP smoke exists. | Run only after source, lifecycle, and two-actor gates pass; integrate develop/wasm with a normal merge commit immediately before final verification. |

PR #42 is not merge-ready: the local runtime contract still fails and the actor/controller/gameplay/browser gates remain incomplete. Full counts and proposed human dispositions are in the consolidated triage and the refreshed REA evidence.

## Actions usage

The owner reports Actions credits will not return for roughly one month from 2026-10-20. No workflow was dispatched during this continuation. For commit 61e64b27, the GitHub workflow-run lookup returned an empty list and combined status returned no checks; this is limited to the PR head and does not enumerate unrelated repository runs. Nothing on this PR head was available to cancel. Deleting artifacts would reclaim storage, not minutes. Do not touch billing, enable paid usage, or cancel unrelated workflows.

## Recovery provenance and next steps

- Original recovery workflow run 38582072502; artifact 11903811738; archive SHA-256 cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d.
- Next, continue with source-faithful Unity dependency and actor lifecycle work in batches. Run the focused contract after a genuine source-backed fix; keep unrecoverable dependencies explicit for human disposition.
- Once local gates pass and Actions credits return, integrate the then-current develop/wasm using a normal merge commit immediately before the exact-source WASM build, full Chromium and WebKit gameplay checks, and local HTTP smoke. Keep PR #42 open/draft and do not claim merge readiness before that evidence exists.

## Latest continuation — 2026-10-21

The next source-package gate is closed: mixer parameter GUIDs are no longer misclassified as asset edges, and the original ammunition package was regenerated. Code commit 1cd7294287021c0c5e5cb7b2410932108b9a83a5 is pushed. Focused evidence and exact commands are in the 2026-10-21 continuation section of docs/evidence/logs/triage-2026-10-20-pr42.md and docs/evidence/REA-ammunition-closure-2026-10-20.md.

The recovered export still lacks source-faithful Unity built-in Material 0000000000000000f000000000000000:10303, so the full runtime contract stops at the original Projectile fragments prefab. Keep that gate fail-closed. Actor lifecycle still needs the authored Mission context; independent two-actor gameplay, source-backed controller playback, final WASM build, Chromium/WebKit gameplay, and HTTP smoke checks remain open. The animation audit's controllerPlaybackImplemented:false is a real implementation gap, not a passing check.

PR #42 remains open/draft. Read-only check on code SHA 1cd7294287021c0c5e5cb7b2410932108b9a83a5 found no workflow run or combined status; Actions were not dispatched. develop/wasm was observed at 2fc0a8e93be97c4d99123910fad4bd25798e71a8. Preserve the user's untracked runtime/Browser/wwwroot/browser-input.mjs. Do not integrate develop until immediately before final verification.


## Projectile material source gate  2026-10-21

- Searched the verified Unity 2017.2.0f3 recovery artifact (workflow run 38582072502, artifact 11903811738, archive SHA-256 cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d). It contains the original Assets/GameObject/Projectile fragments.prefab but not the Editor/Player built-in record for Material fileID 10303 or Shader fileID 46. Exact built-in identity remains unavailable.
- Owner authorized an alternative if the exact source could not be found. The recovered project contains Assets/Material/Projectile chunks.mat (GUID f20eaaaec00f311469b053da1a02332f, fileID 2100000) and its authored Shader Forge_Lit (Basic).shader (GUID 58809839c7f61d54f8933d60fa11e0a9, fileID 4800000). Exact files and matching .meta files are preserved under source-reference/Original/Assets/{Material,Shader}; byte comparisons against the recovery artifact passed.
- The scoped compatibility disposition is source-reference/Compatibility/Projectile-fragments-material-replacement.json. It records source artifact identity and hashes, the original unresolved pointer, the candidate pointer, and the prefab scope. The original prefab remains unchanged. This candidate is not identity-equivalent evidence for built-in Material 10303 and does not establish Shader 46.
- Source-discovery/disposition is complete; runtime replacement is not wired. The next actionable step is a strictly scoped runtime/package overlay for only the original fragment prefab renderer references, retaining the original serialized pointer and candidate provenance, then run the focused runtime contract. Until that passes, the material runtime gate remains open. Other actor/controller/gameplay/browser/CI gates remain open; keep PR #42 draft and do not merge.
