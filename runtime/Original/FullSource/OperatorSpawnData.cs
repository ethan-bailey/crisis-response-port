namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OperatorSpawnData : MonoBehaviour
{
	public const int DefaultNumberOfOperators = 4;

	public int numberOfOperatorsInEditor = 1;

	public int maxNumberOfOperators = 7;

	public SerializableTeam Team { get; private set; }

	public void InstantiateDefaultTeam()
	{
		if (Team == null)
		{
			Team = new SerializableTeam();
		}
		if (Team.entryMembers == null)
		{
			Team.entryMembers = new List<SerializableEquipment>();
		}
		Transform equipmentContainer = Global.Context.MainMenu.equipmentContainer;
		while (equipmentContainer.childCount < 4)
		{
			InstantiateDefaultEquipment();
		}
	}

	public void InstantiateDefaultEquipment()
	{
		Transform equipmentContainer = Global.Context.MainMenu.equipmentContainer;
		if (equipmentContainer.childCount < maxNumberOfOperators)
		{
			SerializableWeapon serializableWeapon = new SerializableWeapon();
			serializableWeapon.prefabName = "M4A1";
			serializableWeapon.ammunitionPrefabName = "5.56-FMJ";
			SerializableWeapon serializableWeapon2 = serializableWeapon;
			serializableWeapon = new SerializableWeapon();
			serializableWeapon.prefabName = "Glock-18";
			serializableWeapon.ammunitionPrefabName = "9-FMJ";
			SerializableWeapon serializableWeapon3 = serializableWeapon;
			SerializableItems serializableItems = new SerializableItems();
			serializableItems.itemPrefabNames = new List<string> { "Primary magazine", "Secondary magazine", "Plasticuffs", "Gauze", "Stun grenade", "Stun grenade", "Stun grenade", "Stun grenade" };
			SerializableItems items = serializableItems;
			ArmorClass.Type armor = ArmorClass.Type.TypeIV;
			GameObject gameObject = EquipmentHelper.Instance.SpawnEquipment(serializableWeapon2, serializableWeapon3, items, armor);
			gameObject.transform.SetParent(equipmentContainer);
			if (Team.entryMembers == null)
			{
				Team.entryMembers = new List<SerializableEquipment>();
			}
			Team.entryMembers.Add(new SerializableEquipment
			{
				primary = serializableWeapon2,
				secondary = serializableWeapon3,
				items = items,
				armor = armor
			});
		}
	}

	public void Save()
	{
		if (!Serializer.SerializeToDisk(Team, typeof(SerializableTeam), Application.persistentDataPath + "/operators.dat"))
		{
			Debug.LogWarning("Failed to save operator settings.");
		}
	}

	public void Load()
	{
		SerializableTeam serializableTeam = null;
		try
		{
			serializableTeam = (SerializableTeam)Serializer.DeserializeFromDisk(typeof(SerializableTeam), Application.persistentDataPath + "/operators.dat");
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
			Global.Context.ClearSettingsFile("operators.dat", "operators", false);
			Global.Context.MessageBox.ShowOK("Failed to load operators.\nDefaults have been restored.");
		}
		if (serializableTeam == null)
		{
			InstantiateDefaultTeam();
		}
		else
		{
			Team = serializableTeam;
		}
	}
}

}
