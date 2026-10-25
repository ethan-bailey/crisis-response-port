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
if(args.Length==2&&args[0]=="--browser-entryteam-session-contract"){
    var package=LoadSourcePackage(args[1]);
    var world=new RuntimeWorld();
    UnityEngine.Application.persistentDataPath=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"crisis-response-browser-entryteam-"+Guid.NewGuid().ToString("N"));
    var session=Crisis.Runtime.Browser.BrowserEntryTeamMission.Start(package,world);
    var bridge=new FullSourceActorBridge(world,session.Actor);
    bridge.SetAimWorldPoint(session.Actor.transform.position.x+5f,session.Actor.transform.position.y+2f,session.Actor.transform.position.z);
    var pressed=bridge.Step(1f/60f,"{\"left\":true}",0f,0f);
    using var pressedDocument=JsonDocument.Parse(JsonSerializer.Serialize(pressed));
    var input=Crisis.Runtime.FullSource.Global.Context.InputHandler.Keys.left;
    if(!input.Pressed)throw new InvalidOperationException("Browser EntryTeam session did not deliver the original left input");
    using var releasedDocument=JsonDocument.Parse(JsonSerializer.Serialize(bridge.Step(1f/60f,"{}",0f,0f)));
    if(input.Pressed||!input.Up)throw new InvalidOperationException("Browser EntryTeam session did not deliver the original left release");
    var physics=releasedDocument.RootElement.GetProperty("physics");
    var commands=physics.GetProperty("commands");
    if(commands.GetArrayLength()!=1||commands[0].GetProperty("mode").GetString()!="Acceleration"||commands[0].GetProperty("force")[0].GetSingle()>=0f)
        throw new InvalidOperationException($"Browser EntryTeam session did not produce one original actor-owned movement force (steps={physics.GetProperty("fixedSteps")}, commands={commands.GetArrayLength()}, actor={session.Actor.name}, player={session.Actor.IsPlayer})");
    using var synchronized=JsonDocument.Parse(JsonSerializer.Serialize(bridge.SynchronizePhysics(1.25f,2.5f,1.5f,-.5f,0f,0f)));
    if(MathF.Abs(synchronized.RootElement.GetProperty("position")[0].GetSingle()-1.25f)>.0001f||MathF.Abs(synchronized.RootElement.GetProperty("velocity")[0].GetSingle()+.5f)>.0001f)
        throw new InvalidOperationException("Browser EntryTeam session did not round-trip the browser PhysX state");
    Console.WriteLine(JsonSerializer.Serialize(new{status="passed",mission=session.Mission.scene,actor=session.Actor.name,sourceActors=session.SourceActorCount,interactionMenu=session.Mission.world.Objects.OfType<Crisis.Runtime.FullSource.Mission>().Single().InteriorMode.UI.InteractionMenu!=null,input="original press/release",movementForce=commands.GetArrayLength(),physics="position/velocity round trip"}));
    return;
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
        throw new InvalidOperationException("Instantiated authored UI did not reta