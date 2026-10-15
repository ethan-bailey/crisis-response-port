#!/usr/bin/env python3
"""Repair only zero-sized RIFF/data length fields in otherwise valid recovered PCM WAVs.

Works on an ephemeral recovered Unity project, not the original archive.
Never synthesizes audio or modifies payload samples.
"""
import argparse, hashlib, json, struct, wave
from pathlib import Path

def inspect(p):
    raw=p.read_bytes()
    if len(raw)<44 or raw[:4]!=b"RIFF" or raw[8:12]!=b"WAVE" or raw[12:16]!=b"fmt ":
        return None
    if raw[16:20]!=b"\x10\0\0\0" or raw[36:40]!=b"data":
        return None
    fmt,channels,rate,byte_rate,block_align,bits=struct.unpack_from("<HHIIHH",raw,20)
    if fmt != 1 or channels not in (1,2) or bits!=16 or block_align!=channels*2:
        return None
    if byte_rate!=rate*block_align or (len(raw)-44)%block_align!=0:
        return None
    old_riff,old_data=struct.unpack_from("<I",raw,4)[0],struct.unpack_from("<I",raw,40)[0]
    if old_riff==len(raw)-8 and old_data==len(raw)-44:
        return {"status":"valid","size":len(raw)}
    if old_riff!=0 or old_data!=0:
        raise ValueError(f"{p.name}: unsafe nonzero size mismatch")
    return {"status":"repair","raw":raw,"size":len(raw),"channels":channels,"rate":rate}

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("project",type=Path)
    ap.add_argument("--out",required=True,type=Path)
    ap.add_argument("--apply",action="store_true")
    ap.add_argument("--expected",type=int,default=250)
    args=ap.parse_args()
    audio=args.project/"Assets"/"AudioClip"
    wavs=list(audio.rglob("*.wav"))
    repairs=[];valid=0;skipped=[]
    for p in wavs:
        data=inspect(p)
        if data is None:skipped.append(str(p.relative_to(args.project)));continue
        if data["status"]=="valid":valid+=1;continue
        repairs.append((p,data))
    if len(repairs)!=args.expected or valid!=46 or skipped:
        raise SystemExit(f"STOP: expected {args.expected} zero-sized PCM and 46 valid WAVs, got {len(repairs)} and {valid}; unsafe/skipped={skipped[:5]}")
    manifest={"mode":"applied" if args.apply else "audit_only","repaired":len(repairs),"already_valid":valid,"skipped":len(skipped),"samples":[]}
    for p,data in repairs:
        original=data["raw"]
        patched=bytearray(original)
        struct.pack_into("<I",patched,4,len(original)-8)
        struct.pack_into("<I",patched,40,len(original)-44)
        if patched[:4]!=original[:4] or patched[8:40]!=original[8:40] or patched[44:]!=original[44:]:
            raise SystemExit("STOP: touched audio payload or unrelated header")
        # WAV decoder validation after header repair, without writing the sample data elsewhere.
        from io import BytesIO
        with wave.open(BytesIO(patched),"rb") as f:
            if f.getnframes() != (len(original)-44)//data["channels"]//2:
                raise SystemExit(f"STOP: decoded frame count mismatch {p.name}")
        if args.apply:p.write_bytes(patched)
        if len(manifest["samples"])<5:
            manifest["samples"].append({"name":p.name,"bytes":len(original),"channels":data["channels"],"sample_rate":data["rate"],
                "payload_sha256":hashlib.sha256(original[44:]).hexdigest()})
    args.out.parent.mkdir(parents=True,exist_ok=True)
    args.out.write_text(json.dumps(manifest,indent=2)+"\n")
    print(f"Recovered WAV RIFF/data header lengths {manifest['mode']}: {len(repairs)}, existing valid: {valid}. PCM data unchanged.")
if __name__=="__main__":main()
