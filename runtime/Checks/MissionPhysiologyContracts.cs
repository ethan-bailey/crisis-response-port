using System.Reflection;
using System.Text.Json;
using Crisis.Runtime;
using Crisis.Runtime.FullSource;
using UnityEngine;

public static class MissionPhysiologyContracts
{
    static readonly BindingFlags InstanceFlags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    static readonly BindingFlags StaticFlags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;

    public static string Run(RuntimeWorld world,CharacterBody[] bodies)
    {
        var operators=bodies.Where(body=>body.GetComponent<TeamOperator>()!=null&&body.Physiology!=null).ToArray();
        var occupants=bodies.Where(body=>body.GetComponent<Occupant>()!=null&&body.Physiology!=null).ToArray();
        if(operators.Length==0||occupants.Length<2)
            throw new InvalidOperationException($"Physiology contract requires the original Operator and two Occupants; operators={operators.Length}, occupants={occupants.Length}");

        var allActors=operators.Concat(occupants).ToArray();
        foreach(var body in allActors)VerifyRecoveredActorConfiguration(body);
        var vitalTraumaResult=VerifyVitalTraumaDispatch(world,operators[0],occupants,allActors);

        var bleedingBody=occupants[0];
        var bleedingLimb=bleedingBody.limbs.Single(limb=>limb.id==LimbId.Torso);
        var bleeding=bleedingLimb.LimbPhysiology??throw new InvalidOperationException("Recovered Occupant torso has no LimbPhysiology");
        bleeding.ApplySettings(false,false,false,1f);
        if(bleeding.wounds.Count!=0)throw new InvalidOperationException("Physiology fixture torso unexpectedly starts with a wound");
        var maxCavity=bleeding.maxWoundCavityVolume;
        var tissueBefore=bleeding.DamagedTissueVolume;
        bleeding.AddTrauma(maxCavity+25f);
        if(MathF.Abs(bleeding.WoundCavityVolume-maxCavity)>.001f||
           MathF.Abs((bleeding.DamagedTissueVolume-tissueBefore)-maxCavity)>.001f)
            throw new InvalidOperationException($"Original LimbPhysiology.AddTrauma did not cap cavity and tissue at the serialized maximum: cavity={bleeding.WoundCavityVolume}/{maxCavity}, tissueDelta={bleeding.DamagedTissueVolume-tissueBefore}");

        var timeSetter=typeof(Time).GetProperty(nameof(Time.time),StaticFlags)!.GetSetMethod(true)!;
        var deltaSetter=typeof(Time).GetProperty(nameof(Time.deltaTime),StaticFlags)!.GetSetMethod(true)!;
        var oldTime=Time.time;
        var oldDelta=Time.deltaTime;
        void SetClock(float time,float delta){timeSetter.Invoke(null,new object[]{time});deltaSetter.Invoke(null,new object[]{delta});}
        object? bleedResult=null;
        object? hypoxiaResult=null;
        try
        {
            const float bleedDelta=.1f;
            SetClock(oldTime+.1f,bleedDelta);
            var pressure=bleeding.LocalPressure;
            if(!(pressure>0f))throw new InvalidOperationException($"Source physiology produced no positive torso pressure for the recovered hemorrhage check: {pressure}");
            var bloodBefore=bleedingBody.Physiology.BloodVolume;
            var expectedBleed=bleeding.WoundCavityVolume*pressure*bleedDelta;
            bleeding.UpdateHemorrhaging();
            var bloodLoss=bloodBefore-bleedingBody.Physiology.BloodVolume;
            if(MathF.Abs(bleeding.InternalHemorrhaging-expectedBleed)>.001f||
               MathF.Abs(bleeding.InternalBloodVolume-expectedBleed)>.001f||
               MathF.Abs(bloodLoss-expectedBleed)>.01f)
                throw new InvalidOperationException($"Original timed hemorrhage diverged from recovered cavity × local pressure × deltaTime: expected={expectedBleed}, internalRate={bleeding.InternalHemorrhaging}, internalBlood={bleeding.InternalBloodVolume}, systemBloodLoss={bloodLoss}");
            var rate=bleeding.InternalHemorrhaging;
                        bleedingBody.Physiology.StopHemorrhage();
            if(bleeding.WoundCavityVolume!=0f||bleeding.InternalBloodVolume!=0f||bleeding.Hemorrhaging!=0f)
                throw new InvalidOperationException($"Original StopHemorrhage did not clear internal/cavity hemorrhage: cavity={bleeding.WoundCavityVolume}, internalBlood={bleeding.InternalBloodVolume}, hemorrhaging={bleeding.Hemorrhaging}");
            bleedResult=new{actor=bleedingBody.name,limb=bleedingLimb.id.ToString(),serializedMaximumCavity=maxCavity,cappedCavity=maxCavity,cappedTissueDelta=bleeding.DamagedTissueVolume-tissueBefore,pressure,deltaTime=bleedDelta,bleedRate=rate,bloodLoss,sourceStopClearedCavityAndInternalBlood=true};

            var hypoxicBody=occupants[1];
            var physiology=hypoxicBody.Physiology??throw new InvalidOperationException("Second recovered Occupant has no Physiology");
            if(!physiology.Alive||!physiology.Conscious||hypoxicBody.IsRagdoll||physiology.invulnerable)
                throw new InvalidOperationException($"Hypoxia fixture did not start alive, conscious, unragdolled and vulnerable: alive={physiology.Alive}, conscious={physiology.Conscious}, ragdoll={hypoxicBody.IsRagdoll}, invulnerable={physiology.invulnerable}");
            var update=typeof(Physiology).GetMethod("Update",InstanceFlags)??throw new MissingMethodException("Original Physiology.Update");
            var events=new List<string>();
            hypoxicBody.EventRagdoll.AddListener(()=>events.Add("ragdoll"));
            hypoxicBody.EventUnconscious.AddListener(()=>events.Add("unconscious"));
            hypoxicBody.EventDeath.AddListener(()=>events.Add("death"));
            var lastOxygenTime=oldTime+.2f;
            SetClock(lastOxygenTime,.01f);
            update.Invoke(physiology,null); // Source SetBrainOxygen records the last oxygenated timestamp.
            if(!(physiology.BloodOxygen>0f))throw new InvalidOperationException("Recovered Occupant did not have oxygen before the source hypoxia fixture");
            var drainedVolume=physiology.BloodVolume;
            physiology.RemoveBlood(drainedVolume);
            if(physiology.BloodVolume>0f||physiology.BloodOxygen>0f)
                throw new InvalidOperationException($"Original Physiology.RemoveBlood did not deplete the selected actor: blood={physiology.BloodVolume}, oxygen={physiology.BloodOxygen}");
            SetClock(lastOxygenTime+.1f,.1f);
            update.Invoke(physiology,null);
            if(physiology.Conscious||!hypoxicBody.IsRagdoll||physiology.BloodOxygen>.5f||
               !events.SequenceEqual(new[]{"ragdoll","unconscious"}))
                throw new InvalidOperationException($"Source hypoxia did not emit the expected unconscious/ragdoll transition order: conscious={physiology.Conscious}, ragdoll={hypoxicBody.IsRagdoll}, oxygen={physiology.BloodOxygen}, events={string.Join(",",events)}");
            SetClock(lastOxygenTime+3.2f,.05f);
            update.Invoke(physiology,null);
            var brainTraumaAfterThreeSeconds=physiology.BrainTrauma;
            if(brainTraumaAfterThreeSeconds<=0f)
                throw new InvalidOperationException("Original prefab's three-second no-oxygen threshold did not add brain trauma");
            SetClock(lastOxygenTime+30.2f,.05f);
            update.Invoke(physiology,null);
            if(physiology.Alive||!events.SequenceEqual(new[]{"ragdoll","unconscious","death"}))
                throw new InvalidOperationException($"Original 30-second brain-oxygen death callback/order failed: alive={physiology.Alive}, events={string.Join(",",events)}");
            hypoxiaResult=new{actor=hypoxicBody.name,drainedBloodVolume=drainedVolume,unconsciousAtSeconds=.1f,brainTraumaAfterSeconds=3.2f,brainTraumaAfterThreeSeconds,deadAtSeconds=30.2f,eventOrder=events,sourceCharacterBodyRagdollAndDeathCallbacks=true,nativeJointPhysicsAdvanced=false};
        }
        finally
        {
            SetClock(oldTime,oldDelta);
        }

        return JsonSerializer.Serialize(new{contract="recovered-original-actor-physiology",actors=allActors.Length,operatorCount=operators.Length,occupantCount=occupants.Length,serializedStaticConfiguration="verified on loaded Mission actors",fitnessConfiguration="source prefab 0.85; runtime-adaptive",authoredVitalTraumaDispatch=vitalTraumaResult,cappedTraumaAndTimedHemorrhage=bleedResult,hypoxicStateTransitions=hypoxiaResult,visualWoundOutput="suppressed",nativeJointPhysicsAdvanced=false});
    }

    static object VerifyVitalTraumaDispatch(RuntimeWorld world,CharacterBody operatorBody,CharacterBody[] occupants,CharacterBody[] actors)
    {
        if(occupants.Length<4)
            throw new InvalidOperationException($"Vital-trauma contract requires four recovered Occupants plus the recovered Operator so each source type can be observed without cross-contaminating another type; found {occupants.Length}");

        var projectileObject=new GameObject("Vital-trauma source contract projectile",world);
        var projectile=projectileObject.AddComponent<Crisis.Runtime.FullSource.Projectile>();
        projectile.DisplacedMaterialVolume=20f;
        var tests=new[]{
            (Body:operatorBody,Limb:LimbId.Head,Name:"Brain",Type:LimbVitalFunction.Type.Brain,Observe:(Func<CharacterBody,float>)(body=>body.Physiology.BrainTrauma)),
            (Body:occupants[0],Limb:LimbId.Torso,Name:"Lungs",Type:LimbVitalFunction.Type.Lungs,Observe:(Func<CharacterBody,float>)(body=>body.Physiology.LungsEffectiveness)),
            (Body:occupants[1],Limb:LimbId.Torso,Name:"Heart",Type:LimbVitalFunction.Type.Heart,Observe:(Func<CharacterBody,float>)(body=>ReadFloat(body.Physiology,"heartCapacity"))),
            (Body:occupants[2],Limb:LimbId.Neck,Name:"Cervical spine",Type:LimbVitalFunction.Type.Spine,Observe:(Func<CharacterBody,float>)(body=>body.Physiology.Paralyzed?1f:0f)),
            (Body:occupants[3],Limb:LimbId.RightUpperLeg,Name:"Femur",Type:LimbVitalFunction.Type.Bone,Observe:(Func<CharacterBody,float>)(body=>body.CrippledRatio))
        };
        var results=new List<object>();
        try
        {
            foreach(var test in tests)
            {
                var limb=test.Body.limbs.Single(value=>value.id==test.Limb);
                var physiology=limb.LimbPhysiology??throw new InvalidOperationException($"Recovered {test.Body.name}/{test.Limb} has no source LimbPhysiology");
                var vitals=limb.GetComponent<VitalFunctions>()??throw new InvalidOperationException($"Recovered {test.Body.name}/{test.Limb} has no source VitalFunctions");
                var vital=vitals.vitalFunctions.SingleOrDefault(value=>value.name==test.Name&&value.type==test.Type)
                    ??throw new InvalidOperationException($"Recovered {test.Body.name}/{test.Limb} has no unique authored {test.Name}/{test.Type} vital");
                var curve=vital.factor??throw new InvalidOperationException($"Recovered {test.Body.name}/{test.Limb}/{test.Name} has no authored factor curve");
                var normalizedY=FindPositiveCurvePosition(curve);
                var factor=curve.Evaluate(normalizedY);
                var heightField=typeof(VitalFunctions).GetField("capsuleHeight",InstanceFlags)!;
                var offsetField=typeof(VitalFunctions).GetField("capsuleOffset",InstanceFlags)!;
                var capsuleHeight=(float)heightField.GetValue(vitals)!;
                var capsuleOffset=(float)offsetField.GetValue(vitals)!;
                if(!(capsuleHeight>0f)||!float.IsFinite(capsuleHeight)||!float.IsFinite(capsuleOffset))
                    throw new InvalidOperationException($"Recovered {test.Body.name}/{test.Limb} VitalFunctions.Start did not bind finite CapsuleCollider dimensions: height={capsuleHeight}, offset={capsuleOffset}");
                var localPoint=new Vector3(0f,normalizedY*capsuleHeight-capsuleOffset,0f);
                var hitPoint=vitals.transform.TransformPoint(localPoint);
                var originalProbabilities=vitals.vitalFunctions.Select(value=>(Value:value,Probability:value.probability)).ToArray();
                var before=test.Observe(test.Body);
                var offTargetBefore=actors.Where(body=>!ReferenceEquals(body,test.Body)).Select(Snapshot).ToArray();
                try
                {
                    foreach(var item in vitals.vitalFunctions)item.probability=ReferenceEquals(item,vital)?1f:-1f;
                    if(!(factor>0f))throw new InvalidOperationException($"Recovered {test.Body.name}/{test.Limb}/{test.Name} authored factor never evaluates above zero in the normalized capsule range");
                    physiology.VitalsTest(projectile,hitPoint,hitPoint,40f);
                }
                finally
                {
                    foreach(var item in originalProbabilities)item.Value.probability=item.Probability;
                }
                var after=test.Observe(test.Body);
                if(MathF.Abs(after-before)<.00001f)
                    throw new InvalidOperationException($"Original VitalFunctions.HitTest did not dispatch the recovered {test.Type} vital to {test.Body.name}/{test.Limb}: before={before}, after={after}, factor={factor}, y={normalizedY}");
                var offTargetAfter=actors.Where(body=>!ReferenceEquals(body,test.Body)).Select(Snapshot).ToArray();
                if(!offTargetBefore.SequenceEqual(offTargetAfter))
                    throw new InvalidOperationException($"Original {test.Type} hit changed physiology owned by a different recovered actor");
                results.Add(new{actor=test.Body.name,limb=test.Limb.ToString(),vital=test.Name,type=test.Type.ToString(),probabilityRestored=vital.probability,factor,normalizedY,sourceDisplacedVolume=projectile.DisplacedMaterialVolume,sourceShock=40f,ownerIsolation=true});
            }

            var head=operatorBody.limbs.Single(limb=>limb.id==LimbId.Head);
            var headVitals=head.GetComponent<VitalFunctions>()??throw new InvalidOperationException("Recovered Operator head has no source VitalFunctions for miss/zero-factor paths");
            var brain=headVitals.vitalFunctions.Single(value=>value.name=="Brain"&&value.type==LimbVitalFunction.Type.Brain);
            var sampleY=FindPositiveCurvePosition(brain.factor);
            var height=(float)typeof(VitalFunctions).GetField("capsuleHeight",InstanceFlags)!.GetValue(headVitals)!;
            var offset=(float)typeof(VitalFunctions).GetField("capsuleOffset",InstanceFlags)!.GetValue(headVitals)!;
            var point=headVitals.transform.TransformPoint(new Vector3(0f,sampleY*height-offset,0f));
            var originalProbability=brain.probability;
            var originalCurve=brain.factor;
            var others=headVitals.vitalFunctions.Where(value=>!ReferenceEquals(value,brain)).Select(value=>(Value:value,Probability:value.probability)).ToArray();
            var brainBefore=operatorBody.Physiology.BrainTrauma;
            try
            {
                foreach(var other in others)other.Value.probability=-1f;
                brain.probability=-1f;
                head.LimbPhysiology.VitalsTest(projectile,point,point,40f);
                if(MathF.Abs(operatorBody.Physiology.BrainTrauma-brainBefore)>.00001f)
                    throw new InvalidOperationException("VitalFunctions probability miss unexpectedly changed source brain trauma");
                brain.probability=1f;
                brain.factor=new AnimationCurve(new Keyframe(0f,0f,0f,0f),new Keyframe(1f,0f,0f,0f));
                head.LimbPhysiology.VitalsTest(projectile,point,point,40f);
                if(MathF.Abs(operatorBody.Physiology.BrainTrauma-brainBefore)>.00001f)
                    throw new InvalidOperationException("VitalFunctions zero-factor branch unexpectedly changed source brain trauma");
            }
            finally
            {
                brain.probability=originalProbability;
                brain.factor=originalCurve;
                foreach(var other in others)other.Value.probability=other.Probability;
            }
            return new{sourceLimbVitalAssignments="verified for 9 authored limb groups on every actor",positiveDispatch=results,probabilityMissNoOp=true,zeroFactorNoOp=true,distinctActorStateIsolated=true,authoredConfigurationRestored=true,nativeJointPhysicsAdvanced=false};
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(projectileObject);
        }

        static float FindPositiveCurvePosition(AnimationCurve curve)
        {
            var keys=curve.keys;
            if(keys.Length==0)throw new InvalidOperationException("Recovered vital factor curve has no authored keys");
            var candidates=keys.Where(key=>key.time>=0f&&key.time<=1f).Select(key=>key.time).ToList();
            candidates.AddRange(keys.Zip(keys.Skip(1),(a,b)=>(a.time+b.time)*.5f).Where(value=>value>=0f&&value<=1f));
            for(var index=0;index<=100;index++)
            {
                var value=index/100f;
                if(curve.clampOutside||(value>=keys[0].time&&value<=keys[^1].time))candidates.Add(value);
            }
            return candidates.Distinct().Select(value=>(Position:value,Factor:curve.Evaluate(value)))
                .OrderByDescending(value=>value.Factor).Select(value=>value.Position).FirstOrDefault(float.NaN);
        }

        static float ReadFloat(object owner,string field)
        {
            var info=owner.GetType().GetField(field,InstanceFlags)??throw new MissingFieldException(owner.GetType().FullName,field);
            return (float)info.GetValue(owner)!;
        }

        static string Snapshot(CharacterBody body)
            =>$"{body.GetInstanceID()}:{body.Physiology.BrainTrauma:R}:{body.Physiology.LungsEffectiveness:R}:{ReadFloat(body.Physiology,"heartCapacity"):R}:{body.Physiology.Paralyzed}:{body.CrippledRatio:R}";
    }

    static void VerifyRecoveredActorConfiguration(CharacterBody body)
    {
        var physiology=body.Physiology??throw new InvalidOperationException($"Recovered actor {body.name} has no Physiology");
        void Near(string field,float actual,float expected,float tolerance=.0001f)
        {
            if(MathF.Abs(actual-expected)>tolerance)
                throw new InvalidOperationException($"Recovered {body.name} prefab field {field}={actual}; expected serialized value {expected}");
        }
        Near(nameof(Physiology.heartCapacity),physiology.heartCapacity,70f);
        Near(nameof(Physiology.respirationToLungsFactor),physiology.respirationToLungsFactor,.7f);
        Near(nameof(Physiology.heartOxygenTake),physiology.heartOxygenTake,.2f);
        Near(nameof(Physiology.heartOxygenDeliver),physiology.heartOxygenDeliver,1f);
        Near(nameof(Physiology.workOxygenFactor),physiology.workOxygenFactor,.3f);
        Near(nameof(Physiology.respirationRateAdaptionRate),physiology.respirationRateAdaptionRate,.1f);
        Near(nameof(Physiology.respirationDepthAdaptionRate),physiology.respirationDepthAdaptionRate,1f);
        if(!float.IsFinite(physiology.fitness)||physiology.fitness<0f||physiology.fitness>1f)
            throw new InvalidOperationException($"Recovered {body.name} runtime-adaptive fitness is outside the source range [0,1]: {physiology.fitness}");
        Near(nameof(Physiology.heartRateMax),physiology.heartRateMax,190f);
        Near(nameof(Physiology.respirationRateMax),physiology.respirationRateMax,160f);
        Near(nameof(Physiology.brainDamageFromLackOfOxygenSeconds),physiology.brainDamageFromLackOfOxygenSeconds,3f);
        Near(nameof(Physiology.brainDamageRateWithoutOxygen),physiology.brainDamageRateWithoutOxygen,1f);
        Near(nameof(Physiology.brainDeathFromLackOfOxygenSeconds),physiology.brainDeathFromLackOfOxygenSeconds,30f);
        if(physiology.invulnerable)throw new InvalidOperationException($"Recovered {body.name} prefab unexpectedly enables physiology invulnerability");
        var curve=physiology.concussionCurve?.keys??throw new InvalidOperationException($"Recovered {body.name} prefab lost its concussion curve");
        if(curve.Length!=2||curve[0].time!=0f||curve[0].value!=0f||curve[1].time!=1f||curve[1].value!=1f||
           MathF.Abs(curve[1].inTangent-2.8872511f)>.00001f||MathF.Abs(curve[1].outTangent-2.8872511f)>.00001f)
            throw new InvalidOperationException($"Recovered {body.name} prefab concussion curve changed: keyCount={curve.Length}");

        var limbs=body.limbs;
        var physiologyCount=limbs.Count(limb=>limb.LimbPhysiology!=null);
        var vitalCount=limbs.Count(limb=>limb.GetComponent<VitalFunctions>()!=null);
        var armorCount=limbs.Count(limb=>limb.GetComponent<LimbArmor>()!=null);
        if(limbs.Count!=19||physiologyCount!=17||vitalCount!=9||armorCount!=4||limbs.Count(limb=>limb.LimbPhysiology==null)!=2)
            throw new InvalidOperationException($"Recovered {body.name} prefab component crosswalk changed: limbs={limbs.Count}, physiology={physiologyCount}, vitalFunctions={vitalCount}, armor={armorCount}, authoredNoPhysiologyHands={limbs.Count(limb=>limb.LimbPhysiology==null)}");
        VerifyAuthoredVitalAssignments(body);
    }

    static void VerifyAuthoredVitalAssignments(CharacterBody body)
    {
        var expected=new Dictionary<LimbId,(string Name,LimbVitalFunction.Type Type,float Probability)[]>
        {
            [LimbId.Pelvis]=[("Lumbar spine",LimbVitalFunction.Type.Spine,.1f),("Pelvis",LimbVitalFunction.Type.Bone,.75f)],
            [LimbId.Abdomen]=[("Lumbar spine",LimbVitalFunction.Type.Spine,.1f)],
            [LimbId.Torso]=[("Lungs",LimbVitalFunction.Type.Lungs,.75f),("Heart",LimbVitalFunction.Type.Heart,.15f),("Thoracic spine",LimbVitalFunction.Type.Spine,.075f)],
            [LimbId.Neck]=[("Cervical spine",LimbVitalFunction.Type.Spine,.2f)],
            [LimbId.Head]=[("Cervical spine",LimbVitalFunction.Type.Spine,.2f),("Brain",LimbVitalFunction.Type.Brain,1f)],
            [LimbId.RightUpperLeg]=[("Femur",LimbVitalFunction.Type.Bone,.5f),("Patella",LimbVitalFunction.Type.Bone,1f)],
            [LimbId.RightLowerLeg]=[("Tibia",LimbVitalFunction.Type.Bone,.6f)],
            [LimbId.LeftUpperLeg]=[("Femur",LimbVitalFunction.Type.Bone,.5f),("Patella",LimbVitalFunction.Type.Bone,1f)],
            [LimbId.LeftLowerLeg]=[("Tibia",LimbVitalFunction.Type.Bone,.6f)]
        };
        foreach(var group in expected)
        {
            var limb=body.limbs.Single(value=>value.id==group.Key);
            var component=limb.GetComponent<VitalFunctions>()??throw new InvalidOperationException($"Recovered {body.name}/{group.Key} lost its source VitalFunctions component");
            var actual=component.vitalFunctions??throw new InvalidOperationException($"Recovered {body.name}/{group.Key} has null source vital list");
            if(actual.Count!=group.Value.Length)
                throw new InvalidOperationException($"Recovered {body.name}/{group.Key} vital count changed: actual={actual.Count}, expected={group.Value.Length}");
            foreach(var wanted in group.Value)
            {
                var found=actual.SingleOrDefault(value=>value.name==wanted.Name&&value.type==wanted.Type)
                    ??throw new InvalidOperationException($"Recovered {body.name}/{group.Key} lacks authored vital {wanted.Name}/{wanted.Type}");
                if(MathF.Abs(found.probability-wanted.Probability)>.00001f||found.factor==null||found.factor.keys.Length==0)
                    throw new InvalidOperationException($"Recovered {body.name}/{group.Key}/{wanted.Name} serialized probability or factor curve changed: probability={found.probability}, expected={wanted.Probability}");
            }
        }
    }
}
