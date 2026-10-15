using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;

public class WeaponBarrel : MonoBehaviour
{


	public bool animateBarrel;

	public float followDistance = 0.005f;

	public AnimationCurve offset;

	public AnimationCurve angle;


	public bool animateChamber = true;

	public Transform chamber;

	public Transform magazine;

	public AnimationCurve feedOffset;

	public AnimationCurve feedAngle;

	private Vector3 barrelPosition;

	private Vector3 barrelAngle;

	private Vector3 closedPosition;

	private Vector3 chamberPosition;

	private Vector3 chamberAngle;

	private float barrelToMagazineDistance;

	private void Awake()
	{
		barrelPosition = base.transform.localPosition;
		barrelAngle = base.transform.localEulerAngles;
		closedPosition = base.transform.localPosition;
		chamberPosition = chamber.localPosition;
		chamberAngle = chamber.localEulerAngles;
		barrelToMagazineDistance = Mathf.Abs(base.transform.position.x - magazine.position.x);
	}

	public void Animate(bool opening, float boltOffset, float openRatio, float cycle)
	{
		if (animateBarrel)
		{
			float num = Mathf.Min(followDistance, boltOffset);
			base.transform.localPosition = barrelPosition - new Vector3(num, offset.Evaluate(openRatio), 0f);
			base.transform.localEulerAngles = barrelAngle + new Vector3(0f, 0f, angle.Evaluate(openRatio));
			boltOffset -= num;
		}
		if (animateChamber)
		{
			if (opening)
			{
				chamber.localPosition = chamberPosition - new Vector3(boltOffset, feedOffset.Evaluate(Mathf.Min(openRatio, 0.999999f)), 0f);
				return;
			}
			float num2 = Mathf.Min(boltOffset, barrelToMagazineDistance);
			chamber.localPosition = chamberPosition + new Vector3(0f - num2, feedOffset.Evaluate(2f - openRatio), 0f);
			chamber.localEulerAngles = new Vector3(chamberAngle.x, chamberAngle.y, chamberAngle.z - feedAngle.Evaluate(1f - openRatio));
		}
	}

	public void SetClosed()
	{
		if (animateBarrel)
		{
			base.transform.localPosition = barrelPosition;
			base.transform.localEulerAngles = barrelAngle;
		}
		if (animateChamber)
		{
			chamber.localPosition = chamberPosition;
			chamber.localEulerAngles = chamberAngle;
		}
	}
}
