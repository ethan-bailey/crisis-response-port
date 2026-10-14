#!/usr/bin/env python3
"""Validate structural invariants in the heuristic Unity scene manifest."""
from __future__ import annotations
import json
import sys
from collections import Counter
from pathlib import Path


def validate(data: dict) -> dict:
    objects = data.get("gameObjects", [])
    by_id = {int(obj["fileID"]): obj for obj in objects}
    errors: list[str] = []
    warnings: list[str] = []
    if len(by_id) != len(objects):
        errors.append("duplicate GameObject fileID values")

    parent_edges = 0
    component_refs = 0
    missing_components = []
    child_edges = 0
    missing_children = []
    for obj in objects:
        oid = int(obj["fileID"])
        parent = obj.get("parentGameObjectFileID")
        if parent is not None:
            parent_edges += 1
            if int(parent) not in by_id:
                errors.append(f"GameObject {oid} references missing parent {parent}")
            elif oid not in by_id[int(parent)].get("childGameObjectFileIDs", []):
                errors.append(f"parent/child edge not reciprocal: parent {parent}, child {oid}")
        for child in obj.get("childGameObjectFileIDs", []):
            child_edges += 1
            if int(child) not in by_id:
                missing_children.append({"parent": oid, "child": child})
            elif by_id[int(child)].get("parentGameObjectFileID") != oid:
                errors.append(f"child/parent edge not reciprocal: parent {oid}, child {child}")
        for cid in obj.get("components", []):
            component_refs += 1
            if int(cid) not in {int(r["fileID"]) for r in data.get("otherRecords", [])}:
                missing_components.append({"gameObject": oid, "component": int(cid)})

    if missing_children:
        errors.append(f"{len(missing_children)} child references point to missing GameObjects")
    if missing_components:
        errors.append(f"{len(missing_components)} component references point to missing serialized records")

    # A parent walk detects cycles without requiring the manifest to be lossless.
    cycle_nodes = []
    for start in by_id:
        seen = set()
        current = start
        while current in by_id:
            if current in seen:
                cycle_nodes.append(start)
                break
            seen.add(current)
            parent = by_id[current].get("parentGameObjectFileID")
            if parent is None:
                break
            current = int(parent)
    if cycle_nodes:
        errors.append(f"parent cycles detected from {len(cycle_nodes)} starting GameObjects")

    hierarchy = data.get("hierarchy", {})
    roots = [oid for oid, obj in by_id.items()
             if obj.get("transform") is not None
             and obj["transform"].get("parentFileID", 0) in (0, None)]
    recorded_roots = set(map(int, hierarchy.get("rootGameObjectFileIDs", [])))
    if set(roots) != recorded_roots:
        errors.append("recorded root list does not match GameObject parent fields")
    if parent_edges != child_edges:
        errors.append(f"parent edge count {parent_edges} differs from child edge count {child_edges}")

    classes = Counter(c.get("className", "unknown")
                      for obj in objects for c in obj.get("componentDetails", []))
    if hierarchy.get("gameObjectsWithoutTransform"):
        warnings.append(f"{len(hierarchy['gameObjectsWithoutTransform'])} GameObjects have no Transform/RectTransform")
    if hierarchy.get("unresolvedParentTransformReferences"):
        warnings.append(f"{len(hierarchy['unresolvedParentTransformReferences'])} parent Transform references are unresolved")

    return {
        "sourceScene": data.get("sourceScene"),
        "gameObjects": len(objects),
        "componentReferences": component_refs,
        "hierarchyParentEdges": parent_edges,
        "hierarchyChildEdges": child_edges,
        "rootGameObjects": len(roots),
        "componentClassOccurrences": dict(sorted(classes.items())),
        "errors": errors,
        "warnings": warnings,
        "valid": not errors,
        "limitations": [
            "Validates the extracted manifest only; it does not prove Unity import/runtime validity.",
            "The manifest parser is heuristic and does not preserve every Unity serialization feature.",
            "Transform math, prefab overrides, animation, physics and script execution are not evaluated."
        ]
    }


def main() -> int:
    if len(sys.argv) != 3:
        print("usage: validate_scene_manifest.py MANIFEST_JSON REPORT_JSON", file=sys.stderr)
        return 2
    source, target = map(Path, sys.argv[1:])
    result = validate(json.loads(source.read_text(encoding="utf-8")))
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps(result, indent=2))
    return 0 if result["valid"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
