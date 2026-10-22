namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;
using FullSerializer;

[fsObject("1", new Type[] { })]
public class SerializableTeam
{
	[fsIgnore]
	public const string Filename = "operators.dat";

	public List<SerializableEquipment> entryMembers = new List<SerializableEquipment>();

	public SerializableMarksman marksman = new SerializableMarksman();

	public SerializableTeamBehavior behavior = new SerializableTeamBehavior();

	public int Count
	{
		get
		{
			return (entryMembers != null) ? entryMembers.Count : 0;
		}
	}

	public int ActiveCount
	{
		get
		{
			if (entryMembers == null)
			{
				return 0;
			}
			int num = 0;
			foreach (SerializableEquipment entryMember in entryMembers)
			{
				if (entryMember.enabled)
				{
					num++;
				}
			}
			return num;
		}
	}

	public void SetWeapon(int equipmentIndex, SerializableWeapon weapon, bool primary, bool skipAmmo = false)
	{
		SerializableWeapon serializableWeapon = ((!primary) ? entryMembers[equipmentIndex].secondary : entryMembers[equipmentIndex].primary);
		serializableWeapon.Overwrite(weapon, skipAmmo);
	}

	public void SetAmmo(int equipmentIndex, string ammoPrefabName, bool primary)
	{
		SerializableWeapon serializableWeapon = ((!primary) ? entryMembers[equipmentIndex].secondary : entryMembers[equipmentIndex].primary);
		serializableWeapon.ammunitionPrefabName = ammoPrefabName;
	}

	public void SetItem(int equipmentIndex, int itemIndex, string item)
	{
		entryMembers[equipmentIndex].items.itemPrefabNames[itemIndex] = item;
	}

	public void SetItems(int equipmentIndex, SerializableItems items)
	{
		entryMembers[equipmentIndex].items.Overwrite(items);
	}

	public void SetArmor(int equipmentIndex, ArmorClass.Type armor)
	{
		entryMembers[equipmentIndex].armor = armor;
	}

	public SerializableWeapon GetWeapon(int equipmentIndex, bool primary)
	{
		return (!primary) ? entryMembers[equipmentIndex].secondary : entryMembers[equipmentIndex].primary;
	}

	public AmmunitionBase GetAmmunition(int equipmentIndex, bool primary)
	{
		SerializableWeapon serializableWeapon = ((!primary) ? entryMembers[equipmentIndex].secondary : entryMembers[equipmentIndex].primary);
		return AmmunitionBase.GetAmmunition(serializableWeapon.ammunitionPrefabName, WeaponBase.DefaultAmmunition(serializableWeapon.prefabName));
	}

	public string GetAmmoName(int equipmentIndex, bool primary)
	{
		SerializableWeapon serializableWeapon = ((!primary) ? entryMembers[equipmentIndex].secondary : entryMembers[equipmentIndex].primary);
		return serializableWeapon.ammunitionPrefabName;
	}

	public SerializableItems GetItems(int equipmentIndex)
	{
		return entryMembers[equipmentIndex].items;
	}

	public ArmorClass.Type GetArmor(int equipmentIndex)
	{
		return entryMembers[equipmentIndex].armor;
	}
}

}
