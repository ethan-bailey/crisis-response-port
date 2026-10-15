using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
public enum BallisticResult
{
	Continue = 0,
	Deflected = 1,
	Stop = 2,
	Delayed = 3,
	Error = 4
}
