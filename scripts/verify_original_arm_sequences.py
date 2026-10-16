#!/usr/bin/env python3
"""Check tracked source/bundle provenance before the actual C#/WASM build."""
import hashlib
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
manifest = json.loads((root / 'runtime/Original/Animation/source-provenance.json').read_text())
bundle_path = root / 'game/assets/original-arm-sequences.json'
assert hashlib.sha256(bundle_path.read_bytes()).hexdigest() == manifest['bundleSha256'], 'Bundle changed'
bundle = json.loads(bundle_path.read_text())
assert bundle['archiveSha256'] == manifest['archiveSha256']
assert bundle['playbackImplemented'] is False
sources = {path: digest for path, digest in manifest['originalFiles'].items() if path.endswith('.cs')}
assert len(sources) == 6
for path, digest in sources.items():
    copy = root / 'runtime/Original/Animation' / Path(path).name
    assert hashlib.sha256(copy.read_bytes()).hexdigest() == digest, 'Original source changed: ' + path
assert len(bundle['sequences']) == 7
assert len({s['guid'] for s in bundle['sequences']}) == 7
assert sum(len(s['right']) + len(s['left']) for s in bundle['sequences']) == 21
for item in bundle['sequences']:
    assert item['sourceSha256'] == manifest['originalFiles'][item['path']]
print(json.dumps(dict(status='pass', unchangedOriginalClasses=6, originalPrefabs=7, originalFrames=21)))
