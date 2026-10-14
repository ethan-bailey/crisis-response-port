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
ORIGINAL_BALLISTIC_MATERIAL="$PROJECT_ROOT/Assets/Scripts/Assembly-CSharp/BallisticMaterial.cs"
test -f "$ORIGINAL_TARGET"
test -f "$ORIGINAL_BALLISTIC_PROPERTIES"
test -f "$ORIGINAL_BALLISTIC_MATERIAL"
rm -rf "$OUT_ROOT"
mkdir -p "$OUT_ROOT"
dotnet new blazorwasm --framework net8.0 --no-restore --output "$OUT_ROOT/app"
cp "$ORIGINAL_TARGET" "$OUT_ROOT/app/OriginalTarget.cs"
cp "$ORIGINAL_BALLISTIC_PROPERTIES" "$OUT_ROOT/app/OriginalBallisticMaterialProperties.cs"
cp "$ORIGINAL_BALLISTIC_MATERIAL" "$OUT_ROOT/app/OriginalBallisticMaterial.cs"
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
    public sealed class RangeAttribute : Attribute
    {
        public RangeAttribute(float min, float max) { }
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

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponentAttribute : Attribute
    {
        public RequireComponentAttribute(Type type) { }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x=x; this.y=y; this.z=z; }
        public static Vector3 zero => new Vector3(0,0,0);
        public static Vector3 one => new Vector3(1,1,1);
        public float magnitude => MathF.Sqrt(x*x+y*y+z*z);
        public Vector3 normalized => magnitude > 0 ? this / magnitude : zero;
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x-b.x,a.y-b.y,a.z-b.z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x,-a.y,-a.z);
        public static Vector3 operator *(Vector3 a, float b) => new Vector3(a.x*b,a.y*b,a.z*b);
        public static Vector3 operator *(float b, Vector3 a) => a*b;
        public static Vector3 operator /(Vector3 a, float b) => new Vector3(a.x/b,a.y/b,a.z/b);
        public static float Dot(Vector3 a, Vector3 b) => a.x*b.x+a.y*b.y+a.z*b.z;
        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.y*b.z-a.z*b.y,a.z*b.x-a.x*b.z,a.x*b.y-a.y*b.x);
        public static float Angle(Vector3 a, Vector3 b) => 0f;
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a+(b-a)*t;
        public static Vector3 Reflect(Vector3 v, Vector3 n) => v-2f*Dot(v,n)*n;
        public static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x*b.x,a.y*b.y,a.z*b.z);
    }

    public struct Quaternion { public static Quaternion LookRotation(Vector3 forward) => new Quaternion(); }
    public class Transform
    {
        public Vector3 position { get; set; }
        public Vector3 forward { get; set; } = new Vector3(0,0,1);
        public Quaternion rotation { get; set; }
        public Vector3 TransformPoint(Vector3 p) => position+p;
        public Vector3 InverseTransformPoint(Vector3 p) => p-position;
        public Vector3 InverseTransformDirection(Vector3 p) => p;
        public void Rotate(Vector3 axis, float angle) { }
    }
    public class Collider { }
    public class GameObject { public void SetActive(bool active) { } }
    public static class Time { public static float time => 0f; public static float deltaTime => 1f/60f; public static int frameCount => 0; }
    public static class Random { public static float Range(float min, float max) => min; public static int Range(int min, int max) => min; }
    public static class Mathf { public static float Pow(float f, float p) => MathF.Pow(f,p); public static float Min(float a,float b) => MathF.Min(a,b); public static float Abs(float x) => MathF.Abs(x); }
    public static class Debug { public static void LogWarningFormat(string format, params object[] args) { } }
    public class Object { public static T Instantiate<T>(T original) where T : class => original; public static void Destroy(object obj) { } }

    // Minimal API surface for this feasibility test only. This is not a full UnityEngine replacement.
    public class MonoBehaviour
    {
        private readonly Dictionary<Type, object> _components = new Dictionary<Type, object>();
        public Transform transform { get; } = new Transform();
        public GameObject gameObject { get; } = new GameObject();
        public string name { get; set; } = "WASM test object";

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
public sealed class BallisticHitEffect { public float offset; public void InitializeTransform(UnityEngine.Transform parent, UnityEngine.Vector3 position, UnityEngine.Vector3 direction) { } public void PlayEntry(Projectile projectile) { } public void PlayExit(Projectile projectile, UnityEngine.Vector3 normal, object instigator) { } }
public sealed class BallisticHitMeshEffect { }
public sealed class ProjectileEvent { public void Invoke(Projectile projectile) { } }
public enum BallisticResult { Continue, Stop, Deflected, Error, Delayed }
public sealed class DelayedBallisticExit { public float activationTime; public Projectile projectile; public UnityEngine.Vector3 localResumePosition; }
public sealed class Trace { public static implicit operator bool(Trace value) => value != null; public void SpawnHitEffect(UnityEngine.Vector3 position, UnityEngine.Transform container, UnityEngine.Vector3 normal) { } }
public sealed class AmmunitionBase
{
 public enum FragmentationMode { None, Randomized, Specified }
 public sealed class FragmentationSpecification { public float mass, diameter, deflection; }
 public float MetricLength, velocity, elasticity, bounceThreshold, fragmentMainBodyMass, fragmentMassMin, fragmentMassMax, fragmentDiameterMin, fragmentDiameterMax, fragmentDeflectionMin, fragmentDeflectionMax;
 public bool fragmentEvenPattern;
 public FragmentationMode fragmentationMode;
 public System.Collections.Generic.List<FragmentationSpecification> specificFragmentation = new System.Collections.Generic.List<FragmentationSpecification>();
}
public sealed class Projectile : UnityEngine.MonoBehaviour
{
 public bool FiredByMarksman, HasFragmented;
 public BallisticResult Status;
 public AmmunitionBase Ammunition = new AmmunitionBase();
 public float Energy, EnergyNormalized, Diameter, DisplacedMaterialVolume, PenetrationDistance, PermanentCavityFactor, Mass, Velocity;
 public UnityEngine.Vector3 StartPosition, NextPosition;
 public Trace Trace;
 public object AudioInstigator;
 public void BeginNewInteraction() { }
 public float EnergyToPenetrate(float distance, float density) => distance*density;
 public void ConvertEnergyToFriction(float energy) { Energy = MathF.Max(0,Energy-energy); }
 public void ConvertRemainingEnergyToFriction() { Energy=0; }
 public void AssignNewSpinAndTumble() { }
 public float PotentialPenetrationDistance(float density) => 0;
 public void Deflect(float amount) { }
 public bool TestFragmentation() => false;
 public void RestartAtPosition(UnityEngine.Vector3 position) { transform.position=position; }
 public void InitializeProjectileFragment(Projectile parent,int frame,UnityEngine.Vector3 position,float mass,float diameter,float velocity,float deflection,float spin,float distance) { }
 public void SpawnRandomFragments(UnityEngine.Vector3 position,System.Action<Projectile,bool> onExit) { }
 public void SpawnSpecificFragments(UnityEngine.Vector3 position,System.Action<Projectile,bool> onExit) { }
 public void OnFragmentation() { }
 public void ChangeToFragmentedMesh(bool value) { }
}
public sealed class PerformanceManager
{
 public static PerformanceManager Instance { get; } = new PerformanceManager();
 public bool EnvironmentHitEffectsEnabled => false;
 public BallisticHitEffect CreateEnvironmentalHitEffect(BallisticHitEffect effect) => null;
 public void SpawnEnvironmentalHitMesh(BallisticHitMeshEffect effect, Projectile projectile, UnityEngine.Transform transform) { }
}
public sealed class MissionHelper { public static MissionHelper Instance { get; } = new MissionHelper(); public UnityEngine.Transform EffectsContainer { get; } = new UnityEngine.Transform(); }
public static class MarksmanMode { public static float DeflectionModifier => 1f; }
CS
cat > "$OUT_ROOT/app/Pages/Home.razor" <<'RAZOR'
@page "/"
@using System.Reflection

<h1>Crisis Response — C# to WebAssembly feasibility gate</h1>
<p id="result">@_result</p>
<p id="scope">This test compiles three original decompiled scripts: Target.cs, BallisticMaterialProperties.cs, and BallisticMaterial.cs. Unity APIs and dependent game types are deliberately minimal compile/test shims; only selected original methods are invoked, so this is not a playable game or a parity claim.</p>

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
            var volume = global::BallisticMaterial.ChannelVolume(2f, 4f, 6f);
            if (Math.Abs(volume - 157.07963f) > 0.001f) throw new InvalidOperationException("Original BallisticMaterial.ChannelVolume result changed: " + volume);
            var material = new global::BallisticMaterial { SurfaceThickness = 0.2f, SurfaceDensity = 1000f };
            var projectile = new global::Projectile();
            projectile.Ammunition.MetricLength = 0.1f;
            var surfaceEnergy = material.EnergyToPenetrateSurface(projectile);
            if (Math.Abs(surfaceEnergy - 50f) > 0.001f) throw new InvalidOperationException("Original BallisticMaterial.EnergyToPenetrateSurface clamp changed: " + surfaceEnergy);
            _result = "PASS — original Target.cs, BallisticMaterialProperties.cs, and BallisticMaterial.cs compiled under browser WebAssembly; Target lifecycle fixture, material defaults, original ChannelVolume, and surface-thickness clamp passed.";
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

- Original sources included: Target.cs, BallisticMaterialProperties.cs, and BallisticMaterial.cs from Assets/Scripts/Assembly-CSharp
- Build target: Blazor WebAssembly on .NET 8
- Unity API surface implemented: MonoBehaviour, TooltipAttribute, HeaderAttribute, RangeAttribute, Vector2, Vector3/Transform test stubs, GetComponent<T>() and a test-only component attachment hook.
- CharacterBody and ballistic effect types are test fixtures, not the game's original classes.
- Scope: compile three original scripts; invoke Target.Awake, check material-property defaults, execute original BallisticMaterial.ChannelVolume, and test EnergyToPenetrateSurface thickness clamping. Projectile.EnergyToPenetrate remains a simplified fixture, not original physics.
- This does not prove the rest of the Unity API, original scene import, physics, rendering parity, or a playable port.
EOF
echo "Published WASM feasibility app at $OUT_ROOT/published/wwwroot"
