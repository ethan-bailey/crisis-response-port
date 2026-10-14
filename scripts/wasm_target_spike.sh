#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$1"
OUT_ROOT="$2"
if [ -z "$PROJECT_ROOT" ] || [ -z "$OUT_ROOT" ]; then
  echo "usage: wasm_target_spike.sh PROJECT_ROOT OUT_ROOT" >&2
  exit 2
fi
ORIGINAL_TARGET="$PROJECT_ROOT/Assets/Scripts/Assembly-CSharp/Target.cs"
test -f "$ORIGINAL_TARGET"
rm -rf "$OUT_ROOT"
mkdir -p "$OUT_ROOT"
dotnet new blazorwasm --framework net8.0 --no-restore --output "$OUT_ROOT/app"
cp "$ORIGINAL_TARGET" "$OUT_ROOT/app/OriginalTarget.cs"
cat > "$OUT_ROOT/app/UnityShim.cs" <<'CS'
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class TooltipAttribute : Attribute
    {
        public TooltipAttribute(string text) { }
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
CS
cat > "$OUT_ROOT/app/Pages/Home.razor" <<'RAZOR'
@page "/"
@using System.Reflection

<h1>Crisis Response — C# to WebAssembly feasibility gate</h1>
<p id="result">@_result</p>
<p id="scope">This test compiles and invokes the original decompiled Target.cs script. CharacterBody and the UnityEngine API surface are deliberately minimal test shims; this is not a playable game or a parity claim.</p>

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
            _result = "PASS — original Target.cs compiled and ran under the browser WebAssembly runtime; Awake/GetComponent behavior matched this test fixture.";
        }
        catch (Exception ex)
        {
            _result = "FAIL — " + ex.ToString();
        }
    }
}
RAZOR
dotnet publish "$OUT_ROOT/app" --configuration Release --output "$OUT_ROOT/published"
cat > "$OUT_ROOT/wasm-spike-report.md" <<EOF
# C# WebAssembly compatibility spike

- Original source included: Assets/Scripts/Assembly-CSharp/Target.cs
- Build target: Blazor WebAssembly on .NET 8
- Unity API surface implemented: MonoBehaviour, TooltipAttribute, GetComponent<T>() and a test-only component attachment hook.
- CharacterBody is a test fixture, not the game's original class.
- Scope: compile the original Target component and invoke its original private Awake method in a browser WASM runtime.
- This does not prove the rest of the Unity API, original scene import, physics, rendering parity, or a playable port.
EOF
echo "Published WASM feasibility app at $OUT_ROOT/published/wwwroot"
