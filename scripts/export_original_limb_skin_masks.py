#!/usr/bin/env python3
"""Preserve the exact initial damage-mask green channel used by LimbRenderer.TestPixel.
Input is exactly the original Operator manifest and its preserved PNG skins.
Original LimbTextureData.Initialize sets green to 0 where skin alpha>0 and 1 otherwise.\nRows are bottom-up, matching Unity Texture2D.GetPixel's Y orientation.
No capsule or opaque-alpha substitution is used.
"""
import base64,gzip,hashlib,io,json,sys
from pathlib import Path

def export(operator_dir:Path, output:Path):
 from PIL import Image
 manifest_path=operator_dir/'operator.json'
 raw=manifest_path.read_bytes();manifest=json.loads(raw)
 entries={}
 for limb in manifest['limbs']:
  texture=limb['texture']
  if not texture.startswith('skins/') or '..' in texture:
   raise ValueError('Unexpected developer limb texture path')
  if texture not in entries:
   skin=operator_dir/texture;original=skin.read_bytes()
   with Image.open(io.BytesIO(original)) as image:
    image.load()
    image=image.convert('RGBA')
    width,height=image.size
    # Unity Texture2D.GetPixel(0,0) references the bottom-left texel.
    pixels=image.load()
    # Original LimbTextureData.Initialize:\n    # pixels[num].g = (!(pixels[num].a > 0f)) ? 1f : 0f;\n    green=bytes(0 if pixels[x,y][3]>0 else 255\n      for y in range(height-1,-1,-1) for x in range(width))
   if not green or width<=0 or height<=0:raise ValueError('Missing original limb coverage')
   entries[texture]={'texture':texture,'sha256':hashlib.sha256(original).hexdigest(),
     'width':width,'height':height,'pixelOrder':'left-to-right, bottom-to-top',
     'originalStaticDamageGreenBottomUpBase64':base64.b64encode(green).decode('ascii'),
     'sourceRenderers':[]}
  entries[texture]['sourceRenderers'].append({
   'transformFileID':limb['transformFileID'],
   'gameObjectFileID':limb['gameObjectFileID'],
   'skinGuids':limb['skinGuids']})
 if len(entries)!=manifest['copiedTextureCount'] or len(manifest['limbs'])!=manifest['limbRendererCount']:
  raise ValueError('Original renderer / skin manifest mismatch')
 bundle={'format':'crisis-original-limb-initial-damage-green-v1',
   'sourceManifestSha256':hashlib.sha256(raw).hexdigest(),
   'originalTextureCount':len(entries),'originalRendererCount':len(manifest['limbs']),
   'skins':[entries[k] for k in sorted(entries)]}
 output.parent.mkdir(parents=True,exist_ok=True)
 content=json.dumps(bundle,sort_keys=True,separators=(',',':')).encode()+b'\n'
 with output.open('wb') as f:
  with gzip.GzipFile(filename='',mode='wb',fileobj=f,mtime=0,compresslevel=9) as writer:writer.write(content)
 print(json.dumps({'textures':len(entries),'renderers':len(manifest['limbs']),
  'bundleSha256':hashlib.sha256(output.read_bytes()).hexdigest(),
  'sourceManifestSha256':bundle['sourceManifestSha256'],'uncompressedBytes':len(content)},sort_keys=True))
if __name__=='__main__':
 if len(sys.argv)!=3:raise SystemExit('usage: export_original_limb_skin_masks.py OPERATOR_ASSET_DIR OUT_GZIP')
 export(Path(sys.argv[1]),Path(sys.argv[2]))
