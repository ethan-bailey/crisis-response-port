# Original arm sequence compilation — 2026-10-16

The user explicitly chose the existing original C#/.NET WASM path over the
separate Unity port. This bounded batch expands original animation dependencies,
without replacing character/animation gameplay or requiring native Unity work.

## Inputs and scope

- Browser baseline: `8ba76d2b4f8a349918b75863c07cfbe2914a3a6d`.
- Isolated continuation: `codex/wasm-original-arm-sequences`; target browser PR #9
  branch only. Main and the independent Unity branches are unchanged.
- Original archive SHA256:
  `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
- Six **byte-for-byte original** classes: IKArmAction, IKArmTarget,
  IKControllerInvoke, IKArmAnimationFrame, IKArmSequence, IKArmSequenceRuntime.
  Source paths/hashes are in `runtime/Original/Animation/source-provenance.json`.
- Seven original prefabs: Apply gauze, Holster pistol, Lower weapon, Pickup
  grenade, Raise shoulder weapon, Return grenade, Unholster pistol; **21 frames**.
  `game/assets/original-arm-sequences.json` preserves both tracks, actions,
  target enums, offsets, timings, callback enums and sibling-frame fields.
- Bundle SHA256:
  `4fd3940e554f406ceee8cfc210a82a09f57cb9dc2f5f0211c74b8cedacb4fa25`.

OriginalArmSequences is a serialization/transport boundary to the original
sequence composition methods. It preserves their shallow frame references and
independent list semantics. Its separate diagnostic world does not reset live
game time or input. Unknown enums fail instead of being silently remapped.

This is **composition, not playback**: IKArmSequencer, IKArmController and the
complete ArmIKSolver/CharacterBody dependency group are not yet imported. No
fake versions or callbacks are introduced. Existing selected weapon methods
and browser gameplay remain unchanged. These classes are prerequisites used
by the original controller for weapon/grenade/gauze sequences; a successful
compile does not finish those features.

## Verification

Local source recovery verified the complete immutable archive, copied all six
source files unchanged, and extracted the original prefab fields. Source/bundle
integrity checks, JavaScript syntax check and the 12 existing package tests pass.
This local environment has no dotnet SDK; local Python tests are not C# proof.

The scoped workflow `.github/workflows/wasm-arm-sequences.yml` pins SDK 8.0.426,
runs all existing native contracts plus source-backed arm composition contracts,
publishes the actual WASM runtime, assembles/checks the complete private site,
and invokes the new original sequence bridge in Chromium and WebKit against
the packaged original prefab bundle. The browser test checks every field across
three compositions, including all seven prefabs. Runtime playback remains false.

Package checker, its 12 tests and workflow-scoped SDK pin are reused unchanged
from PR #19; the PR itself remains unmerged. No new packaging architecture is
introduced. Node 22, Playwright 1.55.0; existing source/input tests are retained.
Only this workflow's own change triggers the bounded build on this branch;
documentation/source edits alone do not automatically rebuild it.

Cloud result: **SUCCESS**, [run 38896212172](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38896212172),
exact tested code `256b574eca62a902e23e9ba45d2b2542d64fe445`.

- SDK **8.0.426**; actual net8.0 native contracts and WASM publish passed.
- Selected original C# inputs increased **35 → 41**; engine files **19 → 20**.
- Existing input/source tests **38/38**, package tests **12/12**, new original
  composition/serialization/clock-isolation contracts all passed.
- Chromium and WebKit each checked **28 frame instances across three original
  compositions**, covering all seven original prefabs; no page errors.
- Package integrity: **725 files, 199 Blazor resources, 195 WASM binaries**;
  independent archive/unpack validation in CI passed.
- Private compiled site artifact **12043404117**, evidence **12043827958**;
  both expire **2026-11-15**. Inner site tar SHA256:
  `593ae3ebffbddc61ab146b34a65e4ee72a18e845ab8c0e6e8ef220b32f7fe2d8`.
- Downloaded evidence ZIP matched GitHub's SHA256 digest
  `953a5bd323b4c617850d5adfc610b4d3419358333a2e6b2c6fd3b45251bee16e`.
  Inspected the actual compiler, source commit, native/browser results and
  package manifest. The site archive was not downloaded a second time locally.
- Only one workflow run was needed. Later documentation commits need no rebuild.

Verification boundary: this browser check executes original sequence composition,
not full animation playback or a fresh entire-game movement/weapon browser suite.
Existing native presentation/weapon/ballistic regressions passed. No deployment,
original-game native parity or physical iPhone acceptance is claimed.

Reproduction from repository root:

```sh
python3 scripts/recover_original_arm_sequences.py <verified-original.zip> --repo .
python3 scripts/verify_original_arm_sequences.py
dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj
```

For WASM and browser reproduction follow the pinned workflow, including its
temporary root SDK selector, site base path and package integrity checks.
Keep artifacts private; no deployment or physical iPhone acceptance is implied.

## Next dependency group and rollback

The original IKArmSequencer requires IKArmController, ArmIKSolver and
CharacterBody, including real weapon/hand/limb/medical callback implementations.
Do not add empty methods, unconditional success flags or substitute character
systems merely to compile it. Inspect the original callback dependency group,
then import a bounded executable group with source provenance. The selected
WASM route remains the priority; native Unity validation is separate evidence,
not a prerequisite or an automatic engine switch.

Selectively revert new animation files, bridge/check entry points and scoped
workflow after checking later dependencies. The package helper/pin files are
shared with PR #19: retain them if another reviewed change depends on them.
Do not reset branches or remove the original archive/previous compiled baseline.
