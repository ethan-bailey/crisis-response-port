using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using System;
using UnityEngine;

[Serializable]
public class LimbVitalFunction
{
	public enum Type
	{
		None,
		Bone,
		BloodVessels,
		Lungs,
		Heart,
		Spine,
		Brain
	}

	public string name = "Unnamed vital function";

	public Type type;

	[Tooltip("The probability that this vital function will be affected when a hit is registered. Simulates organ's relative width to the body.")]
	[Range(0f, 1f)]
	public float probability = 0.5f;

	[Tooltip("Sets the amount to scale inflicted trauma by, as a function of the hit's normalized position on the owner limb's local Y axis (0.5 is the middle of the limb, 1.0 the top).")]
	public AnimationCurve factor;
}

}
