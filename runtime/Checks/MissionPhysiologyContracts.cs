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

        return JsonSerializer.Serialize(new{contract="recovered-original-actor-physiology",actors=allActors.Length,operatorCount=operators.Length,occupantCount=occupants.Length,serializedStaticConfiguration="verified on loaded Mission actors",fitnessConfiguration="source prefab 0.85; runtime-adaptive",cappedTraumaAndTimedHemorrhage=bleedResult,hypoxicStateTransitions=hypoxiaResult,visualWoundOutput="suppressed",nativeJointPhysicsAdvanced=false});
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
    }
}
