using Crisis.Runtime;
using System.Text.Json;

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
