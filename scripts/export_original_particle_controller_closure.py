#!/usr/bin/env python3
"""Export the recovered MonoBehaviour spawn controllers attached to impact prefabs."""
import argparse, gzip, hashlib, json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BALLISTIC = "f9f9f8d0f023b657c5276ea277044f46"
EMISSION = "7f9bb5f5ef1d9fb3ca6dbc9296820005"
PARTICLE_CLASSES = {198, 199}

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("--profile",type=Path,required=True)
    parser.add_argument("--out",type=Path,required=True)
    args=parser.parse_args()
    manifest=json.loads((args.profile/"manifest.json").read_text())
    metadata=json.loads(gzip.open(args.profile/"metadata.json.gz","rt").read())
    particles=json.loads(gzip.open(ROOT/"game/assets/ballistics/original-particles-source.json.gz","rt").read())
    assets={a["path"]:a for a in manifest["assets"]}
    effects=[]
    all_scripts=set(); counts={}
    for effect in particles["effects"]:
        source=assets.get(effect["source"])
        if not source: raise ValueError("effect prefab is outside the verified source profile: "+effect["source"])
        packaged=metadata.get("assetManifest",{}).get(effect["source"])
        if not packaged or source["sha256"]!=packaged.get("sha256"):
            raise ValueError("effect prefab package entry SHA mismatch: "+effect["source"])
        package=json.loads(gzip.open(args.profile/source["file"],"rt").read())
        records={r["fileID"]:r for r in package["records"]}
        objects={}; transforms={}; by_object={}
        for r in package["records"]:
            f=r.get("fields",{})
            if r["classID"]==1: objects[r["fileID"]]=f
            if r["classID"]==4: transforms[r["fileID"]]=f
            if r["classID"]==114:
                go=f.get("m_GameObject",{}).get("fileID")
                by_object.setdefault(go,[]).append(r)
        transform_by_object={f.get("m_GameObject",{}).get("fileID"):fid for fid,f in transforms.items()}
        parent_paths={}
        def path_for(go,seen=None):
            seen=set() if seen is None else seen
            if go in seen: raise ValueError("Transform parent cycle in "+effect["source"])
            seen.add(go)
            obj=objects[go]; tf=transforms[transform_by_object[go]]
            name=obj.get("m_Name") or ""
            parent=tf.get("m_Father",{}).get("fileID","0")
            if parent=="0": return name
            parent_go=transforms[parent].get("m_GameObject",{}).get("fileID")
            return path_for(parent_go,seen)+"/"+name
        for go in objects: parent_paths[go]=path_for(go)
        controllers=[]
        for record in package["records"]:
            if record["classID"]!=114: continue
            fields=record["fields"]; script=fields.get("m_Script",{}).get("guid")
            if script not in (BALLISTIC,EMISSION): continue
            all_scripts.add(script); counts[script]=counts.get(script,0)+1
            go=fields.get("m_GameObject",{}).get("fileID")
            related={r["classID"]:r["fileID"] for r in records.values()
                     if r.get("fields",{}).get("m_GameObject",{}).get("fileID")==go and r["classID"] in PARTICLE_CLASSES}
            controllers.append({"componentFileID":record["fileID"],"scriptGuid":script,
                "scriptPath":metadata["guidPaths"].get(script),"gameObjectFileID":go,
                "hierarchyPath":parent_paths[go],"serializedFields":fields,
                "particleSystemFileID":related.get(198),"particleRendererFileID":related.get(199)})
        if sum(c["scriptGuid"]==BALLISTIC for c in controllers)!=1:
            raise ValueError("impact prefab must have one BallisticHitEffect: "+effect["source"])
        if len([c for c in controllers if c["scriptGuid"]==EMISSION])!=len(effect["emitters"]):
            raise ValueError("each ParticleSystem must pair with its SingleEmissionEffect: "+effect["source"])
        effects.append({"guid":effect["guid"],"source":effect["source"],"sourcePrefabSha256":effect["sourcePrefabSha256"],
            "sourceProfileAssetSha256":source["sha256"],
            "roles":effect["roles"],"controllers":sorted(controllers,key=lambda c:int(c["componentFileID"]))})
    if len(effects)!=14 or counts!={BALLISTIC:14,EMISSION:26}:
        raise ValueError(f"impact controller coverage changed: effects={len(effects)} scripts={counts}")
    source_files={name:sha(ROOT/path) for name,path in {
        "BallisticHitEffect":"runtime/Original/FullSource/BallisticHitEffect.cs",
        "SingleEmissionEffect":"runtime/Original/FullSource/SingleEmissionEffect.cs"}.items()}
    output={"schemaVersion":1,"kind":"original-impact-particle-controller-closure",
        "profile":"global-ui-mission-operator","sourcePackageSha256":manifest["sourcePackageSha256"],
        "sourceFilesSha256":source_files,"counts":{"effects":len(effects),"BallisticHitEffect":counts[BALLISTIC],"SingleEmissionEffect":counts[EMISSION]},
        "effects":effects,"scope":"Source-authored serialized spawn-controller fields linked to the verified original ParticleSystem and Renderer components."}
    args.out.parent.mkdir(parents=True,exist_ok=True)
    args.out.write_bytes(gzip.compress(json.dumps(output,ensure_ascii=False,separators=(",",":"),sort_keys=True).encode(),mtime=0))
    print(json.dumps({"output":str(args.out),"sha256":sha(args.out),"counts":output["counts"],"sourcePackageSha256":output["sourcePackageSha256"]},indent=2))

if __name__=="__main__": main()
