"""Freeze legacy gameplay ownership while the full original actor is integrated.

An intentional exception must update the reviewed manifest and explain why in
the audit log. This guard does not claim that the migration is already complete.
"""
import hashlib
import json
from pathlib import Path
import re
from inventory_source_dependencies import NOISE

ROOT=Path(__file__).resolve().parents[1]
MANIFEST=ROOT/'docs/audit/actor-migration-boundary.json'


def check(root=ROOT):
    manifest=json.loads((root/'docs/audit/actor-migration-boundary.json').read_text())
    errors=[]
    reviewed=manifest.get('reviewedExceptions',{})
    if not isinstance(reviewed,dict):
        errors.append('Reviewed legacy exceptions must be a file-to-review mapping')
        reviewed={}
    for name,decision in reviewed.items():
        if name not in manifest['frozenLegacyFiles']:
            errors.append(f'Reviewed legacy exception is not frozen: {name}')
            continue
        if not isinstance(decision,dict) or decision.get('acceptedSha256')!=manifest['frozenLegacyFiles'][name]:
            errors.append(f'Reviewed legacy exception does not match its frozen hash: {name}')
            continue
        if not str(decision.get('reason','')).strip() or not str(decision.get('evidence','')).strip():
            errors.append(f'Reviewed legacy exception needs a reason and evidence: {name}')
    for name,expected in manifest['frozenLegacyFiles'].items():
        path=root/name
        if not path.is_file() or hashlib.sha256(path.read_bytes()).hexdigest()!=expected:
            errors.append(f'Legacy gameplay owner changed: {name}; migrate to original actor or document a reviewed exception')
    # A new partial declaration must not bypass the freeze by using a new file.
    owners=re.compile(r'\b(?:class|struct|record(?:\s+(?:class|struct))?)\s+(?:CharacterBody|Blackboard|Limb|WeaponBase|Equipment)\b')
    for directory in ('runtime/Engine','runtime/Original'):
        for path in (root/directory).rglob('*.cs'):
            source=NOISE.sub(' ',path.read_text(encoding='utf-8-sig'))
            if re.search(r'\bnamespace\s+Crisis\.Runtime\.Weapons\b',source) and owners.search(source):
                name=path.relative_to(root).as_posix()
                if name not in manifest['frozenLegacyFiles']:
                    errors.append(f'New legacy gameplay owner declaration: {name}; bind to the original actor instead')
    # No exclusion may silently convert the production full build into a slice.
    project=(root/'runtime/Crisis.Runtime.csproj').read_text()
    if 'Original/**/*.cs' not in project or re.search(r'\b(?:Exclude|Remove)\s*=',project):
        errors.append('Full original build must include all Original sources without exclusions')
    return errors


if __name__=='__main__':
    errors=check()
    for error in errors:print('BLOCKER:',error)
    print(f'Actor migration boundary: {len(errors)} violations; full runtime acceptance remains separate')
    raise SystemExit(bool(errors))
