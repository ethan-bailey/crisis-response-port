using System;
using FullSerializer;

[fsObject("2", new Type[] { typeof(SerializableEquipment_v1) })]
public class SerializableEquipment
{
	[fsObject("1", new Type[] { })]
	public class SerializableEquipment_v1
	{
		public bool enabled = true;

		public SerializableWeapon primary = new SerializableWeapon();

		public SerializableWeapon secondary = new SerializableWeapon();

		public SerializableItems items = new SerializableItems();

		public ArmorClass.Type armor = ArmorClass.Type.TypeIV;
	}

	public bool enabled = true;

	public SerializableWeapon primary = new SerializableWeapon();

	public SerializableWeapon secondary = new SerializableWeapon();

	public SerializableItems items = new SerializableItems();

	public ArmorClass.Type armor = ArmorClass.Type.TypeIV;

	public bool restrainOccupants;

	public bool secureWeapons;

	public bool administerFirstAid;

	public SerializableEquipment()
	{
	}

	public SerializableEquipment(SerializableEquipment_v1 v1Data)
	{
		enabled = v1Data.enabled;
		primary.Overwrite(v1Data.primary);
		secondary.Overwrite(v1Data.secondary);
		items.Overwrite(v1Data.items);
		armor = v1Data.armor;
		restrainOccupants = false;
		secureWeapons = false;
		administerFirstAid = false;
	}

	public SerializableEquipment(TutorialEquipment data)
	{
		primary.Overwrite(data.primary);
		secondary.Overwrite(data.secondary);
		items.Overwrite(data.items);
		armor = data.armor;
	}

	public static SerializableEquipment EmptyEquipment()
	{
		SerializableEquipment serializableEquipment = new SerializableEquipment();
		serializableEquipment.primary = null;
		serializableEquipment.secondary = null;
		serializableEquipment.items = null;
		return serializableEquipment;
	}

	public SerializableEquipment Copy()
	{
		SerializableEquipment serializableEquipment = new SerializableEquipment();
		serializableEquipment.Overwrite(this);
		return serializableEquipment;
	}

	public SerializableEquipment Overwrite(SerializableEquipment data)
	{
		primary.Overwrite(data.primary);
		secondary.Overwrite(data.secondary);
		items.Overwrite(data.items);
		armor = data.armor;
		restrainOccupants = data.restrainOccupants;
		secureWeapons = data.secureWeapons;
		administerFirstAid = data.administerFirstAid;
		return this;
	}

	public bool ContainsPlasticuffs()
	{
		foreach (string itemPrefabName in items.itemPrefabNames)
		{
			if (itemPrefabName == "Plasticuffs")
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsGauze()
	{
		foreach (string itemPrefabName in items.itemPrefabNames)
		{
			if (itemPrefabName == "Gauze")
			{
				return true;
			}
		}
		return false;
	}
}
