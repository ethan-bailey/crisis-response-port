namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("0_0_9_4792", new Type[] { typeof(SerializableExtraSettings_NEXT_AFTER_0_0_7_008) }, MemberSerialization = fsMemberSerialization.OptIn)]
public class SerializableExtraSettings_0_0_9_4792
{
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
	public float forceMultiplier = 1f;

	[fsProperty]
	public float emissionsModifier = 1f;

	[fsProperty]
	public float woundPowerMultiplier = 1f;

	[fsProperty]
	public float chunkSizeMultiplier = 1f;

	[fsProperty]
	public float marksmanRecoil = 1f;

	[fsProperty]
	public bool disableFireAlarms;

	[fsProperty]
	public bool disableExteriorWalls;

	[fsProperty]
	public bool disableEndCriteria;

	public SerializableExtraSettings_0_0_9_4792()
	{
	}

	public SerializableExtraSettings_0_0_9_4792(SerializableExtraSettings_NEXT_AFTER_0_0_7_008 data)
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
		disableFireAlarms = false;
		disableExteriorWalls = false;
		disableEndCriteria = false;
	}
}

}
