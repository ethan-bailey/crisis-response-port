using Crisis.Runtime;
using UnityEngine;
using System.Text.Json;

if(args.Length==2&&args[0]=="--builtin-resource-contract"){
    using var builtinPackage=JsonDocument.Parse(File.ReadAllText(args[1]));
    Console.WriteLine(RuntimeContracts.RunBuiltinResource(builtinPackage.RootElement));
    return;
}
if(args.Length==2&&args[0]=="--spawner-contract"){
    Console.WriteLine(SpawnerGameplayContracts.Run(File.ReadAllText(args[1])));
    return;
}
if(args.Length==2&&args[0]=="--mission-contract"){
    var package=File.ReadAllText(args[1]);
    var world=new RuntimeWorld();
    world.MakeCurrent();
    UnityEngine.Application.persistentDataPath=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"crisis-response-mission-contract-"+Guid.NewGuid().ToString("N"));
    Resources.Configure(package,world);
    var contextPrefab=SceneLoader.LoadPrefabInstance(package,"Assets/Resources/Global context.prefab",world,requireComplete:false);
    UnityEngine.QualitySettings.Bind(world,new MissionContractQualitySettingsDriver());
    UnityEngine.Shader.Resolver??=(name=>new UnityEngine.Shader{name=name,isSupported=false});
    UnityEngine.SceneManagement.SceneManager.Bind(world,new MissionContractSceneManagerDriver());
    Material.TextureDriver??=((_,_)=>{});
    world.Initialize();
    var context=Crisis.Runtime.FullSource.Global.Context??throw new InvalidOperationException("Instantiated original Global Context did not initialize before Mission 01 load");
    context.Usage.enabled=false;
    context.CommandHandler.enabled=false;
    foreach(var clip in world.Objects.OfType<AudioClip>())clip.ReportLoadState(AudioDataLoadState.Loaded);
    foreach(var source in world.Objects.OfType<AudioSource>())source.Driver=new MissionContractSilentAudioDriver();
    UnityEngine.Audio.AudioMixer.Bind(world,new MissionContractSilentMixerDriver());
    var team=new Crisis.Runtime.FullSource.SerializableTeam();
    team.marksman.enabled=false;
    var loadout=new SerializableEquipment();loadout.primary.prefabName="M4A1";loadout.primary.ammunitionPrefabName="5.56-FMJ";loadout.secondary.prefabName="Glock-18";loadout.secondary.ammunitionPrefabName="9-FMJ";
    loadout.items.itemPrefabNames.AddRange(new[]{"Primary magazine","Secondary magazine","Plasticuffs","Gauze"});
    team.entryMembers.Add(loadout);
    typeof(Crisis.Runtime.FullSource.OperatorSpawnData).GetProperty(nameof(Crisis.Runtime.FullSource.OperatorSpawnData.Team),System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)!.SetValue(context.OperatorSpawnData,team);
    context.MissionMetaData.DeactivateTutorial();
    // Keep gate 7 focused on the real mission and actor flow in this headless
    // host. The original menus instantiate a large presentation-only prefab
    // graph; suppress those roots before their Awake callbacks, as a UI driver
    // would, while preserving the mission, operators, occupants, and systems.
    var mission=SceneLoader.Load(package,"Assets/Scenes/Mission 01.unity",world,loaded=>{
        foreach(var ui in loaded.objects.Values.OfType<Crisis.Runtime.FullSource.MissionUI>())ui.gameObject.SetActive(false);
        foreach(var component in loaded.objects.Values.OfType<Crisis.Runtime.FullSource.Mission>()){
            // Keep original InteriorMode gameplay state for PlayerController,
            // but keep its original canvas initialized for weapon-pointer and
            // physiology indicator references. The headless gate supplies the
            // pointer's three image sinks below; menu and canvas callbacks are
            // presentation-only.
            if(component.interiorModePrefab!=null){
                component.interiorModePrefab.gameObject.SetActive(false);
                if(component.interiorModePrefab.uiPrefab!=null)
                    component.interiorModePrefab.uiPrefab.gameObject.SetActive(false);
            }
        }
    });
    var missionComponent=mission.objects.Values.OfType<Crisis.Runtime.FullSource.Mission>().Single();
    missionComponent.sandboxStartMode=Crisis.Runtime.FullSource.Mission.MissionView.None;
    mission.objects.Values.OfType<Crisis.Runtime.FullSource.MissionUI>().Single().missionIntro=null;
    foreach(var renderer in world.Objects.OfType<Renderer>().ToArray())renderer.material.OnTextureAssigned??=((_,_)=>{});
    // Fog of war presentation Starts before the Mission Start callback in this
    // host's record order; it is excluded from this actor lifecycle contract.
    foreach(var fog in world.Objects.OfType<Crisis.Runtime.FullSource.FogOfWarElement>())fog.enabled=false;
    // The reticle and indicators are initialized; suppress only the headless
    // pointer's camera/input polling Start callback.
    foreach(var pointer in world.Objects.OfType<Crisis.Runtime.FullSource.WeaponPointer>()){
        pointer.enabled=false;
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        void BindImage(string field,UnityEngine.Transform dot){if(dot!=null&&dot.GetComponent<UnityEngine.UI.Image>() is { } image)pointer.GetType().GetField(field,flags)!.SetValue(pointer,image);}
        BindImage("mainImage",pointer.mainDot);BindImage("startImage",pointer.startDot);BindImage("endImage",pointer.endDot);
    }
    // RuntimeWorld has no native/browser audio device. Mark preserved source
    // clips as accepted by a test adapter so original mission startup and music
    // calls execute without claiming decoded or audible output.
    foreach(var clip in world.Objects.OfType<AudioClip>())clip.ReportLoadState(AudioDataLoadState.Loaded);
    foreach(var source in world.Objects.OfType<AudioSource>())source.Driver=new MissionContractSilentAudioDriver();
    void DisableUnboundVocalizationPresentation(){foreach(var vocalization in world.Objects.OfType<Crisis.Runtime.FullSource.UIVocalizationText>().ToArray())vocalization.enabled=false;}
    world.Tick(1f/60f,DisableUnboundVocalizationPresentation);
    world.Tick(1f/60f,DisableUnboundVocalizationPresentation);
    missionComponent=world.Objects.OfType<Crisis.Runtime.FullSource.Mission>().SingleOrDefault()??throw new InvalidOperationException("Mission 01 did not initialize exactly one original Mission component");
    var bodies=world.Objects.OfType<Crisis.Runtime.FullSource.CharacterBody>().ToArray();
    if(!missionComponent.Started)throw new InvalidOperationException("Original Mission did not reach BeginMission during normal RuntimeWorld startup");
    if(bodies.Length<2||!bodies.Any(body=>body.GetComponent<Crisis.Runtime.FullSource.TeamOperator>()!=null)||!bodies.Any(body=>body.GetComponent<Crisis.Runtime.FullSource.Occupant>()!=null))
        throw new InvalidOperationException($"Mission 01 actor flow did not produce an Operator and Occupant; bodies={bodies.Length}");
    Console.WriteLine(JsonSerializer.Serialize(new{mission=mission.scene,missionStarted=missionComponent.Started,globalContext=context.gameObject.name,globalContextInstantiated=!ReferenceEquals(context.gameObject,contextPrefab.prefabRoot),sourceActors=bodies.Length,operators=bodies.Count(body=>body.GetComponent<Crisis.Runtime.FullSource.TeamOperator>()!=null),occupants=bodies.Count(body=>body.GetComponent<Crisis.Runtime.FullSource.Occupant>()!=null),sceneObjects=mission.gameObjects,originalComponents=mission.originalComponents,missionUnsupported=mission.unsupportedComponents,missionUnresolved=mission.unresolvedReferences,globalContextUnsupported=contextPrefab.unsupportedComponents,globalContextUnresolved=contextPrefab.unresolvedReferences,audio="silent test adapter; original decode/playback not verified"}));
    return;
}
var runLifecycle=args.Contains("--prefab-lifecycle");if(runLifecycle||args.Contains("--prefab-instantiate")){
    if(args.Length!=2)throw new ArgumentException("--prefab-lifecycle and --prefab-instantiate require the generated scene-package.json path");
    var package=File.ReadAllText(args[1]);
    var world=new RuntimeWorld();
    world.MakeCurrent();
    var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
    var awakeField=typeof(UnityEngine.MonoBehaviour).GetField("awoken",flags)??throw new MissingFieldException("MonoBehaviour.awoken lifecycle state is unavailable");
    var startedField=typeof(UnityEngine.MonoBehaviour).GetField("started",flags)??throw new MissingFieldException("MonoBehaviour.started lifecycle state is unavailable");
    void SetProperty(object target,string name,object value)=>(target.GetType().GetProperty(name,flags)??throw new MissingMemberException(target.GetType().FullName,name)).SetValue(target,value);
    // CharacterBody.Awake reads the real Global.Context.Mission, while the
    // isolated prefab gate intentionally does not load a mission scene. Supply
    // only those runtime dependencies; keep Mission disabled so its full scene
    // startup is not implied by this actor-lifecycle check.
    var contextObject=new UnityEngine.GameObject("Operator lifecycle context",world);contextObject.SetActive(false);
    var context=contextObject.AddComponent<Crisis.Runtime.FullSource.GlobalContext>();
    var settings=contextObject.AddComponent<Crisis.Runtime.FullSource.Settings>();
    typeof(Crisis.Runtime.FullSource.GlobalContext).GetProperty(nameof(Crisis.Runtime.FullSource.GlobalContext.Settings),System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)!.SetValue(context,settings);
    Crisis.Runtime.FullSource.Global.Instance.Initialize(contextObject);
    var missionObject=new UnityEngine.GameObject(Crisis.Runtime.FullSource.Mission.ObjectName,world);missionObject.SetActive(false);
    var mission=missionObject.AddComponent<Crisis.Runtime.FullSource.Mission>();mission.enabled=false;awakeField.SetValue(mission,true);missionObject.SetActive(true);
    var effectsContainer=new UnityEngine.GameObject("Operator lifecycle effects fixture",world);
    SetProperty(mission,nameof(Crisis.Runtime.FullSource.Mission.EffectsContainer),effectsContainer.transform);
    UnityEngine.Shader.Resolver??=(name=>new UnityEngine.Shader{name=name,isSupported=false});
    Crisis.Runtime.FullSource.MissionHelper.Instance.Initialize(mission);
    var operators=missionObject.AddComponent<Crisis.Runtime.FullSource.Operators>();operators.enabled=false;awakeField.SetValue(operators,true);
    var teamObject=new UnityEngine.GameObject("Operator lifecycle team",world);teamObject.SetActive(false);
    var mainGroup=teamObject.AddComponent<Crisis.Runtime.FullSource.TeamController>();mainGroup.enabled=false;awakeField.SetValue(mainGroup,true);startedField.SetValue(mainGroup,true);
    SetProperty(operators,nameof(Crisis.Runtime.FullSource.Operators.MainGroup),mainGroup);
    SetProperty(mission,nameof(Crisis.Runtime.FullSource.Mission.Operators),operators);
    var uiObject=new UnityEngine.GameObject("Operator lifecycle mission UI",world);uiObject.SetActive(false);
    var missionUI=uiObject.AddComponent<Crisis.Runtime.FullSource.MissionUI>();missionUI.enabled=false;awakeField.SetValue(missionUI,true);startedField.SetValue(missionUI,true);
    var hitAnnouncementsObject=new UnityEngine.GameObject("Operator lifecycle hit announcements",world);hitAnnouncementsObject.SetActive(false);
    var hitAnnouncements=hitAnnouncementsObject.AddComponent<Crisis.Runtime.FullSource.UIHitAnnouncements>();hitAnnouncements.enabled=false;awakeField.SetValue(hitAnnouncements,true);startedField.SetValue(hitAnnouncements,true);
    missionUI.hitAnnouncements=hitAnnouncements;
    var resourceCounterObject=new UnityEngine.GameObject("Operator lifecycle resource counter",world);resourceCounterObject.SetActive(false);
    missionUI.resourceCounter=resourceCounterObject.AddComponent<Crisis.Runtime.FullSource.UIResourceCounter>();
    var resourceEntryObject=new UnityEngine.GameObject("Operator lifecycle resource entry fixture",world);resourceEntryObject.SetActive(false);
    var resourceEntry=resourceEntryObject.AddComponent<Crisis.Runtime.FullSource.UIResourceCounterEntry>();
    var resourceLabelObject=new UnityEngine.GameObject("Operator lifecycle resource label fixture",world);resourceLabelObject.transform.SetParent(resourceEntryObject.transform,false);
    var resourceValueObject=new UnityEngine.GameObject("Operator lifecycle resource value fixture",world);resourceValueObject.transform.SetParent(resourceEntryObject.transform,false);
    resourceEntry.labelText=resourceLabelObject.AddComponent<UnityEngine.UI.Text>();
    resourceEntry.valueText=resourceValueObject.AddComponent<UnityEngine.UI.Text>();
    missionUI.resourceCounter.entryPrefab=resourceEntry;
    mission.UI=missionUI;
    var actors=new System.Collections.Generic.List<(string,UnityEngine.GameObject,UnityEngine.GameObject)>();
    foreach(var name in new[]{"Operator","Occupant"}){
        var path=$"Assets/Resources/characters/{name}.prefab";
        var loaded=SceneLoader.LoadPrefab(package,path,world,requireComplete:true);
        var instance=UnityEngine.Object.Instantiate(loaded.prefabRoot);
        loaded.prefabRoot.SetActive(false);
        // The original TeamOperator.Start runs after the mission spawner has
        // attached Equipment. Supply an empty test-owned component so the
        // actor can complete Start without claiming weapon/equipment parity.
        var fixtureEquipmentObject=new UnityEngine.GameObject($"{name} lifecycle equipment fixture",world);
        fixtureEquipmentObject.transform.SetParent(instance.transform,false);
        var fixtureEquipment=fixtureEquipmentObject.AddComponent<Crisis.Runtime.FullSource.Equipment>();
        var fixtureBody=instance.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true).Single(component=>component.GetType().Name=="CharacterBody");
        SetProperty(fixtureBody,"Equipment",fixtureEquipment);
        actors.Add((name,loaded.prefabRoot,instance));
    }
    var enabledField=typeof(UnityEngine.MonoBehaviour).GetField("enableNotified",flags)??throw new MissingFieldException("MonoBehaviour.enableNotified lifecycle state is unavailable");
    var startField=startedField;
    // The actor lifecycle gate is physics/component focused. The browser
    // renderer bridge is a separate acceptance gate, so retain material
    // values while accepting texture assignments without claiming GPU output.
    foreach(var renderer in world.Objects.OfType<UnityEngine.Renderer>().ToArray())
        renderer.material.OnTextureAssigned??=((_,_)=>{});
    if(runLifecycle)world.Tick(1f/60f);
    if(runLifecycle)world.Tick(1f/60f);
    var bodies=new System.Collections.Generic.List<UnityEngine.MonoBehaviour>();var curves=new System.Collections.Generic.List<UnityEngine.AnimationCurve>();
    var physiologies=new System.Collections.Generic.List<Crisis.Runtime.FullSource.Physiology>();
    var controllers=new System.Collections.Generic.List<Crisis.Runtime.FullSource.IKArmController>();
    var rightSolvers=new System.Collections.Generic.List<Crisis.Runtime.FullSource.ArmIKSolver>();
    var leftSolvers=new System.Collections.Generic.List<Crisis.Runtime.FullSource.ArmIKSolver>();
    var equipmentRefs=new System.Collections.Generic.List<Crisis.Runtime.FullSource.Equipment>();
    var limbGraphs=new System.Collections.Generic.List<Crisis.Runtime.FullSource.Limb[]>();
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
            var actorLimbs=behaviours.OfType<Crisis.Runtime.FullSource.Limb>().ToArray();
            var limbPhysiology=behaviours.OfType<Crisis.Runtime.FullSource.LimbPhysiology>().ToArray();
            var limbMuscle=behaviours.OfType<Crisis.Runtime.FullSource.LimbMuscle>().ToArray();
            if(actorLimbs.Length!=19||limbPhysiology.Length!=17||limbMuscle.Length!=16)
                throw new InvalidOperationException($"{actor.Item1} original limb component inventory changed: limbs={actorLimbs.Length}, physiology={limbPhysiology.Length}, muscle={limbMuscle.Length}");
            foreach(var limb in fullBody.limbs){
                Crisis.Runtime.FullSource.OriginalActorIdentity.RequireLimb(fullBody,limb);
                if(!behaviours.Any(component=>ReferenceEquals(component,limb))||
                   (limb.LimbPhysiology!=null&&!ReferenceEquals(limb.GetComponent<Crisis.Runtime.FullSource.LimbPhysiology>(),limb.LimbPhysiology))||
                   (limb.LimbMuscle!=null&&!ReferenceEquals(limb.GetComponent<Crisis.Runtime.FullSource.LimbMuscle>(),limb.LimbMuscle)))
                    throw new InvalidOperationException($"{actor.Item1} limb {limb.id} is not bound to its actor-owned physiology and muscle components");
            }
            if(equipment!=null){
                Crisis.Runtime.FullSource.OriginalActorIdentity.RequireEquipment(fullBody,equipment);
                if(!behaviours.Any(component=>ReferenceEquals(component,equipment)))
                    throw new InvalidOperationException($"{actor.Item1} CharacterBody equipment is not owned by its own prefab graph");
            }
        }
        physiologies.Add(physiology);controllers.Add(controller);rightSolvers.Add(rightSolver);leftSolvers.Add(leftSolver);equipmentRefs.Add(equipment);
        limbGraphs.Add(fullBody.limbs.ToArray());
            var originalBody=actor.Item2.GetComponentsInChildren<UnityEngine.MonoBehaviour>(true).Single(component=>component.GetType().Name=="CharacterBody");
            var curveField=originalBody.GetType().GetField("hitRecoveryCurve",flags)??throw new MissingFieldException(originalBody.GetType().FullName,"hitRecoveryCurve");
            var authoredCurve=curveField.GetValue(originalBody) as UnityEngine.AnimationCurve??throw new InvalidOperationException($"{actor.Item1} source prefab has no authored hitRecoveryCurve");
            var clonedCurve=curveField.GetValue(actorBodies[0]) as UnityEngine.AnimationCurve??throw new InvalidOperationException($"{actor.Item1} clone lost hitRecoveryCurve");
            var authoredKeys=authoredCurve.keys;var clonedKeys=clonedCurve.keys;
            if(ReferenceEquals(authoredCurve,clonedCurve)||authoredCurve.preWrapMode!=clonedCurve.preWrapMode||authoredCurve.postWrapMode!=clonedCurve.postWrapMode||authoredCurve.clampOutside!=clonedCurve.clampOutside||authoredCurve.serializedPreInfinity!=clonedCurve.serializedPreInfinity||authoredCurve.serializedPostInfinity!=clonedCurve.serializedPostInfinity||authoredCurve.serializedVersion!=clonedCurve.serializedVersion||authoredCurve.serializedRotationOrder!=clonedCurve.serializedRotationOrder||authoredKeys.Length!=clonedKeys.Length)throw new InvalidOperationException($"{actor.Item1} clone changed authored hitRecoveryCurve metadata");
            for(var keyIndex=0;keyIndex<authoredKeys.Length;keyIndex++){var sourceKey=authoredKeys[keyIndex];var cloneKey=clonedKeys[keyIndex];if(!sourceKey.time.Equals(cloneKey.time)||!sourceKey.value.Equals(cloneKey.value)||!sourceKey.inTangent.Equals(cloneKey.inTangent)||!sourceKey.outTangent.Equals(cloneKey.outTangent)||sourceKey.tangentMode!=cloneKey.tangentMode||sourceKey.serializedVersion!=cloneKey.serializedVersion)throw new InvalidOperationException($"{actor.Item1} clone changed authored hitRecoveryCurve key {keyIndex}");}
            curves.Add(clonedCurve);
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{mode=runLifecycle?"lifecycle":"instantiation",prefab=actor.Item1,instantiatedFromOriginalPrefab=true,activeBehaviours=active.Length,awake=awoken,onEnable=enabled,start=started,characterBody=actorBodies.Length,originalIKController=controller!=null,originalPhysiology=physiology!=null,testEquipmentFixtureAttached=equipment!=null,hitRecoveryCurveKeys=clonedCurve.keys.Length}));
    }
    if(ReferenceEquals(bodies[0],bodies[1])||ReferenceEquals(bodies[0].gameObject,bodies[1].gameObject)||ReferenceEquals(curves[0],curves[1]))throw new InvalidOperationException("Operator and Occupant share a CharacterBody or authored AnimationCurve instance");
    if(runLifecycle&&(ReferenceEquals(physiologies[0],physiologies[1])||ReferenceEquals(controllers[0],controllers[1])||ReferenceEquals(rightSolvers[0],rightSolvers[1])||ReferenceEquals(leftSolvers[0],leftSolvers[1])||(equipmentRefs[0]!=null&&ReferenceEquals(equipmentRefs[0],equipmentRefs[1]))))
        throw new InvalidOperationException("Operator and Occupant share physiology, IK controller, arm solvers, or initialized equipment");
    if(runLifecycle){
        if(limbGraphs[0].Length!=limbGraphs[1].Length||limbGraphs[0].Where((limb,index)=>ReferenceEquals(limb,limbGraphs[1][index])).Any())
            throw new InvalidOperationException("Operator and Occupant share one or more original Limb components");
        var operatorBloodBefore=physiologies[0].BloodVolume;var occupantBloodBefore=physiologies[1].BloodVolume;physiologies[0].RemoveBlood(100f);
        if(physiologies[0].BloodVolume>=operatorBloodBefore||physiologies[1].BloodVolume!=occupantBloodBefore)
            throw new InvalidOperationException("Operator physiology mutation changed or failed to isolate Occupant blood state");
        equipmentRefs[0].gauze=1;
        if(equipmentRefs[1].gauze!=0)throw new InvalidOperationException("Operator equipment fixture mutation changed Occupant equipment state");
    }
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
        if(path=="Assets/Scenes/Marksman range.unity"){
            var glass=loaded.objects.Values.OfType<Crisis.Runtime.FullSource.BallisticWindow>()
                .Select(window=>window.texture).FirstOrDefault(texture=>texture?.OriginalAssetPath=="Assets/Texture2D/window glass.png");
            if(glass==null||glass.OriginalAssetGuid!="bb3d703bd542afc47a1423a6c37397aa"||glass.OriginalAssetFileID!="2800000"||
               glass.OriginalImageBytes==null||Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(glass.OriginalImageBytes)).ToLowerInvariant()!="ba810cf6d18d29ac28aebfc07340188c0cd3d00f0c4fd4071f8e2a7b98d1d4b5"||
               !glass.ImporterSourceText.Contains("  textureFormat: 5\n",StringComparison.Ordinal))
                throw new InvalidOperationException("Original Marksman window texture lost its GUID, fileID, PNG source, or ARGB32 importer metadata");
        }
        Console.WriteLine(JsonSerializer.Serialize(loaded.Report()));
        Console.WriteLine(JsonSerializer.Serialize(SceneLoader.VerifyImportedRay(loaded)));
    }
    foreach(var path in new[]{"Assets/Resources/weapons/ammunition/5.56-JHP.prefab","Assets/Resources/weapons/ammunition/308-Win-BSP.prefab"})
        Console.WriteLine(JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalPrefab(package,path)));
    Console.WriteLine(JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalResource(package,"5.56-JHP")));
    Console.WriteLine(ResourcePrefabContracts.Run(package));
    Console.WriteLine(SpawnerGameplayContracts.Run(package));
}

sealed class MissionContractSilentAudioDriver:IAudioClipPlaybackDriver {
    public bool IsPlaying=>false;
    public void PlayOneShot(AudioClip clip,float volume,int priority){}
    public void Play(AudioClip clip,float volume,float pitch,bool loop,bool muted,int priority){}
    public void Stop(){}
}
sealed class MissionContractSilentMixerDriver:UnityEngine.Audio.IAudioMixerDriver {
    public bool SetFloat(string exposedParameter,float value)=>true;
    public void TransitionTo(UnityEngine.Audio.AudioMixerSnapshot snapshot,float timeToReach){}
}
sealed class MissionContractQualitySettingsDriver:UnityEngine.IQualitySettingsDriver {
    public T Get<T>(string property)=>default;
    public void Set<T>(string property,T value){}
}
sealed class MissionContractSceneManagerDriver:UnityEngine.SceneManagement.ISceneManagerDriver {
    public UnityEngine.SceneManagement.Scene GetActiveScene()=>new(3,"Mission 01");
    public void LoadScene(int buildIndex)=>throw new NotSupportedException("Mission contract does not change scenes");
    public void LoadScene(string sceneName)=>throw new NotSupportedException("Mission contract does not change scenes");
}
