# Repository agent instructions

Follow `AGENT_INSTRUCTIONS.md` for the detailed work loop, verification, branching, and publishing rules. User instructions take precedence. Use `NEXT_AGENT.md` as the current evidence-based handoff; historical notes are not current state unless reverified.

## Project invariants

- `develop/wasm` is the single development line. Check its live HEAD and open PRs before editing. Commit small, focused, verified changes directly there. Use at most one short-lived PR when review or isolation materially helps, and merge it promptly after its gates pass.
- GitHub Actions remain disabled. Use the current Work Mode checkout for repository work. Do not use Codespaces for routine edits; use an existing Codespace only if a required gate cannot run in the current environment and Codespaces are necessary.
- Preserve original Unity source, serialized values, assets, GUIDs, fileIDs, and lifecycle. Compatibility code should provide Unity services to original gameplay rules, not create a competing gameplay implementation.
- A compile, inventory, prefab parse, or synthetic fixture is not proof of playable parity. Keep each claim tied to the exact source SHA and the check that actually passed.
- Preserve the owner-approved proportional joystick movement, rim-triggered sprint, aiming, and hold-to-fire behavior through actor integration.
- A substitute is acceptable only within the owner-approved scope and after the affected behavior is shown to match its acceptance contract. Preserve the missing original pointer, record the substitute identity and provenance, and never describe a substitute as original. The approved casing-root mesh mapping does not authorize substituting the missing mesh in Marksman projectile prefabs.
- Keep raw recovered source and the repository private. Do not deploy intermediate builds. Publish to the existing owner-private Crisis Response Runtime Site only after all required gates pass on one exact SHA and publication is authorized; preserve its access settings.
- At the end of substantive work, replace `NEXT_AGENT.md` once with the observed live branch/head, exact tested checkpoints, checks and results, remaining blockers, and one next action.
