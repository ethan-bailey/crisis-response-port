#!/usr/bin/env python3
"""Inventory UnityEngine API usage in recovered C# source for a browser runtime port."""
from __future__ import annotations
import collections
import re
import sys
from pathlib import Path

CATEGORIES = {
    "Object/lifecycle": r"\b(?:MonoBehaviour|GameObject|Component|Behaviour|Transform|Object|Instantiate|Destroy|DontDestroyOnLoad|GetComponent|GetComponents|SendMessage|FindObjectOfType)\b",
    "Physics/collision": r"\b(?:Rigidbody|Rigidbody2D|Collider|Collider2D|CharacterController|Physics|RaycastHit|RaycastHit2D|Collision|ContactPoint|Joint|PhysicMaterial|LayerMask)\b",
    "Rendering/materials": r"\b(?:Renderer|MeshRenderer|SkinnedMeshRenderer|MeshFilter|Mesh|Material|Shader|Texture|Texture2D|RenderTexture|Camera|Light|QualitySettings|Graphics|GL|LineRenderer|TrailRenderer)\b",
    "Animation": r"\b(?:Animator|AnimatorStateInfo|Animation|AnimationCurve|AnimationClip|HumanBodyBones|Avatar|SkinnedMeshRenderer)\b",
    "Audio": r"\b(?:AudioSource|AudioClip|AudioListener|AudioMixer|AudioMixerGroup|AudioSettings|Microphone)\b",
    "UI/events": r"\b(?:Canvas|CanvasGroup|RectTransform|Graphic|Image|RawImage|Text|Button|Toggle|Slider|ScrollRect|EventSystem|PointerEventData|UnityEvent|LayoutGroup|ContentSizeFitter)\b",
    "Input/time": r"\b(?:Input|KeyCode|Touch|TouchPhase|Time|WaitForSeconds|WaitForFixedUpdate|Coroutine|Cursor|Screen|Application)\b",
    "Scenes/assets": r"\b(?:SceneManager|Scene|Resources|AssetBundle|PlayerPrefs|JsonUtility|Serialization|UnityWebRequest|WWW)\b",
    "Particles/effects": r"\b(?:ParticleSystem|ParticleSystemRenderer|TrailRenderer|Projector|Decal)\b",
}
COMPILED = {name: re.compile(pattern) for name, pattern in CATEGORIES.items()}
TOKEN_RE = re.compile(r"\b(?:MonoBehaviour|GameObject|Component|Behaviour|Transform|Object|Rigidbody|Collider|CharacterController|Physics|RaycastHit|Renderer|MeshRenderer|SkinnedMeshRenderer|MeshFilter|Mesh|Material|Shader|Texture2D|RenderTexture|Camera|Light|Graphics|LineRenderer|Animator|Animation|AnimationCurve|AnimationClip|AudioSource|AudioClip|AudioListener|AudioMixer|Canvas|CanvasGroup|RectTransform|Graphic|Image|RawImage|Text|Button|Toggle|Slider|ScrollRect|EventSystem|UnityEvent|Input|KeyCode|Touch|TouchPhase|Time|WaitForSeconds|WaitForFixedUpdate|Coroutine|Cursor|Screen|Application|SceneManager|Scene|Resources|AssetBundle|PlayerPrefs|JsonUtility|UnityWebRequest|WWW|ParticleSystem|Instantiate|Destroy|DontDestroyOnLoad|GetComponent\w*|GetComponents\w*|SendMessage|FindObjectOfType)\b")

def main() -> int:
    if len(sys.argv) != 3:
        print("usage: unity_api_inventory.py PROJECT_ROOT OUTPUT.md", file=sys.stderr)
        return 2
    project = Path(sys.argv[1]).resolve()
    output = Path(sys.argv[2])
    root = project / "Assets"
    files = sorted(root.rglob("*.cs")) if root.exists() else []
    token_counts: collections.Counter[str] = collections.Counter()
    category_counts: collections.Counter[str] = collections.Counter()
    callsites: dict[str, list[tuple[str, int, str]]] = collections.defaultdict(list)
    for path in files:
        try:
            source = path.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        rel = str(path.relative_to(project))
        for line_no, line in enumerate(source.splitlines(), 1):
            code = line.split("//", 1)[0]
            for token in TOKEN_RE.findall(code):
                token_counts[token] += 1
                if len(callsites[token]) < 80:
                    callsites[token].append((rel, line_no, line.strip()[:180]))
            for category, regex in COMPILED.items():
                if regex.search(code):
                    category_counts[category] += 1
    lines = [
        "# Unity API source inventory for browser/WASM port",
        "",
        "Heuristic lexical inventory of C# source. Counts include repeated references and may include declarations; they are not compiler errors or proof that every API is used at runtime.",
        "",
        f"- C# files scanned: {len(files):,}",
        f"- Distinct recognized Unity API tokens: {len(token_counts):,}",
        f"- Recognized API-token occurrences: {sum(token_counts.values()):,}",
        "",
        "## API categories by source line",
        "",
        "| Category | Lines containing one or more recognized APIs |",
        "|---|---:|",
    ]
    for category, count in sorted(category_counts.items(), key=lambda item: (-item[1], item[0])):
        lines.append(f"| {category} | {count:,} |")
    lines += ["", "## Most frequent recognized API tokens", "", "| Token | Occurrences |", "|---|---:|"]
    for token, count in token_counts.most_common(80):
        lines.append(f"| {token} | {count:,} |")
    lines += ["", "## Representative call sites", ""]
    for token, count in token_counts.most_common(25):
        lines += [f"### {token} — {count:,} occurrences", ""]
        for rel, line_no, snippet in callsites[token][:8]:
            lines.append(f"- {rel}:{line_no}: {snippet}")
        lines.append("")
    lines += [
        "## Porting implications",
        "",
        "- Prioritize API contracts and lifecycle semantics before selecting a replacement runtime. Similar-looking APIs in a different engine do not guarantee matching update order, collision behavior, interpolation, or serialized references.",
        "- Treat the categories as a starting map. Review original call sites and the Marksman range dependency closure before implementing any compatibility layer.",
        "- Keep the original decompiled source as the reference; change only the engine adapter or specific platform-bound call sites where needed.",
        "",
    ]
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text("\n".join(lines), encoding="utf-8")
    print(f"Wrote {output}: {len(files)} C# files, {sum(token_counts.values())} recognized API occurrences")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
