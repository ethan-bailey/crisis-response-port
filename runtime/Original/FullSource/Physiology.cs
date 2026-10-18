using System;
using System.Collections.Generic;
using UnityEngine;

using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
public class Physiology : MonoBehaviour
{
	public enum HealthConditionType
	{
		Unharmed,
		Light,
		Moderate,
		Severe,
		Fatal
	}

	public enum WoundsConditionType
	{
		Stable,
		Critical
	}

	public const float GauzeFillVolume = 750f;

	public const float RestHeartRate = 100f;

	public const float RestRespirationRate = 20f;

	[Header("Modifiers")]
	[Tooltip("Heart capacity is the base for how much oxygen is carried from the lungs to the blood. In milliliters per beat.")]
	public float heartCapacity = 70f;

	[Tooltip("Modifies how much oxygen is added to the lungs by respiration.")]
	[Range(0f, 1f)]
	public float respirationToLungsFactor = 0.35f;

	[Tooltip("Modifies how much oxygen is taken from the lungs by a heart beat.")]
	[Range(0f, 1f)]
	public float heartOxygenTake = 0.02f;

	[Tooltip("Modifies how much oxygen is added to the blood by a heart beat.")]
	[Range(0f, 5f)]
	public float heartOxygenDeliver = 1f;

	[Tooltip("Modifies how much oxygen is burnt while doing work.")]
	[Range(0f, 1f)]
	public float workOxygenFactor = 0.067f;

	[Tooltip("A lerp factor for how fast the body adapts a new respiration rate.")]
	[Range(0f, 5f)]
	public float respirationRateAdaptionRate = 0.1f;

	[Tooltip("A lerp factor for how fast the body adapts a new respiration depth.")]
	[Range(0f, 5f)]
	public float respirationDepthAdaptionRate = 1f;

	[Header("Properties")]
	[Tooltip("A high value allows the character to work longer before reaching peak pulse and respiration. A character with a high value also recovers faster.")]
	[Range(0f, 1f)]
	public float fitness = 1f;

	public float heartRateMax = 190f;

	public float respirationRateMax = 160f;

	[Tooltip("This curve yields the effective concussion as a function of the normalized concussion value.")]
	public AnimationCurve concussionCurve;

	[Header("Thresholds")]
	[Tooltip("How long can the brain be without oxygen before it starts to die?")]
	public float brainDamageFromLackOfOxygenSeconds = 10f;

	[Tooltip("How quick does the brain take damage when without oxygen? A multiplier.")]
	public float brainDamageRateWithoutOxygen = 7.5f;

	[Tooltip("This is used to trigger character death. It has nothing to do with the ticking brain damage from lack of oxygen.")]
	public float brainDeathFromLackOfOxygenSeconds = 30f;

	[Header("Cheats")]
	public bool invulnerable;

	private CharacterBody body;

	private UIHitAnnouncements hitAnnouncements;

	private float maxConsciousBrainTrauma = 3f;

	private float maxAliveBrainTrauma = 125f;

	private float brainTissue = 1000f;

	private float startingBrainTissue = 1000f;

	private float concussion;

	private float heartRate = 100f;

	private float respirationRate = 15f;

	private float respirationDepth = 0.5f;

	private float startingHeartCapacity;

	private float lungsLeakage;

	private bool skipHeartBeat;

	private bool isInhaling;

	private float heartbeatTimestamp;

	private float respirationTimestamp;

	private float lastBrainOxygenTimestamp;

	private float bloodDropOxygen;

	private float heartRateCompensation = 100f;

	private float heartCompensationThreshold = 0.85f;

	private float bloodSquirtAudioVolume;

	private const float MaxShock = 1000f;

	private const float RegainConsciousnessShock = 800f;

	private const float ConcussionBleedThreshold = 0.5f;

	private const float MaxConsciousConcussion = 0.775f;

	private const float RegainConsciousnessConcussion = 0.4f;

	private const float MuscleSpasmsOxygenLevel = 0.75f;

	private const float MinConsciousBloodOxygen = 0.5f;

	private const float RegainConsciousnessBloodOxygen = 0.6f;

	private const float SpasmsOnLowOxygenFraction = 0.1f;

	private const float MaxHemorrhagingScore = 300f;

	private const float MaxTraumaScore = 750f;

	private const float StunResponseThreshold = 0.15f;

	public bool Alive { get; private set; }

	public bool Conscious { get; private set; }

	public bool ConsciousAndKinematic => Conscious && body.Kinematic;

	public bool Paralyzed { get; private set; }

	public HealthConditionType HealthCondition
	{
		get
		{
			if (BrainTraumaNormalized > 0.5f)
			{
				return HealthConditionType.Fatal;
			}
			if (WoundVolume == 0f && Hemorrhaging == 0f)
			{
				return HealthConditionType.Unharmed;
			}
			if (WoundVolume < 50f && Hemorrhaging < 100f)
			{
				return HealthConditionType.Light;
			}
			if (WoundVolume < 150f && Hemorrhaging < 300f)
			{
				return HealthConditionType.Moderate;
			}
			return HealthConditionType.Severe;
		}
	}

	public WoundsConditionType WoundsCondition { get; private set; }

	public bool RespirationDisabled { get; private set; }

	public bool CirculationDisabled { get; private set; }

	public bool MotorCortexDisabled { get; private set; }

	public float BloodVolume { get; private set; }

	public float StartingBloodVolume { get; private set; }

	public float BloodVolumeNormalized => BloodVolume / StartingBloodVolume;

	public float HeartBeatCycle { get; private set; }

	public float DiastolicPressure { get; private set; }

	public float BloodPressure { get; private set; }

	public float BloodPressureAverage => DiastolicPressure + BloodVolumeNormalized * 0.2f;

	public float HeartBeatPeriod => (heartRate != 0f) ? (60f / heartRate) : float.PositiveInfinity;

	public AudioSource HeartBeatAudio { get; set; }

	public float Respiration { get; private set; }

	public float BloodOxygen { get; private set; }

	public float LungsOxygen { get; private set; }

	public float LungsCapacity { get; private set; }

	public float LungsEffectiveness { get; private set; }

	public float LungsOxygenNormalized => Mathf.Clamp(LungsOxygen / (LungsCapacity * LungsEffectiveness), 0f, 1f);

	public bool RespirationBlocked { get; private set; }

	public bool CoughedThisRespirationCycle { get; private set; }

	public float WorkIntensity { get; private set; }

	public float Concussion => concussionCurve.Evaluate(concussion);

	public float BrainTrauma { get; private set; }

	public float BrainTraumaNormalized => BrainTrauma / maxAliveBrainTrauma;

	public float BrainTissueNormalized => brainTissue / startingBrainTissue;

	public float RespirationTimestamp { get; private set; }

	public float RespirationRateFactor => respirationRate / respirationRateMax;

	public float Shock { get; private set; }

	public float ShockNormalized => Shock / 1000f;

	public float ShockResistance { get; set; }

	public float ShockRecovery { get; set; }

	public float Hemorrhaging { get; private set; }

	public float WoundVolume { get; private set; }

	public bool Exsanguinated => BloodVolume <= 0f;

	public Vector3 AnnouncePosition { get; set; }

	public bool AudibleRespiration
	{
		get
		{
			bool flag = (body.Head.LimbPhysiology.Wounded && body.Head.LimbPhysiology.InternalHemorrhaging > 0f) || (body.Neck.LimbPhysiology.Wounded && body.Neck.LimbPhysiology.InternalHemorrhaging > 0f);
			return Alive && (flag || (!Conscious && RespirationBlocked));
		}
	}

	public float BloodSquirtAudioVolume
	{
		get
		{
			return bloodSquirtAudioVolume;
		}
		set
		{
			if (value > bloodSquirtAudioVolume)
			{
				bloodSquirtAudioVolume = value;
			}
		}
	}

	public float UnconsciousTimestamp { get; private set; }

	private float HeartEffectiveness => heartCapacity / startingHeartCapacity;

	private float HeartCapacity => (startingHeartCapacity + fitness * 0.3f) * HeartEffectiveness;

	private float RespirationPeriod => (respirationRate != 0f) ? (60f / respirationRate) : float.PositiveInfinity;

	public bool ShouldLoseFooting()
	{
		return Shock >= 2500f;
	}

	public bool ShouldLoseConsciousness()
	{
		bool flag = Concussion >= 0.775f;
		bool flag2 = BloodOxygen <= 0.5f;
		bool flag3 = BrainTrauma >= maxConsciousBrainTrauma;
		return flag || flag2 || flag3;
	}

	public bool ShouldRegainConsciousness()
	{
		bool flag = Concussion >= 0.4f;
		bool flag2 = BloodOxygen <= 0.6f;
		bool flag3 = BrainTrauma >= maxConsciousBrainTrauma;
		return !flag && !flag2 && !flag3;
	}

	public void AddShock(float shock)
	{
		Shock += shock * (1f - ShockResistance);
	}

	public void AddConcussion(float value)
	{
		concussion += value;
		if (concussion > 1.25f)
		{
			AddBrainTrauma((concussion - 1f) * 10f);
		}
		if (value > 1.5f)
		{
			AddBrainTrauma(value * 10f);
		}
		float num = Concussion;
		if (!body.StunResponseStateActive && num > 0.15f)
		{
			body.SetStunResponseState(enable: true);
		}
		if (num > 0.5f)
		{
			body.Head.CreateConcussiveWounds(num - 0.5f);
		}
	}

	public void AddConcussion(float value, float delay = 0f)
	{
		body.Mission.ActionScheduler.Schedule(() =>
		{
			AddConcussion(value);
		}, delay);
	}

	public void AddBrainTrauma(float volume, bool canTriggerNerves = true)
	{
		BrainTrauma += volume;
		brainTissue = Mathf.Max(0f, brainTissue - volume * 0.5f);
		if (!canTriggerNerves)
		{
			return;
		}
		if (UnityEngine.Random.value < BrainTraumaNormalized * 0.4f)
		{
			RespirationDisabled = true;
		}
		if (BrainTrauma > maxConsciousBrainTrauma && BrainTrauma < maxAliveBrainTrauma)
		{
			float value = UnityEngine.Random.value;
			if (value < 0.1f)
			{
				body.MuscleControl.SetDecerberate();
			}
			else if (value < 0.2f)
			{
				body.MuscleControl.SetDecorticate();
			}
			else if (value < 0.3f)
			{
				body.MuscleControl.StartHighIntensityMuscleSpasms();
			}
		}
		else
		{
			body.MuscleControl.SingleSpasmOnBrainHit();
		}
	}

	public void AddBrainTrauma(float volume, float delay, bool canTriggerNerves = true)
	{
		body.Mission.ActionScheduler.Schedule(() =>
		{
			AddBrainTrauma(volume, canTriggerNerves);
		}, delay);
	}

	public void AddLungsTrauma(float volume, float shock)
	{
		RemoveLungsOxygen(shock * 0.0005f);
		lungsLeakage += volume * 0.001f;
		LungsEffectiveness = Mathf.Clamp(LungsEffectiveness - volume * 0.001f, 0f, 1f);
	}

	public void AddHeartTrauma(float volume, float shock)
	{
		float num = Mathf.Min(volume, heartCapacity) / startingHeartCapacity;
		body.Torso.LimbPhysiology.bloodPressureModifier += num;
		heartCapacity = Mathf.Max(heartCapacity - volume, 0f);
		if (heartCapacity <= 0f || shock >= 500f)
		{
			CirculationDisabled = true;
			if (UnityEngine.Random.value > 0.5f)
			{
				SetUnconscious();
			}
		}
		if (UnityEngine.Random.value > HeartEffectiveness)
		{
			body.MuscleControl.ActivateCramp(4f);
		}
	}

	public void SetParalyzed()
	{
		if (UnityEngine.Random.value > 0.25f)
		{
			body.MuscleControl.ActivateCramp(UnityEngine.Random.Range(3f, 5f));
		}
		SetRagdoll();
		Paralyzed = true;
	}

	public void RemoveBlood(float milliliters)
	{
		BloodVolume -= milliliters;
		BloodOxygen -= bloodDropOxygen * milliliters;
	}

	public void RemoveLungsOxygen(float normalizedPercent)
	{
		LungsOxygen = Mathf.Clamp(LungsOxygen - LungsOxygen * normalizedPercent, 0f, 1f);
	}

	public void StopHemorrhage()
	{
		foreach (Limb limb in body.limbs)
		{
			limb.StopHemorrhage();
		}
	}

	public void OnJointBroken()
	{
		if (ConsciousAndKinematic)
		{
			SetUnconscious();
		}
	}

	public float Score()
	{
		return HemorrhagingScore() * BloodVolumeNormalized * TraumaScore() * VitalFunctionsScore();
	}

	public string Summary()
	{
		List<string> list = new List<string>();
		float num = Score();
		if (num == 1f)
		{
			return "Appears vital";
		}
		bool flag = body.MuscleControl.SpasmsActive || body.MuscleControl.CrampsActive;
		bool flag2 = (!Alive || BloodOxygen < 0.1f) && !flag;
		if (!Conscious)
		{
			if (flag2)
			{
				list.Add("lifeless");
			}
			else
			{
				list.Add("unconscious");
			}
		}
		float bloodVolumeNormalized = BloodVolumeNormalized;
		float num2 = HemorrhagingScore();
		if (num2 < 1f && bloodVolumeNormalized > 0.1f)
		{
			if (num2 >= 0.9f)
			{
				list.Add("mild bleeding");
			}
			else if (num2 >= 0.5f)
			{
				list.Add("bleeding");
			}
			else
			{
				list.Add("severe bleeding");
			}
		}
		if (!flag2 && bloodVolumeNormalized < 0.65f)
		{
			if (bloodVolumeNormalized > 0.45f)
			{
				list.Add("weak pulse");
			}
			else if (bloodVolumeNormalized > 0.25f)
			{
				list.Add("very weak pulse");
			}
			else if (bloodVolumeNormalized > 0.1f)
			{
				list.Add("almost no pulse");
			}
			else
			{
				list.Add("no pulse");
			}
		}
		if (!flag2)
		{
			if (RespirationDisabled)
			{
				list.Add("not breathing");
			}
			else if (RespirationBlocked || LungsEffectiveness < 0.75f)
			{
				list.Add("problems breathing");
			}
		}
		float num3 = TraumaScore();
		if (num3 < 1f)
		{
			if (num3 >= 0.9f)
			{
				list.Add("mild trauma");
			}
			else if (num3 >= 0.75f)
			{
				list.Add("moderate trauma");
			}
			else if (num3 >= 0.5f)
			{
				list.Add("severe trauma");
			}
			else
			{
				list.Add("massive trauma");
			}
		}
		if (body.MuscleControl.SpasmsActive && BloodOxygen >= 0.1f)
		{
			list.Add("seizuring");
		}
		string text = string.Empty;
		foreach (string item in list)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += item;
		}
		return text;
	}

	private void Awake()
	{
		body = GetComponent<CharacterBody>();
		hitAnnouncements = body.Mission.UI.hitAnnouncements;
		Alive = true;
		Conscious = true;
		BloodOxygen = 1f;
		LungsOxygen = 0.75f;
		float bloodVolume = (StartingBloodVolume = UnityEngine.Random.Range(4000, 5000));
		BloodVolume = bloodVolume;
		bloodDropOxygen = 1f / StartingBloodVolume;
		bloodVolume = (LungsCapacity = 1f);
		LungsEffectiveness = bloodVolume;
		startingHeartCapacity = heartCapacity;
		heartbeatTimestamp = Time.time + UnityEngine.Random.value;
		respirationTimestamp = Time.time + UnityEngine.Random.value;
	}

	private void Update()
	{
		if (!(Time.deltaTime <= 0f))
		{
			SetWorkLevel();
			if (Alive)
			{
				SetHeartRate();
				SetRespirationRate();
				Respirate();
				SetBrainOxygen();
				SetShockValue();
			}
			UpdateHeart();
			SetBloodPressure();
			UpdateLimbs();
			SetHemorrhaging();
			SetBloodOxygen();
			SetCharacterState();
		}
	}

	private void SetWorkLevel()
	{
		float num = 0.2f * (respirationRate / respirationRateMax);
		float num2 = 0.1f * (heartRate / heartRateMax);
		float holdWeaponWorkIntensity = body.HoldWeaponWorkIntensity;
		float num3 = Mathf.Abs(body.Velocity) / body.sprintSpeed;
		float num4 = ((!body.MuscleControl.SpasmsActive) ? 0f : 0.5f) + ((!body.MuscleControl.CrampsActive) ? 0f : 0.75f);
		WorkIntensity = Mathf.Clamp01(num2 + num + holdWeaponWorkIntensity + num3 + num4);
	}

	private void SetHeartRate()
	{
		if (CirculationDisabled)
		{
			heartRate = 0f;
		}
		else if (BloodOxygen < heartCompensationThreshold || DiastolicPressure < 0.3f)
		{
			float num = (1f - BloodOxygen) * heartRateCompensation * 0.0167f;
			heartRate = Mathf.Clamp(heartRate + num, 100f, heartRateMax);
		}
		else
		{
			float num2 = 1f / WorkIntensity * Mathf.Clamp(fitness, 0.1f, 1f) * 0.0083f;
			heartRate = Mathf.Clamp(heartRate - num2, 100f, heartRateMax);
		}
	}

	private void UpdateHeart()
	{
		if (CirculationDisabled)
		{
			HeartBeatCycle = Mathf.Max(0f, HeartBeatCycle - Time.deltaTime);
			return;
		}
		float num = Time.time - heartbeatTimestamp;
		float num2 = num / (HeartBeatPeriod * 2f);
		bool flag = num2 <= 1f;
		bool flag2 = num2 <= 2f;
		if (skipHeartBeat)
		{
			if (num < HeartBeatPeriod)
			{
				return;
			}
			skipHeartBeat = false;
		}
		if (flag)
		{
			HeartBeatCycle = 0.5f + Mathf.Sin(4f * num2 * (float)Math.PI - (float)Math.PI / 2f) * 0.5f;
		}
		else if (flag2)
		{
			HeartBeatCycle = 0.5f + Mathf.Sin(1.3f * num2 * (float)Math.PI + (float)Math.PI / 5f) * 0.5f;
		}
		else
		{
			HeartBeatCycle = Mathf.Max(0f, HeartBeatCycle - Time.deltaTime);
		}
		if (num < HeartBeatPeriod)
		{
			return;
		}
		if (UnityEngine.Random.value > HeartEffectiveness * Mathf.Pow(BloodOxygen, 2f))
		{
			skipHeartBeat = true;
			return;
		}
		float num3 = Mathf.Min(LungsOxygen, HeartCapacity * BloodPressure);
		LungsOxygen = Mathf.Max(LungsOxygen - num3 * heartOxygenTake, 0f);
		BloodOxygen = Mathf.Min(BloodOxygen + num3 * heartOxygenDeliver, 1f);
		BloodOxygen = Mathf.Min(BloodOxygen, BloodVolumeNormalized);
		if (HeartBeatAudio != null && body.Mission.View == Mission.MissionView.EntryTeam)
		{
			HeartBeatAudio.Play();
		}
		PlayBloodSquirtAudio();
		heartbeatTimestamp = Time.time;
	}

	private void SetRespirationRate()
	{
		if (RespirationDisabled)
		{
			respirationRate = 0f;
		}
		else
		{
			float num = Mathf.Min(BloodVolumeNormalized / 0.25f, 1f);
			float num2 = HeartCapacity * BloodPressureAverage * heartOxygenTake * heartRate;
			float num3 = Mathf.Max(1f, 2f - LungsOxygen);
			float b = Mathf.Clamp(num2 / (respirationToLungsFactor * respirationDepth * 60f), 20f, respirationRateMax) * num;
			respirationRate = Mathf.Lerp(respirationRate, b, respirationRateAdaptionRate * Time.deltaTime);
		}
		float b2 = 0.5f + (1f - Mathf.Max(LungsOxygenNormalized - 0.25f, 0f) / 0.75f) * 0.5f;
		respirationDepth = Mathf.Min(Mathf.Lerp(respirationDepth, b2, respirationDepthAdaptionRate * Time.deltaTime), BloodVolumeNormalized);
	}

	private void SetBloodPressure()
	{
		float num = heartRate / heartRateMax * HeartEffectiveness * 0.3f;
		float num2 = BloodVolumeNormalized * 0.3f;
		DiastolicPressure = num + num2;
		BloodPressure = DiastolicPressure + HeartBeatCycle * 0.4f;
	}

	private void UpdateLimbs()
	{
		if (Exsanguinated)
		{
			return;
		}
		foreach (Limb limb in body.limbs)
		{
			if (limb.LimbPhysiology != null)
			{
				limb.LimbPhysiology.UpdateHemorrhaging();
			}
		}
	}

	private void SetHemorrhaging()
	{
		Hemorrhaging = 0f;
		WoundVolume = 0f;
		foreach (Limb limb in body.limbs)
		{
			if (limb.LimbPhysiology != null)
			{
				Hemorrhaging += limb.LimbPhysiology.InternalHemorrhaging;
				WoundVolume += limb.LimbPhysiology.WoundCavityVolume;
			}
		}
	}

	private void Respirate()
	{
		if (RespirationDisabled)
		{
			isInhaling = false;
			return;
		}
		Ray ray = new Ray(body.Mouth.position, body.Mouth.right);
		RespirationBlocked = Physics.Raycast(ray, 0.05f, Masks.Instance.BallisticTargetsSansLimbs);
		float num = respirationDepth * ((!RespirationBlocked) ? 1f : 0.1f);
		bool flag = Time.time - respirationTimestamp < RespirationPeriod * 0.5f;
		float num2 = RespirationPeriod * ((!CoughedThisRespirationCycle) ? 0.5f : 0.2f);
		if (flag)
		{
			if (!isInhaling)
			{
				isInhaling = true;
				PlayInhalationAudio();
				CoughedThisRespirationCycle = false;
			}
			LungsOxygen = Mathf.Clamp(LungsOxygen + Time.deltaTime * respirationToLungsFactor * num, 0f, LungsCapacity * LungsEffectiveness * BloodVolumeNormalized);
			Respiration = (Time.time - respirationTimestamp) / (RespirationPeriod * 0.5f) * num;
		}
		else
		{
			if (isInhaling)
			{
				isInhaling = false;
				if (Conscious && !CoughedThisRespirationCycle && lungsLeakage > 0f && UnityEngine.Random.value > LungsEffectiveness)
				{
					CoughedThisRespirationCycle = true;
					body.Audio.Cough();
				}
			}
			Respiration = (1f - (Time.time - respirationTimestamp - num2) / num2) * num;
		}
		if (Time.time - respirationTimestamp >= RespirationPeriod * 0.5f + num2)
		{
			respirationTimestamp = Time.time;
		}
		if (lungsLeakage > 0f)
		{
			LungsOxygen = Mathf.Max(0f, LungsOxygen - lungsLeakage * 0.001f * Time.deltaTime);
		}
	}

	private void SetBloodOxygen()
	{
		BloodOxygen = Mathf.Clamp(BloodOxygen - WorkIntensity * workOxygenFactor * Time.deltaTime, 0f, 1f);
	}

	private void SetBrainOxygen()
	{
		if (BloodOxygen > 0f)
		{
			lastBrainOxygenTimestamp = Time.time;
		}
		else if (Time.time - lastBrainOxygenTimestamp >= brainDamageFromLackOfOxygenSeconds)
		{
			AddBrainTrauma(Time.deltaTime * brainDamageRateWithoutOxygen, canTriggerNerves: false);
		}
	}

	private void SetShockValue()
	{
		Shock = Mathf.Lerp(Shock, 0f, Time.deltaTime * 0.25f * ShockRecovery);
		if (Shock < 0.01f)
		{
			Shock = 0f;
		}
		concussion = Mathf.Clamp01(concussion - Time.deltaTime * 0.025f);
	}

	private void SetCharacterState()
	{
		if (Concussion <= 0.15f && body.StunResponseStateActive)
		{
			body.SetStunResponseState(enable: false);
		}
		if (!body.IsRagdoll)
		{
			if (Shock >= 1000f)
			{
				SetRagdoll();
			}
		}
		else if (!body.MuscleControl.SpasmsActive && BloodOxygen <= 0.75f && BrainTraumaNormalized < 0.25f)
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				body.MuscleControl.StartLowIntensityMuscleSpasms();
			}
			else
			{
				body.MuscleControl.StartHighIntensityMuscleSpasms();
			}
		}
		if (!body.IsRagdoll && ShouldLoseFooting())
		{
			SetRagdoll();
		}
		if (Conscious)
		{
			if (ShouldLoseConsciousness())
			{
				SetUnconscious();
			}
		}
		else if (ShouldRegainConsciousness())
		{
			body.MuscleControl.ActivateMusclesOnRegainedConsciousness();
			Conscious = true;
		}
		if (Alive && Time.time - lastBrainOxygenTimestamp > brainDeathFromLackOfOxygenSeconds)
		{
			SetDead();
		}
	}

	public void SetUnconscious()
	{
		if (Conscious && !invulnerable)
		{
			Conscious = false;
			UnconsciousTimestamp = Time.time;
			body.SetUnconscious();
			HeartBeatAudio = null;
		}
	}

	public void SetRagdoll()
	{
		if (!body.IsRagdoll && !invulnerable)
		{
			body.SetRagdoll();
			HeartBeatAudio = null;
		}
	}

	public void SetDead()
	{
		if (Alive && !invulnerable)
		{
			SetUnconscious();
			Alive = false;
			body.OnDeath();
			body.MuscleControl.OnDeath();
			RespirationDisabled = true;
		}
	}

	public float HemorrhagingScore()
	{
		float num = 0f;
		foreach (Limb limb in body.limbs)
		{
			if (limb.LimbPhysiology != null)
			{
				num += limb.LimbPhysiology.HemorrhagingScore;
			}
		}
		return Mathf.Max(1f - num / 300f, 0f);
	}

	private float TraumaScore()
	{
		float num = 0f;
		foreach (Limb limb in body.limbs)
		{
			if (limb.LimbPhysiology != null)
			{
				num += limb.LimbPhysiology.DamagedTissueVolume * limb.LimbPhysiology.shockModifier;
			}
		}
		return Mathf.Max(1f - num / 750f, 0f);
	}

	private float VitalFunctionsScore()
	{
		return Mathf.Max(HeartEffectiveness * LungsEffectiveness * (1f - BrainTraumaNormalized) - ((!RespirationBlocked) ? 0f : 0.025f), 0f);
	}

	public void AnnounceHit(string text, Projectile projectile)
	{
		if (Global.Context.Settings.Extras.announceHits)
		{
			hitAnnouncements.Announce(text, projectile);
		}
	}

	private void PlayInhalationAudio()
	{
		if (!isInhaling && !(Respiration < 0.05f))
		{
			isInhaling = true;
			if (AudibleRespiration && UnityEngine.Random.value < 0.5f)
			{
				body.Audio.Snore();
			}
		}
	}

	private void PlayBloodSquirtAudio()
	{
		if (bloodSquirtAudioVolume >= 0.1f)
		{
			body.Audio.BloodSquirt(BloodSquirtAudioVolume);
			bloodSquirtAudioVolume = 0f;
		}
	}
}

}
