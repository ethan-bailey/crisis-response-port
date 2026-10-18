using System;
using System.Collections.Generic;

[Serializable]
public class SerializableItems
{
	public List<string> itemPrefabNames = new List<string>();

	public SerializableItems Overwrite(SerializableItems other)
	{
		itemPrefabNames.Clear();
		foreach (string itemPrefabName in other.itemPrefabNames)
		{
			itemPrefabNames.Add(itemPrefabName);
		}
		return this;
	}
}
