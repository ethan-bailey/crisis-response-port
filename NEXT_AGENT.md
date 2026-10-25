# Crisis Response Port — current handoff

## Checkpoint

- Active branch: `develop/wasm`.
- Live and fetched HEAD: `387e274bd210a3842781a4cbac770cd4c7a0907e`.
- Worktree was clean after fetching this SHA. No open PRs were found before the
  change.
- Nothing was deployed. Site v39 remains the last owner-accepted iPhone Safari
  build. Desktop browser automation is optional; iPhone Safari acceptance stays
  open for new gameplay behavior.
- Follow `AGENT_INSTRUCTIONS.md`; batch related work, use the pinned .NET SDK
  8.0.426 for builds, and update this handoff once at the end of the next
  substantive batch.

## Latest batch: bounded Operator source delivery

- Added `scripts/package_fullsource_actor.py` to build the source-preserving
  dependency closure of the recovered `Assets/Resources/characters/Operator.prefab`.
- The generated profile contains 238 source assets and 998 records. Measured
  output: 9,554,760 gzip bytes / 24,296,523 expanded JSON bytes. Hard ceilings
  are 12 MiB compressed / 32 MiB expanded.
- Added `loadFullSourceOperatorPackage()` in
  `scene-data/scene-package-loader.mjs`. It fetches on demand and checks profile
  identity, byte ceilings, exact sizes, SHA-256, and the original prefab root.
- Wired release packaging and validation in `scripts/package_wasm_site.py`.
  The full scene smoke package remains behind the existing opt-in `scene-smoke`
  query parameter.
- Implementation source SHA: `387e274bd210a3842781a4cbac770cd4c7a0907e`.
- Checks run: package generator completed on the tracked recovered archive;
  Node syntax check, Python bytecode compilation, and `git diff --check` passed.
  No .NET build, release package build, test suite, browser smoke, or iPhone
  Safari acceptance was run. No deployment.
- Evidence: `docs/evidence/BOUNDED_OPERATOR_SOURCE_DELIVERY_2026-10-25.md`.

## Active integration blocker

The Operator-only profile is not yet sufficient to initialize the
`FullSourceActorBridge`. That bridge requires original `Global.Context`, its
`InputHandler`, an active original `PlayerController`, and the `CharacterBody`
in one `RuntimeWorld`. The tracked recovered scene archive does not contain
`Assets/Resources/Global context.prefab`. Loading the complete Mission 01 scene
closure would be 825 assets, 15,341 records, 144,342,051 JSON bytes, and
67,400,258 gzip bytes, which is not a bounded phone-startup path.

## Exact next action

Recover the exact original Global Context prefab from available source material
and generate a bounded same-world package for Global Context, Mission UI, and
Operator. Do not bind the isolated Operator prefab as a competing actor. If the
source remains unavailable, record the exact source search and continue only
with independent work that preserves the one-actor ownership rule. Once the
same-world package is ready, wire page actions/ticks, PhysX state, actor snapshot
rendering, and weapon/equipment callbacks through that FullSource actor; run the
combined runtime and package checks on one exact SHA, then request iPhone Safari
acceptance at the playable milestone. Do not deploy without the release gate.
