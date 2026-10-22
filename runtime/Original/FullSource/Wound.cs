using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class Wound
{
	public float available;

	private Vector3 streamVelocity;

	private float multiplier = 1f;

	private float pressure;

	private bool useParticles;

	public Limb Limb { get; private set; }

	public Vector3 LocalPosition { get; private set; }

	public Vector3 LocalDirection { get; private set; }

	public Vector3 LocalBloodDecal { get; private set; }

	public BloodStreamGenerator Stream { get; private set; }

	public float Diameter { get; private set; }

	public float EmissionRadiusMeters { get; private set; }

	public float Timestamp { get; set; }

	public Wound(Limb limb, Vector3 localPosition, Vector3 localDirection, float diameterMillimeters, float emissionRadiusMeters, bool useParticles, bool useStreams, float multiplier)
	{
		Limb = limb;
		LocalPosition = localPosition;
		LocalDirection = localDirection;
		Diameter = diameterMillimeters;
		EmissionRadiusMeters = emissionRadiusMeters;
		available = Random.Range(0f, 1f);
		this.useParticles = useParticles;
		this.multiplier = multiplier;
		Stream = ((!useStreams) ? null : new BloodStreamGenerator(limb.WoundEmitter));
	}

	public float Update(float pressure)
	{
		float num = pressure * Diameter + available;
		float num2 = num * Time.deltaTime;
		Vector3 vector = Limb.transform.TransformPoint(LocalPosition + Random.insideUnitSphere * Diameter * 0.0005f);
		Vector3 vector2 = Limb.transform.TransformDirection(LocalDirection);
		float num3 = pressure * (10f / Diameter) * multiplier;
		streamVelocity = vector2 * num3;
		this.pressure = pressure;
		if (Limb.LimbPhysiology.WoundCavityVolume > 0f)
		{
			if ((Stream == null || !Stream.Update(pressure * 0.3f, vector, streamVelocity)) && useParticles)
			{
				int num4 = Mathf.FloorToInt(num2);
				Limb.WoundEmission(num4, vector, vector2, num3, EmissionRadiusMeters);
				if (num4 == 0)
				{
					num2 = 0f;
				}
			}
		}
		else if (Stream != null)
		{
			Stream.Update(0f, vector, streamVelocity);
		}
		available = num - num2;
		return num2;
	}
}

}
