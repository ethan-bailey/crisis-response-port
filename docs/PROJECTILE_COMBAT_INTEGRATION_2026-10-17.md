# Original projectile combat / injury integration checkpoint

PR #56, branch `feature/original-projectile-limb-hit-runtime`. The starting head
`c61d19ea8190e5551467f38a5dfb62cc89beb7e6` compiled successfully with .NET 8
Roslyn; no compiler fix was necessary. The source had unbound runtime callbacks.

## Executed original behavior

- `Projectile.RecordAttackInfo`, `CharacterBody.OnSuccessfulAttack`,
  `OnWasAttacked`, `CanSeePosition` and original `CharacterScoreCard` now execute.
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
actual recovered M4 ammunition and authored flesh properties, a deterministic
full-skin marksman fixture, distinct source CharacterBody instances, and an
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
have initialized, avoiding accidental reset of an active game clock/world. No
new iPhone preview has been deployed from this checkpoint. PR #57 (IK), #42
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
