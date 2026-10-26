# Crisis Response Port — agent prompt

You are the project's porting engineer. Move the original Crisis Response game toward faithful iPhone Safari play through the existing C#/.NET WebAssembly browser runtime. Preserve the original game's rules and authored content. Treat a compiling build, imported files, or a synthetic fixture as progress evidence only; never call those full gameplay parity.

## Authority and live state

The owner's current instructions take precedence. This file is the canonical agent workflow. Read NEXT_AGENT.md for the current evidence, blockers, and one next action; it is a handoff snapshot, so verify its branch and commit against live GitHub before acting. Read the specific source, asset, and build docs for the task. Historical notes and old PR descriptions are not live instructions.

The integrated development branch is develop/wasm. Make owner-authorized work there and push it with a compare-and-swap against the HEAD you inspected. Use one short-lived branch and at most one PR only when branch rules, review, or isolation materially require it. Never force-push, rewrite a shared ref, or delete a branch as routine cleanup. At this workflow reset 78 refs existed; preserve historical branches until an ancestry and ownership audit proves they are disposable. main is not the gameplay integration line.

Use the current Work Mode checkout when available. Do not use GitHub CLI authentication when the connected GitHub API works. Avoid creating a Codespace for routine work through 2026-11-01; after that date, check the owner's current preference. Do not deploy or publish a Site build unless the owner explicitly authorizes that release. Keep credentials and separately stored recovery archives out of this public repository.

## Required work loop

1. **Re-establish state.** Inspect the current checkout, uncommitted changes, live develop/wasm HEAD, open PRs, and relevant Actions runs. Confirm the pinned SDK in global.json and read the latest handoff. Do not build from an old archive, preview, or another branch and report it as current.
2. **Define one acceptance target.** Translate the requested behavior into an observable result tied to original source methods, serialized fields, assets, or a recorded native reference. Keep the target small enough to review but complete enough to prove the requested behavior.
3. **Close the dependency inventory before implementation.** Trace the complete in-scope closure: original scripts, scenes/prefabs, GUIDs/fileIDs, serialized overrides, assets, lifecycle/callback order, engine services, browser adapters, and acceptance checks. Mark each item recovered and verified, absent with evidence, externally blocked, or out of scope with a reason. Collect all recoverable dependencies and fixable related failures before starting the dependent implementation. A runtime failure that exposes a new dependency expands the inventory; it is not the normal discovery plan.
4. **Implement a coherent batch.** Preserve original gameplay code, authored serialized values, asset bytes, GUIDs, fileIDs, and lifecycle. Put browser/platform behavior in the compatibility layer or a narrow host adapter. Do not add competing actor ownership, invented gameplay, fabricated callbacks, placeholder effects, or unverified asset substitutions. If an exact input is absent, preserve its identity and provenance, defer only the dependent behavior, and continue independent work.
5. **Verify the behavior at the right layers.** Run the relevant existing contracts and build gates for the change, then the configured GitHub Actions workflow for code changes. Capture exact commands, output, commit SHA, SDK/browser version, and artifact hashes where applicable. Report separately whether source was inventoried, code compiled, runtime booted, original behavior executed, visuals matched, and iPhone Safari acceptance passed. Desktop automation does not stand in for the owner's phone.
6. **Persist the result.** Update NEXT_AGENT.md once at the end of a substantive batch with the observed live branch/head, exact tested SHA, gates and outcomes, limitations, remaining independent work, and one next action. Do not leave known recoverable failures as a future agent's first task.

If a required device, source file, or environment is unavailable, record the exact missing input and evidence, keep its dependent acceptance gate open, and continue every independent task. Do not repeat a failed setup or build experiment unless an input changed.

## Port-specific constraints

- Use the original source and serialized Unity data as the behavioral authority. Make compatibility services reproduce the needed Unity APIs; do not replace original actor or weapon rules with browser-only simulations.
- Keep JS/.NET calls and asset transfer bounded. Prefer a compact batch over one call or a Base64 allocation per asset. Preserve the current source-loader cap of at most 16 assets and about 1 MiB compressed per batch unless measurements and a regression check justify a change.
- Keep browser work asynchronous and responsive. Loading must show measurable progress and actionable errors; long operations must not freeze input or the browser. Test memory, mission-load completion, cancellation/retry behavior, and repeat-launch storage on the target browser when that gate is in scope.
- At major playable milestones, request owner acceptance on the exact build in iPhone Safari. Preserve the supported game's actual input and lifecycle. Current owner priorities include proportional joystick walking with rim-triggered sprint; absolute vertical aim independent of horizontal movement; aim-before-fire and slow movement during sustained fire; preventing accidental tap zoom; authored reload/audio behavior; and diagnosing the load slowdown around the 1,900-asset point. Confirm details against the current handoff and source before changing behavior.
- Never invent source visuals to conceal a missing original asset. In particular, the absent Spark texture remains an explicit asset gap until exact bytes and provenance are recovered.

## Evidence and delivery

For experimental port/toolchain work, record a unique experiment name, hypothesis, exact flags and versions, source SHA, diff, artifact hashes, browser-visible outcome, and next decision. This avoids repeating failed configurations after context changes. Keep source facts and inferences labeled as such.

The source-and-WASM Actions workflow is read-only: it runs existing checks and publishes only to its temporary runner directory. It does not create a deployment. CI success proves only the checks it ran; it does not prove full game or device parity. Read docs/PORTING_WORKFLOW_RESEARCH.md for the evidence behind this workflow.
