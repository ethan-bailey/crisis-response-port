SUPERSEDED NUMBERING — see docs/GATE_DEPENDENCY_INDEX.md for current gate status.

# Gate 1 — source character acceptance — 2026-10-23

## Scope and result

Gate 1 from `docs/FOUNDATION_FIRST_COMPLETION_PLAN_2026-10-17.md` is complete for the native focused source acceptance. The tested tree was based on `develop/wasm` at `4e5583ad9a7c4d38bb8242e153c6ddf590798bca`; only the Gate 1 check harness and acceptance record changed in this work.

The original Operator and Occupant prefabs both completed Awake/OnEnable/Start on all active behaviors and retained one full-source CharacterBody, the original IK/Physiology components, and the authored three-key hit recovery curve. A combined source contract then exercised the original M4 aim alignment and facing, Occupant movement and kneeling animation, independent actor-owned trauma and physiology, shock, hemorrhage and oxygen loss, unconsciousness, ragdoll event dispatch, and source-timed hypoxic death.

The check wires a recording `Rigidbody.PhysicsForceSink`/kinematic callback and silent audio playback driver as headless adapters. It verifies original source calls reach those adapters; it does not simulate physical trajectories, rendered animation, or audible audio. Browser/device acceptance and complete PhysX outcome remain outside this gate and are not claimed here.

## Passing verification

- Pinned SDK: .NET `8.0.426`; process shim `/tmp/libprocshim.so` applied per `docs/DOTNET_BUILD_RUNBOOK.md`.
- `python3 scripts/check_characterbody_ownership.py --strict` — passed; original source owns the runtime actor path, required component references and 19 limb identities are present.
- `python3 scripts/verify_full_characterbody_enums.py` — passed; recovered `WeaponMode` and `LimbId` definitions match exactly.
- `python3 -m unittest scripts.test_summarize_characterbody_compile` — passed, 2 tests.
- `dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental -v:q` — passed, 0 errors, 45 existing warnings.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --prefab-lifecycle /tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json` — passed. Operator: 134 active behaviors reached Awake/OnEnable/Start; Occupant: 132; both loaded from original prefabs.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --spawner-contract /tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json` — passed. Output: `PASS: original Operator/Occupant source actors; directional aim/turn, movement/posture, actor-owned trauma/shock/hemorrhage/oxygen, unconscious/ragdoll/death timing, and PhysX command dispatch passed`; targeted Occupant blood loss was 4,831 ml on the final run.

The `.NET` warnings are the existing member-hiding, nullable/unused members, obsolete legacy serialization APIs, and Windows-only Registry analyzer warnings. They did not block compilation.

## Bounded failures found and fixes

1. **New state-path test reached `ArmController.SetKinematic` with uninitialized Rigidbody references.** The actor was instantiated after `RuntimeWorld.Initialize`; the fixture had not run the original `ArmsControl.Start`. The contract now invokes that source Start callback before testing ragdoll, matching Unity's next-frame lifecycle ordering.
2. **The check initially queried the wrong `ArmsControl` type.** A global-namespace legacy type shadowed the imported FullSource type in the check assembly. The lookup is now explicitly `Crisis.Runtime.FullSource.ArmsControl`; the loaded original component was present.
3. **Ragdoll initially failed closed on an unbound dynamic force sink.** The headless check now records force and kinematic commands for the original actor's Rigidbody components. Production code and the original ArmController method bodies remain unchanged.
4. **Unconsciousness initially failed at `CharacterAudio.StopVoice()` because this headless fixture had no decoded-audio driver.** The fixture now marks recovered clips loaded and binds a silent audio driver; no audio parity is inferred.
5. **Applying the original starting blood volume after earlier bleeding briefly made blood volume negative and caused the authored respiration clamp to reject a negative maximum.** The contract now removes the actor's exact current blood volume, leaving zero without over-removal.
6. **An initial attempt to test the death callback on the Operator invoked its original TeamOperator camera-subject callback, which needs a normally initialized mission camera.** The state-cycle test uses the original Occupant instead; the separate prefab lifecycle test continues to verify the Operator. No gameplay callback was suppressed to make the source transition pass.

## Remaining limits

Gate 1 acceptance does not establish full interactive player input, physical ragdoll trajectories, complete weapon/IK sequences, browser playability, or iPhone Safari. Continue with Gate 2's original IK/equipment actions and verify all prior source-only claims against the exact integration SHA.
