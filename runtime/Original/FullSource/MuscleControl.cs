using UnityEngine;

namespace Crisis.Runtime.FullSource
{
public class MuscleControl : MonoBehaviour
{
	private CharacterBody body;

	private Vector2 spasmIntervalRange;

	private Vector2 spasmIntensityRange;

	private float startTime;

	private float spasmTimestamp;

	private float spasmDuration;

	private float spasmIntensity;

	private float crampDuration;

	private bool singleSpasm;

	private bool decorticate;

	private bool decerberateLeft;

	private bool decerberateRight;

	public bool SpasmsActive { get; private set; }

	public bool CrampsActive { get; private set; }

	private void Awake()
	{
		body = GetComponent<CharacterBody>();
	}

	private void Update()
	{
		if (decorticate)
		{
			AnimateDecorticate();
		}
		else if (decerberateLeft || decerberateRight)
		{
			AnimateDecerberate();
		}
		else if (CrampsActive)
		{
			AnimateCramp();
		}
		else if (SpasmsActive || body.PainMovementsActive)
		{
			AnimateSpasms();
		}
	}

	public void SingleSpasmOnBrainHit()
	{
		bool spasmsActive = (CrampsActive = (decerberateLeft = (decerberateRight = (decorticate = false))));
		SpasmsActive = spasmsActive;
		SingleImpulse(body.Physiology.BrainTissueNormalized, 0.05f);
	}

	public void StartHighIntensityMuscleSpasms()
	{
		float num = Random.Range(0.01f, 0.5f);
		StartSpasms(new Vector2(num, num * 2f), new Vector2(0f, 0.3f));
		SpasmsActive = true;
	}

	public void StartLowIntensityMuscleSpasms()
	{
		if (!SpasmsActive)
		{
			float heartBeatPeriod = body.Physiology.HeartBeatPeriod;
			StartSpasms(new Vector2(heartBeatPeriod * 2f, heartBeatPeriod * 4f), new Vector2(0.05f, 0.35f));
			SpasmsActive = true;
		}
	}

	public void ActivateMusclesOnRegainedConsciousness()
	{
		body.PainMovementsActive = true;
		StartSpasms(new Vector2(1f, 3f), new Vector2(0.1f, 0.25f));
	}

	private void StartSpasms(Vector2 interval, Vector2 intensity)
	{
		spasmIntervalRange = interval;
		spasmIntensityRange = intensity;
	}

	public void SingleImpulse(float strength, float duration)
	{
		spasmTimestamp = Time.time;
		spasmDuration = duration;
		SpasmsActive = (singleSpasm = true);
		foreach (Limb limb in body.limbs)
		{
			if (!(limb.LimbMuscle == null))
			{
				limb.LimbMuscle.SetSignal(1f, 0f, Random.Range(0.025f, 0.15f));
			}
		}
	}

	public void ActivateCramp(float duration = 3f)
	{
		SpasmsActive = false;
		CrampsActive = true;
		startTime = Time.time;
		crampDuration = duration;
	}

	public void SetDecorticate()
	{
		SpasmsActive = (decorticate = true);
		startTime = Time.time;
		crampDuration = 60f;
	}

	public void SetDecerberate()
	{
		float value = Random.value;
		if (value < 0.5f)
		{
			decerberateLeft = true;
			decerberateRight = true;
		}
		else if (value < 0.75f)
		{
			decerberateLeft = false;
			decerberateRight = true;
		}
		else
		{
			decerberateLeft = true;
			decerberateRight = false;
		}
		SpasmsActive = true;
		startTime = Time.time;
		crampDuration = 60f;
	}

	public void Relax()
	{
		foreach (Limb limb in body.limbs)
		{
			if (!(limb.LimbMuscle == null))
			{
				limb.LimbMuscle.Relax();
			}
		}
		body.PainMovementsActive = false;
	}

	private void AnimateSpasms()
	{
		float num = Time.time - spasmTimestamp;
		if (num >= spasmDuration)
		{
			if (singleSpasm)
			{
				Relax();
				SpasmsActive = (singleSpasm = false);
				return;
			}
			spasmTimestamp = Time.time;
			spasmDuration = Random.Range(spasmIntervalRange.x, spasmIntervalRange.y);
			spasmIntensity = Random.Range(spasmIntensityRange.x, spasmIntensityRange.y);
			if (body.PainMovementsActive && !body.Physiology.RespirationBlocked && Random.value < 0.5f)
			{
				body.Audio.Hurt();
			}
			{
				foreach (Limb limb in body.limbs)
				{
					if (!(limb.LimbMuscle == null) && limb.id != LimbId.RightLowerLeg && limb.id != LimbId.LeftLowerLeg)
					{
						limb.LimbMuscle.StartSpasm(Random.value, Random.value, spasmDuration, spasmIntensity, body.PainMovementsActive);
					}
				}
				return;
			}
		}
		float num2 = 1f - Mathf.Max(num * 2f / spasmDuration, 0f);
		foreach (Limb limb2 in body.limbs)
		{
			if (!(limb2.LimbMuscle == null))
			{
				limb2.LimbMuscle.UpdateStrength(num2 * body.Physiology.BloodOxygen);
			}
		}
	}

	private void AnimateCramp()
	{
		float num = Time.time - startTime;
		float num2 = crampDuration * 0.75f;
		float num3 = crampDuration * 0.25f;
		float num4 = 1f - Mathf.Max(num - num2, 0f) / num3;
		foreach (Limb limb in body.limbs)
		{
			if (!(limb.LimbMuscle == null))
			{
				limb.LimbMuscle.SetSignal(1f, 1f, 1f, num4 * body.Physiology.BloodOxygen);
			}
		}
		if (num4 <= 0f)
		{
			CrampsActive = false;
		}
	}

	private void AnimateDecerberate()
	{
		if (Time.time >= spasmTimestamp + spasmDuration)
		{
			spasmTimestamp = Time.time;
			spasmIntensity = Random.Range(0.2f, 1f);
			spasmDuration = body.Physiology.HeartBeatPeriod;
		}
		body.rightArm.hand.SetMode(Hand.Mode.Fist);
		body.leftArm.hand.SetMode(Hand.Mode.Fist);
		float bloodOxygen = body.Physiology.BloodOxygen;
		body.Head.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		if (decerberateRight)
		{
			body.RightUpperArm.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.RightLowerArm.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.RightHand.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.RightUpperLeg.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.RightLowerLeg.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.RightFoot.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		}
		if (decerberateLeft)
		{
			body.LeftUpperArm.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.LeftLowerArm.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.LeftHand.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.LeftUpperLeg.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.LeftLowerLeg.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
			body.LeftFoot.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		}
	}

	private void AnimateDecorticate()
	{
		if (Time.time >= spasmTimestamp + spasmDuration)
		{
			spasmTimestamp = Time.time;
			spasmIntensity = Random.Range(0.2f, 1f);
			spasmDuration = body.Physiology.HeartBeatPeriod;
		}
		float bloodOxygen = body.Physiology.BloodOxygen;
		body.rightArm.hand.SetMode(Hand.Mode.Fist);
		body.leftArm.hand.SetMode(Hand.Mode.Fist);
		body.RightUpperArm.LimbMuscle.SetSignal(spasmIntensity, 0.2f, 1f, bloodOxygen);
		body.LeftUpperArm.LimbMuscle.SetSignal(spasmIntensity, 0.2f, 1f, bloodOxygen);
		body.RightLowerArm.LimbMuscle.SetSignal(0f, spasmIntensity, 1f, bloodOxygen);
		body.LeftLowerArm.LimbMuscle.SetSignal(0f, spasmIntensity, 1f, bloodOxygen);
		body.RightHand.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		body.LeftHand.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		body.RightUpperLeg.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		body.LeftUpperLeg.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		body.RightLowerLeg.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		body.LeftLowerLeg.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		body.RightFoot.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
		body.LeftFoot.LimbMuscle.SetSignal(spasmIntensity, 0f, 1f, bloodOxygen);
	}

	public void OnDeath()
	{
		Relax();
		SpasmsActive = false;
		CrampsActive = false;
		decorticate = false;
		decerberateLeft = false;
		decerberateRight = false;
	}
}

}
