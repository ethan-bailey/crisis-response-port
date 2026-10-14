using UnityEngine;
public class Projectile : MonoBehaviour {
public Transform meshTransform;
private float diameter;
private float freeFallTimestamp;
public float FrictionEnergy { get; private set; }
public AmmunitionBase Ammunition { get; private set; }
public Vector3 StartPosition { get; set; }
public float Mass { get; set; }
public float MetricMass
	{
		get
		{
			return Mass * 0.001f;
		}
	}
public float BaseVelocity { get; set; }
public float StartEnergy { get; private set; }
public float Diameter
	{
		get
		{
			return DiameterAtFrictionEnergy(FrictionEnergy);
		}
	}
public float MetricDiameter
	{
		get
		{
			return Diameter * 0.001f;
		}
	}
public float FrictionEnergyNormalized
	{
		get
		{
			return Mathf.Clamp01(FrictionEnergy / StartEnergy);
		}
	}
public float DeformationRatio
	{
		get
		{
			return Ammunition.expansionFunction.Evaluate(FrictionEnergyNormalized);
		}
	}
public float BallisticCoefficient
	{
		get
		{
			return (1f - FrictionEnergyNormalized) * Ammunition.ballisticCoefficient;
		}
	}
public bool SuperSonic
	{
		get
		{
			return BaseVelocity >= 343f;
		}
	}
public float TemporalCavityFactor
	{
		get
		{
			return Ammunition.TemporalCavityFactor(Velocity);
		}
	}
public float PermanentCavityFactor
	{
		get
		{
			return Ammunition.PermanentCavityFactor(Velocity);
		}
	}
public float PenetrationDistance { get; set; }
public float DisplacedMaterialVolume { get; set; }
public float InteractionEnergy { get; set; }
public float Distance
	{
		get
		{
			return DistanceAtPosition(base.transform.position);
		}
	}
public float Velocity { get; private set; }
public float Drag
	{
		get
		{
			return (1f - BallisticCoefficient) * Ammunition.flightDrag;
		}
	}
public float PotentialFlightRangeLeft
	{
		get
		{
			return VelocityAtPosition(base.transform.position) / Drag;
		}
	}
public float Energy
	{
		get
		{
			return KineticEnergy(MetricMass, Velocity);
		}
	}
public float EnergyNormalized
	{
		get
		{
			return Energy / Ammunition.MuzzleEnergy;
		}
	}
public bool HasFragmented { get; set; }
public static float KineticEnergy(float massInKilograms, float velocityInMetersPerSecond)
	{
		return 0.5f * massInKilograms * Mathf.Pow(velocityInMetersPerSecond, 2f);
	}
public void RecalculateVelocity()
	{
		Velocity = Mathf.Max(VelocityAtPosition(base.transform.position), 0f);
	}
public float DistanceAtPosition(Vector3 worldPosition)
	{
		return (worldPosition - StartPosition).magnitude;
	}
public float DragVelocityAtPosition(Vector3 worldPosition)
	{
		return DistanceAtPosition(worldPosition) * Drag;
	}
public float VelocityAtPosition(Vector3 worldPosition)
	{
		return Mathf.Max(BaseVelocity - DragVelocityAtPosition(worldPosition), 0f);
	}
public Vector3 GravityVelocityAtTime(float time)
	{
		return new Vector3(0f, -9.82f * (time - freeFallTimestamp), 0f);
	}
public float DiameterAtFrictionEnergy(float frictionEnergy, bool skipRotation = false)
	{
		float originalDiameterInMillimeters = diameter;
		if (!skipRotation)
		{
			float t = Vector3.Dot(base.transform.forward, meshTransform.forward);
			originalDiameterInMillimeters = Mathf.Lerp(Ammunition.length, diameter, t);
		}
		return Ammunition.DiameterAtEnergy(frictionEnergy, originalDiameterInMillimeters);
	}
public float VelocityToEnergy(float velocity)
	{
		return 0.5f * MetricMass * Mathf.Pow(velocity, 2f);
	}
public void RemoveEnergy(float energy)
	{
		BaseVelocity = Mathf.Sqrt(Mathf.Max(Mathf.Pow(BaseVelocity, 2f) - energy / (0.5f * MetricMass), 0f));
	}
public float ConvertEnergyToFriction(float energy)
	{
		FrictionEnergy += energy;
		InteractionEnergy += energy;
		RemoveEnergy(energy);
		RecalculateVelocity();
		return 0f;
	}
public void ConvertRemainingEnergyToFriction()
	{
		FrictionEnergy += Energy;
		InteractionEnergy += Energy;
		BaseVelocity = 0f;
		Velocity = 0f;
	}
public float PotentialPenetrationDistance(float energy, float density, float diameterInMillimeters, float frictionFactor)
	{
		return Mathf.Atan(energy / (Mathf.Pow(density, 2f) * frictionFactor * 0.001f * diameterInMillimeters));
	}
public float PotentialPenetrationDistance(float density)
	{
		return PotentialPenetrationDistance(Energy, density, Diameter, Ammunition.frictionFactor);
	}
public float EnergyToPenetrate(float distance, float density)
	{
		return EnergyToPenetrate(distance, density, Ammunition.frictionFactor, Diameter);
	}
public float EnergyToPenetrate(float distance, float density, float frictionFactor, float diameterInMillimeters)
	{
		return Mathf.Tan(distance) * Mathf.Pow(density, 2f) * frictionFactor * 0.001f * diameterInMillimeters;
	}
public bool TestFragmentation()
	{
		if (HasFragmented)
		{
			return false;
		}
		return FrictionEnergy >= Ammunition.FragmentationEnergy(Mass);
	}
public static float MaximumRange(float muzzleVelocity, float ballisticCoefficient, float flightDrag)
	{
		return muzzleVelocity / (1f - 1f * ballisticCoefficient * flightDrag);
	}
}
