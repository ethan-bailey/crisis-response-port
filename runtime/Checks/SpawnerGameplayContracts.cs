using System.Reflection;
using Crisis.Runtime;
using Crisis.Runtime.FullSource;
using UnityEngine;

public static class SpawnerGameplayContracts
{
    static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    static readonly FieldInfo Awoken = typeof(MonoBehaviour).GetField("awoken", InstanceFlags)!;

    static void SetProperty(object target, string name, object value)
    {
        var property = target.GetType().GetProperty(name, InstanceFlags)
            ?? throw new MissingMemberException(target.GetType().FullName, name);
        property.SetValue(target, value);
    }

    static void SetField(object target, string name, object value)
    {
        var field = target.GetType().GetField(name, InstanceFlags)
            ?? throw new MissingFieldException(target.GetType().FullName, name);
        field.SetValue(target, value);
    }

    static void MarkAwake(MonoBehaviour component) => Awoken.SetValue(component, true);

    public static string Run(string packageText)
    {
        Resources.Configure(packageText);
        var world = new RuntimeWorld();
        world.MakeCurrent();

        GameObject occupantObject = null;
        GameObject operatorObject = null;
        GameObject missionObject = null;
        GameObject contextObject = null;
        try
        {
            contextObject = new GameObject("Spawner contract global context");
            contextObject.SetActive(false);
            var context = contextObject.AddComponent<GlobalContext>();
            var settings = contextObject.AddComponent<Settings>();
            var input = contextObject.AddComponent<InputHandler>();
            input.SetMapping(new InputMapping());
            var equipmentList = contextObject.AddComponent<EquipmentList>();
            var randomItemObject = new GameObject("Random item", world);\n            randomItemObject.transform.SetParent(contextObject.transform, false);\n            equipmentList.randomItem = randomItemObject.AddComponent<Item>();
            var campaignManager = contextObject.AddComponent<CampaignManager>();
            var sceneTransition = contextObject.AddComponent<SceneTransition>();
            SetProperty(context, nameof(GlobalContext.Settings), settings);
            SetProperty(context, nameof(GlobalContext.InputHandler), input);
            SetProperty(context, nameof(GlobalContext.EquipmentList), equipmentList);
            SetProperty(context, nameof(GlobalContext.CampaignManager), campaignManager);
            SetProperty(context, nameof(GlobalContext.SceneTransition), sceneTransition);
            Global.Instance.Initialize(contextObject);

            missionObject = new GameObject(Mission.ObjectName, world);
            var mission = missionObject.AddComponent<Mission>();
            MarkAwake(mission);
            var containers = new GameObject("Spawner contract containers", world).transform;
            containers.SetParent(missionObject.transform, false);
            SetProperty(mission, nameof(Mission.Containers), containers);

            var uiObject = new GameObject("Spawner contract mission UI", world);
            uiObject.transform.SetParent(missionObject.transform, false);
            var ui = uiObject.AddComponent<MissionUI>();
            MarkAwake(ui);
            var hitAnnouncementsObject = new GameObject("Hit announcements", world);
            hitAnnouncementsObject.transform.SetParent(uiObject.transform, false);
            ui.hitAnnouncements = hitAnnouncementsObject.AddComponent<UIHitAnnouncements>();
            MarkAwake(ui.hitAnnouncements);
            mission.UI = ui;

            var operators = missionObject.AddComponent<Operators>();
            MarkAwake(operators);
            var operatorContainer = new GameObject("Operator group", world).transform;
            operatorContainer.SetParent(containers, false);
            SetProperty(operators, nameof(Operators.Container), operatorContainer);
            var mainGroup = new GameObject("Main team", world).AddComponent<TeamController>();
            mainGroup.transform.SetParent(operatorContainer, false);
            SetProperty(operators, nameof(Operators.MainGroup), mainGroup);
            SetProperty(mission, nameof(Mission.Operators), operators);

            var occupants = missionObject.AddComponent<Occupants>();
            MarkAwake(occupants);
            var occupantContainer = new GameObject("Occupant group", world).transform;
            occupantContainer.SetParent(containers, false);
            SetProperty(occupants, nameof(Occupants.Container), occupantContainer);
            SetProperty(mission, nameof(Mission.Occupants), occupants);

            var spawnerObject = new GameObject("Spawner contract", world);
            spawnerObject.SetActive(false);
            spawnerObject.transform.SetParent(missionObject.transform, false);
            var spawner = spawnerObject.AddComponent<OccupantSpawner>();
            MarkAwake(spawner);
            var area = new SpawnArea
            {
                name = "Deterministic test area",
                xMin = 0f,
                xMax = 0f,
                y = 1.25f,
                zMin = -1.5f,
                zMax = -1.5f,
                walkableMin = -1f,
                walkableMax = 1f,
                probability = 1f,
                probabilityLow = 0f,
                probabilityHigh = 1f
            };
            spawner.areas = new List<SpawnArea> { area };
            SetField(spawner, "mission", mission);
            SetField(spawner, "settings", settings);
            SetField(spawner, "occupantSettings", settings.Occupants);
            SetField(spawner, "availableWeapons", new List<WeaponBase>());
            SetField(spawner, "spawnAreaProbabilitySum", 1f);
            spawnerObject.SetActive(true);

            world.Initialize();

            occupantObject = spawner.SpawnOccupantByCommand(0f).gameObject;
            var occupant = occupantObject.GetComponent<CharacterBody>()
                ?? throw new InvalidOperationException("Original spawner returned no Occupant CharacterBody");
            if (!ReferenceEquals(occupant.transform.parent, occupants.Container))
                throw new InvalidOperationException("Original spawner did not parent the Occupant under Mission.Occupants");
            var occupantPhysiology = occupant.Physiology
                ?? throw new InvalidOperationException("Spawned Occupant has no original Physiology");
            var occupantEquipment = occupant.Equipment
                ?? throw new InvalidOperationException("Spawned Occupant has no original Equipment");
            if (occupantEquipment.primaryMagazineCount <= 0)
                throw new InvalidOperationException("Original spawner did not apply its authored Primary magazine items");

            var operatorPrefab = Resources.Load("Characters/Operator") as GameObject
                ?? throw new InvalidOperationException("Original Operator resource did not load");
            operatorObject = UnityEngine.Object.Instantiate(operatorPrefab);
            operatorObject.name = "Spawner contract Operator";
            operatorObject.transform.SetParent(operators.Container, false);
            var operatorBody = operatorObject.GetComponent<CharacterBody>()
                ?? throw new InvalidOperationException("Original Operator prefab has no CharacterBody");
            var operatorPhysiology = operatorBody.Physiology
                ?? throw new InvalidOperationException("Original Operator has no original Physiology");
            var operatorEquipment = operatorBody.InitializeEquipment(new SerializableEquipment
            {
                primary = null,
                secondary = null,
                armor = ArmorClass.Type.None,
                items = new SerializableItems { itemPrefabNames = new List<string> { "Primary magazine" } }
            });
            if (operatorEquipment == null || ReferenceEquals(operatorEquipment, occupantEquipment))
                throw new InvalidOperationException("Operator and Occupant do not own independent original Equipment components");
            if (!operatorObject.GetComponentsInChildren<MonoBehaviour>(true).Any(c => ReferenceEquals(c, operatorEquipment)))
                throw new InvalidOperationException("Operator Equipment is outside its original prefab clone graph");

            var torso = occupant.Torso ?? throw new InvalidOperationException("Spawned Occupant has no torso limb");
            var targetLimbPhysiology = torso.LimbPhysiology
                ?? throw new InvalidOperationException("Spawned Occupant torso has no original LimbPhysiology");
            var operatorTorso = operatorBody.Torso
                ?? throw new InvalidOperationException("Original Operator has no torso limb");
            var operatorLimbPhysiology = operatorTorso.LimbPhysiology
                ?? throw new InvalidOperationException("Original Operator torso has no original LimbPhysiology");
            var targetBloodBefore = occupantPhysiology.BloodVolume;
            var operatorBloodBefore = operatorPhysiology.BloodVolume;
            var targetTraumaBefore = targetLimbPhysiology.WoundCavityVolume;
            var operatorTraumaBefore = operatorLimbPhysiology.WoundCavityVolume;

            torso.AddTrauma(50f, Vector3.zero, torso.transform.position);
            if (targetLimbPhysiology.WoundCavityVolume <= targetTraumaBefore)
                throw new InvalidOperationException("Original Limb.AddTrauma did not change only the target torso trauma");
            var deltaTime = typeof(Time).GetProperty(nameof(Time.deltaTime), BindingFlags.Static | BindingFlags.Public)!
                .GetSetMethod(true) ?? throw new MissingMethodException("Time.deltaTime setter");
            deltaTime.Invoke(null, new object[] { 0.1f });
            world.Invoke(occupantPhysiology, "Update");
            world.Invoke(operatorPhysiology, "Update");

            if (occupantPhysiology.BloodVolume >= targetBloodBefore)
                throw new InvalidOperationException("Original Physiology.Update did not apply target torso hemorrhage");
            if (operatorPhysiology.BloodVolume != operatorBloodBefore ||
                operatorLimbPhysiology.WoundCavityVolume != operatorTraumaBefore)
                throw new InvalidOperationException("Occupant hit changed the Operator's original physiology");
            return $"PASS: original OccupantSpawner spawned an independent Occupant and Operator; original Limb.AddTrauma and Physiology.Update changed only the target actor (blood loss {targetBloodBefore - occupantPhysiology.BloodVolume:0.##} ml)";
        }
        finally
        {
            if (occupantObject != null) UnityEngine.Object.DestroyImmediate(occupantObject);
            if (operatorObject != null) UnityEngine.Object.DestroyImmediate(operatorObject);
            if (missionObject != null) UnityEngine.Object.DestroyImmediate(missionObject);
            if (contextObject != null) UnityEngine.Object.DestroyImmediate(contextObject);
        }
    }
}