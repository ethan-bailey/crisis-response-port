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
public class SerializableTimeSettings
{
	[fsIgnore]
	public const string Filename = "time.dat";

	[fsIgnore]
	public static float DoorTriggerAngle = 65f;

	public bool enableBulletTime = true;

	public SerializableBulletTimeEntry playerFire = new SerializableBulletTimeEntry(false, 0.5f, 0.1f, 0.5f, 1f);

	public SerializableBulletTimeEntry marksmanFire = new SerializableBulletTimeEntry(true, 0.3f, 0f, 1f, 3f);

	public SerializableBulletTimeEntry computerFire = new SerializableBulletTimeEntry(false, 0.5f, 0.1f, 0.5f, 1f);

	public SerializableBulletTimeEntry grenadeExplode = new SerializableBulletTimeEntry(true, 0.0005f, 0f, 2f, 3f);

	public SerializableBulletTimeEntry playerOpenDoor = new SerializableBulletTimeEntry(false, 0.5f, 0.25f, 2f, 1f);

	public SerializableBulletTimeEntry playerHit = new SerializableBulletTimeEntry(false, 0.1f, 0.25f, 0.5f, 1f);

	public SerializableBulletTimeEntry computerHit = new SerializableBulletTimeEntry(true, 0.15f, 0f, 0.5f, 1f);

	public float slowMotionRate = 0.5f;

	public float slowMotionEaseInOut = 1f;

	public UnityEventVoid onChanged = new UnityEventVoid();

	public bool Different(SerializableTimeSettings other)
	{
		bool flag = false;
		flag |= enableBulletTime != other.enableBulletTime;
		flag |= playerFire.Different(other.playerFire);
		flag |= marksmanFire.Different(other.marksmanFire);
		flag |= computerFire.Different(other.computerFire);
		flag |= grenadeExplode.Different(other.grenadeExplode);
		flag |= playerOpenDoor.Different(other.playerOpenDoor);
		flag |= playerHit.Different(other.playerHit);
		flag |= computerHit.Different(other.computerHit);
		flag |= slowMotionRate != other.slowMotionRate;
		return flag | (slowMotionEaseInOut != other.slowMotionEaseInOut);
	}

	public void Overwrite(SerializableTimeSettings other)
	{
		enableBulletTime = other.enableBulletTime;
		playerFire = other.playerFire;
		marksmanFire = other.marksmanFire;
		computerFire = other.computerFire;
		grenadeExplode = other.grenadeExplode;
		playerOpenDoor = other.playerOpenDoor;
		playerHit = other.playerHit;
		computerHit = other.computerHit;
		slowMotionRate = other.slowMotionRate;
		slowMotionEaseInOut = other.slowMotionEaseInOut;
	}

	public void Save()
	{
		if (!Serializer.SerializeToDisk(this, typeof(SerializableTimeSettings), Application.persistentDataPath + "/time.dat"))
		{
			Debug.LogWarning("Failed to save time settings.");
		}
		onChanged.Invoke();
	}

	public void Load()
	{
		SerializableTimeSettings serializableTimeSettings = null;
		try
		{
			serializableTimeSettings = (SerializableTimeSettings)Serializer.DeserializeFromDisk(typeof(SerializableTimeSettings), Application.persistentDataPath + "/time.dat");
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
			Global.Context.ClearSettingsFile("time.dat", "time", false);
			Global.Context.MessageBox.ShowOK("Failed to load time settings.\nDefaults have been restored.");
		}
		if (serializableTimeSettings == null)
		{
			serializableTimeSettings = new SerializableTimeSettings();
		}
		Overwrite(serializableTimeSettings);
	}
}

}
