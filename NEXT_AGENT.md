# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; Gate 1 acceptance code/evidence commit: `ba3196d5b1522007bf208d6f40b73fe6e344363d`. This handoff update is a documentation-only descendant.
- Open pull requests: none (`gh pr list --repo ethan-bailey/crisis-response-port --state open` returned `[]`).
- Preserve untracked `scene-data/.crisis-response-scenes.json.gz.QEOasz`; it is excluded from commits.
- Keep GitHub Actions disabled. Codespaces use remains at 90% through 2026-11-01; avoid Codespaces where the current checkout can do the work.
- Read `AGENT_INSTRUCTIONS.md` and `docs/DOTNET_BUILD_RUNBOOK.md` before project work. The recurring .NET runner failures and fixes are recorded there.

## Gate 1 — one complete source character

**Passed native focused source acceptance** at code commit `ba3196d5b1522007bf208d6f40b73fe6e344363d` with SDK 8.0.426.

- Full `runtime/Crisis.Runtime.csproj` rebuild: 0 errors, 45 existing warnings.
- Strict ownership scan and exact recovered `WeaponMode`/`LimbId` checks passed; all 19 original limb identities are present.
- Original Operator and Occupant prefab lifecycle passed: all 134 and 132 active behaviors respectively reached Awake, OnEnable and Start; each retained one original FullSource CharacterBody, IK controller and Physiology.
- Combined source contract passed original M4 directional aim/alignment and turn, Occupant movement and kneeling animation, actor-owned trauma/shock/hemorrhage/oxygen, unconsciousness, ragdoll command dispatch and timed hypoxic death. Operator state remained independent of Occupant state.
- The headless contract binds a command-recording Rigidbody sink and silent audio driver. It proves original source dispatches through those adapters, not live physical trajectories, rendered animation or audible output. Detailed failure fixes and exact commands are in `docs/evidence/GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`.

## Gate 8 — still open

The previously built package at code SHA `2530764705352f98c3c42018b8aa2238ce561842` passed WASM publish/package integrity, but exact-build browser gameplay is unverified. Owner authorized Cloud Chrome transmission; the browser could not reach the workspace localhost server (`ERR_CONNECTION_REFUSED`), and no local browser binary is installed. Do not expose the private package publicly. Physical iPhone Safari acceptance is also outstanding.

Other recorded limits: no complete linked two-actor combat chain has been verified in a normal browser mission; broad strict scene loading still fails on unresolved supported fields in `Assets/Scenes/Marksman range.unity`. These do not invalidate Gate 1's native focused acceptance.

## Next action

Proceed to Gate 2: exercise the original IK controller/sequencer and equipment actions on the Gate 1 source actors, fix the first actionable source/runtime blocker, and validate the slice with the pinned SDK and focused contracts. Then continue to Gate 3 before attempting the end-to-end browser/device acceptance path.
