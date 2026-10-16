## Purpose and linked issue

Closes #<issue-number>. Describe one source-backed change and which original gameplay action it enables. Normal base branch: `develop/wasm`.

## Original developer source and provenance

- Original archive SHA256:
- Original C# file paths and exact hashes:
- Original prefabs/assets and GUID/fileIDs:
- Original method(s) executed (rather than merely parsed/compiled):
- Engine/browser adapter changes (no replacement gameplay):

## Acceptance and tests

- Native Unity parity reference or explicit reason it is unavailable:
- Fast source/contract tests and command output:
- WASM/Chromium/WebKit run URL, matching commit SHA and private artifact (when relevant):
- What **is not yet implemented or tested**:

## Review and safety checklist

- [ ] Branched from latest `develop/wasm`, PR targets `develop/wasm` (unless documented release/research exception).
- [ ] Original developer files/assets preserved unchanged, privacy intact.
- [ ] No invented gameplay, simulated source data, fake callbacks, speculative shaders or false parity claims.
- [ ] Source GUID/hash contract and executable regression tests included.
- [ ] Checked other open PRs and CI for overlapping files/runs; no unnecessary duplicate builds.
- [ ] `vercel.json` retains `git.deploymentEnabled: false`. No automatic deploy or public artifact hosting.
- [ ] Handoff and linked issue state whether code is recovered, compiled, executed, browser-tested, native-parity checked or device-tested.
- [ ] **Selective rollback:** list the exact commits/files and downstream impacts.

## Follow-up

One bounded next original-source dependency or blocker, not a new alternative implementation.
