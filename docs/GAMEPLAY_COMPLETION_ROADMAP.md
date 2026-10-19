# Gameplay completion roadmap

## Goal and acceptance standard

Complete the original Crisis Response game as a browser game that can be played
on iPhone Safari with touch controls. Original source methods, assets, update
order and authored scene data remain the gameplay authority. Approved iPhone
usability improvements, including proportional joystick speed, remain part of
the target. A compiler pass alone does not establish gameplay parity.

“100%” means every original player, weapon, enemy, mission, interaction, injury,
animation, presentation and progression path in the recovered game has a
source-backed implementation and a passing acceptance check. Unsupported
actions must stay explicit until their original behavior is connected.

## Priority order

| Priority | Work | Completion evidence | Status |
|---:|---|---|---|
| 1 | Reconcile and checkpoint the current CharacterBody dependency batch. Preserve the exact branch state, review all staged and recovered files, refresh source inventories, and run narrow source/ownership checks. | A reversible checkpoint commit or hash-checked patch/archive; inventory, provenance, migration-boundary and audit tests pass. Record any build limitation without converting it into a pass. | Complete as a local checkpoint bundle. Git commit/push is blocked by missing unchanged Git objects and no configured remote. |
| 2 | Make `Crisis.Runtime.FullSource.CharacterBody` the sole gameplay actor owner. Connect original Awake/lifecycle, limbs, physiology, blackboard, equipment, weapons, projectile callbacks and IK to the same source actor instances. Keep the transport actor out of authoritative gameplay. | Full original-source compile; actor host instantiates the source actor with its authored components and all 19 registered limbs; no live gameplay caller creates or mutates a second CharacterBody; exact source and ownership checks pass. | In progress. Original Operator/Occupant prefab graphs are recovered verbatim and source-verified. `runtime/Engine/WeaponHost.cs` still creates `Crisis.Runtime.Weapons.CharacterBody`; no runtime prefab loader or two-actor test is connected. |
| 3 | Prove the actor lifecycle with two independent original actors. Exercise spawn, equipment, movement, projectile hit, source callback order, wound and physiology progression, unconsciousness/death, ragdoll and destruction. | Native actor tests prove shared per-actor component identities, independent vitals, no self-hits and original event order. | Not started; blocked by priority 2. |
| 4 | Connect player controls and movement to the original actor update path. Preserve keyboard behavior, touch gestures, proportional joystick speed, dead zone and original acceleration. | Native/input tests plus browser checks for movement, aiming, sprinting, firing intent, pause and focus cancellation. | Existing source-backed input work is not sufficient evidence for the integrated original actor path. |
| 5 | Integrate original animation, weapon posing and arm IK on the actor. Wire `IKArmController`, `IKArmSequencer`, `ArmController` and arm physics to the source-owned limbs and targets. | Source action sequences run through the real actor; transition and interruption tests cover supported actions and explicit unsupported boundaries. | Not started; follows priority 2. |
| 6 | Complete physics and body state: collision ownership, joints, muscle control, limb constraints, ragdoll, recovery and self-hit filtering. | Two-actor collision, joint and state-transition checks with source-authored parameters. | Not started; follows priorities 2–3. |
| 7 | Complete original combat flow: weapons, magazines, bolts, reloads, grenades, projectiles, callbacks, hit effects and weapon drops. | Source-order action tests and two-actor combat checks cover each original weapon/action path. | Partial work exists on separate branches; shared actor acceptance is pending. |
| 8 | Complete injury and physiology behavior, including wound placement, blood loss, circulation, oxygen, pain, shock, consciousness, death and treatment. | Controlled fixtures use original source calculations and assets; live actor tests show the same chain from impact through outcome. | Source-backed slices exist; live actor acceptance is pending. |
| 9 | Connect NPC behavior, team control, mission rules, interactions, doors, traps, negotiation, scoring and mission completion. | Each original behavior tree and mission action is exercised against the shared actor/world graph. | Not started as an integrated game loop. |
| 10 | Close the full parity inventory across original scenes, missions, weapons, enemies, settings, UI, audio, particles, shaders, saves and progression. | Every recovered feature is mapped to an implementation and a reproducible acceptance check; no unexplained omissions remain. | Not started. |
| 11 | Build the actual WASM game and verify browser runtime, packaging, iPhone layout and touch gameplay. Keep WebGPU capability diagnostics isolated from game rendering. | One exact tested source SHA passes WASM build, localhost HTTP smoke, Chromium and WebKit game interactions, then a real iPhone Safari check. | Not verified for this branch head. |
| 12 | Publish a preview of that exact tested game build. | Preview source/artifact matches the tested SHA, the game boots from the link, and its access behavior is verified. | Blocked by priorities 2–11 and exact build/browser evidence. |

## Current checkpoint

- Repository: `ethan-bailey/crisis-response-port`, private.
- Active branch: `feature/full-source-characterbody-integration`.
- Branch head inspected on GitHub: `1077c79ae1aa919b17b17b4007bdf777fba41db1`.
- Target `develop/wasm` inspected on GitHub: `2fc0a8e93be97c4d99123910fad4bd25798e71a8`.
- Open overlapping lanes: #7 (historical input/movement), #56 (projectile/limb),
  #57 (IK) and #42 (full CharacterBody). Keep their ownership boundaries intact.
- No Actions run is attached to the exact #42 head `1077c79`. The latest full
  source compile evidence documented on #42 remains a failing older checkpoint;
  it must not be reported as a pass for the current source batch. The successful
  #56 WASM/browser run is from a different branch and does not validate this
  CharacterBody integration.
- The checkpoint bundle preserves every staged path against parent
  `1077c79ae1aa919b17b17b4007bdf777fba41db1`; its companion manifest records
  exact path and SHA-256 lists. A Git commit cannot be written because the local
  object database is missing 820 unchanged blobs; the checkout also has no
  configured Git remote. It has no .NET SDK, so a fresh C# or WASM build is
  unavailable here.
- Current local checks: inventory freshness passes (646 sources, 1,680 symbols,
  1,326 conservatively reachable files, 116 ambiguous names); actor migration
  boundary and strict ownership scan pass; 477 recovered-source identities pass;
  all 8 audit-tool tests pass; original actor prefab extraction/check passes; and
  `git diff --check` passes. These checks do not establish compilation, browser
  boot or gameplay parity.
- Original `Operator.prefab` and `Occupant.prefab` are preserved verbatim under
  `source-reference/Original/CharacterDependencies/ActorPrefabs/`, with original
  `.meta` files, SHA-256 provenance and a structural inventory. The pinned
  archive check confirms Operator: 70 GameObjects, 120 MonoBehaviours, all 13
  required root components and the original 19 limb names; Occupant: 56
  GameObjects, 117 MonoBehaviours and the same 19 limb names. This is source
  asset recovery, not runtime instantiation. The current M4 transport fixture
  still does not create that graph.

## Immediate next work

1. Hydrate a complete Git checkout or restore its missing base objects, then
   commit and push the preserved source checkpoint to PR #42.
2. Build the runtime importer for the recovered actor prefab component graph,
   including local component references and source lifecycle ordering. Do not
   manufacture substitute gameplay state. Replace the weapon fixture's transport
   actor only after its source actor dependencies can be constructed and tested
   together.
3. Run the isolated C# checks and one deliberate full original-source build in an
   environment with the repository's .NET 8 toolchain. Keep the real exit code.
4. Continue the two-actor acceptance sequence before broad mission/NPC work or a
   new public game preview.

## Revert and evidence notes

This roadmap is documentation. Revert it independently with
`git revert <roadmap-commit>`. Source checkpoint commits must be reverted as
ordinary commits in reverse order; do not reset `develop/wasm` or `main`. Keep
the full-source compiler result, actor tests and browser evidence tied to exact
source SHAs, and keep a failed compiler result as a failure.
