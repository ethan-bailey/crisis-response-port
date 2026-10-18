using System;
using System.Collections.Generic;

[Serializable]
public class SerializableWeaponComponentSlot
{
	public string name;

	public WeaponComponentSlotType type;

	public WeaponComponentSlotOrientation orientation;

	public string defaultComponent;

	public string component;

	public List<string> prefabNames = new List<string>();

	public bool required;

	public SerializableWeaponComponentSlot Overwrite(SerializableWeaponComponentSlot data)
	{
		name = data.name;
		type = data.type;
		orientation = data.orientation;
		defaultComponent = data.defaultComponent;
		component = data.component;
		prefabNames.Clear();
		prefabNames.AddRange(data.prefabNames);
		required = data.required;
		return this;
	}
}
