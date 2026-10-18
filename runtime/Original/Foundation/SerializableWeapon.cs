using System;
using System.Collections.Generic;

[Serializable]
public class SerializableWeapon
{
	public string prefabName;

	public string ammunitionPrefabName;

	public List<SerializableWeaponComponentSlot> componentSlots = new List<SerializableWeaponComponentSlot>();

	public SerializableWeapon Overwrite(SerializableWeapon data, bool skipAmmo = false)
	{
		prefabName = data.prefabName;
		if (!skipAmmo)
		{
			ammunitionPrefabName = data.ammunitionPrefabName;
		}
		componentSlots.Clear();
		foreach (SerializableWeaponComponentSlot componentSlot in data.componentSlots)
		{
			componentSlots.Add(new SerializableWeaponComponentSlot().Overwrite(componentSlot));
		}
		return this;
	}
}
