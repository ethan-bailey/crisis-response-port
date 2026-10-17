using System.Text.Json;
using Crisis.Runtime.Character;
using UnityEngine;
public static class OriginalLimbDamageGeometryContracts {
 static void Check(bool cond,string message){if(!cond)throw new Exception(message);}
 public static string Run(){
  var world=new RuntimeWorld();var visual=new GameObject("original skin visual",world).transform;
  visual.position=new Vector3(2f,1f,0f);
  var center=OriginalLimbDamageGeometry.WorldToTextureSpaceInvertY(visual,visual.position,64,32);
  Check(center.x==32f&&center.y==16f,"Original inverted-Y texel center mismatch");
  var shape=OriginalLimbDamageGeometry.RenderPoint(center,10f,0f,10,new Vector2(64f,32f));
  Check(shape.Length==30,"Original GL.TRIANGLES count must be 3 per vertex");
  Check(MathF.Abs(shape[0].X-32f)<.00001f&&MathF.Abs(shape[0].Y-16.32f)<.00001f,
    "Original RenderPoint initial radial triangle mismatch");
  for(int i=0;i<shape.Length;i+=3)
   Check(shape[i+2].X==32f&&shape[i+2].Y==16f,"Source RenderPoint fan center must be final vertex");
  var damage=OriginalLimbDamageGeometry.RenderPoint(center,15f,.3f,30,new Vector2(64f,32f));
  Check(damage.Length==90,"Original source irregular wound 30 GL triangles");
  return JsonSerializer.Serialize(new {originalRenderPoint="PASS",
   sourceTriangleCount=30,sourceYInversion=true,liveTextureBinding=false});
 }
}
