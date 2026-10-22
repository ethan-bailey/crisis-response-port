namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class TargetWeaponMeasure : CharacterPsychologyMeasure
{
	public TargetWeaponMeasure(bool reverse, float weight)
		: base(reverse, weight)
	{
		base.reverse = reverse;
	}

	public override float Calculate(CharacterBody self)
	{
		CharacterBody characterBody = ((!(self.Blackboard.Target != null)) ? null : self.Blackboard.Target.Body);
		if (characterBody == null)
		{
			return (!reverse) ? 0f : 1f;
		}
		base.Source.type = ActionSource.Type.Character;
		base.Source.transform = characterBody.transform;
		base.Source.body = characterBody;
		if (Mathf.Sign(characterBody.Position - self.Position) != self.Direction)
		{
			return (!reverse) ? 0f : 1f;
		}
		WeaponBase activeWeapon = characterBody.ActiveWeapon;
		if (activeWeapon == null)
		{
			return (!reverse) ? 0f : 1f;
		}
		return Mathf.Clamp01((70f - Mathf.Abs(activeWeapon.Angle)) / 70f);
	}
}

}
