using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;

public class Equipment : MonoBehaviour
{
	public WeaponBase primaryWeapon;

	public WeaponBase secondaryWeapon;

	public int primaryMagazineCount;

	public int secondaryMagazineCount;

	public int stunGrenades;

	public int fragGrenades;

	public bool plasticuffs;

	public bool multiTool;

	public int gauze;

	public List<Magazine> PrimaryMagazines { get; private set; }

	public List<Magazine> SecondaryMagazines { get; private set; }

	public AmmunitionStack PrimaryRounds { get; private set; }

	public AmmunitionStack SecondaryRounds { get; private set; }

	public void Initialize()
	{
		PrimaryMagazines = new List<Magazine>();
		SecondaryMagazines = new List<Magazine>();
		PrimaryRounds = new AmmunitionStack();
		SecondaryRounds = new AmmunitionStack();
		InitializeWeaponAmmunition(primaryWeapon, primaryMagazineCount, PrimaryMagazines, PrimaryRounds);
		InitializeWeaponAmmunition(secondaryWeapon, secondaryMagazineCount, SecondaryMagazines, SecondaryRounds);
	}

	private void InitializeWeaponAmmunition(WeaponBase weapon, int magazineCount, List<Magazine> magazineStack, AmmunitionStack ammunitionStack)
	{
		if (weapon == null)
		{
			return;
		}
		if (weapon.useInternalMagazine)
		{
			ammunitionStack.count = magazineCount * weapon.ComponentMagazine.capacity;
			ammunitionStack.caliber = weapon.calibre;
		}
		else if (weapon.ComponentMagazine != null)
		{
			for (int i = 0; i < magazineCount; i++)
			{
				magazineStack.Add(new Magazine(weapon));
			}
		}
		else
		{
			Debug.LogWarningFormat("Initialize weapon ammunition failed: {0}", weapon.name);
		}
	}

	public bool HasAmmunitionForWeapon(WeaponBase weapon)
	{
		int looseAmmoCountForWeapon = GetLooseAmmoCountForWeapon(weapon);
		if (looseAmmoCountForWeapon > 0)
		{
			return true;
		}
		Magazine fullestMagazineForWeapon = GetFullestMagazineForWeapon(weapon);
		if (fullestMagazineForWeapon != null)
		{
			return true;
		}
		return false;
	}

	public WeaponBase GetNonEmptyWeapon()
	{
		if (primaryWeapon != null && primaryWeapon.Magazine != null && !primaryWeapon.Magazine.Empty)
		{
			return primaryWeapon;
		}
		if (secondaryWeapon != null && secondaryWeapon.Magazine != null && !secondaryWeapon.Magazine.Empty)
		{
			return secondaryWeapon;
		}
		return null;
	}

	public int GetLooseAmmoCountForWeapon(WeaponBase weapon)
	{
		if (weapon == null)
		{
			return 0;
		}
		if (weapon.IsPrimary)
		{
			return (PrimaryRounds.caliber == weapon.calibre) ? PrimaryRounds.count : 0;
		}
		return (SecondaryRounds.caliber == weapon.calibre) ? SecondaryRounds.count : 0;
	}

	public Magazine GetFullestMagazineForWeapon(WeaponBase weapon)
	{
		Magazine fullestMagazineFromStack = GetFullestMagazineFromStack(weapon.calibre, (!weapon.IsPrimary) ? SecondaryMagazines : PrimaryMagazines);
		int num = ((fullestMagazineFromStack != null) ? fullestMagazineFromStack.roundCount : 0);
		return (num <= 0) ? null : fullestMagazineFromStack;
	}

	private Magazine GetFullestMagazineFromStack(string caliber, List<Magazine> magazineStack)
	{
		Magazine result = null;
		int num = 0;
		foreach (Magazine item in magazineStack)
		{
			if (!(item.calibre != caliber) && item.roundCount > num)
			{
				result = item;
				num = item.roundCount;
			}
		}
		return result;
	}

	public void AddMagazineForWeapon(Magazine magazine, WeaponBase weapon)
	{
		if (weapon.IsPrimary)
		{
			PrimaryMagazines.Add(magazine);
		}
		else
		{
			SecondaryMagazines.Add(magazine);
		}
	}
}
