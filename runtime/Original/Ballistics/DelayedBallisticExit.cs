using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;

internal class DelayedBallisticExit
{
	public float activationTime;

	public Projectile projectile;

	public Vector3 localResumePosition;
}
