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
if(args.Length==2&&args[0]=="--browser-entryteam-session-contract"){
    var package=LoadSourcePackage(args[1]);
    var world=new RuntimeWorld();
    var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(package,world);
    var bridge=new FullSourceActorBridge(world,session.Actor);
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
    for(var index=0;index<65;index++)sourceAudio.PlayOneShot(sourceClip,.37f);
    using var audioCapture=JsonDocument.Parse(JsonSerializer.Serialize(bridge.Step(1f/60f,"{}",640f,360f)));
    var audioEvents=audioCapture.RootElement.GetProperty("audioEvents");
    if(audioEvents.GetArrayLength()!=64||audioCapture.RootElement.GetProperty("droppedAudioEvents").GetInt32()!=1||audioEvents[0].GetProperty("sourcePath").GetString()!=sourceClip.OriginalAssetPath||
       MathF.Abs(audioEvents[0].GetProperty("volume").GetSingle()-.37f)>.001f||
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
    var capturedWeaponPaths=capturedShotPaths;
    var casingForceCommands=casingPhysicsCommands.Where(item=>item.GetProperty("source").GetString()=="Casing"&&item.GetProperty("operation").GetString()=="force"&&item.GetProperty("mode").GetString()=="VelocityChange").ToArray();
    var casingTorqueCommands=casingPhysicsCommands.Where(item=>item.GetProperty("source").GetString()=="Casing"&&item.GetProperty("operation").GetString()=="torque"&&item.GetProperty("mode").GetString()=="Impulse").ToArray();
    var casingStateIsTransportable=casingForceCommands.Length>0&&casingTorqueCommands.Length>0&&casingForceCommands.Zip(casingTorqueCommands).Any(pair=>
        pair.First.GetProperty("body").GetInt32()==pair.Second.GetProperty("body").GetInt32()&&
        pair.First.GetProperty("sourcePosition").GetArrayLength()==3&&pair.First.GetProperty("sourceRotation").GetArrayLength()==4&&
        pair.First.GetProperty("sourcePosition").EnumerateArray().All(value=>float.IsFinite(value.GetSingle()))&&
        pair.First.GetProperty("sourceRotation").EnumerateArray().All(value=>float.IsFinite(value.GetSingle()))&&
        float.IsFinite(pair.First.GetProperty("time").GetSingle())&&
        MathF.Abs(pair.First.GetProperty("mass").GetSingle()-.002f)<.0001f);
    var casingRoundTripIsTransportable=casingForceCommands.Any(force=>casingSourceStates.Any(state=>
        state.GetProperty("body").GetInt32()==force.GetProperty("body").GetInt32()&&
        MathF.Abs(state.GetProperty("mass").GetSingle()-.002f)<.0001f&&
        state.GetProperty("position").EnumerateArray().All(value=>float.IsFinite(value.GetSingle()))&&
        state.GetProperty("rotation").EnumerateArray().All(value=>float.IsFinite(value.GetSingle()))&&
        state.GetProperty("velocity").EnumerateArray().All(value=>float.IsFinite(value.GetSingle()))&&
        state.GetProperty("angularVelocity").EnumerateArray().All(value=>float.IsFinite(value.GetSingle()))));
    if(actualFirePath!="Assets/AudioClip/5.56x45mm-01-unsuppressed.wav"||actualCrackPath!="Assets/AudioClip/5.56x45mm-crack-interior.wav"||
       !armWasReady||!fireEnabledAfterAuthoredStart||session.Actor.ShotsFired<=shotsBefore||!particleIdentityVerified||particleDriver.DroppedEvents!=0||!casingStateIsTransportable||!casingRoundTripIsTransportable||weaponShotEvents.Length<2||!capturedShotPaths.Contains(actualFirePath)||!capturedShotPaths.Contains(actualCrackPath)||weaponShotEvents.Any(item=>item.GetProperty("action").GetString()!="oneShot"||
       !float.IsFinite(item.GetProperty("volume").GetSingle())||item.GetProperty("volume").GetSingle()<0||item.GetProperty("volume").GetSingle()>1||
       MathF.Abs(actualShotSourceTime-item.GetProperty("time").GetSingle())>.1f))
        throw new InvalidOperationException($"Real fire input did not produce one source shot with mapped audio, particle, and casing callbacks (armReady={armWasReady},fireEnabledAfterAuthoredStart={fireEnabledAfterAuthoredStart},shotsBefore={shotsBefore},shotsAfter={session.Actor.ShotsFired},particleEvents={actualParticleEvents.Count},particleIdentityVerified={particleIdentityVerified},droppedParticleEvents={particleDriver.DroppedEvents},casingForces={casingForceCommands.Length},casingTorques={casingTorqueCommands.Length},casingCallbacks={JsonSerializer.Serialize(casingPhysicsCommands)},capturedPaths={JsonSerializer.Serialize(capturedShotPaths)},fireKey={playerInput.fire.key},fireEnabled={playerInput.fire.Enabled},fireInAll={playerInput.All.Contains(playerInput.fire)},mouseHeldAtPulse={mouseHeldAtPulse},inputSuspendedAtPulse={inputSuspendedAtPulse},menuVisible={session.Mission.world.Objects.OfType<Crisis.Runtime.FullSource.Mission>().Single().InteriorMode!.UI.InteractionMenu.Visible},missionPaused={session.Mission.world.Objects.OfType<Crisis.Runtime.FullSource.Mission>().Single().Paused},fireDownAtPulse={fireDownAtPulse},firePressedAtPulse={firePressedAtPulse},triggerPulledAtPulse={triggerPulledAtPulse},mode={sourceWeapon.ActiveFireMode},held={sourceWeapon.IsHeld},owner={sourceWeapon.Owner?.name},hammerReady={sourceWeapon.HammerReady},boltLive={sourceWeapon.cyclingBolt.Live},weaponReady={sourceWeapon.ReadyToFire})");
    var originalPointer=session.Actor.GetComponent<Crisis.Runtime.FullSource.PlayerController>()?.WeaponPointer
        ??throw new InvalidOperationException("Browser EntryTeam actor has no original WeaponPointer");
    if(originalPointer.enabled)
        throw new InvalidOperationException("Browser source actor must keep the authored screen-ray pointer disabled; absolute browser aim owns its world target");
    var absoluteAim=new Vector3(5f,2.5f,0f);
    bridge.SetAimWorldPoint(absoluteAim.x,absoluteAim.y,absoluteAim.z);
    bridge.Step(1f/60f,"{\"left\":true}",0f,360f);
    if((originalPointer.WorldPoint-absoluteAim).magnitude>.001f)
        throw new InvalidOperationException($"Horizontal movement or browser mouse X overwrote the absolute aim target while moving left: {originalPointer.WorldPoint}");
    bridge.Step(1f/60f,"{\"right\":true}",1280f,360f);
    if((originalPointer.WorldPoint-absoluteAim).magnitude>.001f)
        throw new InvalidOperationException($"Horizontal movement or browser mouse X overwrote the absolute aim target while moving right: {originalPointer.WorldPoint}");
    var before=session.Actor.transform.position;
    var synced=JsonSerializer.Serialize(bridge.SynchronizePhysics(before.x+.25f,before.y,before.z,1f,0f,0f));
    using var roundTrip=JsonDocument.Parse(synced);
    var position=roundTrip.RootElement.GetProperty("position");
    var velocity=roundTrip.RootElement.GetProperty("velocity");
    if(MathF.Abs(position[0].GetSingle()-(before.x+.25f))>.001f||MathF.Abs(velocity[0].GetSingle()-1f)>.001f)
        throw new InvalidOperationException("Browser PhysX position and velocity did not round trip into the source player actor");
    Console.WriteLine(JsonSerializer.Serialize(new{status="passed",mission=session.Mission.scene,actor=session.Actor.name,weapon=sourceWeapon.name,weaponFirePath=actualFirePath,weaponAudioPaths=capturedWeaponPaths,sourceShotAudioEvents=actualShotEvents.Count,sourceParticleEvents=actualParticleEvents.Count,particleIdentityVerified,droppedParticleEvents=particleDriver.DroppedEvents,sourceCasingForceRequests=casingForceCommands.Length,sourceCasingTorqueRequests=casingTorqueCommands.Length,sourceShots=session.Actor.ShotsFired-shotsBefore,armReady=armWasReady,weaponInputEnabled=fireEnabledAfterAuthoredStart,sourceActors=session.SourceActorCount,interactionMenu=true,input="original press/hold/release",movementForce=commands.GetArrayLength(),analogMovement="0.5 eased cap reaches source CharacterBody and restores authored speed",aim="absolute WeaponPointer world target unchanged by horizontal movement and mouse X",audio="real original fire input drives the source firing mechanism and emits mapped fire/crack clips with bounded volume and source time",particleTransport="original ParticleSystem Emit/Simulate/Play commands carry prefab/component identity, pose, and source time",physics="position/velocity round trip; original casing force and torque requests transported with source body, mass, and ForceMode"}));
    return;
}
OriginalSourcePackage LoadSourcePackage(string path){
    if(File.Exists(path))return OriginalSourcePackage.FromJson(File.ReadAllText(path));
    using var profile=JsonDocument.Parse(File.ReadAllText(Path.Combine(path,"manifest.json")));
    var manifest=profile.RootElement;var metadataInfo=manifest.GetProperty("metadata");
    var metadataCompressed=File.ReadAllBytes(Path.Combine(path,metadataInfo.GetProperty("file").GetString()!));
    if(metadataCompressed.LongLength!=metadataInfo.GetProperty("compressedBytes").GetInt64()||Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(metadataCompressed)).ToLowerInvariant()!=metadataInfo.GetProperty("compressedSha256").GetString())
        throw new InvalidDataException("Incremental source profile metadata failed its compressed size or SHA-256 check");
    byte[] metadataBytes;
    using(var input=new MemoryStream(metadataCompressed,writable:false))
    using(var gzip=new System.IO.Compression.GZipStream(input,System.IO.Compression.CompressionMode.Decompress))
    using(var output=new MemoryStream()){gzip.CopyTo(output);metadataBytes=output.ToArray();}
    if(metadataBytes.LongLength!=metadataInfo.GetProperty("bytes").GetInt64()||Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(metadataBytes)).ToLowerInvariant()!=metadataInfo.GetProperty("sha256").GetString())
        throw new InvalidDataException("Incremental source profile metadata failed its source size or SHA-256 check");
    var package=OriginalSourcePackage.CreateIncremental(System.Text.Encoding.UTF8.GetString(metadataBytes));
    foreach(var asset in manifest.GetProperty("assets").EnumerateArray())
        package.AddVerifiedGzipAsset(asset.GetProperty("path").GetString()!,File.ReadAllBytes(Path.Combine(path,asset.GetProperty("file").GetString()!)));
    if(package.AssetCount!=manifest.GetProperty("assetCount").GetInt32()||package.DeliveredBytes!=manifest.GetProperty("uncompressedAssetBytes").GetInt64()||package.DeliveredCompressedBytes!=manifest.GetProperty("compressedAssetBytes").GetInt64())
        throw new InvalidDataException("Incremental source profile totals differ from the verified runtime store");
    return package;
}
if(args.Length==2&&args[0]=="--builtin-resource-contract"){
    using var builtinPackage=JsonDocument.Parse(File.ReadAllText(args[1]));
    Console.WriteLine(RuntimeContracts.RunBuiltinResource(builtinPackage.RootElement));
    return;
}
if(args.Length==2&&args[0]=="--spawner-contract"){
    Console.WriteLine(SpawnerGameplayContracts.Run(File.ReadAllText(args[1])));
    return;
}
if(args.Length==2&&args[0]=="--behavior-graph-contract"){
    Console.WriteLine(BehaviorDesignerContracts.Run(File.ReadAllText(args[1])));
    return;
}
if(args.Length==2&&args[0]=="--source-ui-prefab-contract"){
    var package=File.ReadAllText(args[1]);var world=new RuntimeWorld();world.MakeCurrent();
    var loaded=SceneLoader.LoadPrefab(package,"Assets/GameObject/Interior mode UI.prefab",world,requireComplete:false);
    var rects=loaded.prefabRoot.GetComponentsInChildren<RectTransform>(true);
    var canvases=loaded.prefabRoot.GetComponentsInChildren<Canvas>(true);var canvas=canvases.FirstOrDefault();
    var scalers=loaded.prefabRoot.GetComponentsInChildren<UnityEngine.UI.CanvasScaler>(true);
    var raycasters=loaded.prefabRoot.GetComponentsInChildren<UnityEngine.UI.GraphicRaycaster>(true);
    var pointer=loaded.prefabRoot.GetComponentsInChildren<Crisis.Runtime.FullSource.WeaponPointer>(true).SingleOrDefault();
    var physiology=loaded.prefabRoot.GetComponentsInChildren<Crisis.Runtime.FullSource.UIPhysiologyIndicator>(true).SingleOrDefault();
    if(rects.Length==0||canvas==null||scalers.Length==0||raycasters.Length==0||pointer?.aimQuadPrefab==null||physiology?.circulationImage?.material==null)
        throw new InvalidOperationException($"Authored UI prefab dependency graph missed required UI components (rects={rects.Length}, canvas={canvases.Length}, scalers={scalers.Length}, raycasters={raycasters.Length}, pointer={pointer!=null}, aimQuad={pointer?.aimQuadPrefab!=null}, physiologyMaterial={physiology?.circulationImage?.material!=null})");
    if(canvas.renderMode!=RenderMode.ScreenSpaceOverlay||!canvas.receivesEvents||scalers[0].referenceResolution.x!=1280||scalers[0].referenceResolution.y!=720||!raycasters[0].ignoreReversedGraphics)
        throw new InvalidOperationException("Authored UI Canvas, CanvasScaler, or GraphicRaycaster settings did not import");
    if(pointer.aimQuadPrefab.OriginalAssetPath!="Assets/GameObject/Aim quad.prefab")throw new InvalidOperationException($"Authored WeaponPointer aim-quad prefab resolved as {pointer.aimQuadPrefab.OriginalAssetPath} ({pointer.aimQuadPrefab.name})");
    var uiClone=UnityEngine.Object.Instantiate(loaded.prefabRoot);var clonedPhysiology=uiClone.GetComponentsInChildren<Crisis.Runtime.FullSource.UIPhysiologyIndicator>(true).Single();
    if(clonedPhysiology.circulationImage.material==null||!ReferenceEquals(clonedPhysiology.circulationImage,uiClone.GetComponentsInChildren<UnityEngine.UI.Image>(true).First(image=>image.name==physiology.circulationImage.name)))
        throw new InvalidOperationException("Instantiated authored UI did not retain the physiology overlay material and owned image reference");
    Console.WriteLine(JsonSerializer.Serialize(new{prefab=loaded.scene,rectTransforms=rects.Length,canvasCount=canvases.Length,canvasScalerCount=scalers.Length,graphicRaycasterCount=raycasters.Length,canvasRenderMode=canvas.renderMode,referenceResolution=new{x=scalers[0].referenceResolution.x,y=scalers[0].referenceResolution.y},aimQuadPrefab=pointer.aimQuadPrefab.OriginalAssetPath,physiologyMaterial=physiology.circulationImage.material.OriginalAssetPath,physiologyMaterialCloned=clonedPhysiology.circulationImage.material!=null,unsupportedComponents=loaded.unsupportedComponents,unresolvedReferences=loaded.unresolvedReferences}));
    return;
}
if(args.Length==3&&args[0]=="--ammunition-resource-contract"){
    Console.WriteLine(JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalResource(File.ReadAllText(args[1]),args[2])));
    return;
}
if(args.Length==2&&(args[0]=="--mission-contract"||args[0]=="--mission-ui-contract"||args[0]=="--mission-ui-incremental-contract"||args[0]=="--mission-outcome-contract"||args[0]=="--mission-ai-contract"||args[0]=="--mission-ai-outcome-contract"||args[0]=="--mission-projectile-contract"||args[0]=="--mission-ai-projectile-contract")){
    var runOriginalAI=args[0].StartsWith("--mission-ai-",StringComparison.Ordinal);
    var runWithOriginalUI=args[0] is "--mission-ui-contract" or "--mission-ui-incremental-contract";
    var package=LoadSourcePackage(args[1]);
    var world=new RuntimeWorld();
    world.MakeCurrent();
    UnityEngine.Application.persistentDataPath=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"crisis-response-mission-contract-"+Guid.NewGuid().ToString("N"));
    Resources.Configure(package,world);
    var contextPrefab=SceneLoader.LoadPrefabInstance(package,"Assets/Resources/Global context.prefab",world,requireComplete:false);
    UnityEngine.QualitySettings.Bind(world,new MissionContractQualitySettingsDriver());
    UnityEngine.Shader.Resolver??=(name=>new UnityEngine.Shader{name=name,isSupported=false});
    UnityEngine.SceneManagement.SceneManager.Bind(world,new MissionContractSceneManagerDriver());
    Material.TextureDriver??=((_,_)=>{});
    world.Initialize();
    var context=Crisis.Runtime.FullSource.Global.Context??throw new InvalidOperationException("Instantiated original Global Context did not initialize before Mission 01 load");
    context.Usage.enabled=false;
    context.CommandHandler.enabled=false;
    foreach(var clip in world.Objects.OfType<AudioClip>())clip.ReportLoadState(AudioDataLoadState.Loaded);
    foreach(var source in world.Objects.OfType<AudioSource>())source.Driver=new MissionContractSilentAudioDriver();
    UnityEngine.Audio.AudioMixer.Bind(world,new MissionContractSilentMixerDriver());
    var team=new Crisis.Runtime.FullSource.SerializableTeam();
    team.marksman.enabled=false;
    var loadout=new SerializableEquipment();loadout.primary.prefabName="M4A1";loadout.primary.ammunitionPrefabName="5.56-FMJ";loadout.secondary.prefabName="Glock-18";loadout.secondary.ammunitionPrefabName="9-FMJ";
    loadout.items.itemPrefabNames.AddRange(new[]{"Primary magazine","Secondary magazine","Plasticuffs","Gauze"});
    team.entryMembers.Add(loadout);
    typeof(Crisis.Runtime.FullSource.OperatorSpawnData).GetProperty(nameof(Crisis.Runtime.FullSource.OperatorSpawnData.Team),System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)!.SetValue(context.OperatorSpawnData,team);
    context.MissionMetaData.DeactivateTutorial();
    // Keep gate 7 focused on the real mission and actor flow in this headless
    // host. The original menus instantiate a large presentation-only prefab
    // graph; suppress those roots before their Awake callbacks, as a UI driver
    // would, while preserving the mission, operators, occupants, and systems.
    if(runWithOriginalUI)Console.Error.WriteLine("MISSION-UI stage: loading scene records");
    var mission=SceneLoader.Load(package,"Assets/Scenes/Mission 01.unity",world,loaded=>{
        if(runWithOriginalUI)Console.Error.WriteLine("MISSION-UI stage: scene records loaded; activating authored UI");
        if(!runWithOriginalUI)foreach(var ui in loaded.objects.Values.OfType<Crisis.Runtime.FullSource.MissionUI>())ui.gameObject.SetActive(false);
        if(!runWithOriginalUI)foreach(var component in loaded.objects.Values.OfType<Crisis.Runtime.FullSource.Mission>()){
            // Keep original InteriorMode gameplay state for PlayerController,
            // but keep its original canvas initialized for weapon-pointer and
            // physiology indicator references. The headless gate supplies the
            // pointer's three image sinks below; menu and canvas callbacks are
            // presentation-only.
            if(component.interiorModePrefab!=null){
                component.interiorModePrefab.gameObject.SetActive(false);
                if(component.interiorModePrefab.uiPrefab!=null)
                    component.interiorModePrefab.uiPrefab.gameObject.SetActive(false);
            }
        }
        if(runWithOriginalUI)Console.Error.WriteLine("MISSION-UI stage: pre-activation configuration complete");
    });
    if(runWithOriginalUI)Console.Error.WriteLine("MISSION-UI stage: scene activation completed");
    var missionComponent=mission.objects.Values.OfType<Crisis.Runtime.FullSource.Mission>().Single();
    missionComponent.sandboxStartMode=runWithOriginalUI?Crisis.Runtime.FullSource.Mission.MissionView.EntryTeam:Crisis.Runtime.FullSource.Mission.MissionView.None;
    mission.objects.Values.OfType<Crisis.Runtime.FullSource.MissionUI>().Single().missionIntro=null;
    foreach(var renderer in world.Objects.OfType<Renderer>().ToArray())renderer.material.OnTextureAssigned??=((_,_)=>{});
    // Fog of war presentation Starts before the Mission Start callback in this
    // host's record order; it is excluded from this actor lifecycle contract.
    foreach(var fog in world.Objects.OfType<Crisis.Runtime.FullSource.FogOfWarElement>())fog.enabled=false;
    // The reticle and indicators are initialized; suppress only the headless
    // pointer's camera/input polling Start callback.
    foreach(var pointer in world.Objects.OfType<Crisis.Runtime.FullSource.WeaponPointer>()){
        pointer.enabled=false;
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        void BindImage(string field,UnityEngine.Transform dot){if(dot!=null&&dot.GetComponent<UnityEngine.UI.Image>() is { } image)pointer.GetType().GetField(field,flags)!.SetValue(pointer,image);}
        BindImage("mainImage",pointer.mainDot);BindImage("startImage",pointer.startDot);BindImage("endImage",pointer.endDot);
    }
    // RuntimeWorld has no native/browser audio device. Mark preserved source
    // clips as accepted by a test adapter so original mission startup and music
    // calls execute without claiming decoded or audible output.
    foreach(var clip in world.Objects.OfType<AudioClip>())clip.ReportLoadState(AudioDataLoadState.Loaded);
    foreach(var source in world.Objects.OfType<AudioSource>())source.Driver=new MissionContractSilentAudioDriver();
    void DisableUnboundVocalizationPresentation(){foreach(var vocalization in world.Objects.OfType<Crisis.Runtime.FullSource.UIVocalizationText>().ToArray())vocalization.enabled=false;foreach(var source in world.Objects.OfType<AudioSource>())source.Driver??=new MissionContractSilentAudioDriver();if(!runOriginalAI)foreach(var tree in world.Objects.OfType<BehaviorDesigner.Runtime.BehaviorTree>().ToArray())tree.enabled=false;}
    world.Tick(1f/60f,DisableUnboundVocalizationPresentation);
    // Original AI/movement can emit forces on its next fixed tick. This
    // contract records source force commands without claiming a PhysX solve.
    var aiForceCommands=0;foreach(var rigidbody in world.Objects.OfType<Rigidbody>())rigidbody.PhysicsForceSink??=((_,_,_)=>aiForceCommands++);
    world.Tick(1f/60f,DisableUnboundVocalizationPresentation);
    var aiSimulationTicks=0;if(runOriginalAI)for(var aiTick=0;aiTick<60;aiTick++){world.Tick(1f/60f,DisableUnboundVocalizationPresentation);aiSimulationTicks++;}
    missionComponent=world.Objects.OfType<Crisis.Runtime.FullSource.Mission>().SingleOrDefault()??throw new InvalidOperationException("Mission 01 did not initialize exactly one original Mission component");
    var bodies=world.Objects.OfType<Crisis.Runtime.FullSource.CharacterBody>().ToArray();
    if(!missionComponent.Started)throw new InvalidOperationException("Original Mission did not reach BeginMission during normal RuntimeWorld startup");
    if(bodies.Length<2||!bodies.Any(body=>body.GetComponent<Crisis.Runtime.FullSource.TeamOperator>()!=null)||!bodies.Any(body=>body.GetComponent<Crisis.Runtime.FullSource.Occupant>()!=null))
        throw new InvalidOperationException($"Mission 01 actor flow did not produce an Operator and Occupant; bodies={bodies.Length}");
    var activePlayerControllers=world.Objects.OfType<Crisis.Runtime.FullSource.PlayerController>()
        .Where(controller=>controller&&controller.isActiveAndEnabled&&controller.Body!=null&&controller.Body.IsPlayer).ToArray();
    if(activePlayerControllers.Length!=1)
        throw new InvalidOperationException($"FullSource actor bridge requires exactly one active original player actor; found {activePlayerControllers.Length}");
    var fullSourceBridge=new FullSourceActorBridge(world,activePlayerControllers[0].Body);
    using(var poseSnapshot=JsonDocument.Parse(JsonSerializer.Serialize(fullSourceBridge.Snapshot())))
        if(poseSnapshot.RootElement.GetProperty("owner").GetString()!="FullSource.CharacterBody"||
           poseSnapshot.RootElement.GetProperty("poses").GetArrayLength()<20)
            throw new InvalidOperationException("FullSource actor bridge did not return the actor-owned authored hierarchy snapshot");
    if(runWithOriginalUI){
        if(missionComponent.InteriorMode?.UI?.InteractionMenu==null)throw new InvalidOperationException("EntryTeam mission did not initialize its original InteractionMenu");
        var leftAction=Crisis.Runtime.FullSource.Global.Context.InputHandler.Keys.left;
        var inputHandler=Crisis.Runtime.FullSource.Global.Context.InputHandler;
        fullSourceBridge.Step(1f/60f,"{\"left\":true}",0f,0f);
        if(!leftAction.Pressed)throw new InvalidOperationException($"FullSource bridge did not deliver the original left-action press to InputHandler (rawKey={UnityEngine.Input.GetKey(leftAction.key)}, key={leftAction.key}, enabled={leftAction.Enabled}, handlerEnabled={inputHandler.isActiveAndEnabled}, suspended={inputHandler.GetType().GetField("inputSuspended",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.GetValue(inputHandler)})");
        using var physicsFrame=JsonDocument.Parse(JsonSerializer.Serialize(fullSourceBridge.Step(1f/60f,"{}",0f,0f)));
        if(leftAction.Pressed||!leftAction.Up)throw new InvalidOperationException("FullSource bridge did not deliver the original left-action release to InputHandler");
        var physics=physicsFrame.RootElement.GetProperty("physics");
        var commands=physics.GetProperty("commands");
        if(physics.GetProperty("fixedSteps").GetInt32()!=1||commands.GetArrayLength()!=1||commands[0].GetProperty("mode").GetString()!="Acceleration"||commands[0].GetProperty("force")[0].GetSingle()>=0f)
            throw new InvalidOperationException("FullSource left movement did not emit one actor-owned acceleration command for the browser PhysX step");
        using var synchronized=JsonDocument.Parse(JsonSerializer.Serialize(fullSourceBridge.SynchronizePhysics(1.25f,2.5f,1.5f,-.5f,0f,0f)));
        if(MathF.Abs(synchronized.RootElement.GetProperty("position")[0].GetSingle()-1.25f)>.0001f||MathF.Abs(synchronized.RootElement.GetProperty("velocity")[0].GetSingle()+.5f)>.0001f)
            throw new InvalidOperationException("Browser PhysX result was not synchronized back to the original FullSource actor Rigidbody and Transform");
    }else try{
        fullSourceBridge.Step(1f/60f,"{\"left\":true}",0f,0f);
        throw new InvalidOperationException("FullSource bridge tick accepted Mission 01's intentionally incomplete, headless player UI");
    }catch(NotSupportedException error) when(error.Message.Contains("InteractionMenu",StringComparison.Ordinal)){}
    if(args[0]=="--mission-projectile-contract"||args[0]=="--mission-ai-projectile-contract"){
        var report=MissionProjectileContracts.Describe(world,missionComponent,bodies);
        Console.WriteLine(report);
        using var reportDocument=JsonDocument.Parse(report);
        var reportRoot=reportDocument.RootElement;
        var firingStatus=reportRoot.GetProperty("firingStatus").GetString();
        if(firingStatus!="source-fire-prerequisites-ready")
            throw new InvalidOperationException($"Mission projectile contract was blocked before the original trigger: {report}");
        var attempt=reportRoot.GetProperty("fireAttempt");
        if(attempt.ValueKind!=JsonValueKind.Object)
            throw new InvalidOperationException($"Mission projectile contract produced no fire attempt: {report}");
        var result=attempt.GetProperty("result").GetString();
        if(result!="source-projectile-hit-selected-occupant")
            throw new InvalidOperationException($"Mission projectile contract failed: {result}");
        var selectedTarget=attempt.GetProperty("selectedTarget").GetString();
        var selectedHit=attempt.GetProperty("victimHits").EnumerateArray()
            .FirstOrDefault(hit=>hit.GetProperty("body").GetString()==selectedTarget);
        if(selectedHit.ValueKind!=JsonValueKind.Object)
            throw new InvalidOperationException("Mission projectile contract did not report the selected Occupant hit");
        var damageBefore=selectedHit.GetProperty("damageBefore");
        var damageAfter=selectedHit.GetProperty("damageAfter");
        if(damageAfter.GetProperty("wounds").GetInt32()<=damageBefore.GetProperty("wounds").GetInt32()
            ||damageAfter.GetProperty("damagedTissueVolume").GetSingle()<=damageBefore.GetProperty("damagedTissueVolume").GetSingle()
            ||damageAfter.GetProperty("bodyShock").GetSingle()<=damageBefore.GetProperty("bodyShock").GetSingle())
            throw new InvalidOperationException("Mission projectile contract hit the selected Occupant without verified wound, tissue, and shock increases");
        return;
    }
    var outcomeBehaviorTrees=world.Objects.OfType<BehaviorDesigner.Runtime.BehaviorTree>().Where(tree=>tree.enabled).ToArray();
    if(runOriginalAI&&(outcomeBehaviorTrees.Length==0||outcomeBehaviorTrees.Any(tree=>tree.GetBehaviorSource().RootTask==null)))throw new InvalidOperationException($"Original Mission AI outcome did not retain enabled Behavior Trees with deserialized roots: activeTrees={outcomeBehaviorTrees.Length}");
    if(args[0]=="--mission-outcome-contract"||args[0]=="--mission-ai-outcome-contract"){
        var spawnedOccupants=bodies.Where(body=>body.GetComponent<Crisis.Runtime.FullSource.Occupant>()!=null).ToArray();
        foreach(var source in world.Objects.OfType<AudioSource>())source.Driver=new MissionContractSilentAudioDriver();
        var sourceOperator=bodies.Select(body=>body.GetComponent<Crisis.Runtime.FullSource.TeamOperator>()).FirstOrDefault(op=>op!=null&&op.Body!=null)??throw new InvalidOperationException("Mission outcome contract has no original TeamOperator with an initialized original CharacterBody reference");
        var sourceRoomUpdate=typeof(Crisis.Runtime.FullSource.TeamController).GetMethod("UpdateMissionState",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)??throw new MissingMethodException("Original TeamController.UpdateMissionState");
        var roomsClearedThroughSource=0;
        foreach(var room in missionComponent.rooms.Where(room=>room!=null&&room.gameObject.activeInHierarchy)){
            var actorPosition=sourceOperator.Body.transform.position;
            actorPosition.x=room.CenterCoord;
            sourceOperator.Body.transform.position=actorPosition;
            sourceOperator.Body.Turn(1f);
            world.Invoke(sourceOperator.Body,"UpdateMovement");
            if(!ReferenceEquals(sourceOperator.Body.Room,room))throw new InvalidOperationException($"Original CharacterBody room query did not resolve room {room.name}");
            sourceOperator.Body.VisibleRange=MathF.Max(0f,room.RightEdge-sourceOperator.Body.Eye.position.x+1f);
            sourceRoomUpdate.Invoke(sourceOperator.Team,new object[]{sourceOperator});
            if(!room.cleared)throw new InvalidOperationException($"Original TeamController.UpdateMissionState did not clear traversed room {room.name}");
            roomsClearedThroughSource++;
        }
        if(roomsClearedThroughSource==0)throw new InvalidOperationException("Mission 01 has no active rooms to clear through the original Operator path");
        world.Invoke(missionComponent,"UpdateMissionState");
        if(!missionComponent.AllRoomsChecked)throw new InvalidOperationException("Original Mission.UpdateMissionState did not accept rooms cleared through the Operator path");
        var allDisabledEventCount=0;missionComponent.Occupants.allDisabledEvent.AddListener(()=>allDisabledEventCount++);
        foreach(var body in spawnedOccupants)body.SetUnconscious();
        world.Tick(1f/60f,DisableUnboundVocalizationPresentation);
        if(!missionComponent.Occupants.AllRenderedSafe||allDisabledEventCount!=1||spawnedOccupants.Any(body=>!body.IsRagdoll))
            throw new InvalidOperationException($"Original Occupants manager failed to propagate all-disabled/ragdoll state before restraint: allRenderedSafe={missionComponent.Occupants.AllRenderedSafe}, eventCount={allDisabledEventCount}, ragdoll={spawnedOccupants.Count(body=>body.IsRagdoll)}/{spawnedOccupants.Length}");
        // Restrain invokes the original CharacterBody callback and creates its
        // source SpringJoints. Do not advance a physics tick here: the headless
        // runner intentionally has no native PhysX joint driver. Recompute the
        // original manager and Mission state immediately to verify completion.
        foreach(var body in spawnedOccupants)body.Restrain();
        world.Invoke(missionComponent.Occupants,"Update");
        world.Invoke(missionComponent,"UpdateMissionState");
        if(!missionComponent.Occupants.AllRestrained||spawnedOccupants.Any(body=>!body.IsRestrained)||!missionComponent.missionEnded)
            throw new InvalidOperationException($"Original Mission completion did not propagate restraint and missionEnded state: allRestrained={missionComponent.Occupants.AllRestrained}, restrained={spawnedOccupants.Count(body=>body.IsRestrained)}/{spawnedOccupants.Length}, missionEnded={missionComponent.missionEnded}");
        string presentationBlocker=null;string missionUiInitialization=null;
        var missionUI=world.Objects.OfType<Crisis.Runtime.FullSource.MissionUI>().Single();
        try{
            missionUI.gameObject.SetActive(true);
            if(missionUI.PauseMenu==null)world.Invoke(missionUI,"Awake");
            if(missionUI.PauseMenu==null)throw new InvalidOperationException("Original MissionUI.Awake returned without instantiating its authored PauseMenu prefab");
            missionUiInitialization="original MissionUI.Awake created the authored PauseMenu";
        }catch(Exception error){missionUiInitialization=error.ToString();}
        bool cameraDepthOfFieldFound=false;
        var interiorCamera=missionComponent.InteriorMode?.interiorCamera;
        if(interiorCamera!=null){
            cameraDepthOfFieldFound=interiorCamera.GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>()!=null;
            if(cameraDepthOfFieldFound)world.Invoke(interiorCamera,"Awake");
        }
        try{missionComponent.EndMission(0f);}
        catch(Exception error){presentationBlocker=error.ToString();}
        if(!missionComponent.missionSuccess)throw new InvalidOperationException("Original Mission.EndMission did not derive success from the checked-room and all-rendered-safe source state");
        var endMenuVisible=missionUI.missionEndMenu?.Visible??false;
        if(presentationBlocker==null&&!endMenuVisible)throw new InvalidOperationException("Original Mission.EndMission returned without showing the authored MissionEndMenu");
        Console.WriteLine(JsonSerializer.Serialize(new{mission=mission.scene,missionStarted=missionComponent.Started,missionEnded=missionComponent.missionEnded,missionSuccess=missionComponent.missionSuccess,allRenderedSafe=missionComponent.Occupants.AllRenderedSafe,allDisabledEventCount,allRoomsChecked=missionComponent.AllRoomsChecked,roomsClearedThroughSource,spawnedOccupants=spawnedOccupants.Length,allOccupantsRagdoll=spawnedOccupants.All(body=>body.IsRagdoll),allOccupantsRestrained=spawnedOccupants.All(body=>body.IsRestrained),outcomePath="Operator traversed room -> original CharacterBody room query -> original TeamController.UpdateMissionState/Room.Clear -> original Mission.UpdateMissionState -> original Occupant disable -> original CharacterBody.Restrain -> original Mission completion update -> Mission.EndMission",missionUiInitialization,cameraDepthOfFieldFound,endMenuVisible,presentationTransition=presentationBlocker==null?"completed":"blocked: "+presentationBlocker,ai=runOriginalAI?"original BehaviorTrees remained enabled with deserialized roots during the outcome":"suppressed; original BehaviorDesigner graph separately passes both actor prefabs",behaviorTrees=outcomeBehaviorTrees.Length,behaviorTreeStatus=outcomeBehaviorTrees.Select(tree=>new{root=tree.GetBehaviorSource().RootTask?.GetType().Name,execution=tree.ExecutionStatus.ToString()}).ToArray(),audio="silent adapter"}));
        return;
    }
    var liveTrees=world.Objects.OfType<BehaviorDesigner.Runtime.BehaviorTree>().Where(tree=>tree.enabled).ToArray();
    if(runOriginalAI&&(liveTrees.Length==0||liveTrees.Any(tree=>tree.GetBehaviorSource().RootTask==null)))throw new InvalidOperationException($"Original mission AI trees did not remain active with deserialized roots: activeTrees={liveTrees.Length}");
    Console.WriteLine(JsonSerializer.Serialize(new{mission=mission.scene,missionStarted=missionComponent.Started,globalContext=context.gameObject.name,globalContextInstantiated=!ReferenceEquals(context.gameObject,contextPrefab.prefabRoot),sourceActors=bodies.Length,operators=bodies.Count(body=>body.GetComponent<Crisis.Runtime.FullSource.TeamOperator>()!=null),occupants=bodies.Count(body=>body.GetComponent<Crisis.Runtime.FullSource.Occupant>()!=null),fullSourceActorBridge=runWithOriginalUI?"snapshot passed; EntryTeam UI initialized; original left press and release passed":"snapshot passed; step correctly rejected missing authored InteractionMenu UI",ai=runOriginalAI?"original BehaviorTrees enabled for one simulated second":"suppressed in headless mission contract",aiSimulationTicks,aiForceCommands,behaviorTrees=liveTrees.Length,behaviorTreeStatus=liveTrees.Select(tree=>new{root=tree.GetBehaviorSource().RootTask?.GetType().Name,execution=tree.ExecutionStatus.ToString()}).ToArray(),sceneObjects=mission.gameObjects,originalComponents=mission.originalComponents,missionUnsupported=mission.unsupportedComponents,missionUnresolved=mission.unresolvedReferences,globalContextUnsupported=contextPrefab.unsupportedComponents,globalContextUnresolved=contextPrefab.unresolvedReferences,audio="silent test adapter; original decode/playback not verified"}));
    return;
}
var runLifecycle=args.Contains("--prefab-lifecycle");if(runLifecycle||args.Contains("--prefab-instantiate")){
    if(args.Length!=2)throw new ArgumentException("--prefab-lifecycle and --prefab-instantiate require the generated scene-package.json path");
    var package=File.ReadAllText(args[1]);
    var world=new RuntimeWorld();
    world.MakeCurrent();
    var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
    var awakeField=typeof(UnityEngine.MonoBehaviour).GetField("awoken",flags)??throw new MissingFieldException("MonoBehaviour.awoken lifecycle state is unavailable");
    var startedField=typeof(UnityEngine.MonoBehaviour).GetField("started",flags)??throw new MissingFieldException("MonoBehaviour.started lifecycle state is unavailable");
    void SetProperty(object target,string name,object value)=>(target.GetType().GetProperty(name,flags)??throw new MissingMemberException(target.GetType().FullName,name)).SetValue(target,value);
    // CharacterBody.Awake reads the real Global.Context.Mission, while the
    // isolated prefab gate intentionally does not load a mission scene. Supply
    // only those runtime dependencies; keep Mission disabled so its full scene
    // startup is not implied by this actor-lifecycle check.
    var contextObject=new UnityEngine.GameObject("Operator lifecycle context",world);contextObject.SetActive(false);
    var context=contextObject.AddComponent<Crisis.Runtime.FullSource.GlobalContext>();
    var settings=contextObject.AddComponent<Crisis.Runtime.FullSource.Settings>();
    typeof(Crisis.Runtime.FullSource.GlobalContext).GetProperty(nameof(Crisis.Runtime.FullSource.GlobalContext.Settings),System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)!.SetValue(context,settings);
    Crisis.Runtime.FullSource.Global.Instance.Initialize(contextObject);
    var missionObject=new UnityEngine.GameObject(Crisis.Runtime.FullSource.Mission.ObjectName,world);missionObject.SetActive(false);
    var mission=missionObject.AddComponent<Crisis.Runtime.FullSource.Mission>();mission.enabled=false;awakeField.SetValue(mission,true);missionObject.SetActive(true);
    var effectsContainer=new UnityEngine.GameObject("Operator lifecycle effects fixture",world);
    SetProperty(mission,nameof(Crisis.Runtime.FullSource.Mission.EffectsContainer),effectsContainer.transform);
    UnityEngine.Shader.Resolver??=(name=>new UnityEngine.Shader{name=name,isSupported=false});
    Crisis.Runtime.FullSource.MissionHelper.Instance.Initialize(mission);
    var operators=missionObject.AddComponent<Crisis.Runtime.FullSource.Operators>();operators.enabled=false;awakeField.SetValue(operators,true);
    var teamObject=new UnityEngine.GameObject("Operator lifecycle team",world);teamObject.SetActive(false);
    var mainGroup=teamObject.AddComponent<Crisis.Runtime.FullSource.TeamController>();mainGroup.enabled=false;awakeField.SetValue(mainGroup,true);startedField.SetValue(mainGroup,true);
    SetProperty(operators,nameof(Crisis.Runtime.FullSource.Operators.MainGroup),mainGroup);
    SetProperty(mission,nameof(Crisis.Runtime.FullSource.Mission.Operators),operators);
    var uiObject=new UnityEngine.GameObject("Operator lifecycle mission UI",world);uiObject.SetActive(false);
    var missionUI=uiObject.AddComponent<Crisis.Runtime.FullSource.MissionUI>();missionUI.enabled=false;awakeField.SetValue(missionUI,true);startedField.SetValue(missionUI,true);
    var hitAnnouncementsObject=new UnityEngine.GameObject("Operator lifecycle hit announcements",world);hitAnnouncementsObject.SetActive(false);
    var hitAnnouncements=hitAnnouncementsObject.AddComponent<Crisis.Runtime.FullSource.UIHitAnnouncements>();hitAnnouncements.enabled=false;awakeField.SetValue(hitAnnouncements,true);startedField.SetValue(hitAnnouncements,true);
    missionUI.hitAnnouncements=hitAnnouncements;
    var resourceCounterObject=new UnityEngine.GameObject("Operator lifecycle resource counter",world);resourceCounterObject.SetActive(false);
    missionUI.resourceCounter=resourceCounterObject.AddComponent<Crisis.Runtime.FullSource.UIResourceCounter>();
    var resourceEntryObject=new UnityEngine.GameObject("Operator lifecycle resource entry fixture",world);resourceEntryObject.SetActive(false);
    var resourceEntry=resourceEntryObject.AddComponent<Crisis.Runtime.FullSource.UIResourceCounterEntry>();
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

sealed class MissionContractSilentAudioDriver:IAudioClipPlaybackDriver {
    public bool IsPlaying=>false;
    public void PlayOneShot(AudioClip clip,float volume,int priority){}
    public void Play(AudioClip clip,float volume,float pitch,bool loop,bool muted,int priority){}
    public void Stop(){}
}
sealed class MissionContractSilentMixerDriver:UnityEngine.Audio.IAudioMixerDriver {
    public bool SetFloat(string exposedParameter,float value)=>true;
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
