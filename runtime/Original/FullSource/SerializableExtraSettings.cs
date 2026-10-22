using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using System;
using System.IO;
using FullSerializer;
using UnityEngine;
using UnityEngine.Events;

[fsObject("0_0_9_7313", new Type[] { typeof(SerializableExtraSettings_0_0_9_4792) }, MemberSerialization = fsMemberSerialization.OptIn)]
public class SerializableExtraSettings
{
	public class ExtraSettingsChangedEvent : UnityEvent<SerializableExtraSettings>
	{
	}

	public const string Filename = "extras.dat";

	[fsProperty]
	public bool playerSpectator;

	[fsProperty]
	public bool playerInvisible;

	[fsProperty]
	public bool playerInvulnerable;

	[fsProperty]
	public bool playerDoesNotLimp;

	[fsProperty]
	public bool playerMarksmanControl = true;

	[fsProperty]
	public bool disableOccupants;

	[fsProperty]
	public bool disableLocks;

	[fsProperty]
	public bool floatingBodies;

	[fsProperty]
	public bool pinnedBodies;

	[fsProperty]
	public bool announceHits;

	[fsProperty]
	public bool showPhysiologyData;

	[fsProperty]
	public bool weaponMalfunctions;

	[fsProperty]
	public float forceMultiplier = 1f;

	[fsProperty]
	public float emissionsModifier = 1f;

	[fsProperty]
	public float woundPowerMultiplier = 1f;

	[fsProperty]
	public float chunkSizeMultiplier = 1f;

	[fsProperty]
	public float malfunctionsMultiplier = 1f;

	[fsProperty]
	public float marksmanRecoil = 1f;

	[fsProperty]
	public bool disableFireAlarms;

	[fsProperty]
	public bool disableExteriorWalls;

	[fsProperty]
	public bool disableEndCriteria;

	public ExtraSettingsChangedEvent onChanged = new ExtraSettingsChangedEvent();

	public UnityEventBool onShowPhysiologyChanged = new UnityEventBool();

	public SerializableExtraSettings()
	{
	}

	public SerializableExtraSettings(SerializableExtraSettings_0_0_9_4792 data)
	{
		playerSpectator = data.playerSpectator;
		playerInvisible = data.playerInvisible;
		playerInvulnerable = data.playerInvulnerable;
		playerDoesNotLimp = data.playerDoesNotLimp;
		playerMarksmanControl = data.playerMarksmanControl;
		disableOccupants = data.disableOccupants;
		disableLocks = data.disableLocks;
		floatingBodies = data.floatingBodies;
		pinnedBodies = data.pinnedBodies;
		announceHits = data.announceHits;
		showPhysiologyData = data.showPhysiologyData;
		forceMultiplier = data.forceMultiplier;
		emissionsModifier = data.emissionsModifier;
		woundPowerMultiplier = data.woundPowerMultiplier;
		chunkSizeMultiplier = data.chunkSizeMultiplier;
		marksmanRecoil = data.marksmanRecoil;
		disableFireAlarms = data.disableFireAlarms;
		disableExteriorWalls = data.disableExteriorWalls;
		disableEndCriteria = data.disableEndCriteria;
		weaponMalfunctions = false;
		malfunctionsMultiplier = 1f;
	}

	public bool Different(SerializableExtraSettings other)
	{
		bool flag = false;
		flag |= playerSpectator != other.playerSpectator;
		flag |= playerInvisible != other.playerInvisible;
		flag |= playerInvulnerable != other.playerInvulnerable;
		flag |= playerDoesNotLimp != other.playerDoesNotLimp;
		flag |= playerMarksmanControl != other.playerMarksmanControl;
		flag |= disableOccupants != other.disableOccupants;
		flag |= disableLocks != other.disableLocks;
		flag |= floatingBodies != other.floatingBodies;
		flag |= pinnedBodies != other.pinnedBodies;
		flag |= announceHits != other.announceHits;
		flag |= showPhysiologyData != other.showPhysiologyData;
		flag |= weaponMalfunctions != other.weaponMalfunctions;
		flag |= forceMultiplier != other.forceMultiplier;
		flag |= emissionsModifier != other.emissionsModifier;
		flag |= woundPowerMultiplier != other.woundPowerMultiplier;
		flag |= chunkSizeMultiplier != other.chunkSizeMultiplier;
		flag |= malfunctionsMultiplier != other.malfunctionsMultiplier;
		flag |= marksmanRecoil != other.marksmanRecoil;
		flag |= disableFireAlarms != other.disableFireAlarms;
		flag |= disableExteriorWalls != other.disableExteriorWalls;
		return flag | (disableEndCriteria != other.disableEndCriteria);
	}

	public void Overwrite(SerializableExtraSettings other)
	{
		playerSpectator = other.playerSpectator;
		playerInvisible = other.playerInvisible;
		playerInvulnerable = other.playerInvulnerable;
		playerDoesNotLimp = other.playerDoesNotLimp;
		playerMarksmanControl = other.playerMarksmanControl;
		disableOccupants = other.disableOccupants;
		disableLocks = other.disableLocks;
		floatingBodies = other.floatingBodies;
		pinnedBodies = other.pinnedBodies;
		announceHits = other.announceHits;
		showPhysiologyData = other.showPhysiologyData;
		weaponMalfunctions = other.weaponMalfunctions;
		forceMultiplier = other.forceMultiplier;
		emissionsModifier = other.emissionsModifier;
		woundPowerMultiplier = other.woundPowerMultiplier;
		chunkSizeMultiplier = other.chunkSizeMultiplier;
		malfunctionsMultiplier = other.malfunctionsMultiplier;
		marksmanRecoil = other.marksmanRecoil;
		disableFireAlarms = other.disableFireAlarms;
		disableExteriorWalls = other.disableExteriorWalls;
		disableEndCriteria = other.disableEndCriteria;
	}

	public void Save()
	{
		if (!Serializer.SerializeToDisk(this, typeof(SerializableExtraSettings), Application.persistentDataPath + "/extras.dat"))
		{
			Debug.LogWarning("Failed to save extra settings.");
		}
		onChanged.Invoke(this);
	}

	public void Load()
	{
		SerializableExtraSettings serializableExtraSettings = null;
		try
		{
			serializableExtraSettings = (SerializableExtraSettings)Serializer.DeserializeFromDisk(typeof(SerializableExtraSettings), Application.persistentDataPath + "/extras.dat");
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
			Global.Context.ClearSettingsFile("extras.dat", "extra", false);
			Global.Context.MessageBox.ShowOK("Failed to load extra settings.\nDefaults have been restored.");
		}
		if (serializableExtraSettings == null)
		{
			serializableExtraSettings = new SerializableExtraSettings();
		}
		Overwrite(serializableExtraSettings);
	}

	public void ClearMissionListeners()
	{
		onShowPhysiologyChanged.RemoveAllListeners();
	}
}

}
