using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using Crisis.Runtime;
using Crisis.Runtime.Weapons;
using Crisis.Runtime.Presentation;
using UnityEngine;
public static class BallisticContracts {
 static string Read(string path){using var zip=new GZipStream(File.OpenRead(path),CompressionMode.Decompress);return new StreamReader(zip).ReadToEnd();}
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 public static string Run(){
  var input=new OriginalInputHost(new RuntimeWorld());input.Tick(.016f);
  var presentation=new PresentationHost(Read("game/assets/presentation.json.gz"));
  var host=new WeaponHost(Read("game/assets/weapon-runtime.json.gz"),input.handler,presentation.QueryWorld){queryWorld=presentation.QueryWorld};
  host.InitializeBallistics(Read("game/assets/ballistics.json.gz"),presentation.QueryScene);
  var world=presentation.QueryWorld;var runtime=host.Ballistics;
  Check(runtime.loadedMeshColliders==61&&runtime.unsupportedColliders==2,"Unexpected recovered mesh coverage");
  var weapon=(WeaponBase)typeof(WeaponHost).GetField("weapon",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(host);
  Projectile Bullet(Vector3 position,Vector3 forward){world.MakeCurrent();var go=new GameObject("Contract original M4 projectile",world);var p=go.AddComponent<Projectile>();p.meshTransform=new GameObject("Contract projectile mesh",world).transform;p.meshTransform.SetParent(go.transform,false);p.transform.position=position;p.transform.rotation=Quaternion.LookRotation(forward);p.InitializeXY(weapon,false);p.FlightMask=Masks.Instance.BallisticTargets;return p;}
  BallisticMaterial Material(float width,bool stop=false){var m=new GameObject("Contract uniform material",world).AddComponent<BallisticMaterial>();m.properties=new GameObject("Contract deterministic material properties",world).AddComponent<BallisticMaterialProperties>();m.properties.density=1000;m.properties.surfaceDensity=1000;m.properties.densityRandomness=new(1,1);m.width=width;m.impenetrable=stop;m.Awake();m.Start();return m;}
  // Fixture widths isolate source energy arithmetic. Gameplay still uses only recovered assets.
  var thin=Bullet(new(0,2,0),Vector3.right);float start=thin.Energy;Material(.001f).HandleProjectile(thin,-Vector3.right);
  Check(thin.Status==BallisticResult.Continue&&thin.Energy>0&&thin.Energy<start,"Thin source material must consume energy and allow exit");
  var thick=Bullet(new(0,2,0),Vector3.right);Material(100).HandleProjectile(thick,-Vector3.right);Check(thick.Status==BallisticResult.Stop&&thick.Energy<1,"Thick source material must stop projectile");
  var stopped=Bullet(new(0,2,0),Vector3.right);Material(.001f,true).HandleProjectile(stopped,-Vector3.right);Check(stopped.Status==BallisticResult.Stop,"Original impenetrable flag");
  input.Tick(.0000001f);world.MakeCurrent();var delayed=Bullet(new(0,2,0),Vector3.right);var slowMaterial=Material(.01f);slowMaterial.properties.density=1;slowMaterial.Awake();slowMaterial.HandleProjectile(delayed,-Vector3.right);
  Check(delayed.Status==BallisticResult.Delayed&&!delayed.gameObject.activeSelf,"Original delayed exit must suspend flight");input.Tick(.02f);world.MakeCurrent();slowMaterial.Update();Check(delayed.Status==BallisticResult.Continue&&delayed.gameObject.activeSelf,"Original delayed exit must resume flight");
  var window=world.Objects.OfType<BallisticWindow>().First(w=>w.isActiveAndEnabled);window.Reset();var tex=(Texture2D)typeof(BallisticWindow).GetField("textureInstance",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(window);
  int px=-1,py=-1;for(int y=32;y<tex.height-32&&px<0;y++)for(int x=32;x<tex.width-32;x++)if(tex.GetPixel(x,y).a>.25f){px=x;py=y;break;}
  Check(px>=0,"No solid source glass pixel");var point=window.transform.TransformPoint(new((float)px/tex.width-.5f,(float)py/tex.height-.5f,0));var glass=Bullet(point,window.transform.forward);
  Check(!window.PassThroughWithoutContact(point,glass),"Original solid glass must contact");window.HandleProjectile(glass,-window.transform.forward);
  var repeat=Bullet(point,window.transform.forward);Check(window.PassThroughWithoutContact(point,repeat),"Original stamped glass hole must pass subsequent bullet");
  var fragmented=Bullet(new(0,2,0),Vector3.right);float mass=fragmented.Mass,total=0;int count=0;fragmented.OnFragmentation();fragmented.SpawnRandomFragments(fragmented.Position,(p,_)=>{count++;total+=p.Mass;Check(p.Status==BallisticResult.Continue&&p.Velocity>0&&p.Mass>0,"Invalid original fragment");});
  Check(count>0&&Mathf.Abs(total-mass)<.0001f,"Original randomized fragments must conserve source mass");
  Check((Masks.Instance.BallisticTargets&(1<<window.gameObject.layer))!=0,"Windows omitted from original projectile mask");
  input.Tick(.016f);world.MakeCurrent();int contacts=JsonSerializer.SerializeToElement(runtime.Snapshot()).GetProperty("contacts").GetInt32();
  var target=world.Objects.OfType<BoxCollider>().First(c=>c.isActiveAndEnabled&&!c.isTrigger&&c.GetComponent<BallisticMaterial>()?.properties!=null&&Mathf.Abs(c.transform.position.x)<20&&c.transform.position.y>0&&c.transform.position.y<5&&(Masks.Instance.BallisticTargets&(1<<c.gameObject.layer))!=0);
  var center=target.transform.TransformPoint(target.center);var axis=target.transform.TransformVector(Vector3.right).normalized;float extent=target.transform.TransformVector(new Vector3(target.size.x/2,0,0)).magnitude;
  Check(target.Raycast(new Ray(center+axis*(extent+.01f),-axis),out var hit,extent+.1f),"Source scene collider ray");
  var live=Bullet(hit.point+hit.normal*.005f,-hit.normal);live.StepFlight();int after=JsonSerializer.SerializeToElement(runtime.Snapshot()).GetProperty("contacts").GetInt32();Check(after>contacts,"Live flight did not invoke recovered scene material");
  return JsonSerializer.Serialize(new{ballistics="PASS",sourceMaterials=386,meshColliders=runtime.loadedMeshColliders,unresolvedBuiltins=runtime.unsupportedColliders,liveSceneContact=true,thinPenetration=true,thickStop=true,impenetrable=true,delayedExitResumes=true,glassHolePassThrough=true,fragmentCount=count,fragmentMassConserved=true,fullUnityParity=false});
 }
}
