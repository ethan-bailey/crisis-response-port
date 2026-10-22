namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class WeaponComponentSuppressor : WeaponComponent
{
	[Tooltip("Power factor: 0 = Inaudible, 1 = Suppressor does nothing. How the suppressor affects the audio power at the muzzle.")]
	[Range(0f, 1f)]
	public float powerFactor = 0.5f;

	[Tooltip("Modifies only the volume of the firearm's detonation sample, as perceived by the human player.")]
	public float sampleVolumeFactor = 0.65f;

	[Tooltip("Recoil reduction in percent. A suppressor generally reduces recoil by 20 to 30 percent.")]
	[Range(0.2f, 0.3f)]
	public float recoilReduction = 0.25f;

	public Transform muzzle;

	public float RecoilModifier
	{
		get
		{
			return 1f - recoilReduction;
		}
	}

	public override void ClearWeaponReferences()
	{
		base.Weapon.ComponentSuppressor = null;
	}
}

}
