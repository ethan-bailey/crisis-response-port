"""Compare the four retained BehaviorDesigner plugin sources with pinned ILSpy output."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST_PATH = ROOT / "runtime/Original/BehaviorDesigner/source-provenance.json"


def sha(data):
    return hashlib.sha256(data).hexdigest()


def verify(decompiled_root, decompiler_version=None):
    decompiled_root = Path(decompiled_root)
    manifest = json.loads(MANIFEST_PATH.read_text(encoding="utf-8"))
    if decompiler_version is not None and decompiler_version != manifest["decompiler"].split()[-1]:
        raise AssertionError(f"ILSpy tool version {decompiler_version!r} != pinned {manifest['decompiler']!r}")
    missing = []
    for name, delta in manifest["decompilationDeltas"].items():
        if not delta["status"].startswith("unresolved:"):
            continue
        item = manifest["sources"][name]
        candidates = list(decompiled_root.rglob(Path(name).name))
        if len(candidates) != 1:
            raise AssertionError(f"expected exactly one ILSpy source for {name}, got {len(candidates)}")
        runtime = (ROOT / item["runtimePath"]).read_bytes()
        normalized = normalize_runtime(name, runtime)
        generated = candidates[0].read_bytes()
        if sha(normalized) != delta["normalizedRuntimeSha256"]:
            raise AssertionError(f"runtime normalization hash changed for {name}")
        if generated != normalized:
            raise AssertionError(
                f"pinned ILSpy output differs from runtime source for {name}: "
                f"generated={sha(generated)} expected={sha(normalized)}"
            )
        print(f"PASS: {name} reproduces exact pinned ILSpy bytes {sha(generated)}")
        missing.append(name)
    if len(missing) != 4:
        raise AssertionError(f"expected four unresolved plugin implementation outputs, checked {len(missing)}")
    print(f"PASS: {len(missing)} plugin source files reproduce with {manifest['decompiler']}")


def normalize_runtime(name, runtime):
    for prefix in (
        b"using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;\n",
        b"using Object = UnityEngine.Object;\n",
        b"using Task = BehaviorDesigner.Runtime.Tasks.Task;\n",
    ):
        if runtime.startswith(prefix):
            runtime = runtime[len(prefix):]
    if name in {"BehaviorDesigner.Runtime/ExternalBehavior.cs", "BehaviorDesigner.Runtime/Behavior.cs"}:
        if runtime.count(b"\tint IBehavior.GetInstanceID()") != 1:
            raise AssertionError(f"unexpected explicit IBehavior implementation in {name}")
        runtime = runtime.replace(b"\tint IBehavior.GetInstanceID()", b"\tvirtual int IBehavior.GetInstanceID()")
    return runtime


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("decompiled_root", type=Path)
    parser.add_argument("--decompiler-version")
    args = parser.parse_args()
    verify(args.decompiled_root, args.decompiler_version)
