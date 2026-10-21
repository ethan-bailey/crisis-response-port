using UnityEngine;
using Crisis.Runtime.FullSource;
using Utilities;
public partial class AmmunitionBase : MonoBehaviour {
public enum Type
	{
		Any = 0,
		FMJ = 1,
		JHP = 2,
		BSP = 3,
		Shot = 4,
		Slug = 5,
		XP = 6,
		BT = 7,
		RIP = 8,
		HE = 9,
		SP = 10,
		LTL = 11
	}
public enum FragmentationMode
	{
		None = 0,
		Specified = 1,
		Randomized = 2
	}
public string ammoName = "Name";
public string shortName = "Short name";
public string designation;
public string tooltip = "Ammunition description.";
public float roundWeight = 1f;
public float recoil = 1f;
public bool applyMuzzleBlastForces = true;
public bool cyclesAction = true;
public float muzzleBlastRangeModifier = 1f;
public float mass = 1f;
public float diameter = 5f;
public float length = 10f;
public float elasticity;
public float bounceThreshold = 0.1f;
public int projectileCount = 1;
public float spatialDispersion = 0.1f;
public float velocity = 800f;
public float ballisticCoefficient = 0.2f;
public float flightDrag = 2.5f;
public float frictionFactor = 1f;
public float shockModifier = 1f;
public float temporalCavityFactor = 1f;
public float permanentCavityFactor = 1f;
public float fragmentMainBodyMass = 0.5f;
public float fragmentMassMin = 1f;
public float fragmentMassMax = 2f;
public float fragmentDiameterMin = 3f;
public float fragmentDiameterMax = 5f;
public float fragmentDeflectionMin;
public float fragmentDeflectionMax = 10f;
public float fragmentationThreshold = 10f;
public bool fragmentEvenPattern;
public float explosionTime;
public float inaccuracy = 0.065f;
public float velocityModifier = 0.025f;
public float audioPowerModifier = 1f;
public float suppressedAudioPitch = 1.1f;
public float depthModifier = 1f;
public float entryHoleModifier = 1f;
public float exitHoleModifier = 1f;
public float headExitHoleModifier = 1f;
public float burnModifier = 1f;
public Availability availability;
public Type type = Type.FMJ;
public AnimationCurve muzzleBlastForce;
public Calibre calibre;
public Crisis.Runtime.FullSource.Projectile projectile;
public Casing casing;
public UIAmmoRenderItem highPolyModel;
public MarksmanRound marksmanRound;
public Sprite sprite;
public AnimationCurve expansionFunction;
public FragmentationMode fragmentationMode;
public float MetricMass
	{
		get
		{
			return mass * 0.001f;
		}
	}
public float DiameterInMeters
	{
		get
		{
			return diameter * 0.001f;
		}
	}
public float MetricLength
	{
		get
		{
			return length * 0.001f;
		}
	}
public float MuzzleEnergy
	{
		get
		{
			return 0.5f * MetricMass * Mathf.Pow(velocity, 2f);
		}
	}
public float MuzzleEnergyTotal
	{
		get
		{
			return MuzzleEnergy * (float)projectileCount;
		}
	}
public bool Subsonic
	{
		get
		{
			return velocity < 343f;
		}
	}
public float MuzzleBlastRange
	{
		get
		{
			return MuzzleEnergy * 0.00167f * muzzleBlastRangeModifier * (float)projectileCount;
		}
	}
public float DetonationAudioPower
	{
		get
		{
			return MuzzleEnergyTotal * audioPowerModifier;
		}
	}
public float DiameterAtEnergy(float frictionEnergy, float originalDiameterInMillimeters)
	{
		return originalDiameterInMillimeters * expansionFunction.Evaluate(frictionEnergy / MuzzleEnergy);
	}
public float FragmentationEnergy(float mass)
	{
		if (fragmentationMode == FragmentationMode.None)
		{
			return float.PositiveInfinity;
		}
		return mass * fragmentationThreshold;
	}
public float TemporalCavityFactor(float velocity)
	{
		return 1f + Mathf.Tan(0.001f * velocity) * 3.5f * temporalCavityFactor;
	}
public float PermanentCavityFactor(float velocity)
	{
		return 1f + Mathf.Tan(0.00125f * velocity) * permanentCavityFactor;
	}
public static AmmunitionBase GetAmmunition(string name, AmmunitionBase fallback)
	{
		UnityEngine.Object obj = Resources.Load("Weapons/Ammunition/" + name);
		if (obj == null)
		{
			return fallback;
		}
		return (obj as GameObject).GetComponent<AmmunitionBase>();
	}
}
