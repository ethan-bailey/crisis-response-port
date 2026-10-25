# Current handoff — 2026-10-25

## Verified checkpoint

- Development branch: `develop/wasm`; exact pushed source SHA:
  `fb966cbf277ffe071d9d31a3360e4063834c2eef`.
- FullSource actor mode receives the eased touch-stick movement magnitude.
  The bridge carries it across delayed fixed steps, scales original
  `CharacterBody` walk/sprint caps, then restores authored speeds in `finally`.
  The Mission contract confirms half magnitude yields less than 60% of full
  walk force and authored speeds are restored.
- The absolute-aim source contract passes. It fixes `WeaponPointer.WorldPoint`
  and ticks left/right movement with browser mouse X at both extremes; the
  chosen world target remains unchanged. Existing browser-input tests verify
  vertical joystick aiming remains independent of horizontal travel.
- Exact Mission UI/source actor contract passed for source input press/release,
  movement force, analog movement, absolute aim, and PhysX position/velocity
  round trip. Node passed **81/81** and Python passed **39/39** at this source
  tree. Browser Release publish succeeded with zero errors; optional
  `wasm-tools` optimization was unavailable.
- Package build and independent verify passed at this exact source SHA.
  Manifest SHA-256:
  `587b94e77dcfa6573ffc6212b7d4c0535f5199b07d7886553f2dd2451841b42c`.
  Package contains 2,673 files, 199 Blazor resources, 195 WASM files, and
  1,919 Mission profile assets (97,767,549 compressed bytes; 205,514,345
  source bytes). A separate streaming archive audit verified every manifest
  file size and SHA-256.
- Sites source commit:
  `9c9cc86533dec18147bf6c40a3aab0747eb9d138`; archive SHA-256:
  `47bdb1c9f3be61efb548e31790fe5c412c0d109a3296a0c22c351193b1c7af51`.
  Deployed privately as version
  `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_6e4944b82db0819198d0195173569abe`,
  deployment `appgdep_6ade43c2850081918d427bb426ce45ba` (succeeded) at
  `https://crisis-response-runtime.craftysnipe13.chatgpt.site`. Before deploy,
  Sites confirmed owner role and custom access with one allowed user and no
  groups; no audience settings changed.
- Detailed recovery and integrity evidence:
  `docs/evidence/RECOVERED_SOURCE_AND_EXACT_SITE_PACKAGE_2026-10-25.md`.

## Remaining gates — dependency order

1. **iPhone acceptance:** On the owner-private Site, tap Start once and let
   Mission load. Use Copy report and capture asset 1,919 completion, total
   time, stalled range, and memory. Diagnostics split fetch/hash, base64, and
   WASM bridge costs. The earlier “Warning: truncated output” image was a
   ChatGPT display truncation banner, not a game exception.
2. **Source actor ownership:** Continue parity audit of fire/reload, physics,
   pose/IK, rendering, and effects. Opt-in FullSource Mission uses source
   movement/weapon callbacks; default page startup still uses browser movement
   and the transport weapon host. Add source contracts for migrated edges.
3. **Gun audio:** Browser Web Audio currently drops stale, unloaded, or
   suspended one-frame events and clamps output through gain/compression. The
   FullSource harness intentionally binds silent audio drivers. Preserve the
   user's delayed, loud, unsynced gunshot report as the iPhone acceptance case.
   Audit the event path and add a contract for stale/suspended/late playback,
   then wire original actor events into the browser audio path.
4. **Gameplay parity:** Native PhysX solve, rendered pixels, animation/IK,
   source NPC/component coverage, projectile behavior, and original effects
   need separate source-backed evidence. Playwright/browser binaries and
   iPhone automation are unavailable here; continue all independent checks.
5. **Release closure:** After each source change, commit/push `develop/wasm`,
   run regression checks, publish Browser Release, rebuild and independently
   verify the exact package/archive, then deploy. Preserve the owner-only
   audience.

## Next action

Audit browser gun-audio callbacks and add deterministic checks for the existing
stale-event rejection, suspended-context rejection, and output ceiling before
changing playback. Then keep moving through source-actor ownership and gameplay
parity; defer only the physical-device acceptance, never the independent code
work.
