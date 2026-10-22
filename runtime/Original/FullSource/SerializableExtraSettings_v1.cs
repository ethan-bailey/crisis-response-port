namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("1", new Type[] { })]
public class SerializableExtraSettings_v1
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

	public float chunkSizeMultiplier = 1f;

	public float marksmanRecoil = 1f;
}

}
