namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class WeaponBarrel : MonoBehaviour
{
	[Header("Barrel animation")]
	[Tooltip("Should the barrel be animated? Mainly for use with short recoil operated pistols.")]
	public bool animateBarrel;

	[Tooltip("How far the barrel will follow the slide in the recoil cycle before separating.")]
	public float followDistance = 0.005f;

	public AnimationCurve offset;

	public AnimationCurve angle;

	[Header("Chamber animation")]
	[Tooltip("Should the chamber feeding and extraction be animated?")]
	public bool animateChamber = true;

	[Tooltip("This transform holds casings in the barrel and animates them in and out.")]
	public Transform chamber;

	[Tooltip("Reference to the magazine slot transform. Used to set the positions of rounds being fed.")]
	public Transform magazine;

	[Tooltip("Animated vertical offset for rounds traveling to and from the chamber. Y: Vertical offset, in barrel's local space. X[0:1]: Bolt's normalized position of opening cycle. X[1:2]: Bolt's normalized position of the closing cycle. 0 when cycle starts, 1 on finished.")]
	public AnimationCurve feedOffset;

	[Tooltip("Animated angle for rounds being fed. Y: Angle, in barrel's local space. X: Bolt's normalized position of the closing cycle: 0 when closing starts, 1 when it finishes.")]
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

}
