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
ORIGINAL_PROJECTILE="$PROJECT_ROOT/Assets/Scripts/Assembly-CSharp/Projectile.cs"
test -f "$ORIGINAL_TARGET"
test -f "$ORIGINAL_BALLISTIC_PROPERTIES"
test -f "$ORIGINAL_BALLISTIC_MATERIAL"
test -f "$ORIGINAL_PROJECTILE"
# Print recovered penetration methods to CI logs so the source, not a guessed stub, drives the next compatibility pass.
grep -n -A18 -B4 -E "PotentialPenetrationDistance|EnergyToPenetrate\(|ConvertEnergyToFriction|ConvertRemainingEnergyToFriction" "$ORIGINAL_PROJECTILE" || true
rm -rf "$OUT_ROOT"
mkdir -p "$OUT_ROOT"
dotnet new blazorwasm --framework net8.0 --no-restore --output "$OUT_ROOT/app"
cp "$ORIGINAL_TARGET" "$OUT_ROOT/app/OriginalTarget.cs"
cp "$ORIGINAL_BALLISTIC_PROPERTIES" "$OUT_ROOT/app/OriginalBallisticMaterialProperties.cs"
cp "$ORIGINAL_BALLISTIC_MATERIAL" "$OUT_ROOT/app/OriginalBallisticMaterial.cs"
cp "$ORIGINAL_PROJECTILE" "$OUT_ROOT/app/OriginalProjectile.cs"
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
        public static Vector3 right => new Vector3(1,0,0);
        public static Vector3 up => new Vector3(0,1,0);
        public static Vector3 forward => new Vector3(0,0,1);
        public float magnitude => MathF.Sqrt(x*x+y*y+z*z);
        public Vector3 normalized => magnitude > 1e-20f ? this / magnitude : zero;
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x-b.x,a.y-b.y,a.z-b.z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x,-a.y,-a.z);
        public static Vector3 operator *(Vector3 a, float b) => new Vector3(a.x*b,a.y*b,a.z*b);
        public static Vector3 operator *(float b, Vector3 a) => a*b;
        public static Vector3 operator /(Vector3 a, float b) => new Vector3(a.x/b,a.y/b,a.z/b);
        public static float Dot(Vector3 a, Vector3 b) => a.x*b.x+a.y*b.y+a.z*b.z;
        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.y*b.z-a.z*b.y,a.z*b.x-a.x*b.z,a.x*b.y-a.y*b.x);
        public static float Angle(Vector3 a, Vector3 b)
        {
            float denom = a.magnitude * b.magnitude;
            if (denom < 1e-15f) return 0f;
            float cosine = Math.Clamp(Dot(a,b)/denom,-1f,1f);
            return MathF.Acos(cosine) * (180f / MathF.PI);
        }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a+(b-a)*Math.Clamp(t,0f,1f);
        public static Vector3 Reflect(Vector3 v, Vector3 n) => v-2f*Dot(v,n)*n;
        public static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x*b.x,a.y*b.y,a.z*b.z);
    }
    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x,float y,float z,float w) { this.x=x; this.y=y; this.z=z; this.w=w; }
        public static Quaternion identity => new Quaternion(0,0,0,1);
        public static Quaternion AngleAxis(float degrees, Vector3 axis)
        {
            Vector3 n = axis.normalized;
            if (n.magnitude < 1e-20f) return identity;
            float half = degrees * (MathF.PI / 180f) * 0.5f;
            float sin = MathF.Sin(half);
            return new Quaternion(n.x*sin,n.y*sin,n.z*sin,MathF.Cos(half));
        }
        public static Quaternion Inverse(Quaternion q) => new Quaternion(-q.x,-q.y,-q.z,q.w);
        public static Quaternion operator *(Quaternion a, Quaternion b) => new Quaternion(
            a.w*b.x+a.x*b.w+a.y*b.z-a.z*b.y,
            a.w*b.y-a.x*b.z+a.y*b.w+a.z*b.x,
            a.w*b.z+a.x*b.y-a.y*b.x+a.z*b.w,
            a.w*b.w-a.x*b.x-a.y*b.y-a.z*b.z);
        public static Vector3 operator *(Quaternion q, Vector3 v)
        {
            Vector3 u = new Vector3(q.x,q.y,q.z);
            Vector3 uv = Vector3.Cross(u,v);
            Vector3 uuv = Vector3.Cross(u,uv);
            return v + (uv*q.w + uuv)*2f;
        }
        public static Quaternion LookRotation(Vector3 forward)
        {
            Vector3 f = forward.normalized;
            if (f.magnitude < 1e-20f) return identity;
            Vector3 r = Vector3.Cross(Vector3.up,f).normalized;
            if (r.magnitude < 1e-20f) r = Vector3.Cross(Vector3.right,f).normalized;
            Vector3 u = Vector3.Cross(f,r);
            float m00=r.x, m01=u.x, m02=f.x;
            float m10=r.y, m11=u.y, m12=f.y;
            float m20=r.z, m21=u.z, m22=f.z;
            float trace=m00+m11+m22;
            if (trace > 0f)
            {
                float k=MathF.Sqrt(trace+1f)*2f;
                return new Quaternion((m21-m12)/k,(m02-m20)/k,(m10-m01)/k,0.25f*k);
            }
            if (m00 > m11 && m00 > m22)
            {
                float k=MathF.Sqrt(1f+m00-m11-m22)*2f;
                return new Quaternion(0.25f*k,(m01+m10)/k,(m02+m20)/k,(m21-m12)/k);
            }
            if (m11 > m22)
            {
                float k=MathF.Sqrt(1f+m11-m00-m22)*2f;
                return new Quaternion((m01+m10)/k,0.25f*k,(m12+m21)/k,(m02-m20)/k);
            }
            else
            {
                float k=MathF.Sqrt(1f+m22-m00-m11)*2f;
                return new Quaternion((m02+m20)/k,(m12+m21)/k,0.25f*k,(m10-m01)/k);
            }
        }
    }
    public class Transform
    {
        public Vector3 position { get; set; }
        public Vector3 localScale { get; set; } = Vector3.one;
        public Vector3 localEulerAngles { get; set; }
        public Transform parent { get; private set; }
        public int childCount => 0;
        public Quaternion rotation { get; set; } = Quaternion.identity;
        public void SetParent(Transform value, bool worldPositionStays = true) { parent = value; }
        public Transform GetChild(int index) => new Transform();
        public T GetComponent<T>() where T : class => null;
        public Vector3 forward { get => rotation * Vector3.forward; set => rotation = Quaternion.LookRotation(value); }
        public Vector3 TransformPoint(Vector3 p) => position + rotation*p;
        public Vector3 InverseTransformPoint(Vector3 p) => Quaternion.Inverse(rotation)*(p-position);
        public Vector3 InverseTransformDirection(Vector3 p) => Quaternion.Inverse(rotation)*p;
        public void Rotate(Vector3 axis, float angle) { rotation = rotation * Quaternion.AngleAxis(angle, axis); }
    }
    public class Collider { public Rigidbody attachedRigidbody; public T GetComponent<T>() where T : class => null; }
    public class Rigidbody { public void AddForceAtPosition(Vector3 force, Vector3 position) { } }
    public class Ray { public Vector3 origin, direction; public Ray(Vector3 o, Vector3 d) { origin=o; direction=d; } }
    public struct RaycastHit { public Transform transform; public Collider collider; public Vector3 point, normal; public float distance; }
    public static class Physics { public static bool Raycast(Ray ray, out RaycastHit hit, float distance, int mask) { hit = default; return false; } }
    public class AnimationCurve { public float Evaluate(float value) => 1f; }
    public class Material { }
    public class MeshRenderer { public Material material { get; set; } }
    public class MeshFilter { public Transform transform { get; } = new Transform(); public T GetComponent<T>() where T : class => null; }
    public class GameObject { public void SetActive(bool active) { } public T GetComponent<T>() where T : class => null; }
    public static class Time { public static float time => 0f; public static float deltaTime => 1f/60f; public static int frameCount => 0; }
    public static class Random { public static float Range(float min, float max) => min; public static int Range(int min, int max) => min; public static float value => 0.5f; public static Vector3 onUnitSphere => Vector3.forward; }
    public static class Mathf { public static float Pow(float f, float p) => MathF.Pow(f,p); public static float Min(float a,float b) => MathF.Min(a,b); public static float Max(float a,float b) => MathF.Max(a,b); public static float Abs(float x) => MathF.Abs(x); public static float Sqrt(float x) => MathF.Sqrt(x); public static float Tan(float x) => MathF.Tan(x); public static float Atan(float x) => MathF.Atan(x); public static float Clamp01(float x) => Math.Clamp(x,0f,1f); public static float Lerp(float a,float b,float t) => a+(b-a)*Math.Clamp(t,0f,1f); }
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

namespace UnityEngine.Events
{
    public delegate void UnityAction<T1, T2>(T1 arg1, T2 arg2);
}

public sealed class CharacterBody { public AudioInstigator AudioInstigator; public Limb LastHitLimb; public void OnSuccessfulAttack(object victim, int swarmId) { } public void OnWasHit(Projectile p) { } public void OnWasAttacked(Projectile p) { } }
public sealed class Masks { public static Masks Instance { get; } = new Masks(); public int Limbs => 1; public int BallisticTargets => 2; public int BallisticTargetsSansLimbs => 3; }
public sealed class WeaponBase { public CharacterBody Owner; public AmmunitionBase ammunition = new AmmunitionBase(); public bool rifledBarrel; public float rifleTwistRate; }
public sealed class Marksman { public void OnSuccessfulAttack(object victim) { } }
public sealed class AudioInstigator { }
public sealed class Limb { public CharacterBody Body = new CharacterBody(); public Projectile HandleProjectile(Projectile projectile, UnityEngine.Vector3 normal) => projectile; }
public sealed class ProjectileLimbEvent { public void Invoke(Projectile p, Limb limb) { } }
public sealed class ProjectileTrace { public void Initialize(Projectile p) { } public void AddPosition(UnityEngine.Vector3 p) { } public void SpawnHitEffect(UnityEngine.Vector3 position, UnityEngine.Transform container, UnityEngine.Vector3 normal) { } }
public sealed class Explosion : UnityEngine.MonoBehaviour { }
public sealed class BallisticHitEffect { public float offset; public void InitializeTransform(UnityEngine.Transform parent, UnityEngine.Vector3 position, UnityEngine.Vector3 direction) { } public void PlayEntry(Projectile projectile) { } public void PlayExit(Projectile projectile, UnityEngine.Vector3 normal, object instigator) { } }
public sealed class BallisticHitMeshEffect { }
public sealed class ProjectileEvent { public void Invoke(Projectile projectile) { } }
public enum BallisticResult { Continue, Stop, Deflected, Error, Delayed }
public sealed class DelayedBallisticExit { public float activationTime; public Projectile projectile; public UnityEngine.Vector3 localResumePosition; }
public sealed class Trace { public static implicit operator bool(Trace value) => value != null; public void SpawnHitEffect(UnityEngine.Vector3 position, UnityEngine.Transform container, UnityEngine.Vector3 normal) { } }
public sealed class AmmunitionBase
{
 public float mass = 10f, diameter = 9f, length = 100f, velocity = 800f, velocityModifier, frictionFactor = 0.1f, elasticity, bounceThreshold = 0.1f, flightDrag = 0.001f, ballisticCoefficient = 0.2f;
 public float MetricLength => length * 0.001f;
 public float MuzzleEnergy => 0.5f * mass * 0.001f * velocity * velocity;
 public UnityEngine.AnimationCurve expansionFunction = new UnityEngine.AnimationCurve();
 public float DiameterAtEnergy(float frictionEnergy, float originalDiameterInMillimeters) => originalDiameterInMillimeters * expansionFunction.Evaluate(frictionEnergy / MuzzleEnergy);
 public float TemporalCavityFactor(float velocity) => 1f;
 public float PermanentCavityFactor(float velocity) => 1f;
 public float FragmentationEnergy(float mass) => float.PositiveInfinity;
 public enum ExplosionMode { None, OnFragmentation, Timed }
 public ExplosionMode explosionMode;
 public float explosionTime;
 public Explosion explosionPrefab;
 public enum FragmentationMode { None, Randomized, Specified }
 public sealed class FragmentationSpecification { public float mass, diameter, deflection; }
 public float fragmentMainBodyMass = 0.5f, fragmentMassMin = 1f, fragmentMassMax = 2f, fragmentDiameterMin = 3f, fragmentDiameterMax = 5f, fragmentDeflectionMin, fragmentDeflectionMax = 10f;
 public bool fragmentEvenPattern;
 public FragmentationMode fragmentationMode;
 public System.Collections.Generic.List<FragmentationSpecification> specificFragmentation = new System.Collections.Generic.List<FragmentationSpecification>();
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

<h1>Crisis Response — original ballistics code in WebAssembly</h1>
<p id="result">@_result</p>
<p id="scope">This browser app compiles and executes recovered original Target.cs, BallisticMaterialProperties.cs, BallisticMaterial.cs, and Projectile.cs. The collision control invokes the original BallisticMaterial.HandleProjectile method and original Projectile energy / penetration methods. Unity APIs and dependent game types remain minimal test fixtures; this is a gameplay-system slice, not a complete game.</p>
<section class="card p-3 mt-3" aria-labelledby="shot-heading">
  <h2 id="shot-heading">Original projectile / material interaction</h2>
  <label for="energy">Projectile energy: @_energy</label>
  <input id="energy" type="range" min="1" max="500" step="1" @bind="_energy" />
  <label for="thickness">Surface thickness: @_thickness.ToString("F2")</label>
  <input id="thickness" type="range" min="0.01" max="0.50" step="0.01" @bind="_thickness" />
  <label class="form-check mt-2"><input id="impenetrable" class="form-check-input" type="checkbox" @bind="_impenetrable" /> Impenetrable material (forces the original method to stop the projectile and zero its remaining energy)</label>
  <p class="text-muted">Start with this off to test penetration. Surface thickness is adjustable; the test slab depth is fixed at 0.10 m. Projectile energy and penetration calculations use original Projectile.cs; Unity physics and effects remain simplified fixtures.</p>
  <button id="runShot" class="btn btn-primary mt-3" @onclick="RunShot">Run original collision method</button>
  <pre id="shotResult" class="mt-3">@_shotResult</pre>
</section>

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
            var angle = UnityEngine.Vector3.Angle(UnityEngine.Vector3.forward, UnityEngine.Vector3.right);
            if (Math.Abs(angle - 90f) > 0.01f) throw new InvalidOperationException("Unity Vector3.Angle shim failed: " + angle);
            var quarterTurn = UnityEngine.Quaternion.AngleAxis(90f, UnityEngine.Vector3.up);
            var rotatedForward = quarterTurn * UnityEngine.Vector3.forward;
            if (Math.Abs(rotatedForward.x - 1f) > 0.001f || Math.Abs(rotatedForward.z) > 0.001f)
                throw new InvalidOperationException("Unity Quaternion rotation shim failed: " + rotatedForward.x + "," + rotatedForward.y + "," + rotatedForward.z);
            var transformFixture = new UnityEngine.Transform { position = new UnityEngine.Vector3(1f, 2f, 3f), rotation = quarterTurn };
            var localPoint = new UnityEngine.Vector3(2f, -1f, 4f);
            var roundTrip = transformFixture.InverseTransformPoint(transformFixture.TransformPoint(localPoint));
            if (Math.Abs(roundTrip.x-localPoint.x) > 0.001f || Math.Abs(roundTrip.y-localPoint.y) > 0.001f || Math.Abs(roundTrip.z-localPoint.z) > 0.001f)
                throw new InvalidOperationException("Unity Transform local/world round trip failed.");
            var volume = global::BallisticMaterial.ChannelVolume(2f, 4f, 6f);
            if (Math.Abs(volume - 157.07963f) > 0.001f) throw new InvalidOperationException("Original BallisticMaterial.ChannelVolume result changed: " + volume);
            properties.surfaceThickness = 0.1f;
            var material = new global::BallisticMaterial { properties = properties, impenetrable = true };
            material.Awake();
            var ammo = new global::AmmunitionBase { mass = 10f, diameter = 9f, length = 100f, velocity = 800f, frictionFactor = 0.1f };
            var projectile = new global::Projectile { meshTransform = new UnityEngine.Transform(), Status = global::BallisticResult.Continue };
            projectile.InitializeBenchmark(ammo);
            var surfaceEnergy = material.EnergyToPenetrateSurface(projectile);
            if (!(surfaceEnergy > 0f && surfaceEnergy < projectile.Energy)) throw new InvalidOperationException("Original BallisticMaterial.EnergyToPenetrateSurface returned invalid cost: " + surfaceEnergy + " of " + projectile.Energy);
            var handled = material.HandleProjectile(projectile, new UnityEngine.Vector3(0f, 0f, 1f));
            if (handled.Status != global::BallisticResult.Stop || handled.Energy != 0f)
                throw new InvalidOperationException("Original BallisticMaterial impenetrable entry transition changed: status=" + handled.Status + ", energy=" + handled.Energy);
            _result = "PASS — original Target.cs, BallisticMaterialProperties.cs, BallisticMaterial.cs, and Projectile.cs compiled under browser WebAssembly; lifecycle fixture, material defaults, Unity math shims, ChannelVolume, surface cost, and impenetrable-entry state transition passed.";
            RunShot();
        }
        catch (Exception ex)
        {
            _result = "FAIL — " + ex.ToString();
        }
    }

    private float _energy = 500f;
    private float _thickness = 0.01f;
    private bool _impenetrable = false;
    private string _shotResult = "Ready. Adjust the parameters and run the original collision method.";

    private void RunShot()
    {
        try
        {
            var properties = new global::BallisticMaterialProperties { surfaceThickness = _thickness };
            var material = new global::BallisticMaterial { properties = properties, impenetrable = _impenetrable, width = 0.1f };
            material.Awake();
            var massGrams = 10f;
            var desiredVelocity = MathF.Sqrt(2f * _energy / (massGrams * 0.001f));
            var ammo = new global::AmmunitionBase { mass = massGrams, diameter = 9f, length = 100f, velocity = desiredVelocity, frictionFactor = 0.1f, bounceThreshold = 0.1f };
            var projectile = new global::Projectile { meshTransform = new UnityEngine.Transform(), Status = global::BallisticResult.Continue };
            projectile.InitializeBenchmark(ammo);
            var before = projectile.Energy;
            var result = material.HandleProjectile(projectile, UnityEngine.Vector3.forward);
            _shotResult = $"Original HandleProjectile returned {result.Status}. Energy before: {before:F2}; energy after: {result.Energy:F2}. Surface thickness: {_thickness:F2}; slab depth: 0.10 m; impenetrable: {_impenetrable}. Unity physics and effects are simplified fixtures.";
        }
        catch (Exception ex)
        {
            _shotResult = "This parameter combination reached an unimplemented Unity/game dependency in the test fixtures: " + ex.Message;
        }
    }
}
RAZOR
dotnet publish "$OUT_ROOT/app" --configuration Release --output "$OUT_ROOT/published" -p:PublishTrimmed=false
cat > "$OUT_ROOT/wasm-spike-report.md" <<EOF
# C# WebAssembly compatibility spike

- Original sources included: Target.cs, BallisticMaterialProperties.cs, BallisticMaterial.cs, and Projectile.cs from Assets/Scripts/Assembly-CSharp
- Build target: Blazor WebAssembly on .NET 8
- Unity API surface implemented: MonoBehaviour, inspector attributes, Vector2, Vector3/Quaternion/Transform math, GetComponent<T>() and a test-only component attachment hook.
- CharacterBody and ballistic effect types are test fixtures, not the game's original classes.
- Scope: compile four original scripts, including Projectile.cs, with fixtures only for Unity and other dependencies; invoke Target.Awake, check material-property defaults, execute original BallisticMaterial.ChannelVolume and EnergyToPenetrateSurface, exercise the original impenetrable-entry state transition, and expose a browser control that invokes original BallisticMaterial.HandleProjectile with adjustable energy, surface thickness, and impenetrability. Projectile energy, friction, penetration-distance, and diameter calculations come from original Projectile.cs. Remaining Unity APIs, ammunition data, vectors, transforms, random, effects, and timing are test fixtures.
- This does not prove the rest of the Unity API, original scene import, physics, rendering parity, or a playable port.
EOF
echo "Published WASM feasibility app at $OUT_ROOT/published/wwwroot"
