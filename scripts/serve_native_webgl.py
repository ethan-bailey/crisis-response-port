#!/usr/bin/env python3
"""Owner-private local Unity WebGL asset server for CI browser validation.

Serve at loopback only. Original assets must never be exposed on public hosting.
Unity's *.wasm.br, *.data.br, *.framework.js.br need both the right MIME and
Content-Encoding, unlike the previous HTTP-200-only feasibility check.
"""
import argparse
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

class UnityHandler(SimpleHTTPRequestHandler):
    def guess_type(self, path):
        name = path
        if name.endswith((".br", ".gz")):
            name = name.rsplit(".", 1)[0]
        suffix = Path(name).suffix.lower()
        if suffix == ".wasm":
            return "application/wasm"
        if suffix in (".js", ".mjs"):
            return "text/javascript"
        if suffix in (".data", ".bundle"):
            return "application/octet-stream"
        if suffix == ".json":
            return "application/json"
        return super().guess_type(name)

    def end_headers(self):
        if self.path.split("?", 1)[0].endswith(".br"):
            self.send_header("Content-Encoding", "br")
        elif self.path.split("?", 1)[0].endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        self.send_header("Cache-Control", "no-store")
        super().end_headers()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("root", type=Path)
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=8000)
    a = parser.parse_args()
    root = a.root.expanduser().resolve()
    if not (root / "index.html").is_file():
        parser.error("Original Unity WebGL index.html missing")
    if a.host not in ("127.0.0.1", "::1"):
        parser.error("Native recovered assets must be served privately at loopback")
    handler = partial(UnityHandler, directory=str(root))
    with ThreadingHTTPServer((a.host, a.port), handler) as server:
        print("Unity WebGL private local browser validation server listening", flush=True)
        server.serve_forever()

if __name__ == "__main__":
    main()
