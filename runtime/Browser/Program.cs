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
    static OriginalInputHost input;
    static SceneLoadResult scene;
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
    [JSInvokable] public static string Raycast(float ox,float oy,float oz,float dx,float dy,float dz,float distance,int mask){
        if(scene==null)throw new InvalidOperationException("No original scene loaded");
        bool found=Physics.Raycast(new Ray(new(ox,oy,oz),new(dx,dy,dz)),out var hit,distance,mask);
        return JsonSerializer.Serialize(new{found,distance=found?hit.distance:0,name=found?hit.collider.name:null,point=found?new[]{hit.point.x,hit.point.y,hit.point.z}:null});
    }
}
