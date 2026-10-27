# Gameplay completion contract

A playable port must execute the original game's behavior through the browser host. Source presence, asset inventories, synthetic fixtures, a clean build, or a browser shell alone do not establish gameplay parity.

## Required evidence

- Trace each behavior to original C# methods, serialized scene/prefab fields, exact assets, GUIDs/fileIDs, and lifecycle order. Mark missing source explicitly.
- Exercise the authored actors, weapons, mission flow, physics, animation, UI, audio, particles, save behavior, and mobile lifecycle through the same original gameplay owners.
- Compare port behavior against captured native/reference behavior with stated tolerances. Keep functional traces and visual comparisons separate.
- Verify that the exact Release package loads, reaches mission completion, remains responsive, and reports useful failures. Record load milestones, asset counts, memory samples, and repeat-launch behavior.
- Have the owner test the exact build on physical iPhone Safari at major playable/release milestones. Desktop Chromium/WebKit checks are useful diagnostics, not device acceptance.

## Work order

1. Close source and asset dependency sets before building on them. Record exact identities and the evidence for any absent item.
2. Execute original gameplay owners in the runtime and validate lifecycle, ordering, multiple-actor independence, physics, and browser input.
3. Close combat, physiology, animation/IK, rendering/particles, UI/audio, and mission dependencies in coherent batches derived from the recovered source graph.
4. Measure mobile startup, memory, input latency, interruption/resume, storage, and rendering on the target device. Optimize only against measurements while preserving behavior.
5. Release only one exact tested source/package SHA after all required gates pass and publication is authorized.

## Live priority

The current task is owned by [NEXT_AGENT.md](../../NEXT_AGENT.md). Site version 71 is deployed and its full archive audit was reported passed, but Site source identifier `37e6d5dcebaaba71c405d898d161b3e36e946a78` does not resolve to a commit in the public GitHub repository. Reconcile that source identifier to its repository/tree before claiming exact source parity. Physical iPhone Safari acceptance of version 71 remains pending, especially the 98 MiB Mission load, the reported stall near 1,900 assets, and repeat-run cache behavior. See [the version 71 reconciliation](../evidence/SITE_VERSION_71_RECONCILIATION_2026-10-26.md) and the live gate list in [NEXT_AGENT.md](../../NEXT_AGENT.md).
