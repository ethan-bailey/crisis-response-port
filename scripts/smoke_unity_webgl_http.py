#!/usr/bin/env python3
"""Check built Unity WebGL output over private localhost HTTP, without redistributing assets."""
import argparse, json, mimetypes, threading, urllib.parse, urllib.request
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from functools import partial

p=argparse.ArgumentParser()
p.add_argument("output",type=Path)
p.add_argument("--manifest",type=Path,required=True)
p.add_argument("--results",type=Path,required=True)
args=p.parse_args()
manifest=json.loads(args.manifest.read_text())
needed=("index_present","wasm_present","data_present","loader_present")
if not all(manifest.get(k) for k in needed):
    raise SystemExit("Missing WebGL output type: "+",".join(k for k in needed if not manifest.get(k)))
server=ThreadingHTTPServer(("127.0.0.1",0),partial(SimpleHTTPRequestHandler,directory=str(args.output.resolve())))
thread=threading.Thread(target=server.serve_forever,daemon=True)
thread.start()
host=f"http://127.0.0.1:{server.server_port}"
tested=[]
try:
    for meta in manifest["files"]:
        url=host+"/"+urllib.parse.quote(meta["relative_path"])
        with urllib.request.urlopen(url,timeout=20) as response:
            if response.status!=200:raise SystemExit(f"HTTP status {response.status}: {meta['relative_path']}")
            # Check headers and leading payload, not full proprietary asset contents.
            head=response.read(256)
            if meta["size_bytes"]>0 and not head:
                raise SystemExit(f"File returned empty payload {meta['relative_path']}")
            tested.append({"path":meta["relative_path"],"status":response.status,
                           "content_type":response.headers.get("Content-Type")})
finally:
    server.shutdown()
    server.server_close()
result={"http_localhost_smoke_ok":len(tested)==manifest["file_count"],
        "tested_files":len(tested),"total_files":manifest["file_count"],
        "file_types":{s:sum(1 for x in tested if x["path"].endswith(s)) for s in (".wasm",".data",".html",".js",".unityweb")}}
args.results.parent.mkdir(parents=True,exist_ok=True)
args.results.write_text(json.dumps(result,indent=2)+"\n")
print(json.dumps(result,indent=2))
