# Original managed foundations — native checkpoint, 2026-10-18 UTC

The recovered Behavior Designer scheduler/deserializers, original equipment records, mission scheduling/mode hooks and audio-surface rules now compile against the .NET 8 compatibility engine. **This does not complete the requested full CharacterBody compiler closure.** The unchanged full-class probe still fails on 30 first-pass diagnostics / 21 missing original game types, down from 42 diagnostics / 29 names at the inspected base. Additional method-body and type-identity errors can appear after those declarations are linked; these counts are not a comprehensive remaining-work estimate.

Workstream: existing draft PR #42, `feature/full-source-characterbody-integration`, based on inspected head `eef3ba1bedaa811ff6ad50cfbc4057f843b02def`. No integration-branch merge or public deployment. PR #56 retains projectile/physiology/Occupant ownership, #57 IK callbacks, #7 the older Unity route. These branches were inspected for overlap; no wholesale merges or history rewrites.

## Executable source and verification

- `runtime/Original/BehaviorDesigner/`: 51 decompiled managed plugin sources plus two original exported tree wrappers. Original scheduler, parent/leaf lifecycle, serialization and variable rules retained. Optional `BehaviorGameGUI` remains preserved under `source-reference/Original/BehaviorDesigner/`; it is the plugin's debug GUI helper, not a scheduler or NPC task.
- `runtime/Original/Foundation/`: 30 authentic exported scripts including SerializableEquipment and its complete record/migration/copy dependencies, ArmorClass, ActionScheduler, MissionMode, audio surfaces, shared-variable primitives and original Sequence/Selector/Parallel/Wait/decorator tasks. These compile as real implementations, not empty type declarations.
- `runtime/Engine/`: analytic CapsuleCollider ray/overlap queries, Collision/ContactPoint payloads and message dispatch, SpringJoint native-driver configuration/teardown, hinge-limit observer, typed named Animator parameters and graph-driver boundary, decoded audio-driver boundary, Unity serialized values, coroutine API overloads and serialized curve construction/wraps.
- `runtime/FoundationChecks/`: focused native checks for those compiled foundations. It explicitly omits the still-blocked full CharacterBody and is **not** a replacement for the complete compiler gate. The normal `runtime/Crisis.Runtime.csproj` still includes every original CharacterBody line and has no new exclusion or fallback.
- Local .NET SDK 8.0.426: **59 foundation assertions and all 10 existing RuntimeContracts groups pass**. Two real original BehaviorTree components execute original Sequence/Wait/Selector scheduling independently; fixture leaves observe callback order, pause/resume and completion. These are controlled scheduler fixtures, not mission NPC acceptance.
- `scripts/verify_foundation_sources.py --archive <pinned ZIP>` verifies **84 source identities**, the original plugin DLL hash, and exact source bodies after the documented aliases/decompiler syntax repair. No original method-rule substitutions.
- `.github/workflows/characterbody-full-compile.yml` now has independent native-foundation and complete-CharacterBody jobs, each preserving evidence. A passing foundation job never changes the failing complete-build exit status. Compiler logs and JSON summaries retain precise source locations and GITHUB_SHA for 14 days.

Reproduce:

```sh
python3 scripts/verify_foundation_sources.py
# Optional strongest source check with the private pinned archive:
python3 scripts/verify_foundation_sources.py --archive /path/to/original-recovery.zip
dotnet run --project runtime/FoundationChecks/Crisis.Runtime.FoundationChecks.csproj
dotnet build runtime/Crisis.Runtime.csproj -v:q
```

The final command is intentionally still the real, failing closure probe. There is no expected-failure conversion to a green build. `scripts/summarize_characterbody_compile.py` records compiler output; it does not determine success from an empty or partial log.

## Exact committed-source CI evidence

Gameplay/source commit `7dd57df04cb6a7091ffd5fbf70a8e667456e0ca8`: [run 39084570789](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39084570789). `foundation-contracts` **SUCCESS**, confirming 84 source identities, 59 assertions and all 10 prior contract groups. `compile-closure` **FAILURE**, confirming 30 compiler errors / 21 names; diagnostic summarization and artifact uploads succeeded. [Fast source/package run 39084572491](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39084572491) **SUCCESS** at the same source.

Private native evidence artifact `12119949035`, SHA256 `589ee636acce80ea043808623b3941f3c10c408b2257da453bc887096415c8cc`; full compiler evidence `12119969006`, SHA256 `2d0ccf5550f8a69d75ce02698cd8edab09e04e61dd808853279b3b6aac61a958`. Both expire **2026-11-01 UTC**. Documentation-only commits after this source do not establish a new tested executable SHA.

## Source recovery

Recovery archive SHA256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
Original plugin entry: `export/AuxiliaryFiles/GameAssemblies/BehaviorDesignerRuntime.dll`.
Plugin DLL SHA256: `7811c39c5039dfd9604eab9a1426856e70cead5aa42b60a7aee2cc19cee57b7a`.
Decompiler: ilspycmd 9.1.0.7988, project output from the pinned DLL. Per-file decompiled and runtime hashes are in `runtime/Original/BehaviorDesigner/source-provenance.json`. Runtime adaptations are aliases resolving Object/Task/TaskStatus identity and removal of ILSpy's invalid `virtual` modifier on two explicit interface declarations; no method bodies changed. Exported scripts retain exact archive bytes except a Random alias where .NET implicit imports otherwise conflict.

## Mandatory runtime boundaries

Native compile evidence is not native Unity or browser/device parity. SpringJoint requires a bound native solver driver; its configured anchors/limits are not simulated with invented forces. Hinge limits are forwarded to an observer, and full ragdoll integration still needs its native binding. Collision messages consume actual bridge payloads; no synthetic collision generation is introduced. Analytic overlap supports non-sheared boxes/capsules; mesh/sheared overlaps require the native query hook. Capsule shear is explicitly rejected.

Animator parameter IDs are host-interned names, **not claimed Unity hash equivalents**; the graph driver receives original names. Unbound graph evaluation throws. Source-controlled speed/parameter storage alone does not play an original clip. AudioClip decoding and AudioSource playback require the real decoder/device driver; recording-driver tests are not audible game playback. Global Behavior Designer resource deserialization and source NPC tree/action/navigation bindings remain production dependencies; scheduler tests do not prove these assets are instantiated automatically.

## Remaining complete-class declarations

All following names have recovered source in `source-reference/Original/CharacterDependencies/`. None is classified as a missing original asset simply because it is not executable yet.

| Family | Still-unresolved original declarations | Required ownership/binding work |
|---|---|---|
| Mission/world | Mission, Room, DoorController, DynamicInteractionMarker, BallisticObstacle | Full original mission/context/UI/world services and original shared character ownership. ActionScheduler/MissionMode alone are not Mission. |
| Character audio/dialog | CharacterAudio, Communication, CharacterDialog, DialogTree | Actual original body, AudioInstigator/propagation, coroutine and mission UI services. Audio surfaces/decoder interface alone are not CharacterAudio. |
| Character physiology/scoring | Physiology, MuscleControl, CharacterScoreCard | Reconcile #56's tested source slices with complete shared actor state; no parallel health or score owners. |
| Actions/animation | IKArmController, WeaponAimer, Grenade | Reconcile #57 callbacks and actual source equipment/limb/target identity; attach original graph/native physics services. |
| Effects | Emitter, FloorStreakPainter | Authored source render/particle/paint capabilities and original RNG/order. |
| Settings/perception | Settings, SerializableExtraSettings, Target, TimeScaler | Source persistence/context, target ownership and actual mixer/time/mission UI services. |

Next dependency work must use the actual recovered scripts and shared type identities, not interfaces/classes added solely to silence CS0246. In particular, `CharacterBody.Full.cs` still imports browser-transport weapon/limb/blackboard identities; resolving missing declarations can expose those additional mismatches. Do not infer that 30 individual edits would finish the graph.

No WASM/browser build, PhysX trajectory comparison, source mission startup, iPhone acceptance, merge or deployment is claimed by this checkpoint. Existing gameplay assets, magazine/chamber UI, joystick proportional movement, icon controls and previous preview are unchanged.
