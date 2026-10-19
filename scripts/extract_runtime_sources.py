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


def member(text, name):
    match = re.search(r'^\s*((?:public|private) [\w<>]+ ' + re.escape(name) + r')(?=\s*(?:[;{=]|=>))', text, re.M)
    if not match: raise ValueError('Original member not found: ' + name)
    start = match.start(1); tail = text[match.end(1):].lstrip()
    if tail.startswith('{'): return block(text, match.group(1))
    return text[start:text.index(';', match.end(1)) + 1]


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

    ammunition = read('AmmunitionBase.cs')
    scalar_region = ammunition[ammunition.index('public Availability availability;'):ammunition.index('public float MetricMass')]
    declarations = re.findall(r'^\s*(public (?:float|int|bool|string) \w+(?: = [^;]+)?;)', scalar_region, re.M)
    signatures = ['public float DiameterAtEnergy(float frictionEnergy, float originalDiameterInMillimeters)',
        'public float FragmentationEnergy(float mass)', 'public float TemporalCavityFactor(float velocity)',
        'public float PermanentCavityFactor(float velocity)', 'public static AmmunitionBase GetAmmunition(string name, AmmunitionBase fallback)']
    methods = [block(ammunition, sig) for sig in signatures]
    getters = [member(ammunition, name) for name in ['MetricMass','DiameterInMeters','MetricLength','MuzzleEnergy',
        'MuzzleEnergyTotal','Subsonic','MuzzleBlastRange','DetonationAudioPower']]
    extra_declarations = [member(ammunition, name) for name in ['availability', 'type', 'muzzleBlastForce', 'calibre', 'projectile', 'casing', 'highPolyModel', 'marksmanRound', 'sprite']]
    save('AmmunitionBase.cs', 'using UnityEngine;\nusing Crisis.Runtime.FullSource;\nusing Utilities;\npublic partial class AmmunitionBase : MonoBehaviour {\n' +
        block(ammunition, 'public enum Type') + '\n' + block(ammunition, 'public enum FragmentationMode') + '\n' + '\n'.join(declarations) + '\n' +
        '\n'.join(extra_declarations) + '\n' +
        re.search(r'public AnimationCurve expansionFunction;', ammunition).group() + '\n' +
        re.search(r'public FragmentationMode fragmentationMode;', ammunition).group() + '\n' +
        '\n'.join(getters + methods) + '\n}\n')
    report['methodSelections']['AmmunitionBase.cs'] = {sig: digest(body) for sig, body in zip(signatures, methods)}

    projectile = read('Projectile.cs')
    member_names = ['meshTransform','diameter','freeFallTimestamp','FrictionEnergy','Ammunition','StartPosition',
        'Mass','MetricMass','BaseVelocity','StartEnergy','Diameter','MetricDiameter','FrictionEnergyNormalized',
        'DeformationRatio','BallisticCoefficient','SuperSonic','TemporalCavityFactor','PermanentCavityFactor',
        'PenetrationDistance','DisplacedMaterialVolume','InteractionEnergy','Distance','Velocity','Drag',
        'PotentialFlightRangeLeft','Energy','EnergyNormalized','HasFragmented']
    members = []
    for name in member_names: members.append(member(projectile, name))
    signatures = ['public static float KineticEnergy(float massInKilograms, float velocityInMetersPerSecond)',
        'public void RecalculateVelocity()', 'public float DistanceAtPosition(Vector3 worldPosition)',
        'public float DragVelocityAtPosition(Vector3 worldPosition)', 'public float VelocityAtPosition(Vector3 worldPosition)',
        'public Vector3 GravityVelocityAtTime(float time)', 'public float DiameterAtFrictionEnergy(float frictionEnergy, bool skipRotation = false)',
        'public float VelocityToEnergy(float velocity)', 'public void RemoveEnergy(float energy)',
        'public float ConvertEnergyToFriction(float energy)', 'public void ConvertRemainingEnergyToFriction()',
        'public float PotentialPenetrationDistance(float energy, float density, float diameterInMillimeters, float frictionFactor)',
        'public float PotentialPenetrationDistance(float density)', 'public float EnergyToPenetrate(float distance, float density)',
        'public float EnergyToPenetrate(float distance, float density, float frictionFactor, float diameterInMillimeters)',
        'public bool TestFragmentation()', 'public static float MaximumRange(float muzzleVelocity, float ballisticCoefficient, float flightDrag)']
    methods = [block(projectile, sig) for sig in signatures]
    save('Projectile.cs', 'using UnityEngine;\npublic partial class Projectile : MonoBehaviour {\n' + '\n'.join(members + methods) + '\n}\n')
    report['methodSelections']['Projectile.cs'] = {sig: digest(body) for sig, body in zip(signatures, methods)}
    report['excludedFeatures'].extend(['Projectile flight/collision/damage/effect lifecycle is not selected',
        'Ammunition UI, availability, resource lookup and fragmentation spawning are not selected'])

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
