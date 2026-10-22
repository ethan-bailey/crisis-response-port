namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.IO;
using FullSerializer;
using UnityEngine;
using UnityEngine.Events;

[fsObject("0_0_9_4792", new Type[] { typeof(SerializableGeneralSettings_NEXT_AFTER_1) }, MemberSerialization = fsMemberSerialization.OptIn)]
public class SerializableGeneralSettings
{
	public class GeneralSettingsChangedEvent : UnityEvent<SerializableGeneralSettings>
	{
	}

	public enum OnOffEnum
	{
		Off = 0,
		On = 1
	}

	public enum GrenadeTimerMode
	{
		Never = 0,
		OnlyForPlayer = 1,
		OnlyWhenBaking = 2,
		Always = 3
	}

	public const string Filename = "general.dat";

	[fsProperty]
	public bool lightOn;

	[fsProperty]
	public bool laserOn;

	[fsProperty]
	public bool showInteractionMarkers = true;

	[fsProperty]
	public bool reloadHelp = true;

	[fsProperty]
	public bool showMarksmanTargetIndication = true;

	[fsProperty]
	public bool enableMarksmanBobbing;

	[fsProperty]
	public float interiorAimSensitivity = 1f;

	[fsProperty]
	public float marksmanAimSensitivity = 1f;

	[fsProperty]
	public float marksmanReloadSensitivity = 1f;

	[fsProperty]
	public int grenadeTimerMode = 3;

	[fsProperty]
	public WeaponBase.FireMode pistolFireMode = WeaponBase.FireMode.Semi;

	[fsProperty]
	public WeaponBase.FireMode smgFireMode = WeaponBase.FireMode.Auto;

	[fsProperty]
	public WeaponBase.FireMode shotgunFireMode = WeaponBase.FireMode.Semi;

	[fsProperty]
	public WeaponBase.FireMode rifleFireMode = WeaponBase.FireMode.Auto;

	public GeneralSettingsChangedEvent onChanged = new GeneralSettingsChangedEvent();

	public UnityEventBool onInteractionMarkersChanged = new UnityEventBool();

	public UnityEventBool onMarksmanIndicationChanged = new UnityEventBool();

	public GrenadeTimerMode GetGrenadeTimerMode
	{
		get
		{
			return (GrenadeTimerMode)grenadeTimerMode;
		}
	}

	public SerializableGeneralSettings()
	{
	}

	public SerializableGeneralSettings(SerializableGeneralSettings_NEXT_AFTER_1 other)
	{
		pistolFireMode = other.pistolFireMode;
		smgFireMode = other.smgFireMode;
		shotgunFireMode = other.shotgunFireMode;
		rifleFireMode = other.rifleFireMode;
		lightOn = other.lightOn;
		laserOn = other.laserOn;
		showInteractionMarkers = other.showInteractionMarkers;
		reloadHelp = other.reloadHelp;
		showMarksmanTargetIndication = other.showMarksmanTargetIndication;
		marksmanAimSensitivity = other.marksmanMouseSensitivity;
		grenadeTimerMode = 2;
		enableMarksmanBobbing = !other.disableMarksmanBobbing;
		marksmanReloadSensitivity = 1f;
	}

	public bool Different(SerializableGeneralSettings other)
	{
		bool flag = false;
		flag |= pistolFireMode != other.pistolFireMode;
		flag |= smgFireMode != other.smgFireMode;
		flag |= shotgunFireMode != other.shotgunFireMode;
		flag |= rifleFireMode != other.rifleFireMode;
		flag |= lightOn != other.lightOn;
		flag |= laserOn != other.laserOn;
		flag |= showInteractionMarkers != other.showInteractionMarkers;
		flag |= reloadHelp != other.reloadHelp;
		flag |= showMarksmanTargetIndication != other.showMarksmanTargetIndication;
		flag |= enableMarksmanBobbing != other.enableMarksmanBobbing;
		flag |= interiorAimSensitivity != other.interiorAimSensitivity;
		flag |= marksmanAimSensitivity != other.marksmanAimSensitivity;
		flag |= marksmanReloadSensitivity != other.marksmanReloadSensitivity;
		return flag | (grenadeTimerMode != other.grenadeTimerMode);
	}

	public void Overwrite(SerializableGeneralSettings other)
	{
		pistolFireMode = other.pistolFireMode;
		smgFireMode = other.smgFireMode;
		shotgunFireMode = other.shotgunFireMode;
		rifleFireMode = other.rifleFireMode;
		laserOn = other.laserOn;
		lightOn = other.lightOn;
		showInteractionMarkers = other.showInteractionMarkers;
		reloadHelp = other.reloadHelp;
		showMarksmanTargetIndication = other.showMarksmanTargetIndication;
		enableMarksmanBobbing = other.enableMarksmanBobbing;
		interiorAimSensitivity = other.interiorAimSensitivity;
		marksmanAimSensitivity = other.marksmanAimSensitivity;
		marksmanReloadSensitivity = other.marksmanReloadSensitivity;
		grenadeTimerMode = other.grenadeTimerMode;
	}

	public SerializableGeneralSettings Copy()
	{
		SerializableGeneralSettings serializableGeneralSettings = new SerializableGeneralSettings();
		serializableGeneralSettings.Overwrite(this);
		return serializableGeneralSettings;
	}

	public void Save()
	{
		if (!Serializer.SerializeToDisk(this, typeof(SerializableGeneralSettings), Application.persistentDataPath + "/general.dat"))
		{
			Debug.LogWarning("Failed to save general settings.");
		}
		onChanged.Invoke(this);
	}

	public void Load()
	{
		SerializableGeneralSettings serializableGeneralSettings = null;
		try
		{
			serializableGeneralSettings = (SerializableGeneralSettings)Serializer.DeserializeFromDisk(typeof(SerializableGeneralSettings), Application.persistentDataPath + "/general.dat");
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
			Global.Context.ClearSettingsFile("general.dat", "general", false);
			Global.Context.MessageBox.ShowOK("Failed to load general settings.\nDefaults have been restored.");
		}
		if (serializableGeneralSettings == null)
		{
			serializableGeneralSettings = new SerializableGeneralSettings();
		}
		Overwrite(serializableGeneralSettings);
	}

	public WeaponBase.FireMode FireMode(WeaponType weaponType)
	{
		switch (weaponType)
		{
		case WeaponType.Pistol:
			return pistolFireMode;
		case WeaponType.SMG:
			return smgFireMode;
		case WeaponType.Shotgun:
			return shotgunFireMode;
		case WeaponType.Rifle:
			return rifleFireMode;
		default:
			return WeaponBase.FireMode.Auto;
		}
	}

	public void ClearMissionListeners()
	{
		onInteractionMarkersChanged.RemoveAllListeners();
		onMarksmanIndicationChanged.RemoveAllListeners();
	}
}

}
