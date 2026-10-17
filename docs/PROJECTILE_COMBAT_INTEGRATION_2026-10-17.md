# Original projectile combat / injury integration checkpoint

PR #56, branch `feature/original-projectile-limb-hit-runtime`. The starting head
`c61d19ea8190e5551467f38a5dfb62cc89beb7e6` compiled successfully with .NET 8
Roslyn; no compiler fix was necessary. The source had unbound runtime callbacks.

## Executed original behavior

- `Projectile.RecordAttackInfo`, `CharacterBody.OnSuccessfulAttack`,
  `OnWasAttacked`, `CanSeePosition` and original `CharacterScoreCard` now execute. Original IsCompliant reaction windows are retained, including the half-second compliance delay.
- `OnWasHit` retains its complete original conditions and order, with required
  mission, bullet-time, audio, marker and weapon-drop capabilities at their engine
  boundary. They are **not installed in live gameplay**. Tests use explicit
  observers which are never installed by `WeaponHost`.
- The dispatcher runs tissue interaction, records the attack, sets LastHitLimb,
  runs OnWasHit and invokes both original projectile event boundaries. Empty
  UnityEvents are valid. There are no required fake event listeners.
- FirstHitCharacter follows the original assignment on every hit; success is
  notified only on the first non-marksman character per projectile. The original
  swarm-ID comparison deduplicates ShotsHit across projectiles. Marksman success
  is a mandatory typed capability and remains separate from non-marksman credit.
- Flight ray queries skip owning character limbs and retain the nearest remaining
  eligible collision and requested layer mask. Direct owner contacts do not
  change tissue, energy, hit history or success credit.
- Source physiology now updates circulation, limbs/hemorrhage, oxygen and
  consciousness in original order. Previously the state decision preceded the
  current frame's hemorrhage. Direct physiology-only callers retain the existing
  combined update API.
- Unity Mathf.Clamp uses ordered source comparisons, including inverted bounds.
  System.Math.Clamp threw after genuine source hemorrhage reduced blood volume
  below zero, preventing the later original unconsciousness/death sequence.

## Verification

Locally compiled runtime + native checks with .NET SDK 8.0.130 Roslyn targeting
.NET 8 reference assemblies. `--projectile-injury-chain` passes seven contracts:
mandatory preflight, owner exclusion, source notifications/events, first-hit and
swarm accounting, score penalties, query masks, and the original ammunition to
cavity/wound/circulation/hypoxia/unconsciousness/death sequence. The latter uses
actual recovered M4 ammunition and authored flesh properties, the SHA-pinned recovered
Operator Torso PNG alpha mask in a controlled marksman fixture, distinct source CharacterBody instances, and an
observed rigidbody force boundary. It is **not a live NPC gameplay test or a
native-Unity ragdoll parity comparison**.

Existing `--source-tissue-penetration`, `--source-limb-interior`, and
`--character-systems` contracts pass. `scripts/verify_original_combat_source.py`
checks method bodies against preserved original developer source and hashes the
newly preserved CharacterScoreCard from the pinned original archive. The archive
SHA is `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.

A single milestone workflow compiles/packages .NET WASM, runs the same injury
contracts in Chromium and WebKit on isolated runtime pages, and runs existing
browser gameplay checks. Its exact commit/run/result must be recorded after it
finishes; local native evidence does not establish browser success.

## Remaining gates

The 19 original live limb colliders remain disabled. Do not enable them until
live source mission/audio/time/marker/drop-weapon capabilities, actual distinct
NPC import, the source damage-texture wrapping and dynamic wound-rendering path
are connected and verified. Source LimbTextureData creates a new staticDamage
Texture2D; the PNG importer's clamp setting alone is **not** proof of that runtime
texture's wrap behavior. GPU blood particle/stream emission and the full original
MuscleControl/body-state dependency graph remain incomplete.

The browser diagnostic endpoint refuses to run after presentation or weapons
have initialized, avoiding accidental reset of an active game clock/world. The updated protected iPhone preview is recorded in the verified release section below. PR #57 (IK), #42
(complete CharacterBody), and #7 (Unity) remain separate; this PR stays draft.

Rollback the changed combat/flight methods, physiology ordering and Mathf adapter
as one reviewed unit, along with their focused contracts and workflow hooks. Do
not revert earlier original limb/source assets or unrelated mobile enhancements.

## First full-run failure and repair

Run [38997480801](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38997480801)
at `9fc5f4c97c2d77c5113ab822601e2c9318b1005e` compiled native C# but stopped
before WASM: the existing WeaponContracts fixture omitted the source armPhysics
bundle required by WeaponHost. It now composes original full-body physics and
limb data exactly as the browser loader does. A new ProjectileLimbEvent also
requires Serializable for real projectile prefab cloning; the full native
31-shot/reload fixture now exercises that path and passes.

The old IK source generator still declared Component while the existing runtime
had already changed to Behaviour for ragdoll-enabled state. Update the generator
and provenance hash for that explicit engine inheritance adaptation, keeping all
selected original method bodies intact. Source verification now uses pipefail
so a stale individual check cannot be hidden by a subsequent successful check.

The final injury fixture selects original Torso LimbRenderer transform
`4617365563186364`, skin PNG SHA256
`b361dfbd06cc0d25ef057ca7b30d8367abb47f3f4acd78bbdf5c93ab1d590a5f`,
and its alpha-derived initial damage channel. Geometry placement is still a
controlled diagnostic, not a live NPC. A source equivalence check also covers
original IsCompliant getter/setter timing rather than an instantaneous flag.

## Exact live dependencies still requiring original execution

`IOriginalCharacterHitEffects` must bind actual recovered systems. In particular,
Mission.FirstInjuryTimestamp writes only while its backing value is zero; a raw
float assignment is not a production replacement. CharacterAudio.Hurt uses the
original samples, delay/cooldown, consciousness condition and AudioPropagator.
InvokeBulletTimeOnHit calls TimeScaler with source player/computer settings.
ShowStabilizeMarker uses original mission/occupant state and marker assets.
DropWeapons(bool attachToRagdoll) includes source equipment, dead-grip,
weapon-physics and original IK callbacks; recording an event is insufficient.

The original CharacterBody.SetRagdoll additionally checks WeaponDropTime,
disables the target, starts a readied grenade fuse and invokes each source
Limb.OnBecomeRagdoll before EventRagdoll. SetUnconscious stops voice audio and
releases grab joints. Full Physiology.SetCharacterState also runs MuscleControl
spasms, recovery and death responses. Existing subset release/impulse tests do
not establish this complete graph.

The original Limb.HandleExit may create an additional exit wound inside its
useWoundDecals branches. Those branches consume source renderer/RNG operations;
do not copy only an exit-wound formula and silently skip its dependencies. The
live source damage-texture/renderer path, associated wound streams/particles and
source organ/nerve responses are still required for complete wound propagation.
Actual mission NPC loading and original behavior-tree initialization must also
be connected; a synthetic target inserted at an arbitrary position is not the
requested original game integration.


## Verified build and iPhone preview

- Tested gameplay/source SHA: `a3e792744b590502b9c8af1c98ed0bd7d84b8702`.
- Full [Actions run 38998115604](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38998115604): SUCCESS. Native contracts, all seven combat source members, seven native and Chromium/WebKit injury contracts using original Torso skin, existing gameplay/input/weapon/PhysX smoke and package validation passed. 50 source/input regressions and 12 package integrity tests passed. Physical iPhone Safari remains untested.
- Evidence artifact `12088152433`; private site artifact `12087928555`; retained until 2026-11-16. Both final browser injury results explicitly report `originalTorsoSkin=true` and `liveNpcGameplay=false`.
- Package verifies 742 indexed files, 199 Blazor resources and 195 WASM files.
- [Staging run 38998497740](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38998497740): SUCCESS; verified artifact reused without rebuilding. Snapshot Git SHA `d6122e004d66a7d80746d24bba06ebc60475e92b`; staged build-manifest sourceCommit independently read back and matched the tested source SHA.
- Vercel preview `dpl_HJk265kAiwBra1A8JypKZyQmJ4QF`: READY, project `prj_EgsDBXSKhvina2fDecbdMf6ylTKE`, exact snapshot SHA confirmed in deployment metadata and stable alias. Project SSO protection remains enabled. Git auto-deploy remains disabled and the staging workflow was restored to manual-only after success.
- Stable device-test URL: https://crisis-response-port-git-preview-w-6bd30d-ethanbaileymarsh-8332.vercel.app/game/
- Both final browser gameplay tests pass. The final WebKit screenshot was visually inspected: original hands visible, original magazine displays 29 available rounds for 28 magazine + 1 chamber, and mobile joystick/buttons remain intact.
- Independent protected HTTP fetch could not complete: the Vercel connector returned 403 at `read_protection_bypass` (deployment aliases endpoint). Do not label this a successful remote HTTP/browser or physical iPhone acceptance test. Deployment READY/source/alias/SSO checks and localhost browser proof are available; no protection setting was loosened and no repeated unauthenticated retry was performed.

All ten requested tasks are **not complete**. Live target queries, actual NPC
loading, complete source hit-effects capabilities, wound exit/rendering and the
full source ragdoll/MuscleControl graph remain unfinished. Keep PR #56 draft and
all 19 live limb query colliders gated. The runtime/source improvements and
verified preview above do not establish complete gameplay or native Unity parity.


## Continuation: original limb aim and exit-source binding (2026-10-17 UTC)

[Commit a8f9417](https://github.com/ethan-bailey/crisis-response-port/commit/a8f94172a45d5f9aae571a8d30ec86a6e197e2ca) recovers `Limb.AccumulateExitingProjectiles` from author source, retaining the 0.0167 second accumulation period, cap at 1000, and the default source exit-accumulation factor. The source `HandleExit`'s additional wound and renderer operations have **not** been ported and remain gated.

[Native-tested commit 6cfcc04](https://github.com/ethan-bailey/crisis-response-port/commit/6cfcc046aec781d8cee27e5a65e81398c6a21ecc) restores `Limb.AimPoint` and loads per-limb author aimpoint/radius/exit accumulation fields in `OriginalArmPhysicsHost.BindSourceBallistics`. `BallisticContracts` checks all 19 original Operator instances against source `sourceFields.Limb` and leaves all projectile limb query colliders disabled. Operator and Occupant prefab records both exist, but **only Operator** is instantiated by this host.

The first new native suite [38999885454](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38999885454) failed due to the test fixture composing the original limb component bundle **without** its ballistic-material bundle. The corrected fixture composes both original bundles. [Native suite 38999984171](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38999984171) and [source/package checks 38999987410](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38999987410) **SUCCESS** at identical executable SHA `6cfcc046aec781d8cee27e5a65e81398c6a21ecc`. The previous full WASM/browser milestone `a3e7927`, [38998115604](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38998115604), was **not rerun for these changes**. Current protected Vercel release remains that previous milestone; no new deployment, live-NPC proof or iPhone evidence was created by this continuation.
