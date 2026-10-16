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
    [JSInvokable] public static string InitializeWeapons(string json){
        input??=new OriginalInputHost(new RuntimeWorld());
        weapons=new(json,input.handler,presentation.QueryWorld){queryWorld=presentation.QueryWorld,ballisticMask=presentation.BallisticMask};
        var character=weapons.Character;
        var animator=character.GetComponent<Animator>()??character.gameObject.AddComponent<Animator>();
        var capsule=character.GetComponent<CapsuleCollider>()??character.gameObject.AddComponent<CapsuleCollider>();
        character.InitializeSourceAnimation(animator,capsule);
        return "ready";
    }
    [JSInvokable] public static string StepCharacterAnimation(string json){
        if(weapons==null)throw new InvalidOperationException("Original character weapon host not initialized");
        using var doc=JsonDocument.Parse(json);
        var p=doc.RootElement;
        float F(string n)=>p.GetProperty(n).GetSingle();
        bool B(string n)=>p.GetProperty(n).GetBoolean();
        return JsonSerializer.Serialize(weapons.Character.ExecuteSourceUpdateAnimation(
          B("kneeling"),B("moving"),B("backwards"),F("velocity"),
          F("walkSpeed"),F("sprintSpeed"),F("crippledRatio"),B("operator"),
          F("deltaTime"),F("colliderVerticalOffset")));
    }
    [JSInvokable] public static string InitializeBallistics(string json){weapons.InitializeBallistics(json,presentation.QueryScene);return "ready";}
    [JSInvokable] public static string StepWeapons(string json)=>JsonSerializer.Serialize(weapons.Step(json));
    [JSInvokable] public static string OriginalWindowVisuals()=>JsonSerializer.Serialize(weapons?.Ballistics?.WindowVisuals()??throw new InvalidOperationException("Original windows not initialized"));
    static SceneLoadResult scene;
    static Crisis.Runtime.Presentation.PresentationHost presentation;
    [JSInvokable] public static string InitializePresentation(string json){presentation=new(json);return "ready";}
    [JSInvokable] public static string SolvePresentationArms(string json)=>Crisis.Runtime.Presentation.PresentationHost.SolveArms(json);
    [JSInvokable] public static string StepPresentation(string json){if(weapons!=null)presentation.SetWeaponState(weapons.Kick,weapons.Climb);return presentation.Step(json);}
    // Original CharacterBody.Move / FixedUpdate are executed here with the live
    // PhysX velocity; the browser applies the returned source acceleration.
    static readonly Crisis.Runtime.Weapons.CharacterBody movementSource = new();
    [JSInvokable] public static string StepBodyMovement(string json){
        using var doc=JsonDocument.Parse(json);
        var p=doc.RootElement;
        float Number(string key)=>p.GetProperty(key).GetSingle();
        bool Flag(string key)=>p.GetProperty(key).GetBoolean();
        float dir=Number("direction");
        movementSource.Direction=Number("facing");
        movementSource.Move(dir);
        var result=movementSource.ExecuteSourceMovement(Number("velocity"),Number("step"),
            Number("walkSpeed"),Number("sprintSpeed"),Number("analogMagnitude"),
            Flag("sprint"),Flag("crippled"),Flag("operator"),Flag("noPlayerLimp"),
            Flag("precision"));
        return JsonSerializer.Serialize(result);
    }
    [JSInvokable] public static string Verify()=>RuntimeContracts.Run();
    [JSInvokable] public static string ResetInput(){input=new OriginalInputHost(new RuntimeWorld());return JsonSerializer.Serialize(input.Snapshot());}
    [JSInvokable] public static string Tick(float seconds,string states){
        input??=new OriginalInputHost(new RuntimeWorld());
        var held=JsonSerializer.Deserialize<Dictionary<string,bool>>(states);
        foreach(var pair in held)input.SetAction(pair.Key,pair.Value);
        input.Tick(seconds);return JsonSerializer.Serialize(input.Snapshot());
    }
    [JSInvokable] public static string LoadScene(string package,string path){scene=SceneLoader.Load(package,path);return JsonSerializer.Serialize(scene.Report());}
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
