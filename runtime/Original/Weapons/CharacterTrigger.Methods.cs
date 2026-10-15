using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;
public partial class CharacterBody {
public bool QueryTrigger()
	{
		bool result = triggerPulled;
		triggerPulled = false;
		return result;
	}
public void PullTrigger()
	{
		if (rightArm.OnTarget)
		{
			triggerPulled = true;
		}
	}
public bool MoveMagazineToHand(bool rightHand = true)
	{
		WeaponBase activeWeapon = ActiveWeapon;
		if (activeWeapon == null)
		{
			return false;
		}
		WeaponComponentMagazine componentMagazine = activeWeapon.ComponentMagazine;
		if (componentMagazine == null)
		{
			return false;
		}
		UnityEngine.Object.Destroy(componentMagazine.GetComponent<Rigidbody>());
		Transform transform = ((!rightHand) ? leftArm.hand.grabPoint : rightArm.hand.grabPoint);
		componentMagazine.transform.eulerAngles = new Vector3(0f, RotationY, transform.eulerAngles.z - 90f);
		Vector3 vector = componentMagazine.ikTarget.position - componentMagazine.transform.position;
		componentMagazine.transform.position = transform.position - vector;
		componentMagazine.transform.SetParent(transform, true);
		componentMagazine.gameObject.SetActive(true);
		return true;
	}
}