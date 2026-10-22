namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class GrenadeMeasure : CharacterPsychologyMeasure
{
	public GrenadeMeasure(bool reverse, float weight)
		: base(reverse, weight)
	{
	}

	public override float Calculate(CharacterBody self)
	{
		Grenade perceivedGrenade = self.Blackboard.PerceivedGrenade;
		if (perceivedGrenade == null)
		{
			return (!reverse) ? 0f : 1f;
		}
		base.Source.type = ActionSource.Type.Grenade;
		base.Source.transform = perceivedGrenade.transform;
		float num = Mathf.Clamp(Mathf.Abs(self.Position - perceivedGrenade.Position), 1f, self.VisionRange) / self.VisionRange;
		return (!reverse) ? (1f - num) : num;
	}
}

}
