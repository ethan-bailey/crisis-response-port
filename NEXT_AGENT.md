# Crisis Response Port — current handoff

## Checkpoint

- Work branch: `develop/wasm`; verified source commit for the latest batch is
  `6613a675eec4968929939b5f4db4cbe87f4df7f5` (based on
  `6f91910f45db36a1caac39168a1fa3b2ad494c2a`). Fetch the live branch before
  resuming; a handoff-only commit may sit immediately after the source commit.
- Never deploy. Site v39 remains the last owner-accepted iPhone Safari build.
  Desktop browser automation is optional. Request owner iPhone Safari testing
  at the next major playable milestone and before release; do not claim device
  acceptance for behavior absent from that tested build.
- Keep screen-hold aiming distinct from holding the Fire button; it remains
  documented for later.
- Batch-first applies across development. Inspect the relevant dependency
  group, fix safe related issues together in dependency order, then run the
  combined relevant checks. See `AGENT_INSTRUCTIONS.md`.

## Latest batch

- Implemented serialized Canvas, RectTransform, CanvasScaler, GraphicRaycaster,
  Graphic, Image, and Text UI state; retained UI material/sprite references
  through prefab clones.
- Fixed external GUID resolution to take precedence over a colliding local
  file ID. This resolves `WeaponPointer.aimQuadPrefab` to the original Aim quad
  and prevents recursive Interior mode UI cloning during activation.
- Fixed `FullSourceActorBridge.Step` so multiple source actions mapped to one
  physical key are combined as a held-key union; false actions no longer undo
  another pressed action.
- Verified the original UI prefab import and clone. Verified Mission 01 in
  EntryTeam mode with its authored UI active: original InteractionMenu started,
  2 Operators and 5 Occupants spawned, and bridge left press/release reached
  the original InputHandler. This is a test-host contract, not browser or
  device acceptance.
- Final verification passed: Release runtime build; EngineChecks 69;
  FoundationChecks 121; runtime lifecycle/input/IK/weapon/ballistics contracts;
  source UI prefab contract; headless and active-UI Mission contracts; Release
  Browser/WASM publish. Evidence:
  `docs/evidence/AUTHORED_UI_IMPORT_AND_BRIDGE_INPUT_2026-10-25.md`.
- Full Global Context + Mission 01 + Operator source closure remains about
  1,918 assets / 22,361 records / 122,132,020 source bytes, with 39 assets
  containing unresolved/unsupported references. Keep it out of default phone
  startup until payload and memory are bounded. The original browser layout
  driver and several original built-in assets/components remain unsupported.
- Do not commit the private archive or extracted proprietary project. Use
  pinned .NET SDK 8.0.426 and the process shim as documented in
  `docs/DOTNET_BUILD_RUNBOOK.md`. Do not retry `dotnet-install.sh` or GitHub
  CLI login. Push through the connected GitHub API and verify with `git fetch`.

## Gate state

- Gate 0: open. Normal-page FullSource flow, native physics, authored actors,
  and broader navigation/presentation remain.
- Gate 1: passed within scoped headless original-character acceptance; not full
  game parity.
- Gate 2: partial. Original controller/sequencer callbacks, Operator two-arm
  IK, actor isolation, source snapshots, original UI initialization, and
  source input press/release pass in the test host. `game/index.html` still
  uses browser-side movement/animation and transport `WeaponHost`; actor
  snapshot projection, weapon/equipment migration, and physical continuity
  remain.
- Gate 3: partial. Source projectile-to-physiology evidence exists; native
  PhysX, rendered wounds, and complete live state/event integration remain.
- Gate 4: partial. Headless mission startup/spawning/outcome evidence exists;
  authored mission inputs and instantiation callbacks on the normal page
  remain.
- Gate 5: open. Original AI perception, navigation, decisions, and combat on
  the playable path remain unverified.
- Gate 6: partial. Headless room-clear/success contracts pass; complete normal
  path mission flow remains.
- Gate 7: open. Original presentation and content fidelity remain.
- Gate 8: closed only for the previously owner-accepted Site v39. No new
  gameplay behavior or device-acceptance claim is implied; nothing was
  deployed.

## Exact next action

Batch the normal page migration: wire `game/index.html` input and frame ticks to
the existing FullSource bridge endpoints; project actor-owned transform
snapshots to rendering and physics; then migrate source weapon callbacks under
that actor before retiring transport authority. Keep the full source closure
off default phone startup. Run lifecycle, UI, input, movement, IK, weapon,
browser build, and package-integrity checks together. Request owner iPhone
Safari testing at the next major playable milestone. Commit/push through the
GitHub API, verify the fetched branch head, and never deploy.
