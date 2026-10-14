#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$1"
OUT_ROOT="$2"
if [ -z "$PROJECT_ROOT" ] || [ -z "$OUT_ROOT" ]; then
  echo "usage: wasm_target_spike.sh PROJECT_ROOT OUT_ROOT" >&2
  exit 2
fi
ORIGINAL_TARGET="$PROJECT_ROOT/Assets/Scripts/Assembly-CSharp/Target.cs"
ORIGINAL_BALLISTIC_PROPERTIES="$PROJECT_ROOT/Assets/Scripts/Assembly-CSharp/BallisticMaterialProperties.cs"
test -f "$ORIGINAL_TARGET"
test -f "$ORIGINAL_BALLISTIC_PROPERTIES"
rm -rf "$OUT_ROOT"
mkdir -p "$OUT_ROOT"
dotnet new blazorwasm --framework net8.0 --no-restore --output "$OUT_ROOT/app"
cp "$ORIGINAL_TARGET" "$OUT_ROOT/app/OriginalTarget.cs"
cp "$ORIGINAL_BALLISTIC_PROPERTIES" "$OUT_ROOT/app/OriginalBallisticMaterialProperties.cs"
cat > "$OUT_ROOT/app/UnityShim.cs" <<'CS'
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string text) { }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class TooltipAttribute : Attribute
    {
        public TooltipAttribute(string text) { }
    }

    public struct Vector2
    {
        public float x;
        public float y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }

    // Minimal API surface for this feasibility test only. This is not a full UnityEngine replacement.
    public class MonoBehaviour
    {
        private readonly Dictionary<Type, object> _components = new Dictionary<Type, object>();

        public void AttachComponent<T>(T component) where T : class
        {
            _components[typeof(T)] = component;
        }

        protected T GetComponent<T>() where T : class
        {
            return _components.TryGetValue(typeof(T), out object component) ? (T)component : null;
        }
    }
}

public sealed class CharacterBody { }
public sealed class BallisticHitEffect { }
public sealed class BallisticHitMeshEffect { }
CS
cat > "$OUT_ROOT/app/Pages/Home.razor" <<'RAZOR'
@page "/"
@using System.Reflection

<h1>Crisis Response — C# to WebAssembly feasibility gate</h1>
<p id="result">@_result</p>
<p id="scope">This test compiles two original decompiled scripts: Target.cs and BallisticMaterialProperties.cs. CharacterBody, effect classes, and the UnityEngine API surface are deliberately minimal test shims; this is not a playable game or a parity claim.</p>

@code {
    private string _result = "RUNNING";

    protected override void OnInitialized()
    {
        try
        {
            var target = new global::Target();
            var body = new global::CharacterBody();
            target.AttachComponent(body);
            var awake = typeof(global::Target).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            if (awake == null) throw new InvalidOperationException("Original Target.Awake method was not found.");
            awake.Invoke(target, null);
            if (!target.active) throw new InvalidOperationException("Original Target.active default changed.");
            if (!ReferenceEquals(target.Body, body)) throw new InvalidOperationException("Original Target.Awake/GetComponent behavior failed.");
            var properties = new global::BallisticMaterialProperties();
            if (properties.surfaceDensity != 1000f || properties.density != 1000f || properties.densityRandomness.x != 1f || properties.densityRandomness.y != 1f)
                throw new InvalidOperationException("Original BallisticMaterialProperties defaults changed.");
            _result = "PASS — original Target.cs and BallisticMaterialProperties.cs compiled under the browser WebAssembly runtime; Target.Awake/GetComponent and material-property defaults passed.";
        }
        catch (Exception ex)
        {
            _result = "FAIL — " + ex.ToString();
        }
    }
}
RAZOR
dotnet publish "$OUT_ROOT/app" --configuration Release --output "$OUT_ROOT/published" -p:PublishTrimmed=false
cat > "$OUT_ROOT/wasm-spike-report.md" <<EOF
# C# WebAssembly compatibility spike

- Original sources included: Assets/Scripts/Assembly-CSharp/Target.cs and Assets/Scripts/Assembly-CSharp/BallisticMaterialProperties.cs
- Build target: Blazor WebAssembly on .NET 8
- Unity API surface implemented: MonoBehaviour, TooltipAttribute, HeaderAttribute, Vector2, GetComponent<T>() and a test-only component attachment hook.
- CharacterBody and ballistic effect types are test fixtures, not the game's original classes.
- Scope: compile original Target and BallisticMaterialProperties components; invoke the original Target.Awake method and check original material-property defaults in a browser WASM runtime.
- This does not prove the rest of the Unity API, original scene import, physics, rendering parity, or a playable port.
EOF
echo "Published WASM feasibility app at $OUT_ROOT/published/wwwroot"
