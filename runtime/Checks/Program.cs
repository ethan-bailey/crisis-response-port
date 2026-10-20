using Crisis.Runtime;
using UnityEngine;
using System.Text.Json;

var runLifecycle=args.Contains("--prefab-lifecycle");if(runLifecycle||args.Contains("--prefab-instantiate")){
    if(args.Length!=2)throw new ArgumentException("--prefab-lifecycle and --prefab-instantiate require the generated scene-package.json path");
    var package=File.ReadAllText(args[1]);
    var world=new RuntimeWorld();
    var actors=new System.Collections.Generic.List<(string,UnityEngine.GameObject,UnityEngine.GameObject)>();
    foreach(var name in new[]{"Operator","Occupant"}){
        var path=$"Assets/Resources/characters/{name}.prefab";
        var loaded=SceneLoader.LoadPrefab(package,path,world,requireComplete:true);
        var instance=UnityEngine.Object.Instantiate(loaded.prefabRoot);
        loaded.prefabRoot.SetActive(false);
        actors.Add((name,loaded.prefabRoot,instance));
    }
    var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
    var awakeField=typeof(UnityEngine.MonoBehaviour).GetField("awoken",flags)??throw new MissingFieldException("MonoBehaviour.awoken lifecycle state is unavailable");
    var enabledField=typeof(UnityEngine.MonoBehaviour).GetField("enableNotified",flags)??throw new MissingFieldException("MonoBehaviour.enableNotified lifecycle state is unavailable");
    var startField=typeof(UnityEngine.MonoBehaviour).GetField("started",flags)??throw new MissingFieldException("MonoBehaviour.started lifecycle state is unavailable");
    if(runLifecycle)world.Tick(1f/60f);
    if(runLifecycle)world.Tick(1f/60f);
    var bodies=new System.Collections.Generic.List<UnityEngine.MonoBehaviour>();var curves=new System.Collections.Generic.List<UnityEngine.AnimationCurve>();
    foreach(var actor in actors){
        var behaviours=actor.Item3.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true);
        var active=behaviours.Where(component=>component.isActiveAndEnabled).ToArray();
        var awoken=active.Count(component=>(bool)awakeField.GetValue(component)!);
        var enabled=active.Count(component=>(bool)enabledField.GetValue(component)!);
        var started=active.Count(component=>(bool)startField.GetValue(component)!);
        var actorBodies=behaviours.Where(component=>component.GetType().Name=="CharacterBody").ToArray();
        if(!actor.Item3.activeInHierarchy||actorBodies.Length!=1||active.Length==0||(runLifecycle&&(awoken!=active.Length||enabled!=active.Length||started!=active.Length))||(!runLifecycle&&(awoken!=0||enabled!=0||started!=0)))
            throw new InvalidOperationException($"{actor.Item1} actual prefab lifecycle failed: active={actor.Item3.activeInHierarchy}, bodies={actorBodies.Length}, behaviours={active.Length}, Awake={awoken}, OnEnable={enabled}, Start={started}");
        bodies.Add(actorBodies[0]);
            var originalBody=actor.Item2.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true).Single(component=>component.GetType().Name=="CharacterBody");
            var curveField=originalBody.GetType().GetField("hitRecoveryCurve",flags)??throw new MissingFieldException(originalBody.GetType().FullName,"hitRecoveryCurve");
            var authoredCurve=curveField.GetValue(originalBody) as UnityEngine.AnimationCurve??throw new InvalidOperationException($"{actor.Item1} source prefab has no authored hitRecoveryCurve");
            var clonedCurve=curveField.GetValue(actorBodies[0]) as UnityEngine.AnimationCurve??throw new InvalidOperationException($"{actor.Item1} clone lost hitRecoveryCurve");
            var authoredKeys=authoredCurve.keys;var clonedKeys=clonedCurve.keys;
            if(ReferenceEquals(authoredCurve,clonedCurve)||authoredCurve.preWrapMode!=clonedCurve.preWrapMode||authoredCurve.postWrapMode!=clonedCurve.postWrapMode||authoredCurve.clampOutside!=clonedCurve.clampOutside||authoredCurve.serializedPreInfinity!=clonedCurve.serializedPreInfinity||authoredCurve.serializedPostInfinity!=clonedCurve.serializedPostInfinity||authoredCurve.serializedVersion!=clonedCurve.serializedVersion||authoredCurve.serializedRotationOrder!=clonedCurve.serializedRotationOrder||authoredKeys.Length!=clonedKeys.Length)throw new InvalidOperationException($"{actor.Item1} clone changed authored hitRecoveryCurve metadata");
            for(var keyIndex=0;keyIndex<authoredKeys.Length;keyIndex++){var sourceKey=authoredKeys[keyIndex];var cloneKey=clonedKeys[keyIndex];if(!sourceKey.time.Equals(cloneKey.time)||!sourceKey.value.Equals(cloneKey.value)||!sourceKey.inTangent.Equals(cloneKey.inTangent)||!sourceKey.outTangent.Equals(cloneKey.outTangent)||sourceKey.tangentMode!=cloneKey.tangentMode||sourceKey.serializedVersion!=cloneKey.serializedVersion)throw new InvalidOperationException($"{actor.Item1} clone changed authored hitRecoveryCurve key {keyIndex}");}
            curves.Add(clonedCurve);
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{mode=runLifecycle?"lifecycle":"instantiation",prefab=actor.Item1,instantiatedFromOriginalPrefab=true,activeBehaviours=active.Length,awake=awoken,onEnable=enabled,start=started,characterBody=actorBodies.Length,hitRecoveryCurveKeys=clonedCurve.keys.Length}));
    }
    if(ReferenceEquals(bodies[0],bodies[1])||ReferenceEquals(bodies[0].gameObject,bodies[1].gameObject)||ReferenceEquals(curves[0],curves[1]))throw new InvalidOperationException("Operator and Occupant share a CharacterBody or authored AnimationCurve instance");
    Console.WriteLine(runLifecycle?"Operator and Occupant lifecycle and component ownership passed in one RuntimeWorld.":"Operator and Occupant prefab instantiation and component ownership passed in one RuntimeWorld.");
    foreach(var actor in actors){UnityEngine.Object.DestroyImmediate(actor.Item3);UnityEngine.Object.DestroyImmediate(actor.Item2);}
    return;
}
if(args.Contains("--prefab-only")){
    if(args.Length!=2)throw new ArgumentException("--prefab-only requires the generated scene-package.json path and loads the preserved Operator and Occupant prefabs");
    var package=File.ReadAllText(args[1]);
    foreach(var name in new[]{"Operator","Occupant"}){
        var path=$"Assets/Resources/characters/{name}.prefab";
        var world=new RuntimeWorld();
        var loaded=SceneLoader.LoadPrefab(package,path,world,requireComplete:true);
        Console.WriteLine(JsonSerializer.Serialize(loaded.Report()));
        UnityEngine.Object.DestroyImmediate(loaded.prefabRoot);
    }
    return;
}
Console.WriteLine(RuntimeContracts.Run());
Console.WriteLine(ScenePrefabContracts.Run());
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
