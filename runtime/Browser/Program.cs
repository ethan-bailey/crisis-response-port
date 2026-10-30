using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Crisis.Runtime;
using UnityEngine;
using System.Text.Json;
using Crisis.Runtime.Browser;

var builder=WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(_=>new HttpClient{BaseAddress=new Uri(builder.HostEnvironment.BaseAddress)});
await builder.Build().RunAsync();

public static class RuntimeBridge {
    [JSInvokable] public static string ComposeOriginalArmSequences(string bundle,string namesJson)=>OriginalArmSequences.Compose(bundle,namesJson);
    [JSInvokable] public static string VerifyOriginalHandModes()=>OriginalHandModes.Run();
    static OriginalInputHost input;
    static Crisis.Runtime.Weapons.WeaponHost weapons;
    [JSInvokable] public static string InitializeWeapons(string json){input??=new OriginalInputHost(new RuntimeWorld());weapons=new(json,input.handler,presentation.QueryWorld){queryWorld=presentation.QueryWorld,ballisticMask=presentation.BallisticMask};return "ready";}
    [JSInvokable] public static string InitializeBallistics(string json){weapons.InitializeBallistics(json,presentation.QueryScene);return "ready";}
    [JSInvokable] public static string StepWeapons(string json)=>JsonSerializer.Serialize(weapons.Step(json));
    [JSInvokable] public static string OriginalWindowVisuals()=>JsonSerializer.Serialize(weapons?.Ballistics?.WindowVisuals()??throw new InvalidOperationException("Original windows not initialized"));
    static SceneLoadResult scene;
    static Crisis.Runtime.Presentation.PresentationHost presentation;
    static Crisis.Runtime.FullSourceActorBridge fullSourceActor;
    static OriginalSourcePackage originalSourcePackage;
    static RuntimeWorld incrementalSourceWorld;
    [JSInvokable] public static string InitializePresentation(string json){presentation=new(json);return "ready";}
    [JSInvokable] public static string SolvePresentationArms(string json)=>Crisis.Runtime.Presentation.PresentationHost.SolveArms(json);
    [JSInvokable] public static string StepPresentation(string json){if(weapons!=null)presentation.SetWeaponState(weapons.Kick,weapons.Climb);return presentation.Step(json);}
    [JSInvokable] public static string Verify()=>RuntimeContracts.Run();
    [JSInvokable] public static string ResetInput(){input=new OriginalInputHost(new RuntimeWorld());return JsonSerializer.Serialize(input.Snapshot());}
    [JSInvokable] public static string Tick(float seconds,string states){
        input??=new OriginalInputHost(new RuntimeWorld());
        var held=JsonSerializer.Deserialize<Dictionary<string,bool>>(states);
        foreach(var pair in held)input.SetAction(pair.Key,pair.Value);
        input.Tick(seconds);return JsonSerializer.Serialize(input.Snapshot());
    }
    [JSInvokable] public static string LoadScene(string package,string path){scene=SceneLoader.Load(package,path);return JsonSerializer.Serialize(scene.Report());}
    [JSInvokable] public static string InitializeIncrementalSourcePackage(string metadataJson){
        originalSourcePackage=OriginalSourcePackage.CreateIncremental(metadataJson);
        incrementalSourceWorld=new RuntimeWorld();
        incrementalSourceWorld.ParticleSystemDriver=new Crisis.Runtime.BrowserParticleEventQueue();
        Resources.Configure(originalSourcePackage,incrementalSourceWorld);
        return JsonSerializer.Serialize(new{expectedAssets=originalSourcePackage.ExpectedAssetCount,receivedAssets=originalSourcePackage.AssetCount,receivedBytes=originalSourcePackage.DeliveredBytes,receivedCompressedBytes=originalSourcePackage.DeliveredCompressedBytes});
    }
    [JSInvokable] public static string AddIncrementalSourceAsset(string assetPath,string assetJson){
        var source=originalSourcePackage??throw new InvalidOperationException("Incremental source package is not initialized");
        bool added=source.AddVerifiedAsset(assetPath,assetJson);
        return JsonSerializer.Serialize(new{added,expectedAssets=source.ExpectedAssetCount,receivedAssets=source.AssetCount,receivedBytes=source.DeliveredBytes,receivedCompressedBytes=source.DeliveredCompressedBytes});
    }
    [JSInvokable] public static string AddIncrementalSourceGzipAsset(string assetPath,byte[] compressedAsset){
        var source=originalSourcePackage??throw new InvalidOperationException("Incremental source package is not initialized");
        bool added=source.AddVerifiedGzipAsset(assetPath,compressedAsset);
        return JsonSerializer.Serialize(new{added,expectedAssets=source.ExpectedAssetCount,receivedAssets=source.AssetCount,receivedBytes=source.DeliveredBytes,receivedCompressedBytes=source.DeliveredCompressedBytes});
    }
    [JSInvokable] public static string AddIncrementalSourceGzipAssetBatch(string[] assetPaths,int[] compressedLengths,byte[] compressedBatch){
        var source=originalSourcePackage??throw new InvalidOperationException("Incremental source package is not initialized");
        if(assetPaths is null||compressedLengths is null||compressedBatch is null||assetPaths.Length==0||
            assetPaths.Length!=compressedLengths.Length||assetPaths.Length>16)
            throw new InvalidDataException("Incremental source batch has invalid paths, lengths, or payload");
        long batchBytes=0;
        foreach(int length in compressedLengths){
            if(length<=0)throw new InvalidDataException("Incremental source batch contains an empty asset");
            batchBytes=checked(batchBytes+length);
            if(batchBytes>32L*1024*1024)throw new InvalidDataException("Incremental source batch exceeds the 32 MiB transport limit");
        }
        if(batchBytes!=compressedBatch.LongLength)
            throw new InvalidDataException("Incremental source batch lengths do not match its payload");
        var results=new object[assetPaths.Length];
        int payloadOffset=0;
        for(int i=0;i<assetPaths.Length;i++){
            int length=compressedLengths[i];
            bool added=source.AddVerifiedGzipAsset(assetPaths[i],compressedBatch.AsSpan(payloadOffset,length).ToArray());
            payloadOffset+=length;
            results[i]=new{added,expectedAssets=source.ExpectedAssetCount,receivedAssets=source.AssetCount,
                receivedBytes=source.DeliveredBytes,receivedCompressedBytes=source.DeliveredCompressedBytes};
        }
        return JsonSerializer.Serialize(new{items=results});
    }
    [JSInvokable] public static string GetIncrementalSourceMemorySnapshot(){
        var source=originalSourcePackage??throw new InvalidOperationException("Incremental source package is not initialized");
        var gc=GC.GetGCMemoryInfo();
        return JsonSerializer.Serialize(new{managedBytes=GC.GetTotalMemory(false),managedHeapBytes=gc.HeapSizeBytes,totalAvailableBytes=gc.TotalAvailableMemoryBytes,receivedAssets=source.AssetCount,expectedAssets=source.ExpectedAssetCount,sourceBytes=source.DeliveredBytes,compressedBytes=source.DeliveredCompressedBytes});
    }
    [JSInvokable] public static string LoadIncrementalSourceScene(string path){
        var source=originalSourcePackage??throw new InvalidOperationException("Incremental source package is not initialized");
        scene=SceneLoader.Load(source,path,incrementalSourceWorld??throw new InvalidOperationException("Incremental source world is not initialized"));
        return JsonSerializer.Serialize(scene.Report());
    }
    [JSInvokable] public static string InitializeGlobalContext(){if(scene is null)throw new InvalidOperationException("Cannot initialize Global.Context before a scene is loaded");var context=scene.world.Objects.OfType<Crisis.Runtime.FullSource.GlobalContext>().FirstOrDefault();if(context is null)throw new InvalidOperationException("GlobalContext component is absent from the loaded original scene");Crisis.Runtime.FullSource.Global.Instance.Initialize(context.gameObject);return JsonSerializer.Serialize(new{initialized=true});}
    [JSInvokable] public static string InitializeFullSourceActorBridge(string actorName){
        if(scene is null)throw new InvalidOperationException("Cannot bind a FullSource actor before its original world is loaded");
        scene.world.MakeCurrent();
        var actors=scene.world.Objects.OfType<Crisis.Runtime.FullSource.CharacterBody>().Where(body=>body&&body.name==actorName).ToArray();
        if(actors.Length!=1)throw new InvalidOperationException($"Expected one loaded FullSource actor named '{actorName}', found {actors.Length}");
        fullSourceActor=new(scene.world,actors[0]);
        return JsonSerializer.Serialize(fullSourceActor.Snapshot());
    }
    [JSInvokable] public static string InitializeIncrementalEntryTeamMission(){
        var source=originalSourcePackage??throw new InvalidOperationException("Incremental source package is not initialized");
        var world=incrementalSourceWorld??throw new InvalidOperationException("Incremental source world is not initialized");
        var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(source,world,enablePlayerBehavior:true);
        scene=session.Mission;
        fullSourceActor=new(scene.world,session.Actor);
        return JsonSerializer.Serialize(new{mission=session.Mission.scene,actor=session.Actor.name,sourceActors=session.SourceActorCount,snapshot=fullSourceActor.Snapshot()});
    }
    [JSInvokable] public static string GetMissionAudioSourcePaths(){
        var world=incrementalSourceWorld??throw new InvalidOperationException("Incremental source world is not initialized");
        return JsonSerializer.Serialize(world.Objects.OfType<AudioClip>().Select(clip=>clip.OriginalAssetPath)
            .Where(path=>!string.IsNullOrWhiteSpace(path)&&path.StartsWith("Assets/AudioClip/",StringComparison.Ordinal)&&
                (path.EndsWith(".wav",StringComparison.OrdinalIgnoreCase)||path.EndsWith(".ogg",StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
    }
    [JSInvokable] public static string GetOriginalAudioAssetData(string assetPath){
        if(string.IsNullOrWhiteSpace(assetPath)||!assetPath.StartsWith("Assets/AudioClip/",StringComparison.Ordinal)||
            !(assetPath.EndsWith(".wav",StringComparison.OrdinalIgnoreCase)||assetPath.EndsWith(".ogg",StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("Requested source audio path is outside the selected AudioClip WAV/OGG closure");
        var source=originalSourcePackage??throw new InvalidOperationException("Incremental source package is not initialized");
        if(!source.TryGetAsset(assetPath,out var asset))throw new KeyNotFoundException($"Verified source audio asset is not present: {assetPath}");
        try{
            if(!asset.TryGetProperty("dataBase64",out var data)||data.ValueKind!=JsonValueKind.String||
                !asset.TryGetProperty("sha256",out var hash)||hash.ValueKind!=JsonValueKind.String)
                throw new InvalidDataException($"Verified source audio asset lacks binary data or source hash: {assetPath}");
            var bytes=Convert.FromBase64String(data.GetString());
            var actual=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
            if(!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.ASCII.GetBytes(actual),System.Text.Encoding.ASCII.GetBytes(hash.GetString())))
                throw new InvalidDataException($"Source audio binary hash does not match package record: {assetPath}");
            return JsonSerializer.Serialize(new{assetPath,dataBase64=data.GetString(),sourceSha256=actual});
        }finally{source.ReleaseParsedAsset(assetPath);}
    }
    [JSInvokable] public static string SetOriginalAudioLoadState(string assetPath,int state){
        if(!Enum.IsDefined(typeof(AudioDataLoadState),state))throw new ArgumentOutOfRangeException(nameof(state));
        var world=incrementalSourceWorld??throw new InvalidOperationException("Incremental source world is not initialized");
        var clips=world.Objects.OfType<AudioClip>().Where(clip=>clip.OriginalAssetPath==assetPath).ToArray();
        foreach(var clip in clips){clip.ReportLoadState((AudioDataLoadState)state);if((AudioDataLoadState)state==AudioDataLoadState.Loaded)clip.ReleaseSourceData();}
        return JsonSerializer.Serialize(new{assetPath,state=(AudioDataLoadState)state,instances=clips.Length});
    }
    [JSInvokable] public static string SetBrowserAudioSourcePlaying(int sourceId,bool isPlaying){
        var world=incrementalSourceWorld??throw new InvalidOperationException("Incremental source world is not initialized");
        if(world.AudioSourceDriver is not Crisis.Runtime.BrowserAudioEventQueue audioEvents)
            throw new InvalidOperationException("The current source world has no browser audio event driver");
        audioEvents.ReportPlaybackState(sourceId,isPlaying);
        return JsonSerializer.Serialize(new{sourceId,isPlaying});
    }
    [JSInvokable] public static string SetFullSourceActorAimTarget(float x,float y,float z){
        var point=(fullSourceActor??throw new InvalidOperationException("FullSource actor bridge is not initialized")).SetAimWorldPoint(x,y,z);
        return JsonSerializer.Serialize(new{x=point.x,y=point.y,z=point.z});
    }
    [JSInvokable] public static string SetFullSourceCameraFraming(float fieldOfView,float aspect)=>
        JsonSerializer.Serialize((fullSourceActor??throw new InvalidOperationException("FullSource actor bridge is not initialized")).ConfigureCameraFraming(fieldOfView,aspect));
    [JSInvokable] public static string GetFullSourceCombatSnapshot()=>JsonSerializer.Serialize((fullSourceActor??throw new InvalidOperationException("FullSource actor bridge is not initialized")).CombatSnapshot());
    [JSInvokable] public static string TickFullSourceActorBridge(float elapsedSeconds,string actionsJson,float mouseX,float mouseY,float movementMagnitude)=>JsonSerializer.Serialize((fullSourceActor??throw new InvalidOperationException("FullSource actor bridge is not initialized")).Step(elapsedSeconds,actionsJson,mouseX,mouseY,movementMagnitude));
    [JSInvokable] public static string SynchronizeFullSourceActorPhysics(float x,float y,float z,float velocityX,float velocityY,float velocityZ)=>JsonSerializer.Serialize((fullSourceActor??throw new InvalidOperationException("FullSource actor bridge is not initialized")).SynchronizePhysics(x,y,z,velocityX,velocityY,velocityZ));
    [JSInvokable] public static string SynchronizeFullSourceCasingPhysics(string statesJson)=>JsonSerializer.Serialize((fullSourceActor??throw new InvalidOperationException("FullSource actor bridge is not initialized")).SynchronizeCasingPhysics(statesJson));
    [JSInvokable] public static string VerifyAmmunition(string package,string path)=>JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalPrefab(package,path));
    [JSInvokable] public static string VerifyResource(string package,string name)=>JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalResource(package,name));
    [JSInvokable] public static string VerifySceneRay()=>scene==null?throw new InvalidOperationException("No scene loaded"):JsonSerializer.Serialize(SceneLoader.VerifyImportedRay(scene));
    [JSInvokable] public static string Raycast(float ox,float oy,float oz,float dx,float dy,float dz,float distance,int mask){
        if(scene==null)throw new InvalidOperationException("No original scene loaded");
        scene.world.MakeCurrent();
        bool found=Physics.Raycast(new Ray(new(ox,oy,oz),new(dx,dy,dz)),out var hit,distance,mask);
        return JsonSerializer.Serialize(new{found,distance=found?hit.distance:0,name=found?hit.collider.name:null,point=found?new[]{hit.point.x,hit.point.y,hit.point.z}:null});
    }
}
