"""Record actual compiler diagnostics and compare checkpoints without hiding build failure."""
import argparse
from collections import Counter
import json
import os
from pathlib import Path
import re

LOCATION = re.compile(r"(.+?\.cs)\((\d+),(\d+)\): error (CS\d+): (.+?)(?: \[.*)?$")
GLOBAL = re.compile(r"^(.+?)\s*:\s*error\s+([A-Z]+\d+)\s*:\s*(.+?)(?: \[.*)?$")
STAMP = re.compile(r"^\ufeff?\d{4}-\d\d-\d\dT\S+\s+")


def summarize(log, source_sha=None, build_exit_code=None):
    rows = set()
    for raw in log.splitlines():
        line = STAMP.sub('', raw).strip()
        match = LOCATION.match(line)
        if match:
            path, row, col, code, message = match.groups()
            path = path.replace('\\', '/')
            if '/runtime/' in path:
                path = 'runtime/' + path.split('/runtime/', 1)[1]
            rows.add((path, int(row), int(col), code, message))
        elif match := GLOBAL.match(line):
            path, code, message = match.groups()
            rows.add((path.replace('\\', '/'), 0, 0, code, message))
    return {
        'schemaVersion': 1,
        'sourceSha': source_sha,
        'buildExitCode': build_exit_code,
        'uniqueErrorCount': len(rows),
        'missingSymbols': sorted({m.group(1) for row in rows if (m := re.search(r"type or namespace name '([^']+)'", row[4]))}),
        'compilerDiagnostics': [dict(zip(('path', 'line', 'column', 'code', 'message'), row)) for row in sorted(rows)],
        'scope': 'Reported diagnostics only; unresolved declarations can hide further errors. Build exit code, not parsed error count, determines compilation success.'
    }


def compare(previous, current):
    # Ignore line movement, but retain counts when the same error occurs at
    # multiple call sites. One remaining occurrence must not hide a new one.
    def counts(report):
        return Counter((d['path'], d['code'], d['message']) for d in report['compilerDiagnostics'])
    before, after = counts(previous), counts(current)
    def items(delta):
        return [dict(path=p, code=c, message=m, occurrences=n) for (p,c,m),n in sorted(delta.items())]
    return {
        'baselineSha': previous.get('sourceSha'),
        'currentSha': current.get('sourceSha'),
        'resolved': items(before - after),
        'introduced': items(after - before),
        'remainingCount': sum((before & after).values()),
        'resolvedSymbols': sorted(set(previous['missingSymbols']) - set(current['missingSymbols'])),
        'introducedSymbols': sorted(set(current['missingSymbols']) - set(previous['missingSymbols']))
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('log', type=Path)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--baseline', type=Path)
    parser.add_argument('--source-sha', default=os.getenv('GITHUB_SHA'))
    parser.add_argument('--build-exit-code', type=int)
    args = parser.parse_args()
    report = summarize(args.log.read_text(encoding='utf-8-sig'), args.source_sha, args.build_exit_code)
    if args.baseline:
        report['comparison'] = compare(json.loads(args.baseline.read_text()), report)
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print(f"Recorded {report['uniqueErrorCount']} distinct errors; {len(report['missingSymbols'])} missing names; build exit={args.build_exit_code}")
    if 'comparison' in report:
        delta = report['comparison']
        for label in ('resolved', 'introduced'):
            print(f"{label.upper()}: {sum(d['occurrences'] for d in delta[label])}")
            for d in delta[label]:
                print(f"  {d['path']} {d['code']} x{d['occurrences']}: {d['message']}")
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
