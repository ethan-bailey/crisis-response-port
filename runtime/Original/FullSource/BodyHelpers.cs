namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public static class BodyHelpers
{
	public static bool TargetFacesSubject(CharacterBody subject, CharacterBody target)
	{
		return Mathf.Sign(target.Position - subject.Position) != subject.Direction;
	}

	public static bool SubjectFacesTarget(CharacterBody subject, CharacterBody target)
	{
		return Mathf.Sign(subject.Position - target.Position) != target.Direction;
	}

	public static float TargetWeaponAngleRelativeSubjectHead(CharacterBody subject, CharacterBody target)
	{
		if (!TargetFacesSubject(subject, target))
		{
			return 90f;
		}
		WeaponBase activeWeapon = target.ActiveWeapon;
		if (activeWeapon == null)
		{
			return 90f;
		}
		Vector3 normalized = (subject.Head.transform.position - activeWeapon.Pivot).normalized;
		return Vector3.Angle(activeWeapon.transform.right, normalized);
	}

	public static float TimeSinceSubjectFired(CharacterBody subject)
	{
		if (subject.ActiveWeapon == null)
		{
			return float.PositiveInfinity;
		}
		return Mathf.Max(0f, Time.time - subject.LastFiredTimestamp);
	}
}

}
