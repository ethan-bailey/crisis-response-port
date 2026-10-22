"""Deterministic source dependency inventory; lexical over-approximation, not C# binding.

Optional --archive indexes every exported C# source in the SHA-pinned recovery
without importing anything. Always records every checked-in source declaration,
all known-type edges, duplicate identities, and every compiler missing name.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import zipfile

ROOT=Path(__file__).resolve().parents[1]
PIN='cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d'
DECL=re.compile(r'\b(?:class|struct|interface|enum|record(?:\s+struct|\s+class)?)\s+([A-Za-z_]\w*)')
NAMESPACE=re.compile(r'\bnamespace\s+([A-Za-z_]\w*(?:\.[A-Za-z_]\w*)*)')
TOKEN=re.compile(r'\b[A-Za-z_]\w*\b')
# Remove comments and literals so prose/source filenames do not become edges.
NOISE=re.compile(r'//[^\n]*|/\*.*?\*/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'',re.S)
ENGINE={'ParticleSystem','LightShadows','ReflectionProbe','Rendering'}
ASSETS={'InteriorModeUI','MissionUI','UIHitAnnouncements','UIInteriorModeTextIndicator','UIVocalizationText','UIGrenadeTimer','DialogTree','LimbTextureData','DynamicLimbTexturePool','DynamicLimbDecalRenderer'}


def generate(archive=None):
    texts={p.relative_to(ROOT).as_posix():p.read_text(encoding='utf-8-sig') for parent in ('runtime/Engine','runtime/Original','source-reference') for p in sorted((ROOT/parent).rglob('*.cs'))}
    declarations={}
    clean={p:NOISE.sub(' ',s) for p,s in texts.items()}
    for path,source in clean.items():
        for name in DECL.findall(source):declarations.setdefault(name,set()).add(path)
    archived={}
    archive_sources=[]
    index_path=ROOT/'docs/audit/recovery-source-index.json'
    if archive:
        with archive.open('rb') as stream:
            if hashlib.file_digest(stream,'sha256').hexdigest()!=PIN:raise ValueError('Recovery archive SHA256 mismatch')
        raw_sources=[]
        with zipfile.ZipFile(archive) as z:
            for entry in sorted(z.namelist()):
                if not entry.endswith('.cs') or '/Assets/Scripts/' not in entry:continue
                data=z.read(entry);source=NOISE.sub(' ',data.decode('utf-8-sig'))
                own=sorted(set(DECL.findall(source)))
                namespaces=sorted(set(NAMESPACE.findall(source)))
                raw_sources.append((entry,data,source,own,namespaces))
                aliases=[name[:-9] for name in own if name.endswith('Attribute')]
                for name in own+namespaces+aliases:archived.setdefault(name,set()).add(entry)
        known=set(archived)|set(declarations)|ENGINE|ASSETS
        for entry,data,source,own,namespaces in raw_sources:
            archive_sources.append({'path':'archive:'+entry,'sha256':hashlib.sha256(data).hexdigest(),'declares':own,'namespaces':namespaces,'references':sorted((set(TOKEN.findall(source))&known)-set(own))})
        index_path.write_text(json.dumps({'archiveSha256':PIN,'sources':archive_sources},separators=(',',':'))+'\n')
    elif index_path.exists():
        index=json.loads(index_path.read_text())
        if index['archiveSha256']!=PIN:raise ValueError('Unexpected source index archive pin')
        archive_sources=index['sources']
        for item in archive_sources:
            aliases=[name[:-9] for name in item['declares'] if name.endswith('Attribute')]
            for name in item['declares']+item['namespaces']+aliases:archived.setdefault(name,set()).add(item['path'][8:])
    baseline=json.loads((ROOT/'docs/audit/compiler-baseline.json').read_text())
    names=set(declarations)|set(archived)|set(baseline['missingSymbols'])
    sources=[]
    for path,source in clean.items():
        own=set(DECL.findall(source));refs=sorted((set(TOKEN.findall(source))&names)-own)
        sources.append({'path':path,'sha256':hashlib.sha256((ROOT/path).read_bytes()).hexdigest(),'declares':sorted(own),'references':refs})
    local_sources=sources[:]
    sources.extend(archive_sources)
    symbols=[]
    for name in sorted(names):
        paths=sorted(declarations.get(name,[]));runtime=[p for p in paths if p.startswith('runtime/Original/')];engine=[p for p in paths if p.startswith('runtime/Engine/')];refs=[p for p in paths if p.startswith('source-reference/')]
        category='recovered source' if runtime or refs else 'recoverable source' if name in archived else 'native Unity adapter' if name in ENGINE or engine else 'asset-backed service' if name in ASSETS else 'currently unavailable'
        symbols.append({'name':name,'category':category,'requiresAuthoredAssets':name in ASSETS,'runtimeSources':runtime,'engineSources':engine,'referenceSources':refs,'archiveEntries':sorted(archived.get(name,[])),'compilerMissing':name in baseline['missingSymbols'],'referencedBy':[s['path'] for s in local_sources if name in s['references']]})
    # File-to-file closure includes all candidate declarations for ambiguous names.
    roots=['runtime/Original/CharacterBody.Full.cs','runtime/Original/FullSource/Limb.cs','runtime/Original/Weapons/Equipment.cs','runtime/Original/FullSource/Mission.cs']
    roots.extend(s['path'] for s in archive_sources if s['path'].rsplit('/',1)[-1] in {'CharacterBody.cs','Limb.cs','Equipment.cs','Mission.cs'})
    bypath={s['path']:s for s in sources};seen=set();pending=list(roots)
    while pending:
        path=pending.pop()
        if path in seen or path not in bypath:continue
        seen.add(path)
        for symbol in bypath[path]['references']:
            pending.extend(declarations.get(symbol,[]))
            pending.extend('archive:'+entry for entry in archived.get(symbol,[]))
    return {'schemaVersion':1,'scope':'All checked-in Engine, Original and source-reference C# files. Known-name lexical edges are an over-approximation, NOT semantic type resolution; nested names and conditional branches may collide. Missing names come from the exact pinned compiler checkpoint. Pinned recovery index covers every exported C# declaration and known-type edge. Regenerate with --archive when changing the index. Lexical ambiguity requires review; unindexed dependencies remain explicitly unavailable, never inferred absent from original game.','baselineSha':baseline['sourceSha'],'archiveSha256':PIN if archive_sources else None,'roots':roots,'reachableFiles':sorted(seen),'sources':local_sources,'archivedSourceCount':len(archive_sources),'symbols':symbols,'ambiguousNames':{k:sorted(v) for k,v in sorted(declarations.items()) if len(v)>1}}


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--archive',type=Path);parser.add_argument('--check',action='store_true');parser.add_argument('--output',type=Path,default=ROOT/'docs/audit/source-dependencies.json');args=parser.parse_args()
    report=generate(args.archive);text=json.dumps(report,indent=2)+'\n'
    if args.check:
        if args.output.read_text()!=text:raise SystemExit('Dependency inventory stale: regenerate and review before importing sources')
    else:args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(text)
    print(f"Inventory: {len(report['sources'])} sources, {len(report['symbols'])} symbols, {len(report['reachableFiles'])} reachable files; {len(report['ambiguousNames'])} ambiguous names require ownership review")


if __name__=='__main__':main()
