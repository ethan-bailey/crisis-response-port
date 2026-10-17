#!/usr/bin/env python3
"""Recover Unity 2017 developer-authored texture importer wrapping for original limb skins.
No inference of Unity GetPixel behavior; preserve all source properties and raw SHA.
"""
import hashlib,json,re,sys,zipfile
from pathlib import Path
PIN='cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d'
PREFIX='export/ExportedProject/'
KEYS=('m_WrapMode','m_WrapU','m_WrapV','m_WrapW','m_FilterMode','m_Aniso',
      'm_Readable','isReadable','alphaIsTransparency','m_TextureType','textureType',
      'm_MipMapMode','m_MipMapsPreserveCoverage','m_AlphaIsTransparency')
def main(archive,manifest,out):
 if hashlib.sha256(archive.read_bytes()).hexdigest()!=PIN:
  raise ValueError('Unpinned original game archive')
 data=manifest.read_bytes();m=json.loads(data)
 all_guids=sorted(set(g for limb in m['limbs'] for g in limb['skinGuids']))
 if len(m['limbs'])!=17 or len(all_guids)!=15:
  raise ValueError('Unexpected original 17-renderer, 15 GUID limb texture source selection')
 wanted=set(all_guids);found={}
 with zipfile.ZipFile(archive) as zip:
  for path in zip.namelist():
   if not path.startswith(PREFIX) or not path.endswith('.meta'):continue
   raw=zip.read(path)
   guid=re.search(rb'(?m)^guid: ([0-9a-f]{32})$',raw)
   if guid is None or guid.group(1).decode() not in wanted:continue
   key=guid.group(1).decode()
   if key in found:raise ValueError('Duplicated original texture GUID '+key)
   source=path[:-5]
   if source not in zip.namelist():raise ValueError('Original source texture missing: '+source)
   source_bytes=zip.read(source)
   text=raw.decode('utf8')
   # Keep exact serialized key lines with section indentation, as we cannot
   # assume original Unity's importer schema uses a particular wrap fallback.
   lines=text.splitlines()
   serialized={}
   for name in KEYS:
    occurrences=[]
    for line in lines:
     match=re.match(r'^(\s*)'+re.escape(name)+r':\s*(.*?)\s*$',line)
     if match:occurrences.append({'indent':len(match.group(1)),'value':match.group(2)})
    serialized[name]=occurrences
   found[key]={'guid':key,'assetPath':source[len(PREFIX):],
    'assetSha256':hashlib.sha256(source_bytes).hexdigest(),
    'metaSha256':hashlib.sha256(raw).hexdigest(),
    'importerLines':serialized,
    'importerKind':'TextureImporter' if re.search(r'(?m)^TextureImporter:',text) else 'Other',
   }
 missing=wanted-found.keys()
 if missing:raise ValueError('Developer texture importer metas missing: '+str(sorted(missing)))
 result={'format':'crisis-original-limb-texture-importer-v1','archiveSha256':PIN,
  'operatorManifestSha256':hashlib.sha256(data).hexdigest(),
  'sourceSkinGuids':[found[k] for k in all_guids]}
 out.parent.mkdir(parents=True,exist_ok=True)
 out.write_text(json.dumps(result,sort_keys=True,indent=2)+'\n')
 print(json.dumps({'originalSkins':len(all_guids),
   'sourceMetaSha256':hashlib.sha256(out.read_bytes()).hexdigest(),
   'wrapKeys':{key:sum(bool(f['importerLines'][key]) for f in found.values())
               for key in ('m_WrapMode','m_WrapU','m_WrapV')},
   'kinds':sorted(set(x['importerKind'] for x in found.values()))},sort_keys=True))
if __name__=='__main__':
 if len(sys.argv)!=4:raise SystemExit('usage: recover_original_skin_texture_importers.py ZIP OPERATOR_MANIFEST OUT')
 main(Path(sys.argv[1]),Path(sys.argv[2]),Path(sys.argv[3]))
