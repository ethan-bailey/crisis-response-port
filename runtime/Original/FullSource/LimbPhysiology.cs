using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class LimbPhysiology : MonoBehaviour
{
	[Tooltip("Factor that modifies the body's blood pressure for this limb.")]
	[Range(0f, 2f)]
	public float bloodPressureModifier = 1f;

	[Tooltip("Factor that modifies the amount of shock induced from impacts on this limb. Also used to scale the trauma score value.")]
	[Range(0f, 1f)]
	public float shockModifier = 1f;

	[Tooltip("How much blood flows in the limb. In milliliters. Limits how much it can emit per frame.")]
	public float maxHemorrhaging = 1f;

	[Tooltip("Internal wound cavity volume is limited to this value. In cubic centimeters (milliliters). Determins how quickly blood emission pressure rises.")]
	public float maxWoundCavityVolume = 100f;

	public List<Wound> wounds = new List<Wound>();

	private Limb limb;

	private VitalFunctions vitals;

	private CharacterBody body;

	private bool useBloodParticles;

	private bool useWoundStreams;

	private bool useDynamicDecals;

	private float woundPowerMultiplier;

	public Physiology Physiology { get; private set; }

	public bool Wounded => WoundCavityVolume > 0f;

	public bool HasHemorrhagingWounds => wounds.Count > 0 && Hemorrhaging > 0f;

	public float Hemorrhaging => InternalHemorrhaging * HemorrhagingNormalized * ((!Physiology.Exsanguinated) ? 1f : 0f);

	public float HemorrhagingNormalized => Mathf.Clamp01(WoundCavityVolume / maxWoundCavityVolume);

	public bool IsHemorrhaging => Hemorrhaging > 0f;

	public float InternalHemorrhaging { get; private set; }

	public float WoundCavityVolume { get; private set; }

	public float DamagedTissueVolume { get; private set; }

	public float InternalBloodVolume { get; private set; }

	public float LocalPressure => Physiology.BloodPressure * bloodPressureModifier;

	public float HemorrhagingScore => WoundCavityVolume * bloodPressureModifier;

	private void Awake()
	{
		limb = GetComponent<Limb>();
		vitals = GetComponent<VitalFunctions>();
		body = limb.Body;
		Physiology = body.Physiology;
	}

	public void ApplySettings(bool useBloodParticles, bool useWoundStreams, bool useDynamicDecals, float woundPowerMultiplier)
	{
		this.useBloodParticles = useBloodParticles;
		this.useWoundStreams = useWoundStreams;
		this.useDynamicDecals = useDynamicDecals;
		this.woundPowerMultiplier = woundPowerMultiplier;
	}

	public void StopHemorrhage()
	{
		float woundCavityVolume = (InternalBloodVolume = 0f);
		WoundCavityVolume = woundCavityVolume;
	}

	public void AddTrauma(float volume)
	{
		float num = Mathf.Min(volume, Mathf.Max(maxWoundCavityVolume - WoundCavityVolume, 0f));
		WoundCavityVolume += num;
		DamagedTissueVolume += num;
	}

	public void AddShock(float shock)
	{
		Physiology.AddShock(shock * shockModifier);
		if (limb.id == LimbId.Head || limb.id == LimbId.Neck)
		{
			float value = shock * Random.Range(0.0005f, 0.002f) * ((limb.id != LimbId.Head) ? 0.25f : 1f);
			Physiology.AddConcussion(value);
		}
	}

	public void AddBoneTrauma(float volume, float shock, float normalizedLocalY)
	{
		Physiology.AddShock(shock);
		if (limb.LimbMuscle != null)
		{
			limb.LimbMuscle.AddJointTrauma(volume, normalizedLocalY);
		}
		if (!body.NoPlayerLimp || !body.IsOperator)
		{
			switch (limb.id)
			{
			case LimbId.RightUpperLeg:
			case LimbId.RightLowerLeg:
			case LimbId.RightFoot:
			case LimbId.LeftUpperLeg:
			case LimbId.LeftLowerLeg:
			case LimbId.LeftFoot:
				body.CrippledRatio = 1f;
				break;
			case LimbId.RightToes:
				break;
			}
		}
	}

	public void VitalsTest(Projectile projectile, Vector3 entry, Vector3 exit, float shock)
	{
		if (!(vitals == null))
		{
			vitals.HitTest(projectile, entry, exit, shock);
		}
	}

	public void CreateWound(float diameterMillimeters, float volume, float emissionRadiusMeters, Vector3 worldPoint, Vector3 worldDirection)
	{
		Vector3 vector = transform.InverseTransformPoint(worldPoint);
		Vector3 localDirection = transform.InverseTransformDirection(worldDirection);
		if (Mathf.Abs(localDirection.z) < 0.5f)
		{
			localDirection.z = Random.value * ((!(Random.value > 0.5f)) ? (-1f) : 1f);
		}
		Wound wound = new Wound(limb, new Vector3(vector.x, vector.y, 0f), localDirection, diameterMillimeters, emissionRadiusMeters, useBloodParticles, useWoundStreams, woundPowerMultiplier);
		if (useDynamicDecals)
		{
			limb.SpawnDynamicDecals(wound);
		}
		wounds.Add(wound);
	}

	public void UpdateHemorrhaging()
	{
		if (InternalBloodVolume < WoundCavityVolume)
		{
			InternalHemorrhaging = WoundCavityVolume * LocalPressure * Time.deltaTime;
			InternalBloodVolume += InternalHemorrhaging;
			Physiology.RemoveBlood(InternalHemorrhaging);
		}
		else
		{
			InternalHemorrhaging = 0f;
		}
		float num = ((wounds.Count != 0) ? (2f / (float)wounds.Count) : 0f);
		float num2 = LocalPressure * num;
		float num3 = 0f;
		if (limb.id <= LimbId.Head && num2 > 0.1f)
		{
			Physiology.BloodSquirtAudioVolume = HemorrhagingNormalized;
		}
		foreach (Wound wound in wounds)
		{
			num3 += wound.Update(num2);
		}
		InternalBloodVolume -= num3;
	}

	public void InflictVitalTrauma(LimbVitalFunction vital, float volume, float shock, float normalizedLocalY)
	{
		switch (vital.type)
		{
		case LimbVitalFunction.Type.Brain:
			Physiology.AddBrainTrauma(volume);
			break;
		case LimbVitalFunction.Type.Lungs:
			Physiology.AddLungsTrauma(volume, shock);
			break;
		case LimbVitalFunction.Type.Heart:
			Physiology.AddHeartTrauma(volume, shock);
			break;
		case LimbVitalFunction.Type.Spine:
			Physiology.SetParalyzed();
			break;
		case LimbVitalFunction.Type.Bone:
			AddBoneTrauma(volume, shock, normalizedLocalY);
			break;
		case LimbVitalFunction.Type.BloodVessels:
			break;
		}
	}
}

}
