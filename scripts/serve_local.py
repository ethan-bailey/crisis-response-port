#!/usr/bin/env python3
"""Serve a prepared Crisis Response browser build without Vercel.

Requires one document root containing `game/` and `runtime-web/`, as generated
by the repository's browser test build. This server is development-only.
"""

import argparse
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path


class LocalHandler(SimpleHTTPRequestHandler):
    extensions_map = {
        **SimpleHTTPRequestHandler.extensions_map,
        ".mjs": "text/javascript",
        ".js": "text/javascript",
        ".wasm": "application/wasm",
        ".json": "application/json",
        ".crg": "application/octet-stream",
        ".gz": "application/gzip",  # JS decompresses .gz: no Content-Encoding header.
    }

    def end_headers(self):
        self.send_header("Cache-Control", "no-store")
        super().end_headers()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path("work/game-site"),
                        help="Prepared site containing game/ and runtime-web/")
    parser.add_argument("--host", default="127.0.0.1",
                        help="Bind interface (127.0.0.1 by default)")
    parser.add_argument("--port", type=int, default=8000)
    args = parser.parse_args()
    root = args.root.expanduser().resolve()
    missing = [str(path.relative_to(root)) for path in
               (root / "game/index.html", root / "runtime-web/index.html")
               if not path.is_file()]
    if missing:
        parser.error(f"Incomplete prepared site at {root}: missing {', '.join(missing)}")
    handler = partial(LocalHandler, directory=str(root))
    with ThreadingHTTPServer((args.host, args.port), handler) as server:
        print(f"Serving {root} at http://{args.host}:{server.server_port}/game/", flush=True)
        print(f"Original C# runtime: http://{args.host}:{server.server_port}/runtime-web/", flush=True)
        try:
            server.serve_forever()
        except KeyboardInterrupt:
            pass


if __name__ == "__main__":
    main()
