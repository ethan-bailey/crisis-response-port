namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableWeaponComponent
{
	public string name;

	public List<WeaponComponentSlotType> slotTypes = new List<WeaponComponentSlotType>();

	public List<WeaponComponentSlotOrientation> orientations = new List<WeaponComponentSlotOrientation>();

	public List<WeaponComponent.Function> functions = new List<WeaponComponent.Function>();
}

}
