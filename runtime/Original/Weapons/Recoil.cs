using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;
public class Recoil
{
	public float timestamp;

	public float kick;

	public float climb;

	public Recoil(float time, float kickMagnitude, float climbMagnitude)
	{
		timestamp = time;
		kick = kickMagnitude;
		climb = climbMagnitude;
	}
}
