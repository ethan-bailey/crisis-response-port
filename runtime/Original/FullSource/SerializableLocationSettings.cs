namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.IO;
using FullSerializer;
using UnityEngine;

[fsObject("0.0.9.7885", new Type[] { typeof(SerializableLocationSettings_NEXT_AFTER_0_0_8_005) }, MemberSerialization = fsMemberSerialization.OptIn)]
public class SerializableLocationSettings
{
	public enum LevelType
	{
		Default = 0,
		Generated = 1,
		Test = 2
	}

	public const string Filename = "location.dat";

	public const int MinRooms = 1;

	public const int MaxRecommendedRoomCount = 8;

	public const int MaxRooms = 12;

	public const float MinRoomLength = 2f;

	public const float MaxRoomLength = 10f;

	[fsProperty]
	public int levelType;

	[fsProperty]
	public int minRoomCount = 5;

	[fsProperty]
	public int maxRoomCount = 6;

	[fsProperty]
	public float minRoomLength = 2f;

	[fsProperty]
	public float maxRoomLength = 8f;

	[fsProperty]
	public float openDoorsProbability = 0.2f;

	[fsProperty]
	public float lockedDoorsProbability = 0.5f;

	[fsProperty]
	public float riggedDoorsProbability = 0.025f;

	[fsProperty]
	public float obstacleFrequency;

	public SerializableLocationSettings()
	{
	}

	public SerializableLocationSettings(SerializableLocationSettings_NEXT_AFTER_0_0_8_005 data)
	{
		levelType = (data.generateLevel ? 1 : 0);
		minRoomCount = data.minRoomCount;
		maxRoomCount = data.maxRoomCount;
		minRoomLength = data.minRoomLength;
		maxRoomLength = data.maxRoomLength;
		openDoorsProbability = data.openDoorsProbability;
		lockedDoorsProbability = data.lockedDoorsProbability;
		riggedDoorsProbability = data.riggedDoorsProbability;
		obstacleFrequency = data.obstacleFrequency;
	}

	public bool Different(SerializableLocationSettings other)
	{
		bool flag = false;
		flag |= levelType != other.levelType;
		flag |= minRoomCount != other.minRoomCount;
		flag |= maxRoomCount != other.maxRoomCount;
		flag |= minRoomLength != other.minRoomLength;
		flag |= maxRoomLength != other.maxRoomLength;
		flag |= openDoorsProbability != other.openDoorsProbability;
		flag |= lockedDoorsProbability != other.lockedDoorsProbability;
		flag |= riggedDoorsProbability != other.riggedDoorsProbability;
		return flag | (obstacleFrequency != other.obstacleFrequency);
	}

	public void Overwrite(SerializableLocationSettings other)
	{
		levelType = other.levelType;
		minRoomCount = other.minRoomCount;
		maxRoomCount = other.maxRoomCount;
		minRoomLength = other.minRoomLength;
		maxRoomLength = other.maxRoomLength;
		openDoorsProbability = other.openDoorsProbability;
		lockedDoorsProbability = other.lockedDoorsProbability;
		riggedDoorsProbability = other.riggedDoorsProbability;
		obstacleFrequency = other.obstacleFrequency;
	}

	public void Save()
	{
		if (!Serializer.SerializeToDisk(this, typeof(SerializableLocationSettings), Application.persistentDataPath + "/location.dat"))
		{
			Debug.LogWarning("Failed to save location settings.");
		}
	}

	public void Load()
	{
		SerializableLocationSettings serializableLocationSettings = null;
		try
		{
			serializableLocationSettings = (SerializableLocationSettings)Serializer.DeserializeFromDisk(typeof(SerializableLocationSettings), Application.persistentDataPath + "/location.dat");
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
			Global.Context.ClearSettingsFile("location.dat", "location", false);
			Global.Context.MessageBox.ShowOK("Failed to load location settings.\nDefaults have been restored.");
		}
		if (serializableLocationSettings == null)
		{
			serializableLocationSettings = new SerializableLocationSettings();
		}
		Overwrite(serializableLocationSettings);
	}
}

}
