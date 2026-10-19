namespace Crisis.Runtime.FullSource
{
public class WoundedAlliesMeasure : CharacterPsychologyMeasure
{
	public WoundedAlliesMeasure(bool reverse, float weight)
		: base(reverse, weight)
	{
		period = 0.5f;
	}

	public override float Calculate(CharacterBody self)
	{
		float num = 0f;
		foreach (CharacterBody friend in self.Blackboard.Friends)
		{
			num += ((!friend.IsRagdoll) ? 0f : 1f);
		}
		return num;
	}
}

}
