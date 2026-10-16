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
    Console.WriteLine("{\"CharacterBodyDependencyPrimitives\":\"pass\"}");
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
