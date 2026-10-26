Warning: truncated output (original token count: 21481)
Total output lines: 813

using Crisis.Runtime;
using UnityEngine;
using System.Text.Json;

if(args.Length==1&&args[0]=="--source-package-store-contract"){
    Console.WriteLine(OriginalSourcePackageContracts.Run());
    return;
}
if(args.Length==2&&args[0]=="--verify-incremental-source-profile"){
    Console.WriteLine(OriginalSourcePackageContracts.VerifyProfile(args[1]));
    return;
}
if(args.Length==2&&args[0]=="--mission-fire-alarm-audio-contract"){
    var package=LoadSourcePackage(args[1]);
    var world=new RuntimeWorld();
    var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(package,world);
    var bridge=new FullSourceActorBridge(world,session.Actor);
    var fireAlarms=session.Mission.objects.Values.OfType<Crisis.Runtime.FullSource.FireAlarm>().ToArray();
    if(fireAlarms.Length==0)throw new InvalidOperationException("Mission 01 source scene has no FireAlarm instances to validate");
    var fireAlarm=fireAlarms.FirstOrDefault(value=>value.GetComponent<AudioSource>()?.clip!=null)
        ??throw new InvalidOperationException($"Mission 01 FireAlarm source components are incomplete: {string.Join(",",fireAlarms.Select(value=>$"{value.name}[AudioSource={value.GetComponent<AudioSource>()!=null},clip={value.GetComponent<AudioSource>()?.clip!=null}]"))}");
    var source=fireAlarm.GetComponent<AudioSource>()!;
    fireAlarm.ActivateIn(0f);
    using var frame=JsonDocument.Parse(JsonSerializer.Serialize(bridge.Step(1f/60f,"{}",640f,360f)));
    var played=frame.RootElement.GetProperty("audioEvents").EnumerateArray().Any(value=>
        value.GetProperty("action").GetString()=="play"&&value.GetProperty("sourcePath").GetString()==source.clip.OriginalAssetPath);
    if(!fireAlarm.alarm||!played)
        throw new InvalidOperationException($"Original delayed FireAlarm activation did not queue its authored AudioSource.Play event: alarm={fireAlarm.alarm},clip={source.clip.OriginalAssetPath},events={frame.RootElement.GetProperty("audioEvents")}");
    Console.WriteLine(JsonSerializer.Serialize(new{status="passed",scene=session.Mission.scene,fireAlarm=fireAlarm.name,audioSource=source.name,clip=source.clip.OriginalAssetPath,decoded=source.clip.loadState==AudioDataLoadState.Loaded,delayedCallback="ActivateIn(0) -> SetState(true) -> AudioSource.Play"}));
    return;
}
if(args.Length==1&&args[0]=="--browser-audio-event-contract"){
    var world=new RuntimeWorld();
    var queue=new Crisis.Runtime.BrowserAudioEventQueue();
    world.AudioSourceDriver=queue;
    // This isolated bridge contract intentionally uses a synthetic source ID;
    // asset provenance is assigned by the source-package resolver and is
    // internal to the runtime assembly.
    var clip=new AudioClip("Assets/AudioClip/hurt-07.ogg",.4f);
    var go=new GameObject("Audio source",world);
    var source=go.AddComponent<AudioSource>();source.clip=clip;source.volume=.4f;source.PlayOneShot(clip,.5f);var reportsPlaying=source.isPlaying;source.Stop();var stopped=source.isPlaying;
    using var events=JsonDocument.Parse(JsonSerializer.Serialize(queue.Drain()));
    var items=events.RootElement;
    if(items.GetArrayLength()!=2||items[0].GetProperty("action").GetString()!="oneShot"||
       items[0].GetProperty("sourceId").GetInt32()!=source.GetInstanceID()||
       MathF.Abs(items[0].GetProperty("volume").GetSingle()-.2f)>.001f||
       items[1].GetProperty("action").GetString()!="stop"||items[1].GetProperty("sourceId").GetInt32()!=source.GetInstanceID()||!reportsPlaying||stopped)
        throw new InvalidOperationException($"Browser audio bridge lost source identity, source gain, or stop order: {items}");
    var pressureWorld=new RuntimeWorld();var pressureQueue=new Crisis.Runtime.BrowserAudioEventQueue();pressureWorld.AudioSourceDriver=pressureQueue;
    var pressureSource=new GameObject("Saturated audio source",pressureWorld).AddComponent<AudioSource>();pressureSource.clip=clip;
    for(var index=0;index<64;index++)pressureSource.PlayOneShot(clip);
    pressureSource.Stop();using var pressureEvents=JsonDocument.Parse(JsonSerializer.Serialize(pressureQueue.Drain()));
    if(pressureEvents.RootElement.GetArrayLength()!=64||pressureEvents.RootElement[63].GetProperty("action").GetString()!="stop"||
       pressureQueue.DroppedEvents!=1||pressureSource.isPlaying)
        throw new InvalidOperationException($"A full browser audio queue discarded the source Stop control: dropped={pressureQueue.DroppedEvents}, sourcePlaying={pressureSource.isPlaying}, events={pressureEvents.RootElement}");
    Console.WriteLine(JsonSerializer.Serialize(new{status="passed",sourceId=source.GetInstanceID(),events=items,sourceGainApplied=true,sourceScopedStop=true,sourceIsPlayingMirrored=true,clipLoadState=clip.loadState.ToString()}));
    return;
}
if(args.Length==2&&args[0]=="--mission-hurt-audio-contract"){
    var package=LoadSourcePackage(args[1]);
    var world=new RuntimeWorld();
    var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(package,world);
    var occupant=Crisis.Runtime.SceneLoader.LoadPrefabInstance(package,"Assets/Resources/characters/Occupant.prefab",world,requireComplete:true);
    var audio=occupant.objects.Values.OfType<Crisis.Runtime.FullSource.CharacterAudio>().FirstOrDefault(value=>
        value.hurtSamples?.Any(clip=>clip?.OriginalAssetPath=="Assets/AudioClip/hurt-07.ogg")==true)
        ??throw new InvalidOperationException("The instantiated original Occupant prefab did not retain hurt-07.ogg in CharacterAudio.hurtSamples");
    var body=audio.GetComponent<Crisis.Runtime.FullSource.CharacterBody>()
        ??throw new InvalidOperationException("Instantiated Occupant CharacterAudio has no source CharacterBody owner");
    if(!body.Conscious)throw new InvalidOperationException("Instantiated Occupant is not conscious, so its source PlayHurt branch cannot run");
    var communication=audio.communicationAudio??throw new InvalidOperationException("Instantiated Occupant CharacterAudio has no communication AudioSource");
    var hurt=audio.hurtSamples.Single(clip=>clip.OriginalAssetPath=="Assets/AudioClip/hurt-07.ogg");
    audio.hurtSamples.Clear();audio.hurtSamples.Add(hurt); // isolate the identified source clip from Unity.Random selection
    typeof(Crisis.Runtime.FullSource.CharacterAudio).GetField("hurtTimestamp",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!
        .SetValue(audio,Time.time-4f);
    var queue=world.AudioSourceDriver as Crisis.Runtime.BrowserAudioEventQueue
        ??throw new InvalidOperationException("Mission browser world has no deferred source audio event queue");
    audio.Hurt();
    var events=queue.Drain();
    var dispatched=events.FirstOrDefault(value=>{
        using var json=JsonDocument.Parse(JsonSerializer.Serialize(value));
        return json.RootElement.TryGetProperty("sourcePath",out var path)&&path.GetString()==hurt.OriginalAssetPath;
    });
    if(dispatched==null)throw new InvalidOperationException($"Original CharacterAudio.Hurt did not dispatch hurt-07.ogg through its communication AudioSource; queued={JsonSerializer.Serialize(events)}");
    using var eventJson=JsonDocument.Parse(JsonSerializer.Serialize(dispatched));
    if(eventJson.RootElement.GetProperty("action").GetString()!="oneShot"||
       eventJson.RootElement.GetProperty("sourceId").GetInt32()!=communication.GetInstanceID()||!communication.isPlaying)
        throw new InvalidOperationException($"Original delayed CharacterAudio.PlayHurt lost its communication source identity or playing state: {eventJson.RootElement}");
    Console.WriteLine(JsonSerializer.Serialize(new{status="passed",scene=session.Mission.scene,prefab=occupant.scene,
        owner=audio.name,clip=hurt.OriginalAssetPath,sourceId=communication.GetInstanceID(),deferredDecodeSupported=hurt.loadState!=AudioDataLoadState.Loaded,
        action="CharacterAudio.Hurt -> PlayHurt -> communicationAudio.PlayOneShot -> BrowserAudioEventQueue"}));
    return;
}
if(args.Length==2&&args[0]=="--browser-entryteam-session-contract"){
    var package=LoadSourcePackage(args[1]);
    var world=new RuntimeWorld();
    var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(package,world);
    var listeners=session.Mission.world.Objects.OfType<AudioListener>().ToArray();
    if(listeners.Length!=2||listeners.Count(listener=>listener.isActiveAndEnabled)!=1)
        throw new InvalidOperationException($"Mission 01 listener import did not preserve the two authored components and active-scene selection: total={listeners.Length},active={listeners.Count(listener=>listener.isActiveAndEnabled)}");
    var authoredAudio=session.Mission.world.Objects.OfType<AudioSource>().SingleOrDefault(source=>source.gameObject.name=="Room 4 door")
        ??throw new InvalidOperationException("Mission 01 Room 4 door has no imported AudioSource");
    if(!float.IsFinite(authoredAudio.dopplerLevel)||authoredAudio.dopplerLevel!=0f||authoredAudio.minDistance!=1f||authoredAudio.maxDistance!=10f||authoredAudio.rolloffMode!=1||authoredAudio.rolloffCustomCurve==null||authoredAudio.rolloffCustomCurve.length!=2)
        throw new InvalidOperationException($"Room 4 door AudioSource authored distance/rolloff fields were not imported: doppler={authoredAudio.dopplerLevel},min={authoredAudio.minDistance},max={authoredAudio.maxDistance},mode={authoredAudio.rolloffMode},curveKeys={authoredAudio.rolloffCustomCurve?.length}");
    var sourceMixer=Crisis.Runtime.FullSource.Global.Context?.Mixer?.audioMixer
        ??throw new InvalidOperationException("EntryTeam source Mixer has no recovered AudioMixer");
    var sourceMixerParameters=new[]{"EffectsVolume","VoiceVolume","AmbienceVolume","InterfaceVolume","RealtimePitch"};
    var mixerSetFloatResults=sourceMixerParameters.ToDictionary(name=>name,name=>sourceMixer.SetFloat(name,.5f),StringComparer.Ordinal);
    if(mixerSetFloatResults.Values.Any(applied=>applied))
        throw new InvalidOperationException($"Mixer setter accepted source strings that are not in the recovered exposed-parameter list: {JsonSerializer.Serialize(mixerSetFloatResults)}");
    var bridge=new FullSourceActorBridge(world,session.Actor);
    var fireAlarms=session.Mission.objects.Values.OfType<Crisis.Runtime.FullSource.FireAlarm>().ToArray();
    if(fireAlarms.Length==0)throw new InvalidOperationException("Mission 01 source scene has no FireAlarm instances to validate");
    var fireAlarm=fireAlarms.FirstOrDefault(value=>value.GetComponent<AudioSource>()?.clip!=null)
        ??throw new InvalidOperationException($"Mission 01 FireAlarm source components are incomplete: {string.Join(",",fireAlarms.Select(value=>$"{value.name}[AudioSource={value.GetComponent<AudioSource>()!=null},clip={value.GetComponent<AudioSource>()?.clip!=null}]"))}");
    fireAlarm.ActivateIn(0f);
    bridge.Step(1f/60f,"{}",640f,360f);
    if(!fireAlarm.alarm)throw new InvalidOperationException("Original FireAlarm delayed activation did not reach SetState(true)");
    var authoredWalkSpeed=session.Actor.walkSpeed;
    bridge.SetAimWorldPoint(5f,2.5f,0f);
    bridge.Step(1f/60f,"{\"right\":true}",640f,360f,.5f);
    var state=JsonSerializer.Serialize(bridge.Step(1f/60f,"{\"right\":false}",640f,360f));
    using var snapshot=JsonDocument.Parse(state);
    var commands=snapshot.RootElement.GetProperty("physics").GetProperty("commands");
    if(session.Mission.scene!="Assets/Scenes/Mission 01.unity"||session.Mission.world.Objects.OfType<Crisis.Runtime.FullSource.Mission>().Single().InteriorMode?.UI?.InteractionMenu==null)
        throw new InvalidOperationException("Browser EntryTeam session did not retain the authored Mission UI contract");
    if(commands.GetArrayLength()!=1||commands[0].GetProperty("mode").GetString()!="Acceleration")
        throw new InvalidOperationException($"Original player movement did not emit the expected source acceleration command: {state}");
    var forceX=commands[0].GetProperty("force")[0].GetSingle();
    var fullWalkForce=authoredWalkSpeed*Time.fixedDeltaTime*750f;
    if(!(forceX>0f&&forceX<fullWalkForce*.6f)||MathF.Abs(session.Actor.walkSpeed-authoredWalkSpeed)>.001f)
        throw new InvalidOperationException($"The eased browser stick magnitude did not scale the original walk speed cap and restore the authored speed: forceX={forceX}, fullWalkForce={fullWalkForce}, authoredWalkSpeed={session.Actor.walkSpeed}, snapshot={state}");
    var sourceClip=session.Mission.world.Objects.OfType<AudioClip>().FirstOrDefault(clip=>!string.IsNullOrEmpty(clip.OriginalAssetPath))
        ??throw new InvalidOperationException("Mission source profile has no identified original audio clips");
    var sourceAudio=session.Mission.world.Objects.OfType<AudioSource>().FirstOrDefault()
        ??throw new InvalidOperationException("Mission source profile has no original AudioSource");
    var audioQueue=world.AudioSourceDriver as Crisis.Runtime.BrowserAudioEventQueue
        ??throw new InvalidOperationException("Mission EntryTeam session has no original browser audio event queue");
    audioQueue.Drain(); // Keep the saturation assertion isolated from the earlier scene/audio callbacks.
    var droppedBefore=audioQueue.DroppedEvents;
    for(var index=0;index<65;index++)sourceAudio.PlayOneShot(sourceClip,.37f);
    using var audioCapture=JsonDocument.Parse(JsonSerializer.Serialize(bridge.Step(1f/60f,"{}",640f,360f)));
    var audioEvents=audioCapture.RootElement.GetProperty("audioEvents");
    var expectedSourceGain=Math.Clamp(sourceAudio.volume*.37f,0f,1f);
    if(audioEvents.GetArrayLength()!=64||audioCapture.RootElement.GetProperty("droppedAudioEvents").GetInt32()!=droppedBefore+1||audioEvents[0].GetProperty("sourcePath").GetString()!=sourceClip.OriginalAssetPath||
       MathF.Abs(audioEvents[0].GetProperty("volume").GetSingle()-expectedSourceGain)>.001f||
       MathF.Abs(audioCapture.RootElement.GetProperty("sourceTime").GetSingle()-audioEvents[0].GetProperty("time").GetSingle())>.1f)
        throw new InvalidOperationException($"Original AudioSource callback was not delivered as one bounded, timestamped browser event: {audioCapture.RootElement}");
    var sourceWeapon=session.Actor.ActiveWeapon??throw new InvalidOperationException("FullSource Mission actor has no original active weapon");
    var actualFirePath=sourceWeapon.audioFire.sample?.OriginalAssetPath??string.Empty;
    var actualCrackPath=sourceWeapon.audioSupersonic.sample?.OriginalAssetPath??string.Empty;
    var particleDriver=new Crisis.Runtime.BrowserParticleEventQueue();
    world.ParticleSystemDriver??=particleDriver;
    foreach(var particles in world.Objects.OfType<ParticleSystem>())particles.Driver=particleDriver;
    var casingPhysicsCommands=new List<JsonElement>();
    var casingSourceStates=new List<JsonElement>();
    var rightArm=session.Actor.rightArm;
    var playerInput=session.Mission.world.Objects.OfType<Crisis.Runtime.FullSource.InputHandler>().Single().Keys;
    for(var step=0;step<90&&!rightArm.OnTarget;step++)bridge.Step(1f/60f,"{}",640f,360f);
    var armWasReady=rightArm.OnTarget;
    for(var step=0;step<120&&!playerInput.fire.Enabled;step++)bridge.Step(1f/60f,"{}",640f,360f);
    var fireEnabledAfterAuthoredStart=playerInput.fire.Enabled;
    var shotsBefore=session.Actor.ShotsFired;
    using var actualFireCapture=JsonDocument.Parse(JsonSerializer.Serialize(bridge.Step(1f/60f,"{\"fire\":true}",640f,360f)));
    casingPhysicsCommands.AddRange(actualFireCapture.RootElement.GetProperty("physics").GetProperty("worldCommands").EnumerateArray().Select(value=>value.Clone()));
    casingSourceStates.AddRange(actualFireCapture.RootElement.GetProperty("casings").EnumerateArray().Select(value=>value.Clone()));
    var fireDownAtPulse=playerInput.fire.Down;
    var firePressedAtPulse=playerInput.fire.Pressed;
    var mouseHeldAtPulse=Input.GetKey(playerInput.fire.key);
    var inputSuspendedField=typeof(Crisis.Runtime.FullSource.InputHandler).GetField("inputSuspended",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
    var inputSuspendedAtPulse=(bool)inputSuspendedField.GetValue(session.Mission.world.Objects.OfType<Crisis.Runtime.FullSource.InputHandler>().Single())!;
    var triggerPulledField=typeof(Crisis.Runtime.FullSource.CharacterBody).GetField("triggerPulled",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
    var triggerPulledAtPulse=(bool)triggerPulledField.GetValue(session.Actor)!;
    var actualShotEvents=new List<JsonElement>();
    var actualParticleEvents=new List<JsonElement>();
    var actualShotSourceTime=actualFireCapture.RootElement.GetProperty("sourceTime").GetSingle();
    actualShotEvents.AddRange(actualFireCapture.RootElement.GetProperty("audioEvents").EnumerateArray().Select(value=>value.Clone()));
    actualParticleEvents.AddRange(actualFireCapture.RootElement.GetProperty("particleEvents").EnumerateArray().Select(value=>value.Clone()));
    for(var step=0;step<4&&session.Actor.ShotsFired==shotsBefore;step++){
        using var shotResolution=JsonDocument.Parse(JsonSerializer.Serialize(bridge.Step(1f/60f,"{}",640f,360f)));
        actualShotSourceTime=shotResolution.RootElement.GetProperty("sourceTime").GetSingle();
        actualShotEvents.AddRange(shotResolution.RootElement.GetProperty("audioEvents").EnumerateArray().Select(value=>value.Clone()));
        actualParticleEvents.AddRange(shotResolution.RootElement.GetProperty("particleEvents").EnumerateArray().Select(value=>value.Clone()));
        casingPhysicsCommands.AddRange(shotResolution.RootElement.GetProperty("physics").GetProperty("worldCommands").EnumerateArray().Select(value=>value.Clone()));
        casingSourceStates.AddRange(shotResolution.RootElement.GetProperty("casings").EnumerateArray().Select(value=>value.Clone()));
    }
    for(var step=0;step<3;step++){
        using var cyclingResolution=JsonDocument.Parse(JsonSerializer.Serialize(bridge.Step(1f/60f,"{}",640f,360f)));
        actualShotSourceTime=cyclingResolution.RootElement.GetProperty("sourceTime").GetSingle();
        actualShotEvents.AddRange(cyclingResolution.RootElement.GetProperty("audioEvents").EnumerateArray().Select(value=>value.Clone()));
        actualParticleEvents.AddRange(cyclingResolution.RootElement.GetProperty("particleEvents").EnumerateArray().Select(value=>value.Clone()));
        casingPhysicsCommands.AddRange(cyclingResolution.RootElement.GetProperty("physics").GetProperty("worldCommands").EnumerateArray().Select(value=>value.Clone()));
        casingSourceStates.AddRange(cyclingResolution.RootElement.GetProperty("casings").EnumerateArray().Select(value=>value.Clone()));
    }
    var capturedShotPaths=actualShotEvents.Select(item=>item.GetProperty("sourcePath").GetString()).ToArray();
    var particleIdentityVerified=actualParticleEvents.Any(item=>
        item.GetProperty("type").GetString()=="particleCommand"&&
        !string.IsNullOrEmpty(item.GetProperty("sourceGuid").GetString())&&
        !string.IsNullOrEmpty(item.GetProperty("sourceFileId").GetString())&&
        !string.IsNullOrEmpty(item.GetProperty("sourcePath").GetString())&&
        item.GetProperty("sourceAssetSha256").GetString() is {Length:64} sourceAssetHash&&sourceAssetHash.All(Uri.IsHexDigit)&&
        item.GetProperty("sourceFields").ValueKind==JsonValueKind.Object&&item.GetProperty("sourceFields").EnumerateObject().Any()&&
        item.GetProperty("renderer").ValueKind==JsonValueKind.Object&&
        item.GetProperty("renderer").GetProperty("sourceAssetSha256").GetString()==sourceAssetHash&&
        item.GetProperty("renderer").GetProperty("fields").EnumerateObject().Any()&&
        float.IsFinite(item.GetProperty("time").GetSingle())&&
        item.GetProperty("time").GetSingle()<=actualShotSourceTime+.0001f);
    var weaponShotEvents=actualShotEvents.Where(item=>item.GetProperty("sourcePath").GetString()==actualFirePath||item.GetProperty("sourcePath").GetString()==actualCrackPath).ToArray();
    var capturedWe…11481 tokens truncated…AddComponent<Crisis.Runtime.FullSource.UIResourceCounterEntry>();
    var resourceLabelObject=new UnityEngine.GameObject("Operator lifecycle resource label fixture",world);resourceLabelObject.transform.SetParent(resourceEntryObject.transform,false);
    var resourceValueObject=new UnityEngine.GameObject("Operator lifecycle resource value fixture",world);resourceValueObject.transform.SetParent(resourceEntryObject.transform,false);
    resourceEntry.labelText=resourceLabelObject.AddComponent<UnityEngine.UI.Text>();
    resourceEntry.valueText=resourceValueObject.AddComponent<UnityEngine.UI.Text>();
    missionUI.resourceCounter.entryPrefab=resourceEntry;
    mission.UI=missionUI;
    var actors=new System.Collections.Generic.List<(string,UnityEngine.GameObject,UnityEngine.GameObject)>();
    foreach(var name in new[]{"Operator","Occupant"}){
        var path=$"Assets/Resources/characters/{name}.prefab";
        var loaded=SceneLoader.LoadPrefab(package,path,world,requireComplete:true);
        var instance=UnityEngine.Object.Instantiate(loaded.prefabRoot);
        loaded.prefabRoot.SetActive(false);
        // The original TeamOperator.Start runs after the mission spawner has
        // attached Equipment. Supply an empty test-owned component so the
        // actor can complete Start without claiming weapon/equipment parity.
        var fixtureEquipmentObject=new UnityEngine.GameObject($"{name} lifecycle equipment fixture",world);
        fixtureEquipmentObject.transform.SetParent(instance.transform,false);
        var fixtureEquipment=fixtureEquipmentObject.AddComponent<Crisis.Runtime.FullSource.Equipment>();
        var fixtureBody=instance.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true).Single(component=>component.GetType().Name=="CharacterBody");
        SetProperty(fixtureBody,"Equipment",fixtureEquipment);
        actors.Add((name,loaded.prefabRoot,instance));
    }
    var enabledField=typeof(UnityEngine.MonoBehaviour).GetField("enableNotified",flags)??throw new MissingFieldException("MonoBehaviour.enableNotified lifecycle state is unavailable");
    var startField=startedField;
    // The actor lifecycle gate is physics/component focused. The browser
    // renderer bridge is a separate acceptance gate, so retain material
    // values while accepting texture assignments without claiming GPU output.
    foreach(var renderer in world.Objects.OfType<UnityEngine.Renderer>().ToArray())
        renderer.material.OnTextureAssigned??=((_,_)=>{});
    if(runLifecycle)world.Tick(1f/60f);
    if(runLifecycle)world.Tick(1f/60f);
    var bodies=new System.Collections.Generic.List<UnityEngine.MonoBehaviour>();var curves=new System.Collections.Generic.List<UnityEngine.AnimationCurve>();
    var physiologies=new System.Collections.Generic.List<Crisis.Runtime.FullSource.Physiology>();
    var controllers=new System.Collections.Generic.List<Crisis.Runtime.FullSource.IKArmController>();
    var rightSolvers=new System.Collections.Generic.List<Crisis.Runtime.FullSource.ArmIKSolver>();
    var leftSolvers=new System.Collections.Generic.List<Crisis.Runtime.FullSource.ArmIKSolver>();
    var equipmentRefs=new System.Collections.Generic.List<Crisis.Runtime.FullSource.Equipment>();
    var limbGraphs=new System.Collections.Generic.List<Crisis.Runtime.FullSource.Limb[]>();
    foreach(var actor in actors){
        var behaviours=actor.Item3.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true);
        var active=behaviours.Where(component=>component.isActiveAndEnabled).ToArray();
        var awoken=active.Count(component=>(bool)awakeField.GetValue(component)!);
        var enabled=active.Count(component=>(bool)enabledField.GetValue(component)!);
        var started=active.Count(component=>(bool)startField.GetValue(component)!);
        var actorBodies=behaviours.Where(component=>component.GetType().Name=="CharacterBody").ToArray();
        if(!actor.Item3.activeInHierarchy||actorBodies.Length!=1||active.Length==0||(runLifecycle&&(awoken!=active.Length||enabled!=active.Length||started!=active.Length))||(!runLifecycle&&(awoken!=0||enabled!=0||started!=0)))
            throw new InvalidOperationException($"{actor.Item1} actual prefab lifecycle failed: active={actor.Item3.activeInHierarchy}, bodies={actorBodies.Length}, behaviours={active.Length}, Awake={awoken}, OnEnable={enabled}, Start={started}");
        bodies.Add(actorBodies[0]);
        var fullBody=(Crisis.Runtime.FullSource.CharacterBody)actorBodies[0];
        var physiology=runLifecycle?fullBody.Physiology:null;
        var controller=runLifecycle?fullBody.armController:null;
        var rightSolver=runLifecycle?fullBody.rightArm:null;
        var leftSolver=runLifecycle?fullBody.leftArm:null;
        var equipment=runLifecycle?fullBody.Equipment:null;
        if(runLifecycle){
            if(physiology==null||controller==null||rightSolver==null||leftSolver==null)
                throw new InvalidOperationException($"{actor.Item1} Awake did not bind its original physiology, IK controller, and both arm solvers");
            Crisis.Runtime.FullSource.OriginalActorIdentity.RequirePhysiology(fullBody,physiology);
            Crisis.Runtime.FullSource.OriginalActorIdentity.RequireController(fullBody,controller);
            if(!behaviours.Any(component=>ReferenceEquals(component,physiology))||
               !behaviours.Any(component=>ReferenceEquals(component,controller))||
               !behaviours.Any(component=>ReferenceEquals(component,rightSolver))||
               !behaviours.Any(component=>ReferenceEquals(component,leftSolver)))
                throw new InvalidOperationException($"{actor.Item1} original CharacterBody references components outside its cloned prefab graph");
            if(behaviours.OfType<Crisis.Runtime.FullSource.Physiology>().Count()!=1||
               behaviours.OfType<Crisis.Runtime.FullSource.IKArmController>().Count()!=1||
               behaviours.OfType<Crisis.Runtime.FullSource.ArmIKSolver>().Count()!=2)
                throw new InvalidOperationException($"{actor.Item1} cloned prefab has duplicate or missing original physiology/controller/solver components");
            if(fullBody.limbs==null||fullBody.limbMap==null||fullBody.limbs.Count!=fullBody.limbMap.Count||fullBody.limbs.Count<19)
                throw new InvalidOperationException($"{actor.Item1} original CharacterBody limb registry is incomplete");
            var actorLimbs=behaviours.OfType<Crisis.Runtime.FullSource.Limb>().ToArray();
            var limbPhysiology=behaviours.OfType<Crisis.Runtime.FullSource.LimbPhysiology>().ToArray();
            var limbMuscle=behaviours.OfType<Crisis.Runtime.FullSource.LimbMuscle>().ToArray();
            if(actorLimbs.Length!=19||limbPhysiology.Length!=17||limbMuscle.Length!=16)
                throw new InvalidOperationException($"{actor.Item1} original limb component inventory changed: limbs={actorLimbs.Length}, physiology={limbPhysiology.Length}, muscle={limbMuscle.Length}");
            foreach(var limb in fullBody.limbs){
                Crisis.Runtime.FullSource.OriginalActorIdentity.RequireLimb(fullBody,limb);
                if(!behaviours.Any(component=>ReferenceEquals(component,limb))||
                   (limb.LimbPhysiology!=null&&!ReferenceEquals(limb.GetComponent<Crisis.Runtime.FullSource.LimbPhysiology>(),limb.LimbPhysiology))||
                   (limb.LimbMuscle!=null&&!ReferenceEquals(limb.GetComponent<Crisis.Runtime.FullSource.LimbMuscle>(),limb.LimbMuscle)))
                    throw new InvalidOperationException($"{actor.Item1} limb {limb.id} is not bound to its actor-owned physiology and muscle components");
            }
            if(equipment!=null){
                Crisis.Runtime.FullSource.OriginalActorIdentity.RequireEquipment(fullBody,equipment);
                if(!behaviours.Any(component=>ReferenceEquals(component,equipment)))
                    throw new InvalidOperationException($"{actor.Item1} CharacterBody equipment is not owned by its own prefab graph");
            }
        }
        physiologies.Add(physiology);controllers.Add(controller);rightSolvers.Add(rightSolver);leftSolvers.Add(leftSolver);equipmentRefs.Add(equipment);
        limbGraphs.Add(fullBody.limbs.ToArray());
            var originalBody=actor.Item2.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true).Single(component=>component.GetType().Name=="CharacterBody");
            var curveField=originalBody.GetType().GetField("hitRecoveryCurve",flags)??throw new MissingFieldException(originalBody.GetType().FullName,"hitRecoveryCurve");
            var authoredCurve=curveField.GetValue(originalBody) as UnityEngine.AnimationCurve??throw new InvalidOperationException($"{actor.Item1} source prefab has no authored hitRecoveryCurve");
            var clonedCurve=curveField.GetValue(actorBodies[0]) as UnityEngine.AnimationCurve??throw new InvalidOperationException($"{actor.Item1} clone lost hitRecoveryCurve");
            var authoredKeys=authoredCurve.keys;var clonedKeys=clonedCurve.keys;
            if(ReferenceEquals(authoredCurve,clonedCurve)||authoredCurve.preWrapMode!=clonedCurve.preWrapMode||authoredCurve.postWrapMode!=clonedCurve.postWrapMode||authoredCurve.clampOutside!=clonedCurve.clampOutside||authoredCurve.serializedPreInfinity!=clonedCurve.serializedPreInfinity||authoredCurve.serializedPostInfinity!=clonedCurve.serializedPostInfinity||authoredCurve.serializedVersion!=clonedCurve.serializedVersion||authoredCurve.serializedRotationOrder!=clonedCurve.serializedRotationOrder||authoredKeys.Length!=clonedKeys.Length)throw new InvalidOperationException($"{actor.Item1} clone changed authored hitRecoveryCurve metadata");
            for(var keyIndex=0;keyIndex<authoredKeys.Length;keyIndex++){var sourceKey=authoredKeys[keyIndex];var cloneKey=clonedKeys[keyIndex];if(!sourceKey.time.Equals(cloneKey.time)||!sourceKey.value.Equals(cloneKey.value)||!sourceKey.inTangent.Equals(cloneKey.inTangent)||!sourceKey.outTangent.Equals(cloneKey.outTangent)||sourceKey.tangentMode!=cloneKey.tangentMode||sourceKey.serializedVersion!=cloneKey.serializedVersion)throw new InvalidOperationException($"{actor.Item1} clone changed authored hitRecoveryCurve key {keyIndex}");}
            curves.Add(clonedCurve);
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{mode=runLifecycle?"lifecycle":"instantiation",prefab=actor.Item1,instantiatedFromOriginalPrefab=true,activeBehaviours=active.Length,awake=awoken,onEnable=enabled,start=started,characterBody=actorBodies.Length,originalIKController=controller!=null,originalPhysiology=physiology!=null,testEquipmentFixtureAttached=equipment!=null,hitRecoveryCurveKeys=clonedCurve.keys.Length}));
    }
    if(ReferenceEquals(bodies[0],bodies[1])||ReferenceEquals(bodies[0].gameObject,bodies[1].gameObject)||ReferenceEquals(curves[0],curves[1]))throw new InvalidOperationException("Operator and Occupant share a CharacterBody or authored AnimationCurve instance");
    if(runLifecycle&&(ReferenceEquals(physiologies[0],physiologies[1])||ReferenceEquals(controllers[0],controllers[1])||ReferenceEquals(rightSolvers[0],rightSolvers[1])||ReferenceEquals(leftSolvers[0],leftSolvers[1])||(equipmentRefs[0]!=null&&ReferenceEquals(equipmentRefs[0],equipmentRefs[1]))))
        throw new InvalidOperationException("Operator and Occupant share physiology, IK controller, arm solvers, or initialized equipment");
    if(runLifecycle){
        var operatorBody=(Crisis.Runtime.FullSource.CharacterBody)bodies[0];
        var operatorController=controllers[0];
        if(rightSolvers[0].interpolation?.length!=2||leftSolvers[0].interpolation?.length!=2)
            throw new InvalidOperationException($"Original Operator arm interpolation curves were not deserialized (right={rightSolvers[0].interpolation?.length}, left={leftSolvers[0].interpolation?.length})");
        operatorController.Unarmed();
        if(!operatorController.IsPlaying||
           !ReferenceEquals(rightSolvers[0].TransitionTarget.parent,operatorBody.Pelvis.transform)||
           !ReferenceEquals(leftSolvers[0].TransitionTarget.parent,operatorBody.Pelvis.transform))
            throw new InvalidOperationException("Original Operator did not start its authored two-arm pelvis transition on the actor-owned rig");
        var timeSetter=typeof(UnityEngine.Time).GetProperty(nameof(UnityEngine.Time.time),System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)!.GetSetMethod(true)!;
        var deltaSetter=typeof(UnityEngine.Time).GetProperty(nameof(UnityEngine.Time.deltaTime),System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public)!.GetSetMethod(true)!;
        var controllerUpdate=typeof(Crisis.Runtime.FullSource.IKArmController).GetMethod("Update",flags)!;
        var solverUpdate=typeof(Crisis.Runtime.FullSource.ArmIKSolver).GetMethod("Update",flags)!;
        var sequenceStart=UnityEngine.Time.time;
        for(var tick=1;tick<=40&&operatorController.IsPlaying;tick++){
            const float step=.05f;
            timeSetter.Invoke(null,new object[]{sequenceStart+tick*step});
            deltaSetter.Invoke(null,new object[]{step});
            controllerUpdate.Invoke(operatorController,null);
            solverUpdate.Invoke(rightSolvers[0],null);
            solverUpdate.Invoke(leftSolvers[0],null);
        }
        if(operatorController.IsPlaying||!operatorController.RightSequencer.IsReady||!operatorController.LeftSequencer.IsReady||
           !rightSolvers[0].OnTarget||!leftSolvers[0].OnTarget||
           !ReferenceEquals(rightSolvers[0].ikTarget.parent,operatorBody.Pelvis.transform)||
           !ReferenceEquals(leftSolvers[0].ikTarget.parent,operatorBody.Pelvis.transform))
            throw new InvalidOperationException($"Original Operator authored pelvis transitions did not reach both targets and complete (playing={operatorController.IsPlaying}, rightReady={operatorController.RightSequencer.IsReady}, leftReady={operatorController.LeftSequencer.IsReady}, rightOnTarget={rightSolvers[0].OnTarget}, leftOnTarget={leftSolvers[0].OnTarget}, rightParent={rightSolvers[0].ikTarget.parent?.name}, leftParent={leftSolvers[0].ikTarget.parent?.name})");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{prefab="Operator",authoredIKSequence="Unarmed",transitionTarget="Pelvis",rightOnTarget=rightSolvers[0].OnTarget,leftOnTarget=leftSolvers[0].OnTarget,bothSequencersReady=true}));
        if(limbGraphs[0].Length!=limbGraphs[1].Length||limbGraphs[0].Where((limb,index)=>ReferenceEquals(limb,limbGraphs[1][index])).Any())
            throw new InvalidOperationException("Operator and Occupant share one or more original Limb components");
        var operatorBloodBefore=physiologies[0].BloodVolume;var occupantBloodBefore=physiologies[1].BloodVolume;physiologies[0].RemoveBlood(100f);
        if(physiologies[0].BloodVolume>=operatorBloodBefore||physiologies[1].BloodVolume!=occupantBloodBefore)
            throw new InvalidOperationException("Operator physiology mutation changed or failed to isolate Occupant blood state");
        equipmentRefs[0].gauze=1;
        if(equipmentRefs[1].gauze!=0)throw new InvalidOperationException("Operator equipment fixture mutation changed Occupant equipment state");
    }
    Console.WriteLine(runLifecycle?"Operator and Occupant lifecycle, component ownership, and Operator authored two-arm transition completion passed in one RuntimeWorld.":"Operator and Occupant prefab instantiation and component ownership passed in one RuntimeWorld.");
    foreach(var actor in actors){UnityEngine.Object.DestroyImmediate(actor.Item3);UnityEngine.Object.DestroyImmediate(actor.Item2);}
    return;
}
if(args.Contains("--prefab-only")){
    if(args.Length!=2)throw new ArgumentException("--prefab-only requires the generated scene-package.json path and loads the preserved Operator and Occupant prefabs");
    var package=File.ReadAllText(args[1]);
    foreach(var name in new[]{"Operator","Occupant"}){
        var path=$"Assets/Resources/characters/{name}.prefab";
        var world=new RuntimeWorld();
        var loaded=SceneLoader.LoadPrefab(package,path,world,requireComplete:true);
        Console.WriteLine(JsonSerializer.Serialize(loaded.Report()));
        UnityEngine.Object.DestroyImmediate(loaded.prefabRoot);
    }
    return;
}
Console.WriteLine(RuntimeContracts.Run());
Console.WriteLine(ScenePrefabContracts.Run());
Console.WriteLine(ArmSequenceContracts.Run("game/assets/original-arm-sequences.json"));
if(File.Exists("game/assets/presentation.json.gz"))Console.WriteLine(PresentationContracts.Run("game/assets/presentation.json.gz"));
if(File.Exists("game/assets/weapon-runtime.json.gz"))Console.WriteLine(WeaponContracts.Run("game/assets/weapon-runtime.json.gz"));
if(File.Exists("game/assets/ballistics.json.gz"))Console.WriteLine(BallisticContracts.Run());
if(args.Length>0){
    var package=File.ReadAllText(args[0]);
    foreach(var path in new[]{"Assets/Scenes/Marksman range.unity","Assets/Scenes/Mission 01.unity"}){
        var loaded=SceneLoader.Load(package,path);
        if(loaded.unresolvedReferences.Count>0)throw new Exception("Unresolved supported fields");
        if(path=="Assets/Scenes/Marksman range.unity"){
            var glass=loaded.objects.Values.OfType<Crisis.Runtime.FullSource.BallisticWindow>()
                .Select(window=>window.texture).FirstOrDefault(texture=>texture?.OriginalAssetPath=="Assets/Texture2D/window glass.png");
            if(glass==null||glass.OriginalAssetGuid!="bb3d703bd542afc47a1423a6c37397aa"||glass.OriginalAssetFileID!="2800000"||
               glass.OriginalImageBytes==null||Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(glass.OriginalImageBytes)).ToLowerInvariant()!="ba810cf6d18d29ac28aebfc07340188c0cd3d00f0c4fd4071f8e2a7b98d1d4b5"||
               !glass.ImporterSourceText.Contains("  textureFormat: 5\n",StringComparison.Ordinal))
                throw new InvalidOperationException("Original Marksman window texture lost its GUID, fileID, PNG source, or ARGB32 importer metadata");
        }
        Console.WriteLine(JsonSerializer.Serialize(loaded.Report()));
        Console.WriteLine(JsonSerializer.Serialize(SceneLoader.VerifyImportedRay(loaded)));
    }
    foreach(var path in new[]{"Assets/Resources/weapons/ammunition/5.56-JHP.prefab","Assets/Resources/weapons/ammunition/308-Win-BSP.prefab"})
        Console.WriteLine(JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalPrefab(package,path)));
    Console.WriteLine(JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalResource(package,"5.56-JHP")));
    Console.WriteLine(ResourcePrefabContracts.Run(package));
    Console.WriteLine(SpawnerGameplayContracts.Run(package));
}

sealed class MissionContractSilentAudioDriver:IAudioClipPlaybackDriver,IDeferredAudioSourceDriver {
    public bool IsPlaying=>false;
    public void PlayOneShot(AudioClip clip,float volume,int priority){}
    public void Play(AudioClip clip,float volume,float pitch,bool loop,bool muted,int priority){}
    public void Stop(){}
}
sealed class MissionContractSilentMixerDriver:UnityEngine.Audio.IAudioMixerDriver {
    public bool SetFloat(string exposedParameter,float value)=>false;
    public void TransitionTo(UnityEngine.Audio.AudioMixerSnapshot snapshot,float timeToReach){}
}
sealed class MissionContractQualitySettingsDriver:UnityEngine.IQualitySettingsDriver {
    public T Get<T>(string property)=>default;
    public void Set<T>(string property,T value){}
}
sealed class MissionContractSceneManagerDriver:UnityEngine.SceneManagement.ISceneManagerDriver {
    public UnityEngine.SceneManagement.Scene GetActiveScene()=>new(3,"Mission 01");
    public void LoadScene(int buildIndex)=>throw new NotSupportedException("Mission contract does not change scenes");
    public void LoadScene(string sceneName)=>throw new NotSupportedException("Mission contract does not change scenes");
}
