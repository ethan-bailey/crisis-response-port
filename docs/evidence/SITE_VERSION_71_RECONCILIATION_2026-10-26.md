# Site version 71 deployment and source reconciliation — 2026-10-26

## Current Site state

Fresh read-only Sites checks on 2026-10-26 report:

- Site `Crisis Response Runtime`, project `appgprj_6acfef7174888191bc2bc2f269f60acc`.
- Latest/live version **71**; deployment `appgdep_6adec6d333188191a297e5ff157c996f` has status **succeeded** at `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
- Version source identifier: `37e6d5dcebaaba71c405d898d161b3e36e946a78`.
- Site archive metadata: tar, **178,012,160 bytes**, **2,686** files, SHA-256 `9efc3ee61c42624b6c8d078f3c4cb161b9f690656177c2fd08d7440034347040`.

The deployment return supplied in the preceding conversation reports that the Release WASM build and package verification passed and that an independent archive audit matched all **2,684 packaged files** before version 71 was deployed. That is accepted as prior run evidence. The Sites connector independently confirms the saved version, successful deployment, archive digest, byte count, and file count. It does not expose the saved archive bytes for a second local audit in this workspace.

The prior report's **2,684 packaged files** and Sites' **2,686 archive entries** differ by two entries. The archive itself cannot be inspected here to identify those entries, so record the package audit as reported pass and reconcile the two container/metadata entries when an authorized archive copy is available.

## GitHub source identity discrepancy

The deployment return says source SHA `37e6d5d` was pushed to both Site `main` and GitHub `develop/wasm`. A read-only check of the public repository does not confirm that GitHub side:

- `GET /repos/ethan-bailey/crisis-response-port/commits/37e6d5dcebaaba71c405d898d161b3e36e946a78` returned HTTP **422**, “No commit found for SHA”.
- `git ls-remote` reports `develop/wasm` at `2009015325d5c1bd6440c3875be60b750486547c` and `main` at `d7af0076274ff0ae4da3da5f84e2d346e94e3292`.
- Neither branch points at the Site source hash. The Site source identifier therefore is not proven to be a commit in this GitHub repository. It may belong to a separate Site source repository, but that repository/commit mapping has not been established.

Do not rewrite refs or publish another version to make these identifiers look aligned. Identify the exact source repository/branch used by Site version 71 or obtain an authorized copy of its archive and produce a path/size/SHA-256 crosswalk to a GitHub tree. This is the remaining provenance gate.

## Related acceptance and checks

- The Site version 71 return describes the principal change as moving the largest verified Mission assets earlier in delivery. Total compressed Mission delivery remains about **98 MiB**; this ordering change does not remove that cost.
- The owner has not yet confirmed the full Mission load, the stall near **1,900** assets, or repeat-run phone caching on the live version. Physical iPhone Safari acceptance remains open.
- Current public `develop/wasm` head `2009015` is documentation-only after code/probe head `c23d7f8`. Actions run **#33** (`40273869108`) passed on exact code/probe SHA `c23d7f8`.
- At current `2009015`, `node --test scripts/mobile_zoom_guard_test.mjs scripts/mission_particle_clamp_velocity_serialization_test.mjs scripts/scene_package_loader_test.mjs` passed **12/12**; `git diff --check` passed.
- No Site mutation or deployment was made during this reconciliation batch.
