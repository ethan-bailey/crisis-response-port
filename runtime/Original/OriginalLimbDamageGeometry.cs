using UnityEngine;

namespace Crisis.Runtime.Character;

// Exact GL triangle-fan geometry from recovered original LimbRenderer.RenderPoint.
// Source caller supplies the original texture-space point and pixels-per-meter
// read from the actual source skin/collider. Random is sampled once per vertex,
// preserving gameplay RNG order. Rendering/blending must be bound separately.
public static class OriginalLimbDamageGeometry {
 public readonly record struct SourceVertex(float X,float Y,float Z);
 public static SourceVertex[] RenderPoint(Vector3 localPoint,float radiusMillimeters,
     float irregularity,int vertices,Vector2 pixelsPerMeter){
  if(vertices<=0||!float.IsFinite(radiusMillimeters)||radiusMillimeters<0f||
     !float.IsFinite(irregularity)||!float.IsFinite(pixelsPerMeter.x)||!float.IsFinite(pixelsPerMeter.y))
   throw new ArgumentOutOfRangeException(nameof(vertices));
  float x=localPoint.x,y=localPoint.y;
  float num=2f*MathF.PI;
  float num2=num/vertices;
  float num3=0f;
  float num4=radiusMillimeters*.001f;
  var triangles=new SourceVertex[vertices*3];
  for(int i=0;i<vertices;i++){
   float num5=num4-num4*irregularity*UnityEngine.Random.value;
   triangles[i*3]=new SourceVertex(
     x+num5*pixelsPerMeter.x*Mathf.Sin(num3),
     y+num5*pixelsPerMeter.y*Mathf.Cos(num3),0f);
   triangles[i*3+1]=new SourceVertex(
     x+num5*pixelsPerMeter.x*Mathf.Sin(num3+num2),
     y+num5*pixelsPerMeter.y*Mathf.Cos(num3+num2),0f);
   triangles[i*3+2]=new SourceVertex(x,y,0f);
   num3+=num2;
  }
  return triangles;
 }
 public static Vector3 WorldToTextureSpaceInvertY(Transform originalVisual,
     Vector3 worldPosition,int width,int height){
  if(originalVisual==null||width<=0||height<=0)
   throw new ArgumentException("Original skin renderer transform/dimensions missing");
  var local=originalVisual.InverseTransformPoint(worldPosition);
  return new Vector3((local.x+.5f)*width,(-local.y+.5f)*height,0f);
 }
}
