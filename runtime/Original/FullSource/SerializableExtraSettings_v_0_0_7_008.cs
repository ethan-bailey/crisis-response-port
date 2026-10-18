namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("0.0.7.008", new Type[] { typeof(SerializableExtraSettings_v1) })]
public class SerializableExtraSettings_v_0_0_7_008
{
	public bool playerSpectator;

	public bool playerInvisible;

	public bool playerInvulnerable;

	public bool playerDoesNotLimp;

	public bool playerMarksmanControl = true;

	public bool disableOccupants;

	public bool disableLocks;

	public bool floatingBodies;

	public bool pinnedBodies;

	public bool announceHits;

	public bool showPhysiologyData;

	public float forceMultiplier = 1f;

	public float emissionsModifier = 1f;

	public float woundPowerMultiplier = 1f;

	public float woundStreakLifetimeMultiplier = 1f;

	public float chunkSizeMultiplier = 1f;

	public float marksmanRecoil = 1f;

	public float entrySizeModifier = 1f;

	public float exitSizeModifier = 1f;

	public SerializableExtraSettings_v_0_0_7_008()
	{
	}

	public SerializableExtraSettings_v_0_0_7_008(SerializableExtraSettings_v1 v1Data)
	{
		playerSpectator = v1Data.playerSpectator;
		playerInvisible = v1Data.playerInvisible;
		playerInvulnerable = v1Data.playerInvulnerable;
		playerDoesNotLimp = v1Data.playerDoesNotLimp;
		playerMarksmanControl = v1Data.playerMarksmanControl;
		disableOccupants = v1Data.disableOccupants;
		disableLocks = v1Data.disableLocks;
		floatingBodies = v1Data.floatingBodies;
		pinnedBodies = v1Data.pinnedBodies;
		announceHits = v1Data.announceHits;
		showPhysiologyData = v1Data.showPhysiologyData;
		forceMultiplier = v1Data.forceMultiplier;
		emissionsModifier = v1Data.emissionsModifier;
		woundPowerMultiplier = v1Data.woundPowerMultiplier;
		chunkSizeMultiplier = v1Data.chunkSizeMultiplier;
		marksmanRecoil = v1Data.marksmanRecoil;
		entrySizeModifier = 1f;
		exitSizeModifier = 1f;
		woundStreakLifetimeMultiplier = 1f;
	}
}

}
