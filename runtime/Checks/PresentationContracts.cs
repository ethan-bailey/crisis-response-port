using System.Text.Json;
using System.IO.Compression;
using Crisis.Runtime.Presentation;
using UnityEngine;
public static class PresentationContracts {
 public static string Run(string path){
  using var stream=new GZipStream(File.OpenRead(path),CompressionMode.Decompress);using var reader=new StreamReader(stream);string config=reader.ReadToEnd();
  var input=new Crisis.Runtime.OriginalInputHost(new RuntimeWorld());input.Tick(.1f);float before=Time.time;
  var p=new PresentationHost(config);if(Time.time!=before)throw new Exception("Camera import reset the input clock");
  var checks=new List<object>();
  foreach(var target in new[]{new Vector2(.5f,.2f),new Vector2(.2f,-.3f),new Vector2(2,0)}){
   bool ok=InverseKinematics2D.SolveTwoBoneAnalytic(out var a,out var b,true,.3f,.4f,target.x,target.y);
   if(!float.IsFinite(a)||!float.IsFinite(b))throw new Exception("IK produced non-finite angles");
   if(ok){float x=.3f*MathF.Cos(a)+.4f*MathF.Cos(a+b),y=.3f*MathF.Sin(a)+.4f*MathF.Sin(a+b);if(MathF.Abs(x-target.x)>1e-5f||MathF.Abs(y-target.y)>1e-5f)throw new Exception("Original IK endpoint mismatch");}
   checks.Add(new{ok,a,b});
  }
  return JsonSerializer.Serialize(new{presentation="PASS",clockPreserved=true,originalIK=checks});
 }
}
