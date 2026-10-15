using UnityEngine;
using Object=UnityEngine.Object;
public partial class PerformanceManager {
 public static PerformanceManager Instance;
 public int MaxEnvironmentHitEffects,MaxEnvironmentHitMeshes;
 public bool UnlimitedEnvironmentHitEffects=>MaxEnvironmentHitEffects<0;public bool UnlimitedEnvironmentHitMeshes=>MaxEnvironmentHitMeshes<0;
 public bool EnvironmentHitEffectsEnabled=>MaxEnvironmentHitEffects!=0;
 readonly LinkedList<GameObject> environmentHitEffects=new(),environmentHitMeshes=new();
}
// Native particle rendering is an explicit engine boundary. Source effect
// requests and audio are transported without substituting fake damage/particles.
public sealed class BallisticHitEffect:MonoBehaviour {
 public string SourceID;public float offset;public string[] audioClips=[];float spawnTime;public float Lifetime;
 public void InitializeTransform(Transform parent,Vector3 worldPosition,Vector3 worldDirection){transform.SetParent(parent);transform.position=worldPosition;transform.rotation=Quaternion.LookRotation(worldDirection);}
 public void PlayEntry(Projectile p){Play(p,"entry");}
 public void PlayExit(Projectile p,Vector3 parentVelocity,AudioInstigator instigator){Play(p,"exit");}
 void Play(Projectile p,string phase){spawnTime=Time.time;Lifetime=BallisticRuntimeHost.Current.particlesLifetime;
  // BallisticHitEffect.PlayAudio's exact sample choice/energy volume rule.
  string clip=audioClips.Length==0?null:audioClips[UnityEngine.Random.Range(0,audioClips.Length)];
  BallisticRuntimeHost.Current.Emit("materialEffect",new{source=SourceID,phase,id=p.Id,clip,volume=Mathf.Clamp01(p.Energy/300f),position=new[]{transform.position.x,transform.position.y,transform.position.z},direction=new[]{transform.forward.x,transform.forward.y,transform.forward.z},energy=p.Energy,p.Velocity});
 }
 public void Step(){if(Time.time-spawnTime>=Lifetime)Object.Destroy(gameObject);}
}
public partial class BallisticHitMeshEffect {
 // Play immediately resets localScale after parenting. Preserve only the world
 // pose here, avoiding an impossible shear-to-TRS decomposition of the old scale.
 // Source final scale and growth animation remain unchanged.
 void ReparentBeforeScaleReset(Transform parent){var p=transform.position;var q=transform.rotation;transform.SetParent(parent,false);transform.position=p;transform.rotation=q;}
 public string SourceID;public void StepPresentation(){Update();if(this){var p=transform.position;var q=transform.rotation;var s=transform.localScale;BallisticRuntimeHost.Current.Emit("impactMesh",new{instance=gameObject.GetInstanceID(),source=SourceID,position=new[]{p.x,p.y,p.z},rotation=new[]{q.x,q.y,q.z,q.w},scale=new[]{s.x,s.y,s.z}});}}
}
