using System.Collections.Generic;
using UnityEngine;

public class EquipmentList : MonoBehaviour
{
	[Header("Equipment")]
	public List<WeaponBase> weapons;

	public List<AmmunitionBase> ammunition;

	public List<WeaponComponent> components;

	public List<Item> items;

	public Grenade stunGrenade;

	public Grenade fragGrenade;

	public GameObject plasticuffs;

	public GameObject gauze;

	[Header("Defaults")]
	public AmmunitionBase defaultMarksmanAmmunition;

	public AmmunitionBase explosionFragmentAmmunition;

	[Header("Special representations")]
	public WeaponBase noWeapon;

	public AmmunitionBase noAmmunition;

	public Item noItem;

	public WeaponBase randomWeapon;

	public AmmunitionBase randomAmmunition;

	public Item randomItem;

	[Header("Special items")]
	[Tooltip("Prop to represent drop phone in campaign missions.")]
	public CellPhone cellPhonePrefab;

	public List<WeaponBase> OperatorWeapons { get; private set; }

	public List<WeaponBase> OccupantWeapons { get; private set; }

	public List<AmmunitionBase> OperatorAmmunition { get; private set; }

	public List<AmmunitionBase> OccupantAmmunition { get; private set; }

	public void Initialize(bool stagingMode)
	{
		InitializeWeaponLists(stagingMode);
		InitializeAmmunitionLists(stagingMode);
	}

	private void InitializeWeaponLists(bool stagingMode)
	{
		OperatorWeapons = new List<WeaponBase>();
		OccupantWeapons = new List<WeaponBase>();
		foreach (WeaponBase weapon in weapons)
		{
			if (weapon == null)
			{
				Debug.LogError("Skipped empty weapon slot in the equipment list!");
			}
			else if (weapon.availability.IsAvailable(stagingMode))
			{
				if (weapon.availability.IsAvailable(CharacterBody.Team.Operators))
				{
					OperatorWeapons.Add(weapon);
				}
				if (weapon.availability.IsAvailable(CharacterBody.Team.Occupants))
				{
					OccupantWeapons.Add(weapon);
				}
			}
		}
	}

	private void InitializeAmmunitionLists(bool stagingMode)
	{
		OperatorAmmunition = new List<AmmunitionBase>();
		OccupantAmmunition = new List<AmmunitionBase>();
		foreach (AmmunitionBase item in ammunition)
		{
			string text = item.name;
			if (item.availability.IsAvailable(stagingMode))
			{
				if (item.availability.IsAvailable(CharacterBody.Team.Operators))
				{
					OperatorAmmunition.Add(item);
				}
				if (item.availability.IsAvailable(CharacterBody.Team.Occupants))
				{
					OccupantAmmunition.Add(item);
				}
			}
		}
	}

	public List<AmmunitionBase> GetAmmunition(Calibre caliber, AmmunitionBase.Type type, CharacterBody.Team team, bool matchOnSonic = false, bool subsonic = false)
	{
		List<AmmunitionBase> list = new List<AmmunitionBase>();
		List<AmmunitionBase> list2 = ((team != CharacterBody.Team.Operators) ? OccupantAmmunition : OperatorAmmunition);
		foreach (AmmunitionBase item in list2)
		{
			if ((type == AmmunitionBase.Type.Any || type == item.type) && !(item.calibre.calibreName != caliber.calibreName) && (!matchOnSonic || subsonic == item.Subsonic))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public AmmunitionBase RandomAmmunition(Calibre caliber)
	{
		List<AmmunitionBase> list = GetAmmunition(caliber, AmmunitionBase.Type.Any, CharacterBody.Team.Operators);
		return (list.Count <= 0) ? null : list[Random.Range(0, list.Count)];
	}

	public SerializableWeapon RandomWeapon()
	{
		WeaponBase weaponBase = OperatorWeapons[Random.Range(0, OperatorWeapons.Count)];
		List<AmmunitionBase> list = GetAmmunition(weaponBase.calibre, AmmunitionBase.Type.Any, CharacterBody.Team.Operators);
		AmmunitionBase ammunitionBase = list[Random.Range(0, list.Count)];
		SerializableWeapon serializableWeapon = new SerializableWeapon();
		serializableWeapon.prefabName = weaponBase.name;
		serializableWeapon.ammunitionPrefabName = ammunitionBase.name;
		return serializableWeapon;
	}

	public Item RandomItem()
	{
		return items[Random.Range(0, items.Count)];
	}

	public List<WeaponComponent> ComponentsForWeaponAndFunction(WeaponBase weapon, WeaponComponent.Function function)
	{
		List<WeaponComponentSlot> componentSlots = weapon.componentSlots;
		List<WeaponComponent> list = new List<WeaponComponent>();
		foreach (WeaponComponentSlot item in componentSlots)
		{
			foreach (WeaponComponent component2 in components)
			{
				WeaponComponent component = component2.GetComponent<WeaponComponent>();
				if (component.Compatible(item) && component.functions.Contains(function))
				{
					list.Add(component2);
				}
			}
		}
		return list;
	}

	public List<WeaponComponent> ComponentsForWeaponSlot(WeaponComponentSlot slot)
	{
		List<WeaponComponent> list = new List<WeaponComponent>();
		foreach (WeaponComponent component in components)
		{
			if (component.Compatible(slot))
			{
				list.Add(component);
			}
		}
		return list;
	}

	public void AssignRandomComponents(SerializableWeapon weapon, float noComponentProbability = 1f)
	{
		if (weapon == null)
		{
			return;
		}
		GameObject gameObject = Resources.Load(WeaponBase.WeaponsPath + weapon.prefabName) as GameObject;
		if (gameObject == null)
		{
			return;
		}
		WeaponBase component = gameObject.GetComponent<WeaponBase>();
		if (component == null)
		{
			return;
		}
		foreach (WeaponComponentSlot componentSlot in component.componentSlots)
		{
			List<WeaponComponent> list = ComponentsForWeaponSlot(componentSlot);
			if (!(Random.value >= noComponentProbability) && list.Count != 0)
			{
				WeaponComponent weaponComponent = list[Random.Range(0, list.Count)];
				SerializableWeaponComponentSlot serializableWeaponComponentSlot = componentSlot.Serialize();
				serializableWeaponComponentSlot.component = weaponComponent.name;
				weapon.componentSlots.Add(serializableWeaponComponentSlot);
			}
		}
	}
}
