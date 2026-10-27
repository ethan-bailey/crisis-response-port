"""Compare the four retained BehaviorDesigner plugin sources with pinned ILSpy output."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST_PATH = ROOT / "runtime/Original/BehaviorDesigner/source-provenance.json"


def sha(data):
    return hashlib.sha256(data).hexdigest()


def is_plugin_decompilation_candidate(delta):
    """Include both newly reproduced and not-yet-reproduced plugin outputs."""
    return delta.get("status", "").startswith(("unresolved:", "reproduced:"))


def verify(decompiled_root, decompiler_version=None):
    decompiled_root = Path(decompiled_root)
    manifest = json.loads(MANIFEST_PATH.read_text(encoding="utf-8"))
    if decompiler_version is not None and decompiler_version != manifest["decompiler"].split()[-1]:
        raise AssertionError(f"ILSpy tool version {decompiler_version!r} != pinned {manifest['decompiler']!r}")
    missing = []
    for name, delta in manifest["decompilationDeltas"].items():
        if not is_plugin_decompilation_candidate(delta):
            continue
        item = manifest["sources"][name]
        candidates = list(decompiled_root.rglob(Path(name).name))
        if len(candidates) != 1:
            raise AssertionError(f"expected exactly one ILSpy source for {name}, got {len(candidates)}")
        runtime = (ROOT / item["runtimePath"]).read_bytes()
        generated = candidates[0].read_bytes()
        verify_decompiler_output(name, generated, runtime, item, delta)
        print(
            f"PASS: {name} pinned ILSpy SHA {sha(generated)}; "
            f"runtime adaptation SHA {delta['normalizedRuntimeSha256']}"
        )
        missing.append(name)
    if len(missing) != 4:
        raise AssertionError(f"expected four unresolved plugin implementation outputs, checked {len(missing)}")
    print(f"PASS: {len(missing)} plugin source files reproduce with {manifest['decompiler']}")


def verify_decompiler_output(name, generated, runtime, item, delta):
    """Verify original decompiler identity separately from the port's runtime adaptation."""
    normalized_runtime = normalize_runtime(name, runtime)
    expected_runtime = delta["normalizedRuntimeSha256"]
    actual_runtime = sha(normalized_runtime)
    if actual_runtime != expected_runtime:
        raise AssertionError(
            f"normalized runtime source hash changed for {name}: "
            f"actual={actual_runtime} expected={expected_runtime}"
        )
    expected_decompiler = item.get("decompiledSha256")
    if expected_decompiler is None:
        raise AssertionError(f"missing pinned decompiler source hash for {name}")
    actual_decompiler = sha(generated)
    if actual_decompiler != expected_decompiler:
        raise AssertionError(
            f"pinned ILSpy output hash changed for {name}: "
            f"actual={actual_decompiler} expected={expected_decompiler}"
        )


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
