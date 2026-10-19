namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public sealed class EquipmentHelper
{
	private static EquipmentHelper instance = null;

	private static readonly object padlock = new object();

	private WeaponBase noWeaponInstance;

	public WeaponBase NoWeaponInstance
	{
		get
		{
			if (noWeaponInstance == null)
			{
				EquipmentList equipmentList = Global.Context.EquipmentList;
				WeaponBase weaponBase = Object.Instantiate(equipmentList.noWeapon);
				noWeaponInstance = weaponBase.GetComponent<WeaponBase>();
				noWeaponInstance.name = equipmentList.noWeapon.name;
			}
			return noWeaponInstance;
		}
	}

	public static EquipmentHelper Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new EquipmentHelper();
				}
				return instance;
			}
		}
	}

	public GameObject SpawnEquipment(SerializableEquipment equipmentData)
	{
		return SpawnEquipment(equipmentData.primary, equipmentData.secondary, equipmentData.items);
	}

	public GameObject SpawnEquipment(SerializableWeapon primaryWeapon = null, SerializableWeapon secondaryWeapon = null, SerializableItems items = null, ArmorClass.Type armor = ArmorClass.Type.None)
	{
		EquipmentList equipmentList = Global.Context.EquipmentList;
		GameObject gameObject = new GameObject("Equipment");
		Equipment equipment = gameObject.AddComponent<Equipment>();
		if (Global.Context.Mission != null)
		{
			if (primaryWeapon != null)
			{
				if (primaryWeapon.prefabName == equipmentList.randomWeapon.name)
				{
					equipment.primaryWeapon = SpawnWeapon(equipmentList.RandomWeapon(), true, gameObject.transform);
				}
				else if (primaryWeapon.prefabName != equipmentList.noWeapon.name)
				{
					equipment.primaryWeapon = SpawnWeapon(primaryWeapon, true, gameObject.transform);
				}
			}
			if (secondaryWeapon != null)
			{
				if (secondaryWeapon.prefabName == equipmentList.randomWeapon.name)
				{
					equipment.secondaryWeapon = SpawnWeapon(equipmentList.RandomWeapon(), false, gameObject.transform);
				}
				else if (secondaryWeapon.prefabName != equipmentList.noWeapon.name)
				{
					equipment.secondaryWeapon = SpawnWeapon(secondaryWeapon, false, gameObject.transform);
				}
			}
			InitializeItems(items, equipment);
			equipment.Initialize();
		}
		else
		{
			SpawnWeapon(primaryWeapon, true, gameObject.transform);
			SpawnWeapon(secondaryWeapon, false, gameObject.transform);
		}
		return gameObject;
	}

	private WeaponBase SpawnWeapon(SerializableWeapon data, bool isPrimary, Transform parent = null)
	{
		WeaponBase weaponBase = WeaponBase.Deserialize(data, isPrimary);
		if (weaponBase == null)
		{
			return null;
		}
		if (parent != null)
		{
			weaponBase.transform.SetParent(parent, false);
		}
		return weaponBase;
	}

	private void InitializeItems(SerializableItems items, Equipment equipment)
	{
		if (items == null || items.itemPrefabNames == null)
		{
			return;
		}
		for (int i = 0; i < items.itemPrefabNames.Count; i++)
		{
			string text = items.itemPrefabNames[i];
			if (text == Global.Context.EquipmentList.randomItem.name)
			{
				text = Global.Context.EquipmentList.RandomItem().name;
			}
			if (text.Length == 0)
			{
				continue;
			}
			GameObject gameObject = Resources.Load("Items/" + text) as GameObject;
			if (gameObject == null)
			{
				continue;
			}
			Item component = gameObject.GetComponent<Item>();
			if (!(component == null))
			{
				switch (component.type)
				{
				case ItemType.PrimaryMagazine:
					equipment.primaryMagazineCount += component.quantity;
					break;
				case ItemType.SecondaryMagazine:
					equipment.secondaryMagazineCount += component.quantity;
					break;
				case ItemType.StunGrenade:
					equipment.stunGrenades += component.quantity;
					break;
				case ItemType.FragGrenade:
					equipment.fragGrenades += component.quantity;
					break;
				case ItemType.Handcuffs:
					equipment.plasticuffs = true;
					break;
				case ItemType.Gauze:
					equipment.gauze += component.quantity;
					break;
				case ItemType.Multitool:
					equipment.multiTool = true;
					break;
				}
			}
		}
	}

	public WeaponBase GetWeapon(GameObject equipment, bool primary = true)
	{
		Transform transform = equipment.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			WeaponBase component = transform.GetChild(i).GetComponent<WeaponBase>();
			if (!(component == null) && component.IsPrimary == primary)
			{
				return component;
			}
		}
		return NoWeaponInstance;
	}

	private GameObject FindFirstActiveChild(Transform container)
	{
		if (container == null)
		{
			return null;
		}
		for (int i = 0; i < container.childCount; i++)
		{
			GameObject gameObject = container.GetChild(i).gameObject;
			if (gameObject.activeInHierarchy)
			{
				return gameObject;
			}
		}
		return null;
	}
}

}
