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
    [JSInvokable] public static string AddIncrementalSourceGzipAssetBatch(string[] assetPaths,byte[][] compressedAssets){
        var source=originalSourcePackage??throw new InvalidOperationException("Incremental source package is not initialized");
        if(assetPaths is null||compressedAssets is null||assetPaths.Length==0||assetPaths.Length!=compressedAssets.Length||assetPaths.Length>16)
            throw new InvalidDataException("Incremental source batch has invalid asset arrays");
        long batchBytes=0;
        foreach(var compressed in compressedAssets){
            if(compressed is null)throw new InvalidDataException("Incremental source batch contains a null asset");
            batchBytes=checked(batchBytes+compressed.LongLength);
            if(batchBytes>32L*1024*1024)throw new InvalidDataException("Incremental source batch exceeds the 32 MiB transport limit");
        }
        var results=new object[assetPaths.Length];
        for(int i=0;i<assetPaths.Length;i++){
            bool added=source.AddVerifiedGzipAsset(assetPaths[i],compressedAssets[i]);
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
        var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(source,world);
        scene=session.Mission;
        fullSourceActor=new(scene.world,session.Actor);
        return JsonSerializer.Serialize(new{mission=session.Mission.scene,actor=session.Actor.name,sourceActors=session.SourceActorCount,snapshot=fullSourceActor.Snapshot()});
    }
    [JSInvokable] public static string SetFullSourceActorAimTarget(float x,float y,float z)=>JsonSerializer.Serialize((fullSourceActor??throw new InvalidOperationException("FullSource actor bridge is not initialized")).SetAimWorldPoint(x,y,z));
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
