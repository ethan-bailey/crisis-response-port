# Bounded FullSource Operator package — 2026-10-25

## Checkpoint

- Base: `develop/wasm` at `7821ab9d166e98bc63d248009065ea51e8ee6bcb`.
- Implementation: `387e274bd210a3842781a4cbac770cd4c7a0907e`.
- The implementation was pushed through the GitHub connector and fetched back
  from `origin/develop/wasm`; the local branch matched and was clean.
- No deployment was performed.

## Bounded delivery

The normal game page previously had no demand loader for an original serialized
actor package. `scripts/package_fullsource_actor.py` now builds an Operator-only
asset dependency closure from the recovered source package during site
packaging. It preserves original serialized asset records and package reference
metadata, emits deterministic gzip JSON, and fails if either size exceeds its
fixed budget: 12 MiB compressed or 32 MiB expanded.

The generated package measured:

- 238 source assets and 998 serialized records.
- 9,554,760 compressed bytes and 24,296,523 expanded JSON bytes.
- Package SHA-256: `7abd5f4fd75d8a251a318bd0c52013a02427be8fab3bd81e718ca3d426a2e39e`.
- Source archive SHA-256: `dc979ca92461159aa304b342f58bcde8e7a28c261679e188c8312786e4c2cb79`.

`loadFullSourceOperatorPackage()` fetches this package only when called. It
checks manifest identity, fixed size ceilings, exact compressed and expanded
lengths, SHA-256, and the authored Operator prefab root before returning the
package JSON. The release packager creates the bundle from the recovered
archive and validates its manifest, checksum, counts, and byte budgets. The
full scene smoke package retains its existing opt-in `scene-smoke` path.

## Checks run

- Generated the Operator package from the tracked recovered scene archive; the
  generator completed with the sizes and counts above.
- `node --check scene-data/scene-package-loader.mjs` passed.
- `python3 -m py_compile scripts/package_fullsource_actor.py
  scripts/package_wasm_site.py` passed.
- `git diff --check` passed before commit.
- No .NET build, package build, test suite, browser smoke, or iPhone Safari
  acceptance was run in this batch.

## Remaining integration gate

The bounded package is an Operator prefab closure, not yet a complete actor
world. `FullSourceActorBridge` requires the original `Global.Context`, its
`InputHandler`, and one active original `PlayerController` bound to the
`CharacterBody`, all in the same `RuntimeWorld`. The tracked scene archive does
not contain `Assets/Resources/Global context.prefab`; the Mission 01 scene
closure measured 825 assets, 15,341 records, 144,342,051 JSON bytes, and
67,400,258 gzip bytes. That exceeds the actor-delivery budget and is not a
phone-startup path.

Do not bind the separately packaged Operator prefab as a second transport or
synthetic actor. Recover the exact original Global Context asset and build a
bounded Mission UI + Global Context + Operator source payload, or keep this
integration gate open. After the same-world source package is available, wire
browser actions, fixed ticks, PhysX state, renderer snapshots, and weapon
callbacks through that same original actor and run the combined runtime,
contract, browser-build, and package-integrity checks on one SHA. Obtain iPhone
Safari acceptance at the next playable milestone; do not deploy before the
owner-authorized release gate.
