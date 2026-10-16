using CharacterBody = Crisis.Runtime.Weapons.CharacterBody;
using Grenade = Crisis.Runtime.Weapons.GrenadeReference;
using System;
using UnityEngine;

[Serializable]
public class ArmController
{
	public enum Mode
	{
		HoldWeapon = 0,
		HoldGrenade = 1,
		TargetTransform = 2,
		RelativePosition = 3,
		Free = 4
	}

	public Mode mode;

	[Tooltip("The arm will stretch until the hand reaches the position of this transform.")]
	public Transform targetTransform;

	public Vector3 targetPosition;

	[Tooltip("How close to get to the target to consider it reached. In meters.")]
	public float tolerance = 0.15f;

	[Tooltip("When getting close to target the force magnitude is scaled down, but not lower than this value.")]
	[Range(0f, 1f)]
	public float minimumForce = 0.5f;

	public Transform upperArm;

	public Transform lowerArm;

	public Hand hand;

	public bool isRightArm;

	private CharacterBody body;

	private ArmsControl controller;

	private Rigidbody upperArmRb;

	private Rigidbody lowerArmRb;

	private Rigidbody handRb;

	private HingeJoint upperArmJoint;

	private HingeJoint lowerArmJoint;

	private HingeJoint handJoint;

	private static float maxRange = 0.5f;

	private static float forceFactor = 50f;

	public bool IsKinematic
	{
		get
		{
			return upperArmRb == null || upperArmRb.isKinematic;
		}
	}

	public bool SpringEnabled
	{
		get
		{
			return !(upperArmJoint == null) && upperArmJoint.useSpring;
		}
	}

	public bool TargetReached { get; private set; }

	public void Initialize(ArmsControl controller, CharacterBody body)
	{
		this.body = body;
		this.controller = controller;
		upperArmRb = upperArm.GetComponent<Rigidbody>();
		lowerArmRb = lowerArm.GetComponent<Rigidbody>();
		handRb = hand.GetComponent<Rigidbody>();
		upperArmJoint = upperArm.GetComponent<HingeJoint>();
		lowerArmJoint = lowerArm.GetComponent<HingeJoint>();
		handJoint = hand.GetComponent<HingeJoint>();
	}

	public void Update()
	{
		if (body.Kinematic)
		{
			return;
		}
		switch (mode)
		{
		case Mode.HoldWeapon:
			EnableSpring(false);
			if (!upperArmRb.isKinematic)
			{
				targetTransform = ((!isRightArm) ? body.ActiveWeapon.leftHandAnchor : body.ActiveWeapon.rightHandAnchor);
				StretchTowardTransform();
				if (TargetReached)
				{
					SetKinematic(true);
				}
			}
			break;
		case Mode.HoldGrenade:
			if (upperArmRb.isKinematic)
			{
				SetKinematic(false);
			}
			if (!TargetReached && !SpringEnabled)
			{
				targetTransform = ((body.PendingGrenadeType != Grenade.Type.Frag) ? body.stunGrenadeAnchor : body.fragGrenadeAnchor);
				tolerance = 0.05f;
				StretchTowardTransform();
				if (TargetReached)
				{
					EnableSpring(true);
				}
			}
			break;
		case Mode.TargetTransform:
			EnableSpring(false);
			if (upperArmRb.isKinematic)
			{
				SetKinematic(false);
			}
			StretchTowardTransform();
			break;
		case Mode.RelativePosition:
			EnableSpring(false);
			if (upperArmRb.isKinematic)
			{
				SetKinematic(false);
			}
			StretchTowardRelativePosition();
			break;
		}
	}

	public void SetTargetPosition(Vector3 position)
	{
		targetPosition = position;
	}

	public void Relax()
	{
		mode = Mode.Free;
		EnableSpring(false);
		SetKinematic(false);
	}

	public void SetOnWeapon()
	{
		mode = Mode.HoldWeapon;
		forceFactor = 30f;
		tolerance = 0.05f;
	}

	public void SetOnTarget(Transform target = null, float force = 50f, float tolerance = 0.05f)
	{
		mode = Mode.TargetTransform;
		forceFactor = force;
		this.tolerance = tolerance;
		targetTransform = target;
	}

	public void SetOnRelativePosition()
	{
		mode = Mode.RelativePosition;
	}

	private void StretchTowardTransform()
	{
		if (targetTransform == null)
		{
			return;
		}
		Vector3 vector = hand.transform.TransformPoint(0f, -0.05f, 0f);
		Vector3 vector2 = targetTransform.position - vector;
		vector2.z = 0f;
		float magnitude = vector2.magnitude;
		float num = Mathf.Clamp(magnitude, minimumForce, maxRange) / maxRange;
		Vector3 force = vector2.normalized * num * forceFactor * Time.deltaTime * 60f;
		Vector3 position = hand.transform.TransformPoint(0f, -0.1f, 0f);
		lowerArmRb.AddForceAtPosition(force, position, ForceMode.Acceleration);
		TargetReached = magnitude <= tolerance;
		if (TargetReached)
		{
			if (!SpringEnabled)
			{
				EnableSpring(true);
			}
		}
		else if (SpringEnabled)
		{
			EnableSpring(false);
		}
	}

	private void StretchTowardRelativePosition()
	{
		Vector3 vector = body.transform.position + targetPosition;
		Vector3 vector2 = hand.transform.TransformPoint(0f, -0.05f, 0f);
		Vector3 vector3 = vector - vector2;
		vector3.z = 0f;
		float magnitude = vector3.magnitude;
		float num = Mathf.Clamp(magnitude, minimumForce, maxRange) / maxRange;
		Vector3 force = vector3.normalized * num * forceFactor * Time.deltaTime * 60f;
		Vector3 position = hand.transform.TransformPoint(0f, -0.1f, 0f);
		lowerArmRb.AddForceAtPosition(force, position, ForceMode.Acceleration);
		TargetReached = magnitude <= tolerance;
		if (TargetReached)
		{
			if (!SpringEnabled)
			{
				EnableSpring(true);
			}
		}
		else if (SpringEnabled)
		{
			EnableSpring(false);
		}
	}

	private void SetKinematic(bool state)
	{
		upperArmRb.isKinematic = state;
		lowerArmRb.isKinematic = state;
		handRb.isKinematic = state;
	}

	private void EnableSpring(bool state)
	{
		if ((bool)upperArmJoint)
		{
			upperArmJoint.useSpring = state;
			JointSpring spring = upperArmJoint.spring;
			spring.spring = 500f;
			spring.damper = 10f;
			spring.targetPosition = upperArmJoint.angle;
			upperArmJoint.spring = spring;
		}
		if ((bool)lowerArmJoint)
		{
			lowerArmJoint.useSpring = state;
			JointSpring spring2 = lowerArmJoint.spring;
			spring2.spring = 500f;
			spring2.damper = 10f;
			spring2.targetPosition = lowerArmJoint.angle;
			lowerArmJoint.spring = spring2;
		}
		if ((bool)handJoint)
		{
			handJoint.useSpring = state;
			JointSpring spring3 = handJoint.spring;
			spring3.spring = 500f;
			spring3.damper = 10f;
			spring3.targetPosition = handJoint.angle;
			handJoint.spring = spring3;
		}
	}

	public void SetHandMode(Hand.Mode mode)
	{
		hand.SetMode(mode);
	}

	public Hand.Mode HandMode()
	{
		return hand.CurrentMode;
	}
}
