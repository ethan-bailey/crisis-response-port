#!/usr/bin/env python3
"""Extract only the authored Occupant MeshRenderer materials; never guess textures."""
import json,re,hashlib
from pathlib import Path
ROOT=Path('source-reference/Original/NPCRecovery/Assets')
SCENE=Path('game/assets/occupant/source-visual-spawn.json')
TARGET=Path('game/assets/occupant/source-materials.json')
def field(pattern,text,default=None):
 m=re.search(pattern,text,re.M)
 return m.group(1) if m else default
def material(path):
 src=path.read_bytes();raw=src.decode()
 def f(name,default):
  return float(field(r'^      '+re.escape(name)+r': ([^\n]+)',raw,str(default)))
 rgba=field(r'^      _Color: \{([^}]+)\}',raw,'')
 color={k:float(v) for k,v in re.findall(r'([rgba]): ([^, }]+)',rgba)}
 return {'name':field(r'^  m_Name: ([^\n]+)',raw),
  'sha256':hashlib.sha256(src).hexdigest(),
  'shaderGuid':field(r'^  m_Shader: \{fileID: \d+, guid: ([0-9a-f]{32})',raw),
  'textureGuid':field(r'^      _MainTex:\n        m_Texture: \{fileID: \d+, guid: ([0-9a-f]{32})',raw),
  'cutoff':f('_Cutoff',0.5),'depthWrite':f('_ZWrite',1)!=0,
  'color':color or {'r':1,'g':1,'b':1,'a':1}}
def main():
 src=json.loads(SCENE.read_text())
 used={m['guid'] for render in src['renderers'] for m in render['materials']}
 guids={}
 for meta in (ROOT/'Material').glob('*.meta'):
  code=field(r'^guid: ([0-9a-f]{32})$',meta.read_text())
  if code:guids[code]=meta.with_suffix('')
 if not used.issubset(guids):
  raise ValueError('Missing original prefab renderer materials '+str(used-guids.keys()))
 result={}
 for guid in sorted(used):
  path=guids[guid];m=material(path)
  texturePath=src['guidPaths'].get(m['textureGuid'])
  if not texturePath or not texturePath.startswith('Texture2D/'):
   raise ValueError('Missing source texture GUID for '+guid)
  outputTexture=Path('game/assets/occupant/skins')/texturePath.removeprefix('Texture2D/')
  if not outputTexture.is_file():
   raise ValueError('Original material texture not packaged: '+str(outputTexture))
  m['texturePath']='./assets/occupant/skins/'+outputTexture.name
  m['path']=str(path.relative_to(ROOT))
  result[guid]=m
 assert len(result)==13
 TARGET.write_text(json.dumps({'format':'original-occupant-source-materials-v1','materials':result},sort_keys=True,separators=(',',':'))+'\n')
 print(f'PASS: {len(result)} original material GUIDs; {TARGET.stat().st_size} bytes')
if __name__=='__main__':main()
