# Source particle inherited velocity slice — 2026-10-26

## Exact source and provenance

- Repository: `ethan-bailey/crisis-response-port`, branch `develop/wasm`.
- Tested/pushed source commit: `cc4042f7dc7fcd42a7ff3e36e7c29e3744b706e5`.
- Source tree: `ce0c6c107c2d4d3b91f93b380ccfdcead68fa242`.
- Remote `develop/wasm` was verified at the same SHA after the Git Data API update.
- Hosted Source contracts and WASM build: run #20, ID `40262914576`, success on this exact SHA.
- The implementation is source-adapter code. The preserved FullSource gameplay sources were not changed.

## Dependency trace and change

The original `runtime/Original/FullSource/SingleEmissionEffect.cs` assigns
`Rigidbody.velocity = parentVelocity` before calling `ParticleSystem.Emit` in
`EntryEmission` and `ExitEmission`. The authored impact package has 14 active
`InheritVelocityModule` records among its 26 emitters; each uses serialized
`m_Mode: 0` (Initial) and a constant multiplier of 1. The renderer previously
transported the particle seed, speed, lifetime, and size override but omitted
the owning Rigidbody velocity.

`BrowserParticleEventQueue` now records `GetComponentInParent<Rigidbody>()` at
the native Emit callback and includes that world velocity in each source
particle command, using zero only when the source hierarchy has no Rigidbody.
The browser source binder validates the three finite components. The renderer
evaluates the authored initial-mode curve and adds the transported velocity to
the source-shaped initial velocity only for emitters whose original module is
enabled. If the needed source command is missing, the renderer fails visibly
instead of silently dropping the module. The test fixture places the emitter
under a Rigidbody-owning parent, matching the authored hierarchy contract.

The source dependency inventory was stale before this batch. Regeneration
recorded the currently tracked source declarations and hashes (810 files,
1,710 symbols, 1,384 reachable files; 127 ambiguous names remain explicitly
listed). The freshness check now passes.

## Verification on exact source tree / SHA

| Check | Result |
| --- | --- |
| `node --test scripts/source_particle_values_test.mjs scripts/original_particle_event_binding_test.mjs scripts/source_particle_render_modes_test.mjs` | 23/23 passed |
| `node --test scripts/*_test.mjs scripts/*_contracts.mjs` | 128/128 passed |
| `python3 -m unittest discover -s scripts -p '*test*.py' -v` | 43/43 passed |
| `python3 scripts/inventory_source_dependencies.py --check` | Passed |
| EngineChecks | 75 assertions passed |
| FoundationChecks | 121 assertions passed |
| Runtime checks | Passed: original actor clone, presentation, weapon/reload, and ballistic contracts; output retains `fullGameParity:false` |
| Pinned SDK / Release publish | .NET SDK 8.0.426; `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release -o /tmp/cpr-cc4042f-publish` passed |
| WASM package | `status: verified`, source commit `cc4042f...`, 2,684 files, 199 Blazor resources, 195 WASM files, bounded 1,919-asset profile |
| Local HTTP smoke | `/`, `/game/`, `/runtime-web/` returned 200 HTML; the full 5,347,727-byte `physx-js-webidl.wasm` response returned `application/wasm` and matched SHA-256 `7d4696e6273166308ce1f0db5dd5aee96c843b6e3722d96db7e4422aa1fbee1f` |
| `git diff --check` | Passed on the clean exact-SHA worktree |

Full exact-SHA verification log: `/tmp/cpr-cc4042f-verification.log`.
Package: `/tmp/cpr-cc4042f-package`; build manifest records the exact source
commit. No package was deployed or preview changed.

## Not established by this slice

The tests establish source field transport and the serialized initial-mode
multiplier. They do not establish native Unity random-stream behavior, particle
pixels, shader equivalence, complete particle lifecycle, interactive browser
gameplay, or iPhone Safari acceptance. `sourceVisualParity` remains false.

## Next dependency-ordered work

Continue the full 51-system source inventory against one matching exact source
profile. First resolve the profile/asset provenance mismatch before merging
fields across inventories. Then implement one complete source-backed module or
lifecycle slice with a real runtime-path acceptance check. Keep the missing
Spark texture unresolved and visible until authoritative bytes are recovered.

## Runtime adapter regression found in the hosted game — 2026-10-26

The hosted game reported:

```text
Original particle InheritVelocity requires its source Rigidbody command: 198243736805655561
```

The earlier runtime path above covered native ParticleSystem Emit callbacks, but this error arrived from a ballistic `materialEffect` event. In the recovered source, `Limb.OnBallisticHit` calls `BallisticHitEffect.PlayExit(projectile, Rigidbody.velocity, ...)`; `BallisticHitEffect` passes that velocity to each `SingleEmissionEffect.ExitEmission`, which assigns it to the child Rigidbody before particle emission. The browser adapter's `BallisticHitEffect.PlayExit` accepted this value but omitted it from the event payload. Since this event path can run without a matching `sourceEmissions` command, the renderer had no source velocity and threw.

The adapter now includes `sourceRigidbodyVelocity` in material-effect events. The browser binder validates it as a finite three-component vector. The renderer continues to prefer the matching native Emit command's attached Rigidbody velocity and uses the ballistic event's source velocity when that command is absent. The runtime ballistic contract checks exact vector transport, and the focused Node test checks the fallback wiring.

Focused verification on the current local source tree:

- `node --test scripts/original_particle_event_binding_test.mjs`: 13/13 passed.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -c Release`: ballistics contract passed with `sourceRigidbodyVelocityTransported=true`, including the original material/mesh hit checks. Existing nullable-context warnings remain in `MissionPhysiologyContracts.cs`.
- `git diff --check`: passed.

This fixes the missing velocity input for the adapter path that raised the reported error. The live Site remains v73 until a separate release authorization; this source/test batch is not a deployment. These checks do not establish native Unity particle pixels or broader particle/render parity.


## Exact-source acceptance — run #68

The adapter fix at source commit `4926b8013d3a40797ee18b6870582fe0eebc014c` is present in verified branch SHA `b173b1daebcb0337fb1def6da948d08f3d990d79`. Run #68 / ID `40322845592` passed the Contracts/Release job `121571910792` and Chromium job `121571911030`. The focused change is covered by the full Node suite (143/143), full Python suite (45/45), and runtime ballistics contract output `sourceRigidbodyVelocityTransported=true`. The full Mission package verifier reported 2,686 files, 199 Blazor resources, 195 WASM files, and 1,919 assets / 22,365 records; deterministic tar.gz SHA-256 `a5522d46a7dc7976aac57787ddf8100b8b3e4ef5e9fc201b6c549ad841fd01ec`. Retained artifact `12649418198` has outer ZIP SHA-256 `f30f8ccdd34e0829706e98f3736003f7bc152486f7d49d550b32ab26a7c6a883`.

Run #68 also passed Chromium IndexedDB and the real weapon audio smoke: seven source clips decoded, the four fire/magazine/bolt events played after a trusted user gesture, drops remained zero, and peak output analyser RMS was 0.1590921945306555. This verifies the browser graph path only. The live Site remains v73; nothing was deployed. Physical iPhone audibility and particle pixel/native Unity parity are still open.