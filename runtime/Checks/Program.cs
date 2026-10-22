using Crisis.Runtime;
using UnityEngine;
using System.Text.Json;

if(args.Length==2&&args[0]=="--builtin-resource-contract"){
    using var builtinPackage=JsonDocument.Parse(File.ReadAllText(args[1]));
    Console.WriteLine(RuntimeContracts.RunBuiltinResource(builtinPackage.RootElement));
    return;
}
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
    var physiologies=new System.Collections.Generic.List<Crisis.Runtime.FullSource.Physiology>();
    var controllers=new System.Collections.Generic.List<Crisis.Runtime.FullSource.IKArmController>();
    var rightSolvers=new System.Collections.Generic.List<Crisis.Runtime.FullSource.ArmIKSolver>();
    var leftSolvers=new System.Collections.Generic.List<Crisis.Runtime.FullSource.ArmIKSolver>();
    var equipmentRefs=new System.Collections.Generic.List<Crisis.Runtime.FullSource.Equipment>();
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
        var fullBody=(Crisis.Runtime.FullSource.CharacterBody)actorBodies[0];
        var physiology=runLifecycle?fullBody.Physiology:null;
        var controller=runLifecycle?fullBody.armController:null;
        var rightSolver=runLifecycle?fullBody.rightArm:null;
        var leftSolver=runLifecycle?fullBody.leftArm:null;
        var equipment=runLifecycle?fullBody.Equipment:null;
        if(runLifecycle){
            if(physiology==null||controller==null||rightSolver==null||leftSolver==null)
                throw new InvalidOperationException($"{actor.Item1} Awake did not bind its original physiology, IK controller, and both arm solvers");
            Crisis.Runtime.FullSource.OriginalActorIdentity.RequirePhysiology(fullBody,physiology);
            Crisis.Runtime.FullSource.OriginalActorIdentity.RequireController(fullBody,controller);
            if(!behaviours.Any(component=>ReferenceEquals(component,physiology))||
               !behaviours.Any(component=>ReferenceEquals(component,controller))||
               !behaviours.Any(component=>ReferenceEquals(component,rightSolver))||
               !behaviours.Any(component=>ReferenceEquals(component,leftSolver)))
                throw new InvalidOperationException($"{actor.Item1} original CharacterBody references components outside its cloned prefab graph");
            if(behaviours.OfType<Crisis.Runtime.FullSource.Physiology>().Count()!=1||
               behaviours.OfType<Crisis.Runtime.FullSource.IKArmController>().Count()!=1||
               behaviours.OfType<Crisis.Runtime.FullSource.ArmIKSolver>().Count()!=2)
                throw new InvalidOperationException($"{actor.Item1} cloned prefab has duplicate or missing original physiology/controller/solver components");
            if(fullBody.limbs==null||fullBody.limbMap==null||fullBody.limbs.Count!=fullBody.limbMap.Count||fullBody.limbs.Count<19)
                throw new InvalidOperationException($"{actor.Item1} original CharacterBody limb registry is incomplete");
            foreach(var limb in fullBody.limbs){
                Crisis.Runtime.FullSource.OriginalActorIdentity.RequireLimb(fullBody,limb);
                if(!behaviours.Any(component=>ReferenceEquals(component,limb))||
                   limb.LimbPhysiology==null||!ReferenceEquals(limb.GetComponent<Crisis.Runtime.FullSource.LimbPhysiology>(),limb.LimbPhysiology)||
                   limb.LimbMuscle==null||!ReferenceEquals(limb.GetComponent<Crisis.Runtime.FullSource.LimbMuscle>(),limb.LimbMuscle))
                    throw new InvalidOperationException($"{actor.Item1} limb {limb.id} is not bound to its actor-owned physiology and muscle components");
            }
            if(equipment!=null){
                Crisis.Runtime.FullSource.OriginalActorIdentity.RequireEquipment(fullBody,equipment);
                if(!behaviours.Any(component=>ReferenceEquals(component,equipment)))
                    throw new InvalidOperationException($"{actor.Item1} CharacterBody equipment is not owned by its own prefab graph");
            }
        }
        physiologies.Add(physiology);controllers.Add(controller);rightSolvers.Add(rightSolver);leftSolvers.Add(leftSolver);equipmentRefs.Add(equipment);
            var originalBody=actor.Item2.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true).Single(component=>component.GetType().Name=="CharacterBody");
            var curveField=originalBody.GetType().GetField("hitRecoveryCurve",flags)??throw new MissingFieldException(originalBody.GetType().FullName,"hitRecoveryCurve");
            var authoredCurve=curveField.GetValue(originalBody) as UnityEngine.AnimationCurve??throw new InvalidOperationException($"{actor.Item1} source prefab has no authored hitRecoveryCurve");
            var clonedCurve=curveField.GetValue(actorBodies[0]) as UnityEngine.AnimationCurve??throw new InvalidOperationException($"{actor.Item1} clone lost hitRecoveryCurve");
            var authoredKeys=authoredCurve.keys;var clonedKeys=clonedCurve.keys;
            if(ReferenceEquals(authoredCurve,clonedCurve)||authoredCurve.preWrapMode!=clonedCurve.preWrapMode||authoredCurve.postWrapMode!=clonedCurve.postWrapMode||authoredCurve.clampOutside!=clonedCurve.clampOutside||authoredCurve.serializedPreInfinity!=clonedCurve.serializedPreInfinity||authoredCurve.serializedPostInfinity!=clonedCurve.serializedPostInfinity||authoredCurve.serializedVersion!=clonedCurve.serializedVersion||authoredCurve.serializedRotationOrder!=clonedCurve.serializedRotationOrder||authoredKeys.Length!=clonedKeys.Length)throw new InvalidOperationException($"{actor.Item1} clone changed authored hitRecoveryCurve metadata");
            for(var keyIndex=0;keyIndex<authoredKeys.Length;keyIndex++){var sourceKey=authoredKeys[keyIndex];var cloneKey=clonedKeys[keyIndex];if(!sourceKey.time.Equals(cloneKey.time)||!sourceKey.value.Equals(cloneKey.value)||!sourceKey.inTangent.Equals(cloneKey.inTangent)||!sourceKey.outTangent.Equals(cloneKey.outTangent)||sourceKey.tangentMode!=cloneKey.tangentMode||sourceKey.serializedVersion!=cloneKey.serializedVersion)throw new InvalidOperationException($"{actor.Item1} clone changed authored hitRecoveryCurve key {keyIndex}");}
            curves.Add(clonedCurve);
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{mode=runLifecycle?"lifecycle":"instantiation",prefab=actor.Item1,instantiatedFromOriginalPrefab=true,activeBehaviours=active.Length,awake=awoken,onEnable=enabled,start=started,characterBody=actorBodies.Length,originalIKController=controller!=null,originalPhysiology=physiology!=null,initializedEquipment=equipment!=null,hitRecoveryCurveKeys=clonedCurve.keys.Length}));
    }
    if(ReferenceEquals(bodies[0],bodies[1])||ReferenceEquals(bodies[0].gameObject,bodies[1].gameObject)||ReferenceEquals(curves[0],curves[1]))throw new InvalidOperationException("Operator and Occupant share a CharacterBody or authored AnimationCurve instance");
    if(runLifecycle&&(ReferenceEquals(physiologies[0],physiologies[1])||ReferenceEquals(controllers[0],controllers[1])||ReferenceEquals(rightSolvers[0],rightSolvers[1])||ReferenceEquals(leftSolvers[0],leftSolvers[1])||(equipmentRefs[0]!=null&&ReferenceEquals(equipmentRefs[0],equipmentRefs[1]))))
        throw new InvalidOperationException("Operator and Occupant share physiology, IK controller, arm solvers, or initialized equipment");
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
    Console.WriteLine(ResourcePrefabContracts.Run(package));
}
