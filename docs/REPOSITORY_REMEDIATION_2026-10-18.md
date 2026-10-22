# Repository audit remediation — 2026-10-18 UTC

## Baseline and rollback anchor

Repository: `ethan-bailey/crisis-response-port`. Inspected full-source branch head:
`407c66d026784b72c96490eabae6085dd87c22e6` (PR #42). PR #42's recorded fork-base snapshot is
`e0a9ad26fb7def7c171dd04171eb93c9b79191af`; direct branch verification found
`develop/wasm` at `ba12245122a5fc256a4bbb6c3f815050d25bcc20` (integrated
Occupant spawn contracts, PR #65). That browser branch was not changed by this batch.

Authoritative compiler evidence: [run 39091701268](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39091701268),
job `117569703653`, at the full-source head above: **144 distinct errors, 60 missing names**.
The earlier 30-error / 21-name checkpoint is historical, not the current baseline.
`docs/audit/compiler-baseline.json` is normalized from the actual latest job log;
its source SHA, build exit code, run ID and job ID are pinned.

This batch changes development controls, diagnostics, inventory and engine file
boundaries. It does **not** complete the original actor migration or NPC AI.
Full dependency compilation continues to fail honestly. No original gameplay
method bodies, assets, input controls, repository visibility, deployment aliases,
or existing gameplay branches are replaced by this batch.

## Steps taken (revert in reverse order)

1. Inspected all four open PR heads and all 21 existing workflow definitions;
   verified the current compiler run independently of the handoff. No rebases,
   force pushes, asset deletion or historical evidence deletion.
2. Made `characterbody-full-compile.yml` dispatch-only. Full closure now follows
   successful isolated engine and original foundation checks. One repository-wide
   concurrency group serializes this expensive workflow. Its actual compiler exit
   status remains failing on compiler errors; no `continue-on-error` conversion.
3. Manually gated the ten historical push-triggered recovery/audit/mesh workflows
   listed below, retaining every job and recovery command. Existing manual-only
   recovery workflows remain available. Active gameplay/browser verification lanes
   and the ordinary fast PR checks remain in place.
4. Added stable diagnostic diffs. Paths normalize across runner workspaces;
   movement of source lines does not manufacture progress; repeated occurrences
   remain counted; MSBuild infrastructure errors are captured. JSON always records
   a supplied compiler exit status independently of the parsed error count. The
   baseline is immutable until a deliberate new checkpoint is reviewed.
5. Verified the original recovery ZIP's SHA256 and indexed **all 1,384 exported C#
   sources**, including declaration names, namespace/attribute aliases, source
   hashes and known-type reference edges, without importing new classes. Combined
   inventory covers 238 checked-in C# sources, 1,567 symbols, 916 conservatively
   reachable files and 72 ambiguous names. All 60 compiler names are classified:
   55 have recoverable source, one has checked-in recovered source (`DialogTree`),
   and four need Unity contracts (`ParticleSystem`, `LightShadows`,
   `ReflectionProbe`, `Rendering`). Asset-backed dependencies have an additional
   explicit flag. No missing name is silently assumed absent from the original.
6. Added an executable actor migration boundary: freeze seven legacy gameplay
   owner files, reject new partial transport-owner declarations, reject exclusions from the complete original project, and fix
   nested conditional handling in the existing source ownership scan. A reviewed
   migration or necessary bug fix must update the manifest with a rationale here;
   arbitrary new gameplay in the transport graph now fails the fast check.
7. Extracted the existing Unity graphics/collision declarations verbatim from
   `BallisticEngine.cs` into `Engine/Rendering/GraphicsPrimitives.cs`, and the
   existing Camera/Rigidbody engine block from `PresentationHost.cs` into
   `Engine/PhysicsComponents.cs`. The hosts retain their existing gameplay code.
   This isolates the real contracts without adding substitute gameplay.
8. Added `EngineChecks`, which compiles no gameplay actor, weapon host or Behavior
   Designer sources. It verifies one graphics transaction spanning texture clone
   isolation, pixel upload ordering, material binding, render targets, GL commands,
   mesh ownership and isolation across worlds. Added a narrow PR workflow for these
   checks, the existing foundations, inventory freshness and migration boundaries.
9. Documented cross-PR contracts and ordered gates below. Preserved #56/#57 as
   workstreams and #7 as historical reference; none is superseded by this tooling
   batch, so closing or wholesale-merging them would lose useful work or mix actor
   ownership. This change has its own review path into #42: [PR #66](https://github.com/ethan-bailey/crisis-response-port/pull/66).

### Workflows changed from push to manual

- `characterbody-full-compile.yml`
- `recover-characterbody-dependencies.yml`
- `audit-original-particle-render-assets.yml`
- `audit-original-particle-settings.yml`
- `audit-original-window-visuals.yml`
- `recover-original-particle-materials.yml`
- `recover-original-particle-meshes.yml`
- `recover-original-particle-source.yml`
- `source-window-shader-audit.yml`
- `verify-original-particle-assets.yml`
- `publish-original-mesh-viewer.yml`

The new `engine-contracts.yml` retains automatic narrow PR verification.
Completed recovery workflows are gated, not deleted; original source/asset
history remains available. Large tracked originals were left intact because
there is no measured runtime benefit from deleting them.

## Validation of this batch

Local .NET SDK 8.0.426:

- Isolated engine package: **13 assertions pass**.
- Existing original foundations: **90 assertions and all ten RuntimeContracts
  groups pass**.
- Original source provenance: **84 identities pass**; original WeaponMode and
  LimbId definitions verified.
- Audit-tool regression suite: **seven tests pass**, including diagnostic
  multiplicity, line movement, failed/empty logs, transport mutation, production
  exclusions, nested foundation conditionals and full missing-name coverage.
- One local full compile after engine extraction: **144 errors / 60 names,
  zero introduced and zero resolved diagnostics** against the pinned checkpoint.
  Exit status 1 is retained. This confirms no compiler regression; it is not a
  successful full-source build.
- All workflow YAML parses; inventory freshness, actor guards and diff whitespace
  checks pass. Existing gameplay files and assets are unchanged.

The first local SDK invocation failed on the container's process-stat lookup;
subsequent SDK execution and direct MSBuild invocation provided the results
above. No failed invocation is represented as successful validation.

Committed-source GitHub verification also passed: [run 39092992691](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39092992691) at
`46c4bae1749a656bb36019253ee4d62daf79520a` (all inventory, ownership, seven tool
tests, isolated rendering and existing foundation checks).

Final remediation verification: [run 39093133256](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39093133256) **SUCCESS** at
`389d8e8dcc3cc49d4327cc55588d874271dc4271`. Private artifact `12123895692`,
SHA256 `ac33366d664bb03f7573f751c44df80837e04c686f5c54d6b6284f3167730d8c`,
expires **2026-11-01T03:18:44Z**. PR #66 merged into the full-source branch as
`6c4415e62cef53646818c8c08412d5604282a740`; the three reviewed remediation
commits are `46c4bae`, `6d584cb` and `389d8e8`. This final log update changes
no executable files. No full closure build was automatically launched for the
remediation branch.

The narrow workflow preserves private native logs and the exact tested SHA as
`engine-and-foundation-contracts` artifacts for 14 days.

No WASM rebuild or public deployment was performed: this batch changes no
runtime behavior and does not meet the actor integration acceptance gate below.
No new browser/device parity or deployed-SHA claim is made.

## Cross-PR actor contracts and implementation order

The final authority is `Crisis.Runtime.FullSource.CharacterBody` on the original
actor GameObject. Its original limb, physiology, blackboard, equipment and weapon
components must be shared by projectile, IK, mission and presentation code.
Engine drivers supply services; browser code issues input and consumes results.
The checked-in transport actor is a protected compatibility lane pending migration.

| Boundary | Required identity and flow | Existing conflict / merge gate |
| --- | --- | --- |
| Actor and limbs | Full CharacterBody's named limbs, `limbs`, `limbMap` and each Limb's Body refer to the same actor/component instances | #56 adds combat state to `Crisis.Runtime.Weapons.CharacterBody` and its Limb; rebind those callsites to original components before integration |
| Attack notifications | Original projectile Owner and FirstHitCharacter use that same actor type; invoke original `OnSuccessfulAttack`, `OnWasAttacked`, `OnWasHit` in source order | Preserve #56 provenance/tests; do not copy its transport fields over the originals or count first hits twice |
| Physiology and scoring | Original Physiology, LimbPhysiology, MuscleControl and CharacterScoreCard remain the only mutable owners | #56's source slices and hit effects must consume these instances, rather than create a parallel vital host |
| Equipment and weapons | Original spawned Equipment and WeaponBase remain shared by actor, projectile and IK | FullSource currently imports weapon/blackboard transport contracts; close type identity before adding mission/NPC callers |
| IK lifecycle | Original IKArmController/IKArmSequencer execute on that actor and its original hand/weapon targets | #57's `OriginalIKArmControllerLive(CharacterBody, Func<float>)` currently takes the transport type; migrate callbacks instead of installing a second controller |
| Presentation | Immutable observed results / existing input commands | Preserve proportional joystick, ammo/chamber count and icon controls; JS must not acquire authoritative health or inventory |

Do not implement a new generic actor or health simulation to bridge these types.
An interface cannot conceal an underlying pair of independently mutable actors.

### Ordered closure batches (remaining implementation)

1. Use the pinned source index to select **complete transitive batches**; review
   the 72 ambiguous names first. Recover selected original bytes from the pinned
   archive; verify their index hashes before adapting namespaces. Generate the
   inventory after each batch, without triggering the full compiler automatically.
2. Finish cohesive Unity contracts needed by source rendering/physics (including
   real particle, reflection/shadow, decal/texture-pool bindings), using isolated
   native contracts and explicit unsupported boundaries until drivers exist.
3. Reconcile full actor/limb/blackboard/equipment/weapon identities. Migrate #56
   projectile/physiology and #57 IK callsites in reviewed slices against this one
   graph. Remove transport ownership only when consumers have moved and tests pass.
4. Close original mission, settings, audio propagation, scoring and world-service
   dependencies from the now-inventoried archive. Mission/NPC imports must follow
   stable actor/physics/injury/equipment ownership, not drive additional facades.
5. Dispatch one real full closure build for the combined batch. Review introduced,
   resolved and remaining diagnostics. Update the baseline only with exact tested
   SHA/run evidence; a failed build remains a failure.
6. After successful full compilation, execute two original actors through spawn,
   equip, move, projectile hit, first-hit notifications, wound/circulation,
   unconsciousness/death/ragdoll and destruction. Assert independent vitals,
   shared per-actor component identities, no self-hits and source callback order.
7. Build one consolidated WASM package at that exact SHA; run localhost Chromium
   and WebKit smoke/interaction checks. Publish the stable preview only when
   requested and after those checks, then verify iPhone behavior separately.

These are explicit outstanding acceptance gates, not completed-test claims.
The new tooling makes their order and proof requirements enforceable; it does
not turn the present 144-error graph into a functioning original actor.

## Original dependency continuation — 2026-10-18

Live-state gate inspected `feature/full-source-characterbody-integration` at
`f24f4dc6fbf12ae8a4fa6c85b090e5416fb97655`, `develop/wasm` at
`2fc0a8e93be97c4d99123910fad4bd25798e71a8`, all four open PRs (#7/#42/#56/#57),
recent merges #63–#67 and latest Actions results. Full compile evidence remains
run `39091701268`; narrow remediation evidence remains run `39093133256`.
The older 30-error graph is superseded. Next unmet gate is full original native
compilation, followed by actor/IK/projectile ownership migration and two-actor tests.

Recovered thirteen original sources from the pinned archive: the audio
propagator/instigator/stimulus family, CommunicationLine, VitalFunctions, LimbLayer,
three door-state enums and the complete three-class dialog data/tree family.
`FullSource.AudioInstigator.Awake` binds the original actor component; original
VitalFunctions retains organ probability, normalized hit position and trauma/
announcement order. These are recovered dependencies, not proof that their gameplay
consumers execute. Namespace wrapping and explicit Unity Random/Object aliases in
VitalFunctions are the only adaptations; the dependency provenance manifest and
existing verifier compare every recovered byte with the pinned archive.

Corrected the newer actor guard's nonexistent `Weapons.Projectile` type reference
to the actual global `Projectile`. Corrected the migration scanner to reject
record/struct transport owners; its already-present regression test failed before
this correction and passes afterward. No frozen transport gameplay file changed.

Local SDK 8.0.426 validation: 13 isolated engine assertions, 90 foundation
assertions/ten runtime groups, eight audit-tool tests, 97 source identity checks,
ownership guards and refreshed inventory pass. Full compilation still exits 1:
**117 diagnostics / 50 missing names**, with 27 baseline diagnostics resolved and
zero introduced, compared with the same complete-graph baseline. Missing declarations
still mask later method-body diagnostics; this is not a remaining-work estimate.
One intervening SDK CLI invocation aborted on process-stat retrieval (exit 134);
direct MSBuild execution produced the actual failing compiler result above.

The two-character checkpoint remains incomplete. No consolidated WASM/Chromium/
WebKit gameplay validation, merge to `develop/wasm` or preview update occurred.
Retain PR #42 as the workstream; do not merge it as a finished combat checkpoint.

## Reproduce / maintain

```sh
python3 -m unittest discover -s scripts -p audit_tools_test.py -v
python3 scripts/check_actor_migration.py
python3 scripts/check_characterbody_ownership.py --strict
python3 scripts/inventory_source_dependencies.py --check
dotnet run --project runtime/EngineChecks/Crisis.Runtime.EngineChecks.csproj
dotnet run --project runtime/FoundationChecks/Crisis.Runtime.FoundationChecks.csproj
# Regenerate the recovery index only with the exact pinned archive:
python3 scripts/inventory_source_dependencies.py --archive /path/to/original-recovery.zip
# Deliberate closure checkpoint (must preserve actual build exit status):
dotnet build runtime/Crisis.Runtime.csproj -v:q > characterbody-compile.log 2>&1
python3 scripts/summarize_characterbody_compile.py characterbody-compile.log \
  --output characterbody-diagnostics.json --baseline docs/audit/compiler-baseline.json \
  --source-sha <tested-commit> --build-exit-code <actual-build-exit-code>
```

## Revert procedure

The primary remediation commit is `46c4bae1749a656bb36019253ee4d62daf79520a`;
its PR also records evidence-only follow-ups. Revert all remediation commits in
reverse order, or revert PR #66's merge as described below. This reverses the
batch without deleting source history or resetting another branch.
If integrated with a merge commit, revert the remediation's ordinary commit(s),
or use `git revert -m 1 6c4415e62cef53646818c8c08412d5604282a740` after
verifying its first parent is `407c66d026784b72c96490eabae6085dd87c22e6`.
A subsequent documentation-only log commit can be reverted separately if desired. Never reset `develop/wasm` or `main` to the rollback anchor.

For a partial rollback: restore only `.github/workflows/` from the anchor to
restore old triggers; revert the paired engine host/extracted-file changes
**together** to avoid duplicate or missing C# types. Inventory JSON, scripts,
manifest, tests and narrow workflow should be reverted together if removing
migration enforcement. Original assets require no restoration because this
batch did not modify or remove them.

