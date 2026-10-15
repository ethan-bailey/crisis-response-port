# Local HTTP server (no Vercel)

The game's browser host needs HTTP: JavaScript ES modules, original data assets,
and a separately compiled C# WebAssembly runtime must load from a common origin.
Opening `game/index.html` as a `file://` URL is not a supported substitute.

## Preparing the site

From the repository root in an environment with Python 3, Node.js 22,
.NET SDK 8 and the recovered original assets already present:

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
python3 scripts/serve_local.py --root work/game-site --port 8000
```

Open `http://127.0.0.1:8000/game/` on that same machine. The Blazor runtime
loads at `http://127.0.0.1:8000/runtime-web/`. The server intentionally sets
`Cache-Control: no-store`, correct `.wasm`/`.mjs` MIME types and leaves
the explicit `.json.gz` decoding to the browser application.

The **Mission 01 local browser smoke workflow** in
`.github/workflows/game-movement-smoke.yml` builds and serves the same layout
using `scripts/serve_local.py`; its `crisis-response-local-http-site`
artifact contains the assembled HTTP document root if the build succeeds.
A runner artifact is not a running server. Do not claim a complete browser build
unless the build and browser checks actually succeed.

## iPhone limitation

A server bound to `127.0.0.1` inside an agent's workspace is accessible **only
inside that workspace**, not from the user's iPhone Safari. An iPhone can access
an independently running server only if it has a reachable LAN/public address
and connectivity. A phone-only local solution additionally requires moving the
assembled site files to an iPhone-capable HTTP server, with the limitation that
iOS can suspend background apps. No browser URL is supplied by this script.

Do not replace the existing original-source architecture with a mock game just
to make a static `file://` demo or a fake mobile preview.

## Current game status

Recovered original camera/upper-body/IK methods and original input, asset and
contact slices execute in the scoped prototype. A tap is classified separately
from a drag and queued as original `InputHandler` fire key-down/key-up frames.
Original `PlayerController`/weapon/bolt/projectile firing is **not**
implemented end-to-end. No shot or reload is simulated in the browser; its
on-screen FIRE/RELOAD/SWAP buttons correctly remain pending.

The user should not mistake the local-server smoke for an original-game parity
certification. Full weapon gameplay, much of Unity physics/animation/scene
lifecycle, actual iPhone Safari testing and original-versus-port behavioral
comparison remain open.
