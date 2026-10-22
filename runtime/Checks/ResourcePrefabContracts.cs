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
        return $"PASS: original Items/Primary magazine resource loaded and cloned with type {sourceItem.type}, quantity {sourceItem.quantity}";
    }
}
