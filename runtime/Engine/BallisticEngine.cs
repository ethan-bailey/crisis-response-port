using System.Text.Json;
using UnityEngine;
[Serializable] public sealed class ProjectileEvent {event Action<Projectile> listeners;public void AddListener(Action<Projectile> action)=>listeners+=action;public void Invoke(Projectile p)=>listeners?.Invoke(p);}
public sealed class ProjectileTrace:Component {public void Initialize(Projectile p)=>throw new NotSupportedException("Source M4 trace prefab is null");public void AddPosition(Vector3 p)=>throw new NotSupportedException("Trace not configured");public void SpawnHitEffect(Vector3 p,Transform t,Vector3 n)=>throw new NotSupportedException("Trace not configured");}
public sealed class AudioInstigator:Component {}
public static class MarksmanMode {public static float DeflectionModifier=.15f;}
public sealed class Masks {public static Masks Instance=new();public int Limbs,BallisticTargets,BallisticTargetsSansLimbs;}
public sealed class MissionHelper {public static MissionHelper Instance=new();public Transform EffectsContainer;}

