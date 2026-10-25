# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; live and local checkpoint before this batch:
  `5752f38a9c09eb4d0a6c2fffbde8aa35c4773a95`.
- This batch adds the guarded FullSource actor input/tick/snapshot bridge and
  contract coverage. Verification was run against the working tree based on the
  checkpoint above; the pushed commit SHA is recorded in Git history after this
  handoff is committed.
- Never deploy. Site v39 remains the last owner-accepted iPhone Safari build.
  Desktop browser automation is optional. Ask for owner iPhone Safari testing at
  the next major playable milestone and before release; do not claim behavior
  acceptance for features not present in the tested build.
- Keep the operator's screen-hold behavior distinct from holding the Fire
  button; it remains documented for later.
- Batch-first rule applies across development: inspect the relevant dependency
  group, collect actionable items, apply safe related changes together in
  dependency order, then run the combined checks once. See
  `AGENT_INSTRUCTIONS.md`.

## Latest verified batch

- Added `runtime/Engine/FullSourceActorBridge.cs` and browser interop methods in
  `runtime/Browser/Program.cs`. The bridge validates shared RuntimeWorld and
  actor identity, maps browser actions/pointer input, advances the original
  world, and snapshots the source actor's authored transform hierarchy.
- The bridge rejects stepping when the original Mission InteriorMode UI or
  `InteractionMenu` is absent. The original PlayerController.Update requires
  these references; the current headless Mission test suppresses that UI graph.
  Normal `game/index.html` still uses browser-side movement/animation and the
  transport `WeaponHost`; the new entry points are not yet wired to that page.
- Release Browser/WASM build passed; EngineChecks 69 and FoundationChecks 121
  passed. FullSource Operator/Occupant lifecycle and authored IK, Mission 01
  startup (2 Operators, 5 Occupants), and M4/Glock spawner/weapon/physiology
  contracts passed. Details: `docs/evidence/FULLSOURCE_ACTOR_BRIDGE_BATCH_2026-10-25.md`.
- The recovered full source closure has 1,918 assets / 22,361 records and 39
  assets with unresolved/unsupported references. It must not be shipped whole
  as the phone startup payload. The complete Global/Mission/Operator/UI closure
  needs a compact authored payload and runtime support for the original UI.
- The private authoritative archive remains in ChatGPT Library as
  `crisis-response-recovery-direct-v3-1.zip`, SHA-256
  `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
  Retrieve by exact filename and recheck the hash if needed; never commit the
  archive or extracted proprietary project. Runbook:
  `docs/DOTNET_BUILD_RUNBOOK.md`.
- Use pinned .NET SDK 8.0.426 with the existing process shim when available;
  follow `docs/DOTNET_BUILD_RUNBOOK.md`. Do not retry `dotnet-install.sh` or
  GitHub CLI login. Push verified changes through the GitHub API and verify
  with `git fetch`. Do not deploy.

## Gate state

- Gate 0: open. Normal-page FullSource flow, native physics, authored actors,
  and broader navigation/presentation remain.
- Gate 1: passed within scoped headless original-character acceptance; not full
  game parity.
- Gate 2: partial. Original FullSource controller/sequencer callbacks,
  Operator two-arm IK, actor isolation, and source actor snapshots pass. Actual
  page input/ticks still use the transport actor. Original UI initialization,
  live page projection, weapon/equipment migration, and physical continuity
  remain.
- Gate 3: partial. Source projectile-to-physiology evidence exists; native
  PhysX, rendered wounds, and complete live state/event integration remain.
- Gate 4: partial. Headless mission startup/spawning/outcome evidence exists;
  authored mission inputs and instantiation callbacks on the normal page remain.
- Gate 5: open. Original AI perception, navigation, decisions, and combat on
  the playable path remain unverified.
- Gate 6: partial. Headless room-clear/success contracts pass; complete normal
  path mission flow remains.
- Gate 7: open. Original presentation and content fidelity remain.
- Gate 8: closed only for the previously owner-accepted Site v39. No new
  gameplay behavior or device-acceptance claim is implied; nothing was deployed.

## Exact next action

Continue Gate 2 in one dependency batch: extract/measure the compact authored
Global Context + Mission + player UI dependency payload from the recovered
package; classify and implement only the original UI components needed for the
source PlayerController path; then run the original Mission in EntryTeam view
without suppressing its UI and verify browser action press/release through
`FullSourceActorBridge.Step`. If that source graph passes, wire the active page
input/frame loop to that one actor and project its transform snapshot to the
renderer/physics adapter. Migrate the source weapon callbacks under that same
actor before retiring transport authority. Keep the payload off default phone
startup until its size and memory use are bounded. Run lifecycle, input,
movement, IK, and weapon checks together. Request owner iPhone Safari testing
when this becomes a major playable milestone. Commit/push through the GitHub
API, verify the fetched branch head, and never deploy.
