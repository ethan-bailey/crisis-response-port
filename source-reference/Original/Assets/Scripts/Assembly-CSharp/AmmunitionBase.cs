using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class AmmunitionBase : MonoBehaviour
{
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

	public enum ExplosionMode
	{
		None = 0,
		OnFragmentation = 1,
		Timed = 2,
		TimedAfterFragmentation = 3
	}

	[Serializable]
	public class FragmentationSpecification
	{
		public float mass = 1f;

		public float diameter = 5f;

		public float deflection;

		public FragmentationSpecification()
		{
		}

		public FragmentationSpecification(FragmentationSpecification original)
		{
			mass = original.mass;
			diameter = original.diameter;
			deflection = original.deflection;
		}
	}

	public const string AmmunitionPath = "Weapons/Ammunition/";

	public static float AirDensity = 1.204f;

	public Availability availability;

	public string ammoName = "Name";

	public string shortName = "Short name";

	public string designation;

	public string tooltip = "Ammunition description.";

	public Type type = Type.FMJ;

	public float roundWeight = 1f;

	public float recoil = 1f;

	public bool applyMuzzleBlastForces = true;

	public bool cyclesAction = true;

	public float muzzleBlastRangeModifier = 1f;

	public AnimationCurve muzzleBlastForce;

	public Calibre calibre;

	public Projectile projectile;

	public Casing casing;

	public UIAmmoRenderItem highPolyModel;

	public MarksmanRound marksmanRound;

	public Sprite sprite;

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

	public AnimationCurve expansionFunction;

	public float temporalCavityFactor = 1f;

	public float permanentCavityFactor = 1f;

	public FragmentationMode fragmentationMode;

	public float fragmentMainBodyMass = 0.5f;

	public float fragmentMassMin = 1f;

	public float fragmentMassMax = 2f;

	public float fragmentDiameterMin = 3f;

	public float fragmentDiameterMax = 5f;

	public float fragmentDeflectionMin;

	public float fragmentDeflectionMax = 10f;

	public float fragmentationThreshold = 10f;

	public bool fragmentEvenPattern;

	public List<FragmentationSpecification> specificFragmentation = new List<FragmentationSpecification>();

	public ExplosionMode explosionMode;

	public Explosion explosionPrefab;

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

	public static AmmunitionBase GetAmmunition(string name, AmmunitionBase fallback)
	{
		UnityEngine.Object obj = Resources.Load("Weapons/Ammunition/" + name);
		if (obj == null)
		{
			return fallback;
		}
		return (obj as GameObject).GetComponent<AmmunitionBase>();
	}

	public static string Unabbreviate(Type type)
	{
		switch (type)
		{
		case Type.FMJ:
			return "Full metal jacket";
		case Type.JHP:
			return "Jacketed hollow point";
		case Type.BSP:
			return "Bonded soft point";
		case Type.Shot:
			return "Shot";
		case Type.Slug:
			return "Slug";
		case Type.XP:
			return "Extreme penetrator";
		case Type.BT:
			return "Ballistic tip";
		case Type.RIP:
			return "Really invasive projectile";
		case Type.HE:
			return "High explosive";
		case Type.SP:
			return "Soft point";
		default:
			return string.Empty;
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
}
