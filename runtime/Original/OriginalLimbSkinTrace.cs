using System.Numerics;
using UnityEngine;
using V2=System.Numerics.Vector2;
using V3=UnityEngine.Vector3;

namespace Crisis.Runtime.Character;

// Exact CPU selection of recovered developer LimbRenderer.FindEntryExit,
// FindEdgePoint, Intersects, PixelTrace, and WorldPointToTextureSpace.
// The green buffer is initialized by ORIGINAL LimbTextureData.Initialize:
// staticDamage.g = skinAlpha > 0 ? 0 : 1, in Unity's bottom-left texel order.
// Native shader-driven dynamic damage and texture wrap beyond source bounds
// remain outside this boundary; never call it a full LimbRenderer port.
public sealed class OriginalLimbSkinTrace {
 public enum TraceResult { Failure=0,Entry=1,EntryExit=2 }
 enum EdgeFindMode { Closest=0,Furthest=1 }
 readonly Transform visual;
 readonly byte[] originalDamageGreen;
 public int Width{get;}
 public int Height{get;}
 public OriginalLimbSkinTrace(Transform originalVisual,int width,int height,byte[] sourceDamageGreen){
  visual=originalVisual??throw new ArgumentNullException(nameof(originalVisual));
  if(width<=0||height<=0||sourceDamageGreen==null||
      (long)width*height!=sourceDamageGreen.LongLength)
   throw new ArgumentException("Unverified source LimbTextureData dimensions");
  if(sourceDamageGreen.Any(x=>x!=0&&x!=255))
   throw new ArgumentException("Initial source staticDamage green mask must be authored alpha-derived binary");
  Width=width;Height=height;originalDamageGreen=(byte[])sourceDamageGreen.Clone();
 }
 public float InitialDamageGreen(int x,int y){
  // Unity imported texture wrap mode must be verified before enabling
  // out-of-bounds queries; do not invent clamp/repeat parity.
  if(x<0||x>=Width||y<0||y>=Height)
   throw new NotSupportedException("Original skin Texture2D.GetPixel wrap mode unresolved");
  return originalDamageGreen[y*Width+x]/255f;
 }
 bool TestPixel(int x,int y,float threshold)=>InitialDamageGreen(x,y)<=threshold;
 // Recovered original LimbRenderer.PixelTrace.
 bool PixelTrace(V2 texel,V2 direction,ref V2 point,float threshold,
                 float stepSize=1.4f,int maxTries=200){
  for(int i=0;i<maxTries;i++){
   if(TestPixel(Mathf.FloorToInt(texel.X),Mathf.FloorToInt(texel.Y),threshold)){
    point=new V2(Mathf.FloorToInt(texel.X),Mathf.FloorToInt(texel.Y));
    return true;
   }
   texel+=direction*stepSize;
  }
  return false;
 }
 V2 WorldPointToTextureSpace(V3 worldPoint){
  var v=visual.InverseTransformPoint(worldPoint)+V3.one*.5f;
  return new V2(v.x*Width,v.y*Height);
 }
 V3 TexelToWorldPoint(V2 texel)
   =>visual.TransformPoint(new V3(texel.X/Width-.5f,texel.Y/Height-.5f,0f));
 public TraceResult FindEntryExit(V3 worldOrigin,V3 worldDirection,float threshold,
                                 ref V3 worldEntry,ref V3 worldExit){
  V3 point=V3.zero,worldOrigin2=worldOrigin-worldDirection*.25f;
  if(!FindEdgePoint(worldOrigin2,worldDirection,ref point,EdgeFindMode.Closest))
   return TraceResult.Failure;
  var v=visual.InverseTransformDirection(worldDirection);
  V2 direction=new(v.x,v.y),texel=WorldPointToTextureSpace(point),point2=default;
  if(PixelTrace(texel,direction,ref point2,threshold)){
   worldEntry=TexelToWorldPoint(point2);
   if(FindEdgePoint(worldOrigin2,worldDirection,ref point,EdgeFindMode.Furthest)){
    texel=WorldPointToTextureSpace(point);
    if(PixelTrace(texel,-direction,ref point2,threshold)){
     worldExit=TexelToWorldPoint(point2);
     return TraceResult.EntryExit;
    }
   }
   return TraceResult.Entry;
  }
  return TraceResult.Failure;
 }
 public TraceResult FindMarksmanEntryAndExit(V3 worldPosition,V3 worldDirection,
                                           ref V3 worldEntry,ref V3 worldExit){
  V2 v=WorldPointToTextureSpace(worldPosition);
  if(TestPixel((int)v.X,(int)v.Y,0f)){
   worldEntry=worldPosition;
   worldExit=worldPosition+worldDirection*.2f;
   return TraceResult.EntryExit;
  }
  return TraceResult.Failure;
 }
 bool FindEdgePoint(V3 worldOrigin,V3 worldDirection,ref V3 point,EdgeFindMode mode){
  V3 vector=visual.InverseTransformPoint(worldOrigin);
  V3 vector2=visual.InverseTransformVector(worldDirection);
  V3 vector3=vector+vector2*1000f;
  V2 b=new(-.5f,.5f),vector4=new(.5f,.5f),
     vector5=new(-.5f,-.5f),b2=new(.5f,-.5f),intersection=V2.Zero;
  List<V2> list=new();
  V2 a=new(vector.x,vector.y),a2=new(vector3.x,vector3.y);
  if(Intersects(a,a2,b,vector5,ref intersection))list.Add(intersection);
  if(Intersects(a,a2,vector4,b2,ref intersection))list.Add(intersection);
  if(Intersects(a,a2,b,vector4,ref intersection))list.Add(intersection);
  if(Intersects(a,a2,vector5,b2,ref intersection))list.Add(intersection);
  float num=mode==EdgeFindMode.Closest?float.PositiveInfinity:float.NegativeInfinity;
  V2 vector6=V2.Zero,vector7=a;
  if(mode==EdgeFindMode.Closest){
   foreach(V2 item in list){
    float magnitude=(item-vector7).Length();
    if(magnitude<num){num=magnitude;vector6=item;}
   }
  } else {
   foreach(V2 item in list){
    float magnitude=(item-vector7).Length();
    if(magnitude>num){num=magnitude;vector6=item;}
   }
  }
  // The zero-vector sentinel is in the original source (even at a valid origin).
  if(vector6!=V2.Zero){
   point=visual.TransformPoint(new V3(vector6.X,vector6.Y,0f));
   return true;
  }
  return false;
 }
 static bool Intersects(V2 a1,V2 a2,V2 b1,V2 b2,ref V2 intersection){
  V2 v=a2-a1,v2=b2-b1;
  float num=v.X*v2.Y-v.Y*v2.X;
  if(num==0f)return false;
  V2 v3=b1-a1;
  float num2=(v3.X*v2.Y-v3.Y*v2.X)/num;
  if(num2<0f||num2>1f)return false;
  float num3=(v3.X*v.Y-v3.Y*v.X)/num;
  if(num3<0f||num3>1f)return false;
  intersection=a1+num2*v;
  return true;
 }
}
