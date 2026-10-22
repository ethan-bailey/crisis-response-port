using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;
// Original selected method bodies. Explicit host scheduling, not full controller playback.
public class ArmIKSolver:Component {
 public enum HandRotationMode{Local=0,Global=1,TargetLocal=2}
 public AnimationCurve interpolation;public bool flip;
 public Limb upperArm,lowerArm;public Hand hand;public bool smoothing;public float smoothFactor=10f;
 public Transform ikTarget,transitionTarget;private Limb handLimb;
 private Hand.Mode transitionHandMode;private HandRotationMode transitionHandRotationMode;
 private float upperArmLength,lowerArmLength,upperArmAngle,lowerArmAngle;
 private float transitionTimestamp,transitionPeriod,transitionHandAngle;
 private bool transition;private static float HalfPi=(float)Math.PI/2f;
 public bool HasSolution{get;private set;}public bool OnTarget{get;private set;}public bool OnWeapon{get;private set;}
 public bool Transitioning=>transition;
public Transform TransitionTarget
	{
		get
		{
			if (transitionTarget == null)
			{
				GameObject gameObject = new GameObject("IK transition");
				gameObject.transform.SetParent(base.transform);
				transitionTarget = gameObject.transform;
			}
			return transitionTarget;
		}
	}
 public void InitializeSourcePose()=>Awake();
 public void Step()=>Update();
private void Awake()
	{
		GameObject gameObject = new GameObject("IK target");
		gameObject.transform.SetParent(base.transform.parent);
		ikTarget = gameObject.transform;
		handLimb = hand.GetComponent<Limb>();
		upperArmLength = (lowerArm.transform.position - upperArm.transform.position).magnitude;
		lowerArmLength = (hand.transform.position - lowerArm.transform.position).magnitude + 0.08f;
		TargetTransition(base.transform.parent, new Vector3(UnityEngine.Random.Range(0.15f, 0.3f) * upperArm.Body.Direction, UnityEngine.Random.Range(-0.05f, -0.075f), 0f), 0.5f);
	}
private void Update()
	{
		if (transition)
		{
			UpdateTransition();
		}
		CharacterBody body = upperArm.Body;
		float x = ((body.Direction != 1f) ? (base.transform.position.x - ikTarget.position.x) : (ikTarget.position.x - base.transform.position.x));
		Vector3 vector = new Vector3(x, ikTarget.position.y - base.transform.position.y, 0f);
		HasSolution = InverseKinematics2D.SolveTwoBoneAnalytic(out upperArmAngle, out lowerArmAngle, !flip, upperArmLength, lowerArmLength, vector.x, vector.y);
		upperArmAngle = (upperArmAngle + HalfPi) * 57.29578f;
		lowerArmAngle = upperArmAngle + lowerArmAngle * 57.29578f;
		if (smoothing || transition)
		{
			upperArmAngle = Mathf.LerpAngle(upperArm.transform.eulerAngles.z, upperArmAngle, Time.deltaTime * smoothFactor);
			lowerArmAngle = Mathf.LerpAngle(lowerArm.transform.eulerAngles.z, lowerArmAngle, Time.deltaTime * smoothFactor);
		}
		float rotationY = body.RotationY;
		upperArm.transform.eulerAngles = new Vector3(0f, rotationY, upperArmAngle);
		lowerArm.transform.eulerAngles = new Vector3(0f, rotationY, lowerArmAngle);
	}
private void UpdateTransition()
	{
		if (ikTarget == null)
		{
			OnTarget = true;
			transition = false;
			return;
		}
		float num = interpolation.Evaluate((Time.time - transitionTimestamp) / transitionPeriod);
		ikTarget.position = Vector3.Lerp(ikTarget.position, TransitionTarget.position, num);
		SetHandAngle(Mathf.LerpAngle(GetHandAngle(transitionHandRotationMode), transitionHandAngle, num), transitionHandRotationMode);
		if (num >= 1f)
		{
			OnTarget = true;
			transition = false;
			ikTarget.SetParent(TransitionTarget.parent);
			ikTarget.position = TransitionTarget.position;
			TransitionTarget.SetParent(base.transform);
			hand.SetMode(transitionHandMode);
			SetHandAngle(transitionHandAngle);
		}
	}
public void SetTargetImmediate(Transform target, Hand.Mode handMode = Hand.Mode.Fist, float handAngle = 0f, HandRotationMode handRotationMode = HandRotationMode.Local)
	{
		transition = false;
		OnWeapon = false;
		ikTarget.SetParent(target);
		TransitionTarget.SetParent(target);
		hand.SetMode(handMode);
		SetHandAngle(handAngle, handRotationMode);
	}
public void TargetTransition(Transform target, float seconds, Hand.Mode handMode = Hand.Mode.Fist, float handAngle = 0f, HandRotationMode handRotationMode = HandRotationMode.Local)
	{
		TargetTransition(target, Vector3.zero, seconds, handMode, handAngle, handRotationMode);
	}
public void TargetTransition(Transform target, Vector3 worldSpaceOffset, float seconds, Hand.Mode handMode = Hand.Mode.Fist, float handAngle = 0f, HandRotationMode handRotationMode = HandRotationMode.Local)
	{
		OnTarget = false;
		OnWeapon = false;
		transition = true;
		TransitionTarget.SetParent(target);
		TransitionTarget.localEulerAngles = Vector3.zero;
		TransitionTarget.localPosition = target.InverseTransformVector(worldSpaceOffset);
		transitionTimestamp = Time.time;
		transitionPeriod = seconds;
		transitionHandMode = handMode;
		transitionHandAngle = handAngle;
		transitionHandRotationMode = handRotationMode;
		
	}
public void StartWait()
	{
		OnTarget = false;
	}
public void ReturnTransforms()
	{
		ikTarget.SetParent(upperArm.transform);
		ikTarget.position = TransitionTarget.position;
		ikTarget.eulerAngles = TransitionTarget.eulerAngles;
	}
public void SetHandAngle(float angle, HandRotationMode mode = HandRotationMode.Local)
	{
		switch (mode)
		{
		case HandRotationMode.Local:
			hand.transform.localEulerAngles = new Vector3(0f, 0f, angle);
			break;
		case HandRotationMode.Global:
			hand.transform.eulerAngles = new Vector3(0f, 0f, angle);
			break;
		}
	}
public float GetHandAngle(HandRotationMode mode = HandRotationMode.Local)
	{
		switch (mode)
		{
		case HandRotationMode.Local:
			return hand.transform.localEulerAngles.z;
		case HandRotationMode.Global:
			return hand.transform.eulerAngles.z;
		case HandRotationMode.TargetLocal:
			return (!(TransitionTarget != null)) ? hand.transform.localEulerAngles.z : (TransitionTarget.eulerAngles.z - hand.transform.eulerAngles.z);
		default:
			return hand.transform.localEulerAngles.z;
		}
	}
public void SetOnWeapon(WeaponBase weapon, float seconds = 0.1f, bool rightArm = true)
	{
		if (weapon == null)
		{
			SetUnarmed();
			return;
		}
		Transform transform = null;
		if (weapon.type == WeaponBrace.Freehand && weapon.Owner != null)
		{
			transform = weapon.Owner.pistolIKTarget;
			smoothing = true;
		}
		else
		{
			transform = ((!rightArm) ? weapon.leftHandAnchor : weapon.rightHandAnchor);
			smoothing = false;
		}
		Hand.Mode handMode = ((!rightArm) ? weapon.armConfiguration.leftHandMode : weapon.armConfiguration.rightHandMode);
		float handAngle = ((!rightArm) ? weapon.armConfiguration.leftHandAngle : weapon.armConfiguration.rightHandAngle);
		if (seconds == 0f)
		{
			SetTargetImmediate(transform, handMode, handAngle);
		}
		else
		{
			TargetTransition(transform, seconds, handMode, handAngle);
		}
		OnWeapon = true;
	}
public void SetUnarmed()
	{
		Transform target = upperArm.Body.Pelvis.transform;
		Vector3 worldSpaceOffset = new Vector3(0.25f, 0.15f, 0f) * upperArm.Body.Direction;
		TargetTransition(target, worldSpaceOffset, 0.3f);
	}
}
