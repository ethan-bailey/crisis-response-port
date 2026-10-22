namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class WeaponArmConfiguration : MonoBehaviour
{
	[Header("Right hand")]
	[Tooltip("This is the IK target location for the right hand, relative to the weapon origin.")]
	public Vector2 rightHandOffset;

	[Range(-90f, 90f)]
	public float rightHandAngle;

	public Hand.Mode rightHandMode;

	[Header("Left hand")]
	[Tooltip("This is the IK target location for the left hand, relative to the weapon origin.")]
	public Vector2 leftHandOffset;

	[Range(-90f, 90f)]
	public float leftHandAngle;

	public Hand.Mode leftHandMode = Hand.Mode.Fist;

	[Header("Upper body")]
	[Range(-50f, 50f)]
	public float headAngle;

	[Range(-50f, 50f)]
	public float torsoAngle;

	[Header("Weapon")]
	[Tooltip("Only for free-hand weapons (not pivoted on shoulder): The distance the weapon will be held from the torso.")]
	[Range(0f, 1f)]
	public float holdOffDistance = 0.5f;

	[Tooltip("While retracted the weapon will be angled onto a point on the ground this far ahead of the character.")]
	[Range(0f, 3f)]
	public float retractDistance = 1f;
}

}
