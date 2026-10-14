#!/usr/bin/env python3
"""Static recursive GUID dependency audit for a recovered Unity scene.

This does not parse Unity YAML semantics or execute the project. It follows GUID
references through readable serialized assets and records unresolved references.
"""
from __future__ import annotations
import collections
import re
import sys
from pathlib import Path

GUID_RE = re.compile(r"guid: ([0-9a-f]{32})")
ASSET_REF_RE = re.compile(r"\{fileID: -?\d+, guid: ([0-9a-f]{32}), type: \d+\}")
UNITY_UI_SCRIPT_GUIDS = {"f5f67c52d1564df4a8936ccd202a3bd8"}
BUILTIN_GUIDS = {"0000000000000000e000000000000000", "0000000000000000f000000000000000"}
TEXT_EXTS = {
    ".unity", ".prefab", ".asset", ".mat", ".controller", ".anim",
    ".overrideController", ".renderTexture", ".mixer", ".guiskin", ".meta",
    ".cs", ".shader", ".cginc", ".json", ".xml", ".txt",
}
SCRIPT_PREFIX = "Assets/Scripts/Assembly-CSharp/"

def main() -> int:
    if len(sys.argv) != 3:
        print("usage: scene_dependency_audit.py PROJECT_ROOT OUTPUT.md", file=sys.stderr)
        return 2
    project = Path(sys.argv[1]).resolve()
    output = Path(sys.argv[2])
    assets = project / "Assets"
    if not assets.is_dir():
        raise SystemExit(f"Assets directory not found: {assets}")

    guid_to_asset: dict[str, Path] = {}
    for meta in assets.rglob("*.meta"):
        try:
            text = meta.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        match = re.search(r"^guid: ([0-9a-f]{32})", text, re.M)
        if match:
            guid_to_asset[match.group(1)] = meta.with_suffix("")

    scene = assets / "Scenes" / "Marksman range.unity"
    if not scene.is_file():
        raise SystemExit(f"Expected scene not found: {scene}")
    try:
        scene_text = scene.read_text(encoding="utf-8", errors="ignore")
    except OSError as exc:
        raise SystemExit(str(exc))

    direct_counts = collections.Counter(ASSET_REF_RE.findall(scene_text))
    unresolved_direct = sorted(g for g in direct_counts if g not in guid_to_asset and g not in BUILTIN_GUIDS)
    visited: set[Path] = set()
    unresolved: collections.Counter[str] = collections.Counter()
    edges: set[tuple[str, str]] = set()
    queue = collections.deque([scene])
    while queue:
        current = queue.popleft()
        current = current.resolve()
        if current in visited or not current.is_file():
            continue
        visited.add(current)
        if current.suffix not in TEXT_EXTS:
            continue
        try:
            data = current.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        source = str(current.relative_to(project))
        for guid in set(GUID_RE.findall(data)):
            if guid in BUILTIN_GUIDS:
                continue
            target = guid_to_asset.get(guid)
            if target is None:
                unresolved[guid] += 1
                continue
            target_rel = str(target.relative_to(project))
            edges.add((source, target_rel))
            if target not in visited:
                queue.append(target)

    scene_rel = str(scene.relative_to(project))
    script_refs = collections.Counter()
    direct_assets = collections.Counter()
    for guid, count in direct_counts.items():
        target = guid_to_asset.get(guid)
        if target is None:
            continue
        rel = str(target.relative_to(project))
        direct_assets[rel] += count
        if rel.startswith(SCRIPT_PREFIX):
            script_refs[rel] += count

    lines = [
        "# Recursive Marksman range dependency audit",
        "",
        "> Static GUID traversal only. It does not prove that Unity can import, compile, or execute these assets.",
        "",
        f"- Root scene: `{scene_rel}`",
        f"- Scene text bytes: {len(scene_text.encode('utf-8')):,}",
        f"- Scene GameObject records: {scene_text.count('!u!1 &'):,}",
        f"- Scene Transform records: {scene_text.count('!u!4 &'):,}",
        f"- Scene MonoBehaviour records: {scene_text.count('!u!114 &'):,}",
        f"- Scene MeshRenderer records: {scene_text.count('!u!23 &'):,}",
        f"- Scene MeshFilter records: {scene_text.count('!u!33 &'):,}",
        f"- Unique serialized asset GUID references directly by scene: {len(direct_counts):,}",
        f"- Direct serialized asset-reference occurrences: {sum(direct_counts.values()):,}",
        f"- Unique readable serialized assets reached recursively (including scene): {len(visited):,}",
        f"- Dependency edges found: {len(edges):,}",
        f"- Distinct unresolved serialized asset GUIDs seen recursively (excluding known Unity built-ins and Unity UI package script GUID): {len(unresolved):,}",
        "",
        "## Most frequent direct scene asset references",
        "",
        "| Occurrences | Asset path |",
        "|---:|---|",
    ]
    for path, count in direct_assets.most_common(40):
        lines.append(f"| {count} | `{path}` |")
    lines += ["", "## Direct scene script references", "", "| Occurrences | Script path |", "|---:|---|"]
    for path, count in sorted(script_refs.items(), key=lambda item: (-item[1], item[0])):
        lines.append(f"| {count} | `{path}` |")
    lines += ["", "## Unresolved GUIDs", ""]
    if unresolved:
        lines += ["| GUID | Occurrences encountered |", "|---|---:|"]
        for guid, count in sorted(unresolved.items(), key=lambda item: (-item[1], item[0]))[:100]:
            lines.append(f"| `{guid}` | {count} |")
    else:
        lines.append("No unresolved serialized asset references were found in traversed readable assets.")
    lines += [
        "",
        "## Dependency edge list",
        "",
        "The first 500 edges are listed to keep the report compact; the counts above cover the full traversal.",
        "",
    ]
    for source, target in sorted(edges)[:500]:
        lines.append(f"- `{source}` → `{target}`")
    lines += [
        "",
        "## Interpretation",
        "",
        "- Asset-reference resolution only means a matching `.meta` file exists in the recovered project; it does not guarantee that the asset imports correctly.",
        "- Some Unity-generated or binary dependencies may not be traversable from text. AssetBundles, built-in resources, shader behavior, physics settings, and runtime-instantiated content need separate inspection.",
        "- Use this report to define the source-preserving vertical-slice dependency closure, not as proof of browser compatibility or behavioral parity.",
        "",
    ]
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text("\n".join(lines), encoding="utf-8")
    print(f"Wrote {output}: {len(visited)} assets, {len(edges)} edges, {len(unresolved)} unresolved GUIDs")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
