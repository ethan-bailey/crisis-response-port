using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;
public class ArmIKSolver:Component {
 public enum HandRotationMode{Local,Global,TargetLocal}
 public AnimationCurve interpolation;public Limb upperArm;public Hand hand;public Transform ikTarget,transitionTarget;
 public bool smoothing;private bool transition;private float transitionTimestamp,transitionPeriod,transitionHandAngle;
 private Hand.Mode transitionHandMode;private HandRotationMode transitionHandRotationMode;
 public bool OnTarget{get;private set;} public bool OnWeapon{get;private set;}
 public Transform TransitionTarget=>transitionTarget;
 public void Step(){if(transition)UpdateTransition();}
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