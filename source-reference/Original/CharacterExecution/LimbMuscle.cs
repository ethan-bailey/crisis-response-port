using System;
using UnityEngine;

public class LimbMuscle : MonoBehaviour
{
	public enum Mode
	{
		Contract,
		Extend,
		Angle,
		Spasm
	}

	[Range(-180f, 180f)]
	public float contractAngle;

	[Range(-180f, 180f)]
	public float extendAngle;

	[Tooltip("The strength of this muscle. (Not used for all muscle actions.)")]
	public float strength = 100f;

	[Tooltip("How much trauma the bone in this joint can take before it fails.")]
	public float boneIntegrity = 500f;

	private HingeJoint joint;

	private JointSpring spring;

	private Rigidbody rigidBody;

	private Physiology physiology;

	private Collider limbCollider;

	private bool jointLocked;

	private bool fadeUp;

	private float startBoneIntegrity;

	private float extend;

	private float contract;

	private float impulseTimestamp;

	private float impulseDuration;

	private float impulseIntensity;

	private float jointIntegrity;

	public float BoneIntegrityNormalized => boneIntegrity / startBoneIntegrity;

	public float JointIntegrityNormalized => jointIntegrity / startBoneIntegrity;

	private void Awake()
	{
		joint = GetComponent<HingeJoint>();
		rigidBody = GetComponent<Rigidbody>();
		physiology = GetComponent<Limb>().Body.Physiology;
		limbCollider = GetComponent<Limb>().GetComponent<Collider>();
		startBoneIntegrity = (jointIntegrity = boneIntegrity);
		if (joint != null)
		{
			spring = joint.spring;
		}
	}

	private void Update()
	{
		if (physiology.BrainTissueNormalized < 0.5f || joint == null || rigidBody.isKinematic)
		{
			return;
		}
		float num = Mathf.Clamp01((Time.time - impulseTimestamp) / impulseDuration);
		float num2 = ((!fadeUp) ? (1f - num) : Mathf.Abs(Mathf.Sin(num * (float)Math.PI)));
		float num3 = (extend + contract) * impulseIntensity * num2;
		if (num3 <= Mathf.Epsilon)
		{
			joint.useSpring = false;
			return;
		}
		float num4 = extend - contract;
		spring.spring = Mathf.Min(num3 * strength, strength * physiology.BloodOxygen);
		if (Mathf.Abs(num4) > Mathf.Epsilon)
		{
			spring.targetPosition = ((!(num4 > 0f)) ? contractAngle : extendAngle);
		}
		joint.spring = spring;
		joint.useSpring = true;
	}

	public void SetSignal(float extend, float contract, float duration, float intensity = 1f, bool fadeUp = false)
	{
		this.extend = Mathf.Clamp01(extend);
		this.contract = Mathf.Clamp01(contract);
		impulseTimestamp = Time.time;
		impulseDuration = duration;
		impulseIntensity = intensity;
		this.fadeUp = fadeUp;
	}

	private void SetTargetPosition(float position, float strength)
	{
		if (!(joint == null))
		{
			spring.spring = strength;
			spring.targetPosition = position;
			joint.spring = spring;
			if (!rigidBody.isKinematic)
			{
				joint.useSpring = true;
			}
		}
	}

	public void SetAngle(float angle, float strength)
	{
		SetTargetPosition(angle, strength);
	}

	public void LockCurrentAngle(float strengthScale = 1f, float duration = 1f)
	{
		if (!(joint == null) && !jointLocked)
		{
			SetTargetPosition(joint.angle, strength * strengthScale);
			jointLocked = true;
		}
	}

	public void Extend(float strength)
	{
		SetTargetPosition(extendAngle, strength);
	}

	public void Contract(float strength)
	{
		SetTargetPosition(contractAngle, strength);
	}

	public void StartSpasm(float extend, float contract, float duration, float intensity, bool fadeUp = false)
	{
		if (!(joint == null) && !rigidBody.isKinematic)
		{
			SetSignal(extend, contract, duration, intensity, fadeUp);
			joint.useSpring = true;
		}
	}

	public void Relax()
	{
		if (!(joint == null))
		{
			jointLocked = false;
			joint.useSpring = false;
		}
	}

	public void UpdateStrength(float strengthScale)
	{
		if (!(joint == null))
		{
			spring.spring = strength * strengthScale;
			joint.spring = spring;
			if (!rigidBody.isKinematic)
			{
				joint.useSpring = true;
			}
		}
	}

	public void AddJointTrauma(float volume, float normalizedLocalY)
	{
		float num = boneIntegrity;
		boneIntegrity -= volume;
		float num2 = Mathf.Abs(normalizedLocalY - 0.5f) * 2f;
		float num3 = volume * num2;
		jointIntegrity -= num3;
		if (num > 0f && boneIntegrity <= 0f)
		{
			physiology.OnJointBroken();
		}
	}
}
