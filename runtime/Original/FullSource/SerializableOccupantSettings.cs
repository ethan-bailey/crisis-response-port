namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.IO;
using FullSerializer;
using UnityEngine;

[fsObject("1", new Type[] { })]
public class SerializableOccupantSettings
{
	[fsIgnore]
	public const string Filename = "occupants.dat";

	[fsIgnore]
	public const int MaxRecommendedSpawnCount = 8;

	[fsIgnore]
	public const int MaxAllowedSpawnCount = 16;

	public int minCount = 3;

	public int maxCount = 6;

	public SerializableRangeSlider morale = new SerializableRangeSlider(0f, 1f);

	public SerializableRangeSlider aggression = new SerializableRangeSlider(0f, 1f);

	public SerializableRangeSlider fear = new SerializableRangeSlider(0f, 1f);

	public SerializableRangeSlider weaponSkill = new SerializableRangeSlider(0f, 1f);

	public SerializableRangeSlider shockResistance = new SerializableRangeSlider(0f, 0.5f);

	public bool handguns = true;

	public bool smgs = true;

	public bool shotguns = true;

	public bool rifles = true;

	public bool Different(SerializableOccupantSettings other)
	{
		bool flag = false;
		flag |= minCount != other.minCount;
		flag |= maxCount != other.maxCount;
		flag |= morale.Different(other.morale);
		flag |= aggression.Different(other.aggression);
		flag |= fear.Different(other.fear);
		flag |= weaponSkill.Different(other.weaponSkill);
		flag |= shockResistance.Different(other.shockResistance);
		flag |= handguns != other.handguns;
		flag |= smgs != other.smgs;
		flag |= shotguns != other.shotguns;
		return flag | (rifles != other.rifles);
	}

	public void Overwrite(SerializableOccupantSettings other)
	{
		minCount = other.minCount;
		maxCount = other.maxCount;
		morale = other.morale;
		aggression = other.aggression;
		fear = other.fear;
		weaponSkill = other.weaponSkill;
		shockResistance = other.shockResistance;
		handguns = other.handguns;
		smgs = other.smgs;
		shotguns = other.shotguns;
		rifles = other.rifles;
	}

	public void Save()
	{
		if (!Serializer.SerializeToDisk(this, typeof(SerializableOccupantSettings), Application.persistentDataPath + "/occupants.dat"))
		{
			Debug.LogWarning("Failed to save occupant settings.");
		}
	}

	public void Load()
	{
		SerializableOccupantSettings serializableOccupantSettings = null;
		try
		{
			serializableOccupantSettings = (SerializableOccupantSettings)Serializer.DeserializeFromDisk(typeof(SerializableOccupantSettings), Application.persistentDataPath + "/occupants.dat");
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
			Global.Context.ClearSettingsFile("occupants.dat", "occupant", false);
			Global.Context.MessageBox.ShowOK("Failed to load occupant settings.\nDefaults have been restored.");
		}
		if (serializableOccupantSettings == null)
		{
			serializableOccupantSettings = new SerializableOccupantSettings();
		}
		Overwrite(serializableOccupantSettings);
	}
}

}
