# Crisis Response — iPhone-accessible private preview deployment handoff

Updated: 2026-10-15. Active development PR #7:
https://github.com/ethan-bailey/crisis-response-port/pull/7
Branch `codex/iphone-input-parity-foundation`, **NOT main**.

## Goal

Publish an accessible, **private** https URL the user can open directly in
landscape Safari on their **iPhone 16** to inspect the current recovered Unity
runtime prototype and test the tap/hold/drag gestures. Do not say firing works:
original projectile/bolt/magazine/cadence machinery is still pending.
No placeholder weapon, movement, or ballistics implementation may substitute
for recovered developer systems.

## Why this handoff exists

- A previous ChatGPT Site at `https://crisis-response-runtime.craftysnipe13.chatgpt.site` was published from older source (`0f9b4a41...`); it does **not** contain the latest gestures.
- GitHub auto-creates Vercel preview deployments for this branch and reports
  `READY`. Attempts to access /game/, /runtime-web/, and create a shareable
  authentication-bypass link from the current ChatGPT Vercel connector return
  **403 forbidden** (team/project deployment-protection permissions). READY
  is **not proof** that the full game or Blazor WASM loads.
- This conversation's available tools **do not include ChatGPT Sites edit or
  publish**, and a Python server running in an agent sandbox is not reachable
  from the user's phone.
- The supported alternative is opening **ChatGPT Work** on **chatgpt.com in a
  browser**, requesting `@Sites` and publishing a preview privately.
  Whether Sites supports this particular large .NET WebAssembly application
  must be verified; do not assume it does.

## Exact deployment procedure for Work/Sites or an accessible cloud host

1. Retrieve the private GitHub repo above and **check out the active branch**.
   Read root `AGENTS.md`,
   `docs/TOUCH_ERGONOMICS_RESEARCH_2026-10-15.md`,
   `docs/LOCAL_HTTP_SERVER.md` and
   `.github/workflows/game-movement-smoke.yml`.
2. Use Node 22, .NET SDK 8, Python 3. Build the actual recovered game + original
   C# WebAssembly runtime exactly as the current workflow:
   ```sh
   npm ci --prefix game --ignore-scripts --no-audit --no-fund
   node scripts/vendor_physics.mjs
   python3 scripts/build_scene_binary.py game/assets/mission01.obj game/assets/mission01.crg
   cp game/browser-input.mjs runtime/Browser/wwwroot/browser-input.mjs
   dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release -o work/game-runtime
   mkdir -p work/game-site/runtime-web
   cp -a game work/game-site/game
   cp -a work/game-runtime/wwwroot/. work/game-site/runtime-web/
   sed -i 's#<base href="/">#<base href="/runtime-web/">#' work/game-site/runtime-web/index.html
   ```
   Alternatively, retrieve the most recent successful
   `crisis-response-local-http-site` artifact from the
   `Mission 01 separate game movement smoke test` GitHub Action, **only if**
   it was built from the latest desired commit and contains both entry points.
3. Serve the assembled `work/game-site/` directory at one HTTPS origin,
   preserving URLs `/game/`, `/runtime-web/` and all asset subpaths.
   Keep correct `.wasm` MIME and original `.gz` decoding conventions. Do
   not expose the repository source tree or private unrelated files.
4. Verify `/game/` returns 200 and the page actually loads:
   original C# `../runtime-web/?bridge=1` iframe, original Operator asset,
   Mission 01 scene, selected original PhysX backend, M4 visual and touch
   event adapter. Look for network 404, WASM MIME, crossorigin, secure
   context and runtime errors. Test mobile landscape and Safari rendering.
5. On the prototype, test **quick repeat taps**, stationary 210ms **hold**,
   drag past 11 CSS px, **hold then drag**, a movement-button second finger,
   and cancelling/backgrounding the tab. A visible `WEAPON PENDING` warning is
   correct: this build reports raw trigger intent but has no original firing.
   No requestAnimationFrame stutter or input left stuck after release.
6. Limit Site sharing to the owner; do not select public/anonymous publishing
   of recovered third-party game assets without appropriate distribution rights.
   Generate a real private preview link, test it from a signed-in iPhone and
   report the exact source commit SHA and what works/does not.

## What to ask ChatGPT Work / @Sites

> Update the existing private Crisis Response Site from the GitHub repository
> `ethan-bailey/crisis-response-port`, branch
> `codex/iphone-input-parity-foundation`. First read
> `docs/IPHONE_PLAYABLE_LINK_HANDOFF.md` and `AGENTS.md`.
> Build the actual recovered game and .NET 8 WebAssembly runtime, keep
> original assets/systems intact, publish a **private** playable Safari URL
> only after testing that /game/ and /runtime-web/ load. No fake gameplay.
> Report source commit, loading errors and real testing limitations.

When already in Work/Sites, using **Edit** on the prior Site may avoid making
a duplicate site; verify it is the same project and authorized source first.

## Fallback if Sites cannot host this app

An authorized external HTTPS static host can serve the prebuilt
`work/game-site` bundle. Vercel is currently blocked by connector team
authorization for preview verification/share-link creation. Correcting
team/project authorization is required before presenting Vercel links as
verified. Do not change the production deployment or make the repo public
just to bypass authentication.

## Smoke / input validation provenance

The current browser gesture adapter and input-pulse code have 39 isolated
source-fetched JavaScript regression assertions passing (22 adapter,
12 queue, 5 integrated), **not** native physical iPhone or actual full WebKit
gameplay verification. CI has been updated to run all three suites but current
full GitHub Actions status has not yet been independently confirmed.
