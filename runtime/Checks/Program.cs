using Crisis.Runtime;
using System.Text.Json;

if (args.Contains("--character-facing")) {
{
    var w = new UnityEngine.RuntimeWorld();
    var a = new UnityEngine.GameObject("source facing A", w).AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
    var b = new UnityEngine.GameObject("source facing B", w).AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
    a.Direction=1f;
    b.Direction=-1f;
    a.transform.position=new UnityEngine.Vector3(0f,0f,0f);
    b.transform.position=new UnityEngine.Vector3(3f,0f,0f);
    a.Torso=new UnityEngine.GameObject("source torso A",w).AddComponent<Crisis.Runtime.Weapons.Limb>();
    a.Torso.transform.position=new UnityEngine.Vector3(1f,0f,0f);
    if (!a.Facing(3f) || !a.Facing(b) || !a.BodyFacingMe(b) || !a.FacingDirection(1f))
        throw new Exception("Recovered CharacterBody facing methods differ from source");
    if (a.Facing(-3f) || a.FacingDirection(-1f) || a.Facing((Crisis.Runtime.Weapons.CharacterBody)null))
        throw new Exception("Recovered CharacterBody facing rejection failed");
    if (Math.Abs(a.KinematicDistanceToPosition(4f)-3f)>1e-6f)
        throw new Exception("Recovered CharacterBody kinematic distance failed");
    Console.WriteLine("{\"CharacterBodyFacingSource\":\"pass\"}");
}

return;
}
if (args.Contains("--character-dependencies")) {
    var world=new UnityEngine.RuntimeWorld();
    var obj=new UnityEngine.GameObject("character deps",world);
    var animator=obj.AddComponent<UnityEngine.Animator>();
    animator.SetFloat("KneelRatio",.35f);
    animator.SetBool("Moving",true);
    if(Math.Abs(animator.GetFloat("KneelRatio")-.35f)>1e-6f||!animator.GetBool("Moving"))
       throw new Exception("Original animator parameter storage failed");
    var col=obj.AddComponent<UnityEngine.CapsuleCollider>();
    col.direction=0;col.radius=.5f;col.height=2f;
    world.Initialize();
    if(!col.Raycast(new UnityEngine.Ray(new UnityEngine.Vector3(-3,0,0),UnityEngine.Vector3.right),out var h,10f)
        || Math.Abs(h.distance-2f)>1e-4f)
       throw new Exception("Original axis-X capsule raycast failed");
    if(col.Raycast(new UnityEngine.Ray(new UnityEngine.Vector3(0,0,0),UnityEngine.Vector3.right),out _,10f))
       throw new Exception("Capsule raycast should ignore ray origin inside collider");
    var blood=new Crisis.Runtime.Character.OriginalPhysiologyConditions();
    blood.InitializeBloodVolume(4500f);
    blood.RemoveBlood(450f);
    if(Math.Abs(blood.BloodVolume-4050f)>1e-5f ||
       Math.Abs(blood.BloodOxygen-.9f)>1e-5f ||
       Math.Abs(blood.BloodVolumeNormalized-.9f)>1e-5f)
       throw new Exception("Original Physiology.RemoveBlood source regression");
    blood.RemoveLungsOxygen(.2f);
    if(Math.Abs(blood.LungsOxygen-.6f)>1e-5f)
       throw new Exception("Original Physiology.RemoveLungsOxygen source regression");
    var physiology=new Crisis.Runtime.Character.OriginalPhysiologyConditions();
    if(physiology.ShouldLoseFooting()||physiology.ShouldLoseConsciousness()||!physiology.ShouldRegainConsciousness())
       throw new Exception("Original healthy physiology thresholds failed");
    physiology.ShockResistance=.5f;
    physiology.AddShock(5000f);
    if(!physiology.ShouldLoseFooting()||Math.Abs(physiology.Shock-2500f)>1e-6f)
       throw new Exception("Original shock resistance or footing threshold failed");
    physiology.Concussion=.775f;
    if(!physiology.ShouldLoseConsciousness()||physiology.ShouldRegainConsciousness())
       throw new Exception("Original concussion unconsciousness threshold failed");
    physiology.Concussion=.0f;physiology.BloodOxygen=.5f;
    if(!physiology.ShouldLoseConsciousness())throw new Exception("Original blood oxygen threshold failed");
    physiology.BloodOxygen=.9f;physiology.BrainTrauma=3f;
    if(!physiology.ShouldLoseConsciousness())throw new Exception("Original brain trauma threshold failed");
    var body=obj.AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
    body.Direction=1f;
    var limb=obj.AddComponent<Crisis.Runtime.Weapons.Limb>();
    limb.Body=body;
    limb.visual=new UnityEngine.GameObject("limb visual",world).transform;
    limb.visual.localPosition=new UnityEngine.Vector3(0,0,.4f);
    limb.zLeft=-.25f;
    var attach=new UnityEngine.GameObject("attachment",world).AddComponent<Crisis.Runtime.Weapons.LimbAttachment>();
    attach.zLeft=-.15f;attach.zRight=.2f;
    limb.attachments.Add(attach);
    limb.CaptureSourceVisualDepth();
    body.Direction=-1f;
    limb.Flip();
    if(Math.Abs(limb.visual.localPosition.z+.25f)>1e-6f || Math.Abs(attach.transform.localPosition.z+.15f)>1e-6f)
       throw new Exception("Recovered original Limb.Flip left-depth source behavior failed");
    body.Direction=1f;
    limb.Flip();
    if(Math.Abs(limb.visual.localPosition.z-.4f)>1e-6f || Math.Abs(attach.transform.localPosition.z-.2f)>1e-6f)
       throw new Exception("Recovered original Limb.Flip right-depth source behavior failed");
    Console.WriteLine("{\"CharacterBodyDependencyPrimitives\":\"pass\"}");
    return;
}
if(args.Contains("--character-animation")) {
 var world=new UnityEngine.RuntimeWorld();
 var go=new UnityEngine.GameObject("Original CharacterBody animation",world);
 var body=go.AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
 var animator=go.AddComponent<UnityEngine.Animator>();
 var capsule=go.AddComponent<UnityEngine.CapsuleCollider>();
 body.InitializeSourceAnimation(animator,capsule);
 for(int i=0;i<10;i++)body.ExecuteSourceUpdateAnimation(true,true,false,2.55f,1.5f,4.2f,.6f,true,.02f,-.725f);
 if(body.KneelRatio<=.5f || Math.Abs(capsule.height-1.25f)>1e-5f
   || Math.Abs(capsule.center.x+.15f)>1e-5f || Math.Abs(capsule.center.y+.725f)>1e-5f)
   throw new Exception("Original CharacterBody kneeling collider interpolation failed");
 if(!animator.GetBool("Moving") || animator.GetBool("Backwards")
   || Math.Abs(animator.GetFloat("MoveSpeed")-.19444445f)>1e-4f
   || animator.GetFloat("CrippledRatio")!=0f)
   throw new Exception("Original CharacterBody animation parameters failed");
 body.ExecuteSourceUpdateAnimation(false,true,true,0f,1.5f,4.2f,.6f,false,.02f,-.725f);
 if(!animator.GetBool("Backwards") || Math.Abs(animator.GetFloat("CrippledRatio")-.6f)>1e-5f
    || body.KneelRatio>=1f)
   throw new Exception("Original CharacterBody backwards and civilian cripple state failed");
 Console.WriteLine("{\"CharacterBodyUpdateAnimationSource\":\"pass\"}");
 return;
}
Console.WriteLine(RuntimeContracts.Run());
Console.WriteLine(ArmSequenceContracts.Run("game/assets/original-arm-sequences.json"));
if(File.Exists("game/assets/presentation.json.gz"))Console.WriteLine(PresentationContracts.Run("game/assets/presentation.json.gz"));
if(File.Exists("game/assets/weapon-runtime.json.gz"))Console.WriteLine(WeaponContracts.Run("game/assets/weapon-runtime.json.gz"));
if(File.Exists("game/assets/ballistics.json.gz"))Console.WriteLine(BallisticContracts.Run());
if(args.Length>0){
    var package=File.ReadAllText(args[0]);
    foreach(var path in new[]{"Assets/Scenes/Marksman range.unity","Assets/Scenes/Mission 01.unity"}){
        var loaded=SceneLoader.Load(package,path);
        if(loaded.unresolvedReferences.Count>0)throw new Exception("Unresolved supported fields");
        Console.WriteLine(JsonSerializer.Serialize(loaded.Report()));
        Console.WriteLine(JsonSerializer.Serialize(SceneLoader.VerifyImportedRay(loaded)));
    }
    foreach(var path in new[]{"Assets/Resources/weapons/ammunition/5.56-JHP.prefab","Assets/Resources/weapons/ammunition/308-Win-BSP.prefab"})
        Console.WriteLine(JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalPrefab(package,path)));
    Console.WriteLine(JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalResource(package,"5.56-JHP")));
}

{
    var body = new Crisis.Runtime.Weapons.CharacterBody { Direction=1f };
    // Original CharacterBody.Move sets acceleration and reset behavior;
    // our browser boundary returns the source ForceMode.Acceleration value.
    body.Move(1f);
    var first=JsonSerializer.SerializeToElement(body.ExecuteSourceMovement(0f,.02f,1.5f,4.2f,1f,true,false,true,true,false));
    if(Math.Abs(first.GetProperty("forceX").GetSingle()-63f)>0.001f || !first.GetProperty("reset").GetBoolean())
        throw new Exception("Original full-pull sprint force regression");
    body.Move(1f);
    var partial=JsonSerializer.SerializeToElement(body.ExecuteSourceMovement(0f,.02f,1.5f,4.2f,.5f,true,false,true,true,false));
    if(Math.Abs(partial.GetProperty("forceX").GetSingle()-31.5f)>0.001f)
        throw new Exception("Owner-approved proportional source sprint force regression");
    body.Move(-1f);
    var opposite=JsonSerializer.SerializeToElement(body.ExecuteSourceMovement(0f,.02f,1.5f,4.2f,1f,true,false,true,true,false));
    if(Math.Abs(opposite.GetProperty("forceX").GetSingle()+22.5f)>0.001f)
        throw new Exception("Original backwards sprint restriction regression");
    body.Move(1f);
    var overSpeed=JsonSerializer.SerializeToElement(body.ExecuteSourceMovement(5f,.02f,1.5f,4.2f,1f,false,false,true,true,false));
    if(overSpeed.GetProperty("forceX").GetSingle()!=0f || overSpeed.GetProperty("reset").GetBoolean())
        throw new Exception("Original overspeed no-force behavior regression");
    Console.WriteLine("{\"CharacterBodySourceMovement\":\"pass\",\"AnalogEnhancement\":true}");
}
