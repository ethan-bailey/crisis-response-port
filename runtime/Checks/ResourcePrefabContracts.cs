using Crisis.Runtime.FullSource;
using UnityEngine;

public static class ResourcePrefabContracts
{
    public static string Run(string packageText)
    {
        Resources.Configure(packageText);
        var source = Resources.Load("Items/Primary magazine") as GameObject
            ?? throw new InvalidOperationException("Original Primary magazine resource did not load");
        var sourceItem = source.GetComponent<Item>()
            ?? throw new InvalidOperationException("Original Primary magazine resource has no Item component");
        if (sourceItem.type != ItemType.PrimaryMagazine || sourceItem.quantity <= 0)
            throw new InvalidOperationException($"Original Primary magazine data is invalid: type={sourceItem.type}, quantity={sourceItem.quantity}");
        if (source.activeInHierarchy)
            throw new InvalidOperationException("Loading an original item prefab activated its source object");
        var clone = UnityEngine.Object.Instantiate(source);
        try
        {
            var clonedItem = clone.GetComponent<Item>();
            if (clonedItem == null || ReferenceEquals(sourceItem, clonedItem) || clonedItem.type != sourceItem.type || clonedItem.quantity != sourceItem.quantity)
                throw new InvalidOperationException("Original item prefab clone lost its independent serialized Item data");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(clone);
        }

        var world = new Crisis.Runtime.RuntimeWorld();
        world.MakeCurrent();
        var contextObject = new GameObject("Original equipment resource context");
        var context = contextObject.AddComponent<GlobalContext>();
        var equipmentListObject = new GameObject("Original equipment list");
        var equipmentList = equipmentListObject.AddComponent<EquipmentList>();
        equipmentList.randomItem = new GameObject("Random item").AddComponent<Item>();
        typeof(GlobalContext).GetProperty(nameof(GlobalContext.EquipmentList))!.SetValue(context, equipmentList);
        Global.Instance.Initialize(contextObject);
        var missionObject = new GameObject(Mission.ObjectName);
        missionObject.AddComponent<Mission>();
        var serializedEquipment = new SerializableEquipment
        {
            primary = null,
            secondary = null,
            items = new SerializableItems { itemPrefabNames = new System.Collections.Generic.List<string> { "Primary magazine" } }
        };
        var equipmentObject = EquipmentHelper.Instance.SpawnEquipment(serializedEquipment);
        try
        {
            var equipment = equipmentObject.GetComponent<Equipment>();
            if (equipment == null || equipment.primaryMagazineCount != sourceItem.quantity)
                throw new InvalidOperationException($"Original EquipmentHelper did not apply the loaded Primary magazine quantity: expected={sourceItem.quantity}, actual={equipment?.primaryMagazineCount}");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(equipmentObject);
        }
        return $"PASS: original Items/Primary magazine resource loaded, cloned, and applied by EquipmentHelper ({sourceItem.quantity} magazines)";
    }
}
