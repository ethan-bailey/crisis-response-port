using System.Text.Json;
using Crisis.Runtime.FullSource;
using SourceProjectile = Crisis.Runtime.FullSource.Projectile;
using SourceLimb = Crisis.Runtime.FullSource.Limb;
using SourceBallisticMaterial = Crisis.Runtime.FullSource.BallisticMaterial;
using UnityEngine;

public static class MissionProjectileContracts
{
    public static string Describe(RuntimeWorld world, Mission mission, CharacterBody[] bodies)
    {
        var shooters=bodies.Where(body=>body.GetComponent<TeamOperator>()!=null).ToArray();
        var victims=bodies.Where(body=>body.GetComponent<Occupant>()!=null).ToArray();
        var operators=shooters.Select(body=>new{
            body=body.name,
            position=Pos(body.transform.position),
            activeWeapon=DescribeWeapon(body.ActiveWeapon,body)
        }).ToArray();
        var occupants=victims.Select(body=>{
            var limbs=world.Objects.OfType<Limb>().Where(limb=>ReferenceEquals(limb.Body,body)).ToArray();
            return new{
                body=body.name,
                position=Pos(body.transform.position),
                limbCount=limbs.Length,
                renderedLimbs=limbs.Count(limb=>limb.limbRenderer!=null),
                readableSkinLimbs=limbs.Count(HasReadableSkin),
                torso=limbs.Where(limb=>limb.id==LimbId.Torso).Select(DescribeLimb).FirstOrDefault()
            };
        }).ToArray();
        var shooter=shooters.FirstOrDefault(body=>body.ActiveWeapon!=null);
        var weapon=shooter?.ActiveWeapon;
        var blockers=new List<string>();
        if(weapon?.ammunition==null)blockers.Add("Original Operator weapon has no selected ammunition");
        if(weapon?.ammunition?.projectile==null)blockers.Add("Selected original ammunition has no source projectile prefab");
        if(weapon?.ReadyToFire!=true)blockers.Add("Original weapon is not ready to fire");
        // IK readiness is established by FireOnce after it stages the original actor and advances the authored pose updates.
        object fireAttempt=null;
        if(blockers.Count==0)fireAttempt=FireOnce(world,shooter,weapon,victims);
        return JsonSerializer.Serialize(new{
            mission=mission.gameObject.name,
            started=mission.Started,
            firingStatus=blockers.Count==0?"source-fire-prerequisites-ready":"blocked-before-original-trigger",
            blockers,
            fireAttempt,
            sourceMasks=new{
                limbMask=Crisis.Runtime.FullSource.Masks.Instance.Limbs,
                ballisticTargetsMask=Crisis.Runtime.FullSource.Masks.Instance.BallisticTargets,
                sourceLimbsLayer=LayerMask.NameToLayer("Limbs"),
                directMaskForLimbs=LayerMask.GetMask("Limbs"),
                directMaskForAllBallisticLayers=LayerMask.GetMask("Limbs","Obstacle geometry","Wall geometry","Exterior geometry","Doors","Physical props","Windows")
            },
            operators,
            occupants
        });
    }

    static object FireOnce(RuntimeWorld world,CharacterBody shooter,WeaponBase weapon,CharacterBody[] victims)
    {
        int shotsBefore=shooter.ShotsFired;
        int forceRequests=0;
        var spawnTrace=new List<object>();
        var observedProjectiles=new List<SourceProjectile>();
        world.MakeCurrent();
        world.PhysicsForceSink??=((_,_,_,_)=>forceRequests++);
        world.PhysicsTorqueSink??=((_,_,_)=>forceRequests++);
        var particleDriver=new MissionContractParticleEmissionDriver();
        world.ParticleSystemDriver??=particleDriver;
        world.AudioSourceDriver??=new MissionContractSilentAudioDriver();
        // Keep the target beyond the nearby ceiling tiles when the shooter is
        // staged inside the authored roof edge below.
        var selectedVictim=victims.OrderBy(victim=>MathF.Abs(victim.transform.position.x-2f))
            .FirstOrDefault(victim=>MathF.Abs(victim.transform.position.x-2f)>6f);
        var selectedTarget=selectedVictim?.GetComponent<Target>();
        var selectedVictimPositionBefore=selectedVictim?.transform.position;
        var damageBefore=world.Objects.OfType<SourceLimb>()
            .Where(limb=>victims.Contains(limb.Body))
            .ToDictionary(limb=>limb,limb=>DescribeDamageState(limb.Body,limb));
        if(selectedTarget!=null&&shooter.WeaponAimer!=null){
            // Keep the original tissue and physiology path active while
            // disabling only wound texture decals: this headless contract has
            // no GL rasterizer, and must not pretend that decals were drawn.
            foreach(var limb in world.Objects.OfType<SourceLimb>())
                typeof(SourceLimb).GetField("useWoundDecals",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.SetValue(limb,false);
            // Stage this already-initialized source actor in a clear lane
            // before the authored roof edge; the serialized spawn is outside
            // it, where shots toward these Occupants collide with the roof.
            shooter.transform.position=new Vector3(4f,shooter.transform.position.y,shooter.transform.position.z);
            shooter.IsKneeling=true;
            // Move the selected original Occupant into a short clear corridor
            // before the authored overhead tiles, and move other original
            // Occupants out of that fixture lane. Keep every full CharacterBody
            // hierarchy and its original limb colliders; the long serialized
            // spawn path crosses the roof before reaching the selected victim.
            int stagedOccupant=0;
            foreach(var otherVictim in victims.Where(victim=>!ReferenceEquals(victim,selectedVictim)))
                otherVictim.transform.position=new Vector3(40f+stagedOccupant++*2f,otherVictim.transform.position.y,otherVictim.transform.position.z);
            var stagedVictimPosition=selectedVictim.transform.position;
            selectedVictim.transform.position=new Vector3(7f,stagedVictimPosition.y+.75f,stagedVictimPosition.z);
            // Give the original aimer a real Mission 01 target through its
            // recovered Blackboard/Target inputs, then let CharacterBody's
            // original weapon-pose update apply that aim.
            shooter.Blackboard.Target=selectedTarget;
            shooter.WeaponAimer.Mode=WeaponAimer.AimMode.Ready;
            world.Invoke(shooter.WeaponAimer,"Update");
            // This Operator is player-controlled in the source prefab, so the
            // original WeaponAimer intentionally waits for PlayerController's
            // input. Supply the target point that the normal input layer would
            // produce, while retaining CharacterBody's original pose solution.
            var aimLimb=selectedVictim.limbMap.TryGetValue(LimbId.Abdomen,out var abdomen)?abdomen:selectedVictim.limbMap[LimbId.Torso];
            shooter.AimPoint=aimLimb.AimPointWithExtents(shooter.RightUpperArm.transform.position);
            for(int tick=0;tick<30;tick++)world.Tick(1f/60f,()=>{
                foreach(var source in world.Objects.OfType<AudioSource>())source.Driver??=new MissionContractSilentAudioDriver();
                foreach(var system in world.Objects.OfType<ParticleSystem>())system.Driver??=particleDriver;
            });
        }
        foreach(var system in world.Objects.OfType<ParticleSystem>())system.Driver??=particleDriver;
        try{
            // Exercise the recovered trigger path: CharacterBody.PullTrigger,
            // then normal RuntimeWorld dispatch reaches the recovered
            // WeaponBase.Update/UpdateTriggerMechanism and SpawnProjectiles.
            shotsBefore=shooter.ShotsFired;
            weapon.onSpawnProjectile.AddListener(projectile=>{
                observedProjectiles.Add(projectile);
                spawnTrace.Add(DescribeProjectile("onSpawnProjectile",0,world,shooter,projectile));
            });
            shooter.PullTrigger();
            world.Tick(.005f,()=>{
                foreach(var source in world.Objects.OfType<AudioSource>())source.Driver??=new MissionContractSilentAudioDriver();
                foreach(var system in world.Objects.OfType<ParticleSystem>())system.Driver??=particleDriver;
            });
            int shotsAfter=shooter.ShotsFired;
            var projectiles=world.Objects.OfType<SourceProjectile>().Where(projectile=>ReferenceEquals(projectile.Owner,shooter)).ToArray();
            var flightTrace=new List<object>();
            foreach(var projectile in observedProjectiles)
                flightTrace.Add(DescribeProjectile("after-trigger-tick",0,world,shooter,projectile));
            for(int tick=0;tick<12&&shotsAfter>shotsBefore&&!victims.Any(victim=>victim.LastHitLimb!=null);tick++){
                world.Tick(.005f,()=>{
                    foreach(var source in world.Objects.OfType<AudioSource>())source.Driver??=new MissionContractSilentAudioDriver();
                    foreach(var system in world.Objects.OfType<ParticleSystem>())system.Driver??=particleDriver;
                });
                projectiles=world.Objects.OfType<SourceProjectile>().Where(projectile=>ReferenceEquals(projectile.Owner,shooter)).ToArray();
                foreach(var projectile in observedProjectiles)
                    flightTrace.Add(DescribeProjectile("after-flight-tick",tick+1,world,shooter,projectile));
            }
            object? secondShot=null;
            CharacterBody? secondVictim=null;
            if(selectedVictim.LastHitLimb!=null){
                secondVictim=victims.FirstOrDefault(victim=>!ReferenceEquals(victim,selectedVictim));
                if(secondVictim!=null){
                    var firstVictimPosition=selectedVictim.transform.position;
                    selectedVictim.transform.position=new Vector3(40f,firstVictimPosition.y,firstVictimPosition.z);
                    int otherIndex=0;
                    foreach(var otherVictim in victims.Where(victim=>!ReferenceEquals(victim,selectedVictim)&&!ReferenceEquals(victim,secondVictim)))
                        otherVictim.transform.position=new Vector3(42f+otherIndex++*2f,otherVictim.transform.position.y,otherVictim.transform.position.z);
                    var secondVictimPosition=secondVictim.transform.position;
                    secondVictim.transform.position=new Vector3(7f,secondVictimPosition.y+.75f,secondVictimPosition.z);
                    var secondTarget=secondVictim.GetComponent<Target>()??throw new InvalidOperationException("Second recovered Occupant has no original Target component");
                    shooter.Blackboard.Target=secondTarget;
                    shooter.WeaponAimer.Mode=WeaponAimer.AimMode.Ready;
                    world.Invoke(shooter.WeaponAimer,"Update");
                    var secondAimLimb=secondVictim.limbMap.TryGetValue(LimbId.Abdomen,out var secondAbdomen)?secondAbdomen:secondVictim.limbMap[LimbId.Torso];
                    shooter.AimPoint=secondAimLimb.AimPointWithExtents(shooter.RightUpperArm.transform.position);
                    for(int tick=0;tick<30;tick++)world.Tick(1f/60f,()=>{
                        foreach(var source in world.Objects.OfType<AudioSource>())source.Driver??=new MissionContractSilentAudioDriver();
                        foreach(var system in world.Objects.OfType<ParticleSystem>())system.Driver??=particleDriver;
                    });
                    if(!shooter.RightArm.OnTarget||!weapon.ReadyToFire)
                        throw new InvalidOperationException($"Second original shot prerequisites did not recover after authored pose/cycle: rightArmOnTarget={shooter.RightArm.OnTarget}, ready={weapon.ReadyToFire}");
                    var secondDamageLimb=secondVictim.limbMap.TryGetValue(LimbId.Abdomen,out secondAbdomen)?secondAbdomen:secondVictim.limbMap[LimbId.Torso];
                    var secondDamageBefore=DescribeDamageState(secondVictim,secondDamageLimb);
                    var secondShotsBefore=shooter.ShotsFired;
                    var secondSpawnCountBefore=observedProjectiles.Count;
                    shooter.PullTrigger();
                    world.Tick(.005f,()=>{
                        foreach(var source in world.Objects.OfType<AudioSource>())source.Driver??=new MissionContractSilentAudioDriver();
                        foreach(var system in world.Objects.OfType<ParticleSystem>())system.Driver??=particleDriver;
                    });
                    var secondShotsAfterTrigger=shooter.ShotsFired;
                    if(secondShotsAfterTrigger!=secondShotsBefore+1)
                        throw new InvalidOperationException($"A single second source trigger did not fire exactly one round: {secondShotsBefore}->{secondShotsAfterTrigger}");
                    var secondFlightTrace=new List<object>();
                    for(int tick=0;tick<12&&secondVictim.LastHitLimb==null;tick++){
                        world.Tick(.005f,()=>{
                            foreach(var source in world.Objects.OfType<AudioSource>())source.Driver??=new MissionContractSilentAudioDriver();
                            foreach(var system in world.Objects.OfType<ParticleSystem>())system.Driver??=particleDriver;
                        });
                        foreach(var projectile in observedProjectiles.Skip(secondSpawnCountBefore))
                            secondFlightTrace.Add(DescribeProjectile("second-shot-flight",tick+1,world,shooter,projectile));
                    }
                    var secondHitLimb=secondVictim.LastHitLimb;
                    var secondDamageAfter=secondHitLimb==null?null:DescribeDamageState(secondVictim,secondHitLimb);
                    var secondLiveProjectiles=world.Objects.OfType<SourceProjectile>().Count(projectile=>ReferenceEquals(projectile.Owner,shooter));
                    secondShot=new{
                        body=secondVictim.name,
                        selectedTarget=secondTarget.name,
                        shotsBefore=secondShotsBefore,
                        shotsAfter=shooter.ShotsFired,
                        expectedProjectiles=weapon.ammunition.projectileCount,
                        spawnedProjectiles=observedProjectiles.Count-secondSpawnCountBefore,
                        hit=secondHitLimb!=null,
                        limb=secondHitLimb?.id.ToString(),
                        damageBefore=secondDamageBefore,
                        damageAfter=secondDamageAfter,
                        liveOwnedProjectilesAfterImpact=secondLiveProjectiles,
                        flightTrace=secondFlightTrace
                    };
                }
            }
            bool secondVictimWasHit=secondVictim?.LastHitLimb!=null;
            var hits=victims.Where(victim=>victim.LastHitLimb!=null).Select(victim=>new{
                body=victim.name,
                limb=victim.LastHitLimb.id.ToString(),
                conscious=victim.Physiology?.Conscious,
                alive=victim.Physiology?.Alive,
                damageBefore=damageBefore.TryGetValue(victim.LastHitLimb,out var before)?before:null,
                damageAfter=DescribeDamageState(victim,victim.LastHitLimb)
            }).ToArray();
            bool selectedVictimWasHit=hits.Any(hit=>hit.body==selectedVictim.name);
            string result=selectedVictimWasHit&&secondVictimWasHit?"source-projectile-hit-two-occupants":selectedVictimWasHit?"source-first-shot-only":"source-shot-fired-no-occupant-hit";
            return new{
                sourcePullTriggerCalled=true,
                selectedTarget=selectedTarget?.name,
                shooterKneeling=shooter.IsKneeling,
                targetComponentAvailable=selectedTarget!=null,
                selectedVictimPositionBefore=selectedVictimPositionBefore.HasValue?Pos(selectedVictimPositionBefore.Value):null,
                selectedVictimPositionAfter=Pos(selectedVictim.transform.position),
                selectedVictimWasHit,
                secondVictimWasHit,
                secondShot,
                currentWorldMatchesMissionWorld=ReferenceEquals(RuntimeWorld.Current,world),
                ammunitionProjectileCount=weapon.ammunition.projectileCount,
                ammunitionProjectileType=weapon.ammunition.projectile.GetType().FullName,
                weaponBlocked=weapon.IsBlocked,
                aimPoint=Pos(shooter.AimPoint),
                muzzleForward=Pos(weapon.Muzzle.forward),
                shotsBefore,
                shotsAfter=shooter.ShotsFired,
                spawnedSourceProjectileCount=projectiles.Length,
                sourceProjectileCount=projectiles.Length,
                projectileTypes=projectiles.Select(projectile=>projectile.GetType().FullName).Distinct().ToArray(),
                spawnTrace,
                flightTrace,
                particleEmissionRequests=particleDriver.EmissionRequests,
                particleSimulationRequests=particleDriver.SimulationRequests,
                particlePlaybackRequests=particleDriver.PlaybackRequests,
                woundDecalsEnabledForMissionOccupants=false,
                woundDecalRenderingVerified=false,
                particleRenderingOrSimulationVerified=false,
                recordedForceRequests=forceRequests,
                nativePhysXIntegrationVerified=false,
                victimHits=hits,
                result
            };
        }catch(Exception error){
            return new{sourcePullTriggerCalled=true,shotsBefore,shotsAfter=shooter.ShotsFired,spawnTrace,error=error.InnerException?.ToString()??error.ToString(),result="source-fire-attempt-threw"};
        }
    }

    static object DescribeProjectile(string stage,int tick,RuntimeWorld world,CharacterBody shooter,SourceProjectile projectile)
    {
        try{
            bool alive=projectile;
            var hitField=typeof(SourceProjectile).GetField("nonDiscriminatingResult",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var hit=hitField==null?default: (RaycastHit)hitField.GetValue(projectile);
            var collider=hit.collider;
            return new{
                stage,
                tick,
                instanceId=projectile.GetInstanceID(),
                alive,
                componentRegistered=world.Objects.Contains(projectile),
                gameObjectRegistered=projectile.gameObject!=null&&world.Objects.Contains(projectile.gameObject),
                active=projectile.gameObject!=null&&projectile.gameObject.activeInHierarchy,
                ownerMatches=ReferenceEquals(projectile.Owner,shooter),
                owner=projectile.Owner?.name,
                parent=projectile.transform.parent?.name,
                status=projectile.Status.ToString(),
                position=Pos(projectile.transform.position),
                forward=Pos(projectile.transform.forward),
                startPosition=Pos(projectile.StartPosition),
                nextPosition=Pos(projectile.NextPosition),
                lastRayHit=collider==null?null:new{
                    name=collider.name,
                    layer=collider.gameObject.layer,
                    distance=hit.distance,
                    point=Pos(hit.point),
                    limb=collider.GetComponent<SourceLimb>()?.id.ToString(),
                    ballisticMaterial=collider.GetComponent<SourceBallisticMaterial>()?.GetType().FullName,
                    colliderType=collider.GetType().FullName
                },
                velocity=projectile.Velocity,
                energy=projectile.Energy,
                testTermination=projectile.TestTermination(),
                currentTime=Time.time,
                deltaTime=Time.deltaTime
            };
        }catch(Exception error){
            return new{stage,tick,alive=false,componentRegistered=world.Objects.Contains(projectile),error=error.GetType().Name+": "+error.Message};
        }
    }

    static object DescribeDamageState(CharacterBody body,SourceLimb limb)=>new{
        bodyShock=body.Physiology?.Shock,
        wounds=limb.LimbPhysiology?.wounds.Count,
        woundCavityVolume=limb.LimbPhysiology?.WoundCavityVolume,
        damagedTissueVolume=limb.LimbPhysiology?.DamagedTissueVolume
    };

    static object DescribeWeapon(WeaponBase weapon, CharacterBody owner)
    {
        if(weapon==null)return new{available=false};
        var projectile=weapon.ammunition?.projectile;
        var sourceAmmo=Resources.Load<GameObject>("Weapons/Ammunition/5.56-FMJ")?.GetComponent<Crisis.Runtime.FullSource.AmmunitionBase>();
        string rayResult="not tested";
        if(weapon.Muzzle!=null){
            var target=UnityEngine.Object.FindObjectsOfType<Limb>().FirstOrDefault(limb=>limb.id==LimbId.Torso&&limb.Body?.GetComponent<Occupant>()!=null);
            if(target!=null){
                var delta=target.transform.position-weapon.Muzzle.position;
                try{
                    var hits=Physics.RaycastAll(new Ray(weapon.Muzzle.position,delta.normalized),delta.magnitude+0.5f,Crisis.Runtime.FullSource.Masks.Instance.BallisticTargets);
                    var allHits=Physics.RaycastAll(new Ray(weapon.Muzzle.position,delta.normalized),delta.magnitude+0.5f,-1);
                    var collider=target.GetComponent<Collider>();
                    var direct=collider!=null&&collider.Raycast(new Ray(weapon.Muzzle.position,delta.normalized),out _,delta.magnitude+0.5f);
                    rayResult=$"targetLayer={target.gameObject.layer}; layerInMask={((1<<target.gameObject.layer)&Crisis.Runtime.FullSource.Masks.Instance.BallisticTargets)!=0}; maskedHits={hits.Length}; allLayerHits={allHits.Length}; directTargetHit={direct}";
                }catch(Exception error){rayResult=$"{error.GetType().Name}: {error.Message}";}
            }
        }
        return new{
            available=true,
            weapon=weapon.name,
            ownerMatches=ReferenceEquals(weapon.Owner,owner),
            calibre=weapon.calibre?.name,
            ammunition=weapon.ammunition?.name,
            defaultAmmunition=weapon.defaultAmmunition?.name,
            selectedFmj=Crisis.Runtime.FullSource.AmmunitionBase.GetAmmunition("5.56-FMJ",weapon.defaultAmmunition)?.name,
            defaultAmmunitionCalibre=weapon.defaultAmmunition?.calibre?.name,
            defaultAmmunitionProjectile=weapon.defaultAmmunition?.projectile?.name,
            sourceFmjResource=sourceAmmo?.name,
            sourceFmjProjectile=sourceAmmo?.projectile?.name,
            sourceFmjCalibre=sourceAmmo?.calibre?.name,
            projectilePrefab=projectile?.name,
            projectilePrefabActive=projectile?.gameObject.activeSelf,
            projectileMeshTransform=projectile?.meshTransform!=null,
            muzzle=weapon.Muzzle==null?null:Pos(weapon.Muzzle.position),
            muzzleForward=weapon.Muzzle==null?null:Pos(weapon.Muzzle.forward),
            rightArmOnTarget=owner.rightArm?.OnTarget,
            fireMode=weapon.ActiveFireMode.ToString(),
            ready=weapon.ReadyToFire,
            held=weapon.IsHeld,
            holstered=weapon.IsHolstered,
            rayResult
        };
    }

    static object DescribeLimb(Limb limb)
    {
        var collider=limb.GetComponent<Collider>();
        var renderer=limb.limbRenderer;
        var meshRenderer=renderer?.GetComponent<MeshRenderer>();
        var skinInfo=renderer?.skins?.Select(skin=>{
            string readable;
            try{_ = skin.GetPixels();readable="yes";}
            catch(Exception error){readable=$"{error.GetType().Name}: {error.Message}";}
            return new{name=skin.name,width=skin.width,height=skin.height,readable};
        }).ToArray();
        return new{
            identity=limb.id.ToString(),
            objectName=limb.name,
            position=Pos(limb.transform.position),
            collider=collider?.GetType().Name,
            renderer=renderer!=null,
            meshRenderer=meshRenderer!=null,
            parentCollider=renderer?.transform.parent?.GetComponent<Collider>()?.GetType().Name,
            skins=skinInfo,
            rendererMaterial=Crisis.Runtime.FullSource.MissionHelper.Instance.RenderMaterialInstance!=null
        };
    }

    static bool HasReadableSkin(Limb limb){
        var skins=limb.limbRenderer?.skins;
        if(skins==null||skins.Count==0)return false;
        try{_ = skins[0].GetPixels();return true;}catch{return false;}
    }

    static float[] Pos(Vector3 v)=>[v.x,v.y,v.z];
}

sealed class MissionContractParticleEmissionDriver:ParticleSystem.IParticleDriver
{
    public int EmissionRequests{get;private set;}
    public int SimulationRequests{get;private set;}
    public int PlaybackRequests{get;private set;}
    public void Emit(ParticleSystem system,int count,ParticleSystem.EmissionState state)=>EmissionRequests+=count;
    public void Simulate(ParticleSystem system,float time,bool withChildren,bool restart,bool fixedTimeStep)=>SimulationRequests++;
    public void Play(ParticleSystem system)=>PlaybackRequests++;
}
