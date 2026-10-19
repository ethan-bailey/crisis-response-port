namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class TargetProximityMeasure : CharacterPsychologyMeasure
{
	public float minimumDistance = 1f;

	public float facingAwayFactor = 0.1f;

	public TargetProximityMeasure(float minimumDistance, float facingAwayFactor, bool reverse, float weight)
		: base(reverse, weight)
	{
		base.reverse = reverse;
		this.minimumDistance = minimumDistance;
		this.facingAwayFactor = facingAwayFactor;
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
		float f = characterBody.Position - self.Position;
		float num = ((Mathf.Sign(f) != self.Direction) ? facingAwayFactor : 1f);
		return 1f - Mathf.Clamp01(Mathf.Max(Mathf.Abs(f) - minimumDistance, 0f) / self.VisibleRange) * num;
	}
}

}
