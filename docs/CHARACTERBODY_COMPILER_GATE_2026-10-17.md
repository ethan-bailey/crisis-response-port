# Full original CharacterBody native compilation: measured dependency failures

**Exact source:** `ed089b5d39e9cf3a95379544c3cda2fa981cb96e`
**Native CI:** [Run 39087392983](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39087392983), `compile-closure` FAILED.
**Evidence:** [characterbody-full-compiler-diagnostics](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39087392983/artifacts/12121088683) (14-day retention).

The compiler emitted **30 errors**, classified as **21 distinct missing type/namespace names** by `scripts/summarize_characterbody_compile.py`. This is actual native compiler evidence, not a WASM result.

## Missing dependencies explicitly seen in compiler output

- **Original physiology and muscle:** `Physiology`, `MuscleControl`.
- **Original animation and actor interaction:** `IKArmController`, `DynamicInteractionMarker`, `CharacterDialog`, `DialogTree`.
- **Original mission, time, settings:** `TimeScaler`, `Room`, `Settings`, `SerializableExtraSettings`.
- **Original audio and effects:** `CharacterAudio`, `Emitter`, `FloorStreakPainter`.
- **Original equipment / combat / AI:** `WeaponAimer`, `Grenade`, `Target`, `DoorController`, `BallisticObstacle`, `CharacterScoreCard`.
- Additional unlisted missing symbols must be read from the uploaded full log; do **not** treat this topical summary as an exhaustive enumeration.

A separate `foundation-contracts` job also failed, but specifically because the original arm controllers now reference `Crisis.Runtime.FullSource.CharacterBody` while that harness excludes `CharacterBody.Full.cs`. That project boundary was corrected in later commit `774568c` by excluding the two original arm-control classes from the **foundation-only** compilation; the main `runtime/Crisis.Runtime.csproj` still includes all original sources and must continue to report missing closure dependencies.

## Sequenced remediation

1. Preserve original `CharacterBody`, original limb and equipment identities; do not add aliases to the browser transport actor to satisfy native signatures.
2. Recover original component definitions and engine adapters for `Physiology` and `MuscleControl` as a consistent limb graph; select verified changes from PR #56.
3. Recover original `IKArmController` and animation/equipment dependency closure and integrate callbacks from PR #57.
4. Close mission/audio/time/interaction dependencies; preserve original side effects and lookup paths.
5. Rerun the **whole-class** native CI and the independent foundation contracts. After both pass, run real two-character lifecycle tests, then one pinned WASM/browser milestone.

No full CharacterBody compilation, two-character lifecycle or browser milestone passed at the measured SHA.
