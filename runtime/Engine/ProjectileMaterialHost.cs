using UnityEngine;
using Crisis.Runtime.Weapons;
using Object=UnityEngine.Object;
public partial class Projectile {
 public BallisticResult Status {get;set;} public bool HasExploded {get;set;} public float FragmentationTime {get;set;}
 public Component MarksmanOwner {get;set;}public bool FiredByMarksman=>MarksmanOwner!=null;
 public WeaponBase Weapon {get;set;} public CharacterBody Owner {get;set;} public AudioInstigator AudioInstigator {get;set;}
 public ProjectileTrace Trace {get;set;} public ProjectileTrace tracePrefab;public ProjectileEvent onInteraction=new();
 public Vector3 Position=>transform.position;float segmentLength;
 void CommonInitialize(){
  SpawnTime=Time.time;freeFallTimestamp=Time.time;StartPosition=transform.position;
  FlightMask=Masks.Instance.BallisticTargets;InteractionPosition=transform.position;
  if(tracePrefab!=null){Trace=Object.Instantiate(tracePrefab);Trace.Initialize(this);}
  BallisticRuntimeHost.Current?.RegisterProjectile(this);
 }
 public void ChangeToFragmentedMesh(bool isMainBody=false)=>BallisticRuntimeHost.Current?.Emit("fragmentMeshRequest",new{id=Id,Mass,isMainBody});
 public void SetActiveSegment(float start,float length)=>throw new NotSupportedException("Precision limb segments are outside this material milestone");
 public void Explode(Vector3 position)=>throw new NotSupportedException("Explosive ammunition is not configured in this source M4 loadout");
}
