#!/usr/bin/env python3
"""Extract original M4 mechanics, reload tasks, curves and presentation assets.
Engine-side effect calls are explicit browser transport boundaries, not replacement gameplay.
"""
import sys,json,re,gzip,hashlib,shutil,struct
from pathlib import Path
from extract_runtime_scene import parse_asset
from extract_runtime_sources import block,member
root=Path(sys.argv[1]);out=Path(sys.argv[2]);original=Path(sys.argv[3]);original.mkdir(parents=True,exist_ok=True)
source=root/'Assets/Scripts/Assembly-CSharp';hashes={};adapt=[]
def read(n):
 p=source/n
 if not p.exists():p=next(source.rglob(n))
 s=p.read_text();hashes[n]=hashlib.sha256(p.read_bytes()).hexdigest();return s
def write(n,s): (original/n).write_text('using UnityEngine;\nusing System;\nusing System.Collections.Generic;\nusing Object=UnityEngine.Object;\nusing Random=UnityEngine.Random;\nnamespace Crisis.Runtime.Weapons;\n'+s)
def clean(s):
 s=re.sub(r'^using .*?;\n','',s,flags=re.M);s=re.sub(r'^\s*\[(?:Tooltip|Header|Range|TaskCategory|TaskIcon)\(.*?\)\]\n','\n',s,flags=re.M)
 return s
for n in ['Magazine.cs','Recoil.cs','WeaponBarrel.cs','Equipment.cs']:
 write(n,clean(read(n)).replace('Calibre','string'))
# Full original bolt class; ejection is an explicit rendering event. Casings do not have fake physics.
s=clean(read('CyclingBolt.cs')).replace('using UnityEngine.Events;','')
eject=block(s,'public void EjectCasing(float forceScale = 1f, int index = -1)')
s=s.replace(eject,'''public void EjectCasing(float forceScale = 1f, int index = -1) {
 isCarrying=false;
 if(Round!=null){weapon.Effects.Emit("eject",new{spent=Round.Spent,forceScale});Object.Destroy(Round.gameObject);Round=null;}
}''').replace('public class CyclingBolt : MonoBehaviour','public class CyclingBolt : MonoBehaviour')
s='using UnityEngine.Events;\n'+s
write('CyclingBolt.cs',s)
adapt.append('CyclingBolt.EjectCasing emits an explicit effect event; casing physics is not yet supported.')
# Original transition methods. Analytic solve remains shared with original presentation host.
s=read('ArmIKSolver.cs');methods=[block(s,x) for x in ['private void UpdateTransition()','public void SetTargetImmediate(','public void TargetTransition(Transform target, float seconds','public void TargetTransition(Transform target, Vector3 worldSpaceOffset','public void StartWait()','public void SetHandAngle(','public float GetHandAngle(','public void SetOnWeapon(','public void SetUnarmed()']]
methods=[m.replace('Debug.DrawLine(TransitionTarget.position, TransitionTarget.position + worldSpaceOffset, Color.yellow);','') for m in methods]
write('ArmTransition.cs','''public class ArmIKSolver:Component {
 public enum HandRotationMode{Local,Global,TargetLocal}
 public AnimationCurve interpolation;public Limb upperArm;public Hand hand;public Transform ikTarget,transitionTarget;
 public bool smoothing;private bool transition;private float transitionTimestamp,transitionPeriod,transitionHandAngle;
 private Hand.Mode transitionHandMode;private HandRotationMode transitionHandRotationMode;
 public bool OnTarget{get;private set;} public bool OnWeapon{get;private set;}
 public Transform TransitionTarget=>transitionTarget;
 public void Step(){if(transition)UpdateTransition();}
'''+ '\n'.join(methods)+'\n}')
for name in ['ChangeMagazine.cs','WorkBolt.cs']:
 # Exported sources keep these at the root, unlike decompiler's display tree.
 s=clean(read(name));s=s.replace('namespace CharacterBehavior;','').replace('class '+name[:-3]+' : Action','class '+name[:-3]+' : WeaponTask')
 if 'namespace CharacterBehavior' in s:s=s[s.index('{')+1:s.rfind('}')]
 s=s.replace('Calibre','string')
 write(name,s)
# selected source weapon methods retain state machine and arithmetic.
s=read('WeaponBase.cs');signatures=['private void UpdateTriggerMechanism()','private void ReleaseFiringPin()','public void StrikeFiringPin()','private void DetonateRound()','public void ResetBurstCount()','public void OnBoltOpened()','public void OnBoltClosed(bool feedRound)','public Casing GetNextRoundFromMagazine(bool disableMalfunctions)','private void CalculateRecoil()','public float RecoilImpulse()','private void AddSingleRecoil()','private float MagazineWeight()','public bool InsertMagazine(Magazine magazine)','public void RemoveMagazine(bool drop = false, bool audio = true)','private void SpawnProjectiles()']
methods=[]
for sig in signatures:
 m=block(s,sig)
 if sig=='private void DetonateRound()':
  for call in ['InvokeBulletTime();','EmitMuzzleSmoke();','EmitMuzzleFlash();','ApplyMuzzleBlastForces();']:
   m=m.replace(call,'Effects.Emit("'+call.split('(')[0]+'",null);')
  m=m.replace('Global.Context.Mixer.PlayActionMusic(5f);','Effects.Emit("actionMusic",5f);')
 if sig=='private void AddSingleRecoil()':
  m=m.replace('RigidBody.AddForce(base.transform.right * num * -2f, ForceMode.Impulse);','throw new NotSupportedException("Ragdoll firing is not ported");')
 methods.append(m.replace('AudioInstigator instigator','object instigator'))
write('WeaponMechanism.Methods.cs','public partial class WeaponBase {\n'+'\n'.join(methods)+'\n}')
adapt.append('WeaponBase detonation side effects are transported to browser; music, smoke, muzzle blast forces and bullet time are recorded but not simulated. Trigger, bolt, recoil and ammo state methods are original.')
bs=read('CharacterBody.cs')
write('CharacterTrigger.Methods.cs','public partial class CharacterBody {\n'+'\n'.join(block(bs,x) for x in ['public bool QueryTrigger()','public void PullTrigger()','public bool MoveMagazineToHand(bool rightHand = true)'])+'\n}')
ps=read('Projectile.cs')
flight=block(ps,'private void Update()').replace('private void Update()','public void StepFlight()').replace('TestExplosion(num);','/* Non-explosive source FMJ scope; asserted at configuration load. */').replace('Object.Destroy(base.gameObject);','Finished=true;Object.Destroy(base.gameObject);')
# Query geometry currently has no material/limb components. Keep the original Error outcome and disclose this boundary.
extra='\n'.join(block(ps,x) for x in ['private void InitializeVelocity(float velocityModifier)','public bool TestTermination()','public void AssignNewSpin()','public void UpdateRotation()'])
(original/'ProjectileFlight.Methods.cs').write_text('using UnityEngine;\nusing Object=UnityEngine.Object;\nusing Random=UnityEngine.Random;\nusing Crisis.Runtime.Weapons;\npublic partial class Projectile {\n'+flight+'\n'+extra+'\n}')
# Data lookup with exact source IDs / refs, no inferred defaults.
index={}
for p in (root/'Assets').rglob('*.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(errors='ignore'),re.M)
 if m:index[m[1]]=p.with_suffix('').relative_to(root)
def records(p):return parse_asset(root/p,str(p))['records']
def component(rs,n):
 guid=re.search(r'^guid: (\w+)',(source/(n+'.cs.meta')).read_text(),re.M)[1]
 return next(r['fields'] for r in rs if r['classID']==114 and r['fields'].get('m_Script',{}).get('guid')==guid)
wpath=Path('Assets/Resources/weapons/M4A1.prefab');rs=records(wpath);w=component(rs,'WeaponBase');bolt=component(rs,'CyclingBolt');barrel=component(rs,'WeaponBarrel')
fmode=re.search(r'fireModes: (\w+)',(root/wpath).read_text())[1];fireModes=list(struct.unpack('<'+'i'*(len(fmode)//8),bytes.fromhex(fmode)))
slots={r['fileID']:r['fields'] for r in rs};comp=list(rs)
for ref in w['componentSlots']:
 slot=slots[str(ref['fileID'])];c=slot.get('component',{});c=c if c.get('guid') else slot.get('defaultComponent',{});g=c.get('guid')
 if g in index:comp.extend(records(index[g]))
mag=next(r['fields'] for r in comp if 'capacity' in r['fields'] and 'malfunctionRate' in r['fields'])
ammoPath=index[w['defaultAmmunition']['guid']];ars=records(ammoPath);ammo=component(ars,'AmmunitionBase')
op=records(Path('Assets/Resources/characters/Operator.prefab'));body=component(op,'CharacterBody')
arm=next(r['fields'] for r in op if 'interpolation' in r['fields'] and 'upperArm' in r['fields'])
# Resolve original hand/chest/weapon transforms for engine snapshots.
def transforms(rs):return {r['fileID']:r['fields'] for r in rs if r['classID'] in [4,224]}
audio={};assets=out.parent/'weapon-runtime';assets.mkdir(parents=True,exist_ok=True)
for key in ['audioFire','audioFiringPin','audioBoltExtract','audioBoltReturn','audioExtractMagazine','audioInsertMagazine','audioSupersonic']:
 a=w[key];g=a['sample'].get('guid')
 if g in index:
  p=root/index[g];dest=assets/(key+p.suffix);shutil.copy2(p,dest);audio[key]={'url':'./assets/weapon-runtime/'+dest.name,'volume':a['sampleVolume'],'source':str(index[g])}
# Preserve source flash animation sprites and texture rather than drawing an invented flash.
flashPath=index[w['muzzleFlash']['guid']];frs=records(flashPath)
flashAnim=records(Path('Assets/AnimationClip/Muzzle flash.anim'))
flash={'prefab':frs,'animation':flashAnim,'sprites':{},'textures':{}}
for r in flashAnim:
 for curve in r['fields'].get('m_PPtrCurves',[]):
  for key in curve.get('curve',[]):
   ref=key['value'];g=ref.get('guid')
   if g in index:
    sp=records(index[g]);flash['sprites'][g]=sp
    for item in sp:
     tex=item['fields'].get('m_RD',{}).get('texture',{}).get('guid')
     if tex in index and tex not in flash['textures']:
      p=root/index[tex];dest=assets/('flash'+p.suffix);shutil.copy2(p,dest);flash['textures'][tex]={'url':'./assets/weapon-runtime/'+dest.name}
byid={r['fileID']:r['fields'] for r in op}
ots=transforms(op)
def bt(key):
 ref=str(body[key]['fileID'])
 if ref in ots:return ref
 go=str(byid[ref]['m_GameObject']['fileID'])
 return next(i for i,f in ots.items() if str(f['m_GameObject']['fileID'])==go)
armconf=component(records(index[w['armConfiguration']['guid']]),'WeaponArmConfiguration')
prs=records(index[ammo['projectile']['guid']]);projectile=component(prs,'Projectile')
payload={'projectile':projectile,'bodyTransforms':{key:bt(key) for key in ['chestRigIKTarget','pistolPivot','pistolIKTarget','Torso','Pelvis']},'armConfiguration':armconf,'weapon':w,'fireModes':fireModes,'bolt':bolt,'barrel':barrel,'magazine':mag,'ammunition':ammo,'ammoPath':str(ammoPath),'ammoPackage':{'assets':{str(ammoPath):{'records':[{k:v for k,v in r.items() if k!='rawYaml'} for r in ars]}},'guidPaths':{ammo['m_Script']['guid']:'Assets/Scripts/Assembly-CSharp/AmmunitionBase.cs'}},'weaponTransforms':transforms(rs),'operatorTransforms':transforms(op),'armInterpolation':arm['interpolation'],'body':body,'audio':audio,'flash':flash,'sourceHashes':hashes,'adaptations':adapt,'scope':'M4 source mechanics, original reload/bolt tasks and arm transition math. Static scene projectile flight; damage/material interactions, casing physics and complete effects remain pending.'}
# rawYaml is evidence, not required at runtime.
def strip(v):
 if isinstance(v,dict):return {k:strip(x) for k,x in v.items() if k!='rawYaml'}
 if isinstance(v,list):return [strip(x) for x in v]
 return v
out.write_bytes(gzip.compress(json.dumps(strip(payload),separators=(',',':')).encode(),mtime=0));print({'bytes':out.stat().st_size,'fireModes':fireModes,'magCapacity':mag['capacity'],'audio':list(audio)})
