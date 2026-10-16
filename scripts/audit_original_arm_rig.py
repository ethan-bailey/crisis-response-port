#!/usr/bin/env python3
"""Read-only source arm rig audit: exported records, never inferred gameplay."""
import argparse
import json
from pathlib import Path

def audit(data):
    if data.get("format") != "crisis-original-arm-physics-v1":
        raise ValueError("Unexpected source format")
    output = {"format": "crisis-arm-source-import-audit-v1", "archiveSha256": data["archiveSha256"],
              "readyForControllerPlayback": False, "prefabs": [],
              "blockers": ["Joint rest-frame/native trajectory comparison",
                           "Unity layer collision filter and PhysicMaterial import",
                           "Original CharacterBody/ArmController lifecycle and force bridge"]}
    count = 0
    for prefab in data["prefabs"]:
        records = {r["fileID"]: r for r in prefab["records"]}
        if len(records) != len(prefab["records"]):
            raise ValueError("Duplicate source fileID")
        exported = {"name": prefab["name"], "arms": []}
        for side, arm in prefab["arms"].items():
            out_arm = {"side": side, "limbs": []}
            for limb in arm["limbs"]:
                def field(key, expected_class):
                    record = records[limb[key]]
                    if record["classID"] != expected_class:
                        raise ValueError("Unexpected component type: " + key)
                    return record["fields"]
                obj = field("gameObject", 1)
                transform = field("transform", 4)
                body = field("rigidbody", 54)
                joint = field("joint", 59)
                if any(v["m_GameObject"]["fileID"] != limb["gameObject"]
                       for v in (transform, body, joint)):
                    raise ValueError("Component ownership mismatch")
                if joint["m_ConnectedBody"]["fileID"] != limb["connectedBody"]:
                    raise ValueError("Connected rigidbody mismatch")
                if records[limb["connectedBody"]]["classID"] != 54:
                    raise ValueError("Missing connected Rigidbody")
                capsules = []
                for cid in limb["colliders"]:
                    record = records[cid]
                    c = record["fields"]
                    if record["classID"] != 136 or c["m_GameObject"]["fileID"] != limb["gameObject"]:
                        raise ValueError("Capsule ownership/type mismatch")
                    capsules.append({"fileID": cid, "radius": c["m_Radius"],
                                     "height": c["m_Height"], "direction": c["m_Direction"],
                                     "center": c["m_Center"], "enabled": bool(c["m_Enabled"]),
                                     "trigger": bool(c["m_IsTrigger"]), "material": c["m_Material"]})
                if not capsules or joint["m_Axis"] != {"x": 0, "y": 0, "z": 1}:
                    raise ValueError("Missing capsule or unexpected hinge axis")
                out_arm["limbs"].append({
                    "name": obj["m_Name"], "gameObject": limb["gameObject"],
                    "transformFileID": limb["transform"], "bodyFileID": limb["rigidbody"],
                    "jointFileID": limb["joint"], "connectedBodyFileID": limb["connectedBody"],
                    "parentTransformFileID": transform["m_Father"]["fileID"],
                    "localPosition": transform["m_LocalPosition"],
                    "localRotation": transform["m_LocalRotation"],
                    "localScale": transform["m_LocalScale"],
                    "mass": body["m_Mass"], "kinematic": bool(body["m_IsKinematic"]),
                    "gravity": bool(body.get("m_UseGravity", 1)),
                    "linearDrag": body["m_Drag"], "angularDrag": body["m_AngularDrag"],
                    "constraints": body["m_Constraints"], "capsules": capsules,
                    "joint": {"axis": joint["m_Axis"], "anchor": joint["m_Anchor"],
                              "connectedAnchor": joint["m_ConnectedAnchor"],
                              "autoConfigureConnectedAnchor": bool(joint["m_AutoConfigureConnectedAnchor"]),
                              "collisionEnabled": bool(joint["m_EnableCollision"]),
                              "useLimits": bool(joint["m_UseLimits"]), "limits": joint["m_Limits"],
                              "useSpring": bool(joint["m_UseSpring"]), "spring": joint["m_Spring"],
                              "useMotor": bool(joint["m_UseMotor"]), "motor": joint["m_Motor"]}})
                count += 1
            exported["arms"].append(out_arm)
        output["prefabs"].append(exported)
    if len(output["prefabs"]) != 2 or count != 12:
        raise ValueError("Unexpected source arm topology")
    output["limbBodies"] = count
    return output

if __name__ == "__main__":
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--source", type=Path, default=Path(__file__).resolve().parents[1] /
                   "source-reference/Original/ArmPhysics/prefabs.json")
    p.add_argument("--output", type=Path)
    args = p.parse_args()
    report = json.dumps(audit(json.loads(args.source.read_text())), sort_keys=True, indent=2) + "\n"
    if args.output:
        args.output.write_text(report)
    else:
        print(report)
