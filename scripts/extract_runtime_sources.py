#!/usr/bin/env python3
"""Select recovered code without rewriting its algorithms; record every selection."""
import hashlib
import json
import re
import sys
from pathlib import Path


def digest(text):
    return hashlib.sha256(text.encode()).hexdigest()


def block(text, signature):
    start = text.index(signature)
    opening = text.index('{', start)
    depth = 0
    # The selected methods contain no braces in strings/comments. Fail if that
    # changes rather than extracting a guessed body.
    for pos in range(opening, len(text)):
        if text[pos] == '{': depth += 1
        if text[pos] == '}': depth -= 1
        if depth == 0: return text[start:pos + 1]
    raise ValueError(f'Unbalanced method: {signature}')


def main():
    source = Path(sys.argv[1]) / 'Assets/Scripts/Assembly-CSharp'
    out = Path(sys.argv[2]); out.mkdir(parents=True, exist_ok=True)
    report = {'originalFiles': {}, 'methodSelections': {}, 'excludedFeatures': [
        'Input binding dialog/UI', 'InputMapping disk serialization and version migration',
        'Unselected gameplay classes; this extraction is not the whole game']}

    def read(name):
        text = (source / name).read_text()
        report['originalFiles'][name] = digest(text)
        return text

    def save(name, text): (out / name).write_text(text)

    # These complete original components require implemented engine APIs only.
    for name in ['DecayingObject.cs', 'ObjectRemover.cs', 'AudioMaterial.cs', 'BuildingController.cs']:
        text = read(name)
        save(name, ('using Object = UnityEngine.Object;\n' if 'Object.' in text else '') + text)

    text = read('InputKey.cs')
    # Serialization-only attributes are omitted; all properties and code remain.
    text = re.sub(r'^using FullSerializer;\n', '', text, flags=re.M)
    text = re.sub(r'^\s*\[fs(?:Object|Property|Ignore)[^\n]*\]\n', '\n', text, flags=re.M)
    save('InputKey.cs', text)

    mapping = read('InputMapping.cs')
    # Preserve field initializers, categories, All, and exact registration code.
    prefix = mapping[mapping.index('public class InputMapping'):mapping.index('\tpublic InputMapping()')]
    prefix = re.sub(r'^\s*\[fsIgnore\]\n', '\n', prefix, flags=re.M)
    signatures = ['public InputMapping()', 'private void RegisterAllKeys()', 'private void Overwrite(InputMapping data)']
    methods = [block(mapping, sig) for sig in signatures]
    save('InputMapping.cs', 'using System.Collections.Generic;\nusing UnityEngine;\n' + prefix + '\n'.join(methods) + '\n}\n')
    report['methodSelections']['InputMapping.cs'] = {sig: digest(body) for sig, body in zip(signatures, methods)}

    handler = read('InputHandler.cs')
    signatures = ['public void SetMapping(InputMapping keys)', 'private void UpdateKeys()',
        'public List<InputKey> FindKeyCodeBindings(KeyCode code)', 'public List<InputKey> FindAxisBindings(string axis, float direction)',
        'public void EnableAll()', 'public void DisableAll()', 'public void OnMissionStart()', 'public void OnMissionEnd()',
        'public void DisableOperatorInput(bool disable = true)', 'public void SuspendInput(bool suspend, bool reset = false)',
        'public void ResetAll()', 'public void EnableCategory(InputKey.Category category, bool enable = true)']
    methods = [block(handler, sig) for sig in signatures]
    save('InputHandler.cs', '''using System.Collections.Generic;
using UnityEngine;
public class InputHandler : MonoBehaviour {
''' + re.search(r'public static float DefaultLongPress = [^;]+;', handler).group() + '\n' +
         re.search(r'private bool inputSuspended;', handler).group() + '\n' +
         re.search(r'public InputMapping Keys \{ get; private set; \}', handler).group() + '\n' +
         '\n'.join(methods) + '\n}\n')
    report['methodSelections']['InputHandler.cs'] = {sig: digest(body) for sig, body in zip(signatures, methods)}

    keys = sorted(set(re.findall(r'KeyCode\.(\w+)', mapping + handler)))
    # Enum names are extracted, not used as invented Unity numeric key values.
    save('KeyCode.cs', 'namespace UnityEngine { public enum KeyCode { ' + ','.join(keys) + ' } }\n')
    report['keyCodeNames'] = keys
    report['transformations'] = ['Unity Object alias resolves .NET name collision',
        'Omitted FullSerializer annotations; no serialization implementation claimed',
        'Selected original method bodies copied verbatim with individual SHA-256',
        'Runtime host invokes original UpdateKeys before scene Update; rebinding is excluded']
    report['generatedFiles'] = {p.name: digest(p.read_text()) for p in out.glob('*.cs')}
    (out / 'source-provenance.json').write_text(json.dumps(report, indent=2))
    print(json.dumps({'originalFiles':len(report['originalFiles']), 'selectedMethods':sum(len(x) for x in report['methodSelections'].values())}))


if __name__ == '__main__': main()
