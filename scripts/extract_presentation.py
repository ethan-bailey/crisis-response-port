#!/usr/bin/env python3
"""Preserve original camera/pose settings and selected C# methods, with hashes."""
import json,re,sys,hashlib
from pathlib import Path
from extract_runtime_scene import parse_asset
from extract_runtime_sources import block
root=Path(sys.argv[1]);out=Path(sys.argv[2]);original=Path(sys.argv[3]);original.mkdir(parents=True,exist_ok=True)
sources={}
def records(path):
 a=parse_asset(root/path,path);sources[path]=a['sha256'];return a['records']
def source(name):
 p=root/'Assets/Scripts/Assembly-CSharp'/name;s=p.read_text();sources[str(p.relative_to(root))]=hashlib.sha256(p.read_bytes()).hexdigest();return s
def fields(rs,condition):return next(r['fields'] for r in rs if condition(r))
def script(rs,name):
 meta=(root/'Assets/Scripts/Assembly-CSharp'/f'{name}.cs.meta').read_text();guid=re.search(r'^guid: (\w+)',meta,re.M)[1]
 return fields(rs,lambda r:r['classID']==114 and r['fields']['m_Script'].get('guid')==guid)
def vec(x):return [x[k] for k in 'xyz']
interior=records('Assets/GameObject/Interior mode.prefab');camera=fields(interior,lambda r:r['classID']==20);camgo=str(camera['m_GameObject']['fileID']);ct=fields(interior,lambda r:r['classID']==4 and str(r['fields']['m_GameObject']['fileID'])==camgo)
operator=records('Assets/Resources/characters/Operator.prefab');byid={r['fileID']:r for r in operator};body=script(operator,'CharacterBody')
# Existing manifests use stable source fileIDs: link actual IK and hand references.
arms=[]
for r in operator:
 if r['classID']==114 and 'upperArm' in r['fields'] and 'smoothFactor' in r['fields']:
  f=r['fields'];bones={}
  for key in ['upperArm','lowerArm','hand']:
   component=byid[str(f[key]['fileID'])]['fields'];bones[key]=str(component['m_GameObject']['fileID'])
  hand=byid[str(f['hand']['fileID'])]['fields'];modes={k:str(byid[str(hand[k]['fileID'])]['fileID']) for k in ['fist','trigger','forwardAssist']}
  arms.append({'bones':bones,'settings':f,'handModes':modes})
weapon=records('Assets/Resources/weapons/M4A1.prefab');base=script(weapon,'WeaponBase')
# Recover actual component definitions used by CalculateStaticWeight.
index={}
for meta in (root/'Assets').rglob('*.meta'):
 m=re.search(r'^guid: (\w+)',meta.read_text(errors='ignore'),re.M)
 if m:index[m[1]]=str(meta.with_suffix('').relative_to(root))
configRef=base['armConfiguration']; config=script(records(index[configRef['guid']]),'WeaponArmConfiguration')
slotRecords={r['fileID']:r for r in weapon};components=[]
for ref in base['componentSlots']:
 slot=slotRecords[str(ref['fileID'])]['fields']; c=slot.get('component',{});guid=c.get('guid')
 if guid and guid in index:
  rs=records(index[guid]); component=next((r['fields'] for r in rs if r['fileID']==str(c['fileID'])),None)
  if component:components.append(component)
scenePath='Assets/Scenes/Mission 01.unity';scene=records(scenePath)
# Keep exact hierarchy and box fields for original camera physics queries.
timeRecords=records('ProjectSettings/TimeManager.asset')
queryRecords=[{k:v for k,v in r.items() if k!='rawYaml'} for r in scene if r['classID'] in [1,4,224,65]]
layers=records('ProjectSettings/TagManager.asset')[0]['fields']['layers'];mask=sum(1<<layers.index(x) for x in ['Doors','Obstacle geometry'])
doorGuid=re.search(r'^guid: (\w+)',(root/'Assets/Scripts/Assembly-CSharp/DoorController.cs.meta').read_text(),re.M)[1]
doors=[str(r['fields']['m_GameObject']['fileID']) for r in scene if r['classID']==114 and r['fields'].get('m_Script',{}).get('guid')==doorGuid]
ik=source('InverseKinematics2D.cs');(original/'InverseKinematics2D.cs').write_text(ik)
cam=source('InteriorCamera.cs');methods=[block(cam,sig) for sig in ['private void Update()','private void UpdateTargetPosition()','private void SetInitialPosition()','public void SetManualTargetPosition(float worldX)','public float SubjectViewDistance()','private void UpdateListenerPosition()']]
source('ArmIKSolver.cs');source('PlayerController.cs');bodySource=source('CharacterBody.cs');aimerSource=source('WeaponAimer.cs');weaponSource=source('WeaponBase.cs');graphics=source('SerializableGraphicsSettings.cs');source('WeaponPointer.cs')

poseMethod=block(bodySource,'private void UpdateUpperBodyTransforms(WeaponBase weapon)').replace('WeaponBase','WeaponPoseSnapshot').replace('WeaponArmConfiguration','ArmConfigurationSnapshot')
poseMethod=poseMethod.replace('hitRecoveryCurve.Evaluate(Time.time - LastHitTimestamp) * Mathf.Lerp(0f, 10f, LastHitEnergy / 1500f) * LastDirectionAttackedFrom','0f /* no-hit scope: original multiplier LastHitEnergy is zero */')
aimMethod=block(aimerSource,'private void UpdateHeadState()').replace('WeaponBase','WeaponPoseSnapshot')
(original/'BodyPoseSlice.Methods.cs').write_text('using UnityEngine;\nnamespace Crisis.Runtime.Presentation;\npublic sealed partial class BodyPoseSlice {\n'+poseMethod+'\n}\npublic sealed partial class WeaponAimerSlice {\n'+aimMethod+'\n}\n')
(original/'WeaponBrace.cs').write_text(source('WeaponBrace.cs'))

camMethods='\n'.join(methods).replace('DoorController','CameraDoorSnapshot')
(original/'InteriorCameraSlice.cs').write_text('''using System;
using UnityEngine;
namespace Crisis.Runtime.Presentation;
// Original method bodies; gameplay objects are read-only views of actual renderer/PhysX state.
public sealed class InteriorCameraSlice:MonoBehaviour {
 public Camera interiorCamera;public Transform audioListener; public CameraSubjectSnapshot Subject;
 public float smoothing,fovSmoothing,centerOffset,obstacleHoldOff,doorHoldOff,fieldOfViewOverride;
 public bool overrideFieldOfView,disableScrollPastDoors,useDynamicFov;
 public float cameraDistance,staticFov,dynamicFovMin,dynamicFovMax;
 float targetFOV,subjectToObstacle=4.5f; Vector3 targetPosition;RaycastHit hit;public int obstacleMask;
 public float FrustumWidth{get;private set;}
 private float StaticFieldOfView=>(!overrideFieldOfView)?staticFov:fieldOfViewOverride;
 private float MaxFieldOfView=>(!useDynamicFov)?StaticFieldOfView:dynamicFovMax;
 public void Initialize(){targetFOV=interiorCamera.fieldOfView;cameraDistance=-1.5f-transform.position.z;SetInitialPosition();}
 public void Step()=>Update();
 public object Snapshot()=>new{position=new[]{transform.position.x,transform.position.y,transform.position.z},fov=interiorCamera.fieldOfView,FrustumWidth};
'''+camMethods+'\n}\n')
# Original geometry is a 2-by-2 plane scaled 25-by-5: 50-by-10, not 25-by-5.
quad=records('Assets/GameObject/Aim quad.prefab');qt=fields(quad,lambda r:r['classID']==4)
mesh=records('Assets/Mesh/Plane.asset')[0]['fields'];bounds=mesh['m_LocalAABB']['m_Extent']
payload={'sources':sources,'camera':camera,'cameraTransform':ct,'cameraSettings':script(interior,'InteriorCamera'),
 'graphics':{'useDynamicFov':False,'staticFov':float(re.search(r'staticFieldOfView = ([\d.]+)f',graphics)[1]),'dynamicMin':float(re.search(r'dynamicFieldOfViewMin = ([\d.]+)f',graphics)[1]),'dynamicMax':float(re.search(r'dynamicFieldOfViewMax = ([\d.]+)f',graphics)[1])},
 'queryPackage':{'assets':{scenePath:{'records':queryRecords},'ProjectSettings/TimeManager.asset':{'records':[{k:v for k,v in r.items() if k!='rawYaml'} for r in timeRecords]}},'guidPaths':{}},'queryScene':scenePath,'cameraObstacleMask':mask,'doors':doors,
 'cameraBallisticMask':sum(1<<layers.index(x) for x in ['Doors','Obstacle geometry','Wall geometry','Exterior geometry','Physical props']),'objectActive':{r['fileID']:bool(r['fields']['m_IsActive']) for r in operator if r['classID']==1},'componentGameObjects':{r['fileID']:str(r['fields']['m_GameObject']['fileID']) for r in operator if 'm_GameObject' in r['fields']},'arms':arms,'body':body,'weapon':base,'armConfiguration':config,'weaponComponents':components,
 'aimQuadSize':[bounds[k]*qt['m_LocalScale'][k]*2 for k in ['x','y']],
 'poseMethodOriginalHash':hashlib.sha256(block(bodySource,'private void UpdateUpperBodyTransforms(WeaponBase weapon)').encode()).hexdigest(),'poseAdaptations':['Weapon/limb/physiology types are transport views of executable uninjured/unfired state','No-hit recovery term constant-folded to zero because original LastHitEnergy is zero; hit reactions excluded','Camera DoorController type mapped to serialized Door snapshot; moving/interactive doors excluded'],'methodHashes':{m[:m.index('{')].strip():hashlib.sha256(m.encode()).hexdigest() for m in methods},
 'scope':'Original camera follow/FOV/box queries and analytic IK; post effects, dynamic door simulation, equipment sequences, injury/respiration poses are not executable.'}
out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(payload,separators=(',',':')));import gzip;out.with_suffix('.json.gz').write_bytes(gzip.compress(out.read_bytes(),mtime=0));print({'bytes':out.stat().st_size,'arms':len(arms),'camera':ct['m_LocalPosition'],'components':len(components)})
