namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class MarksmanMeasure : CharacterPsychologyMeasure
{
	private Marksman marksman;

	public MarksmanMeasure(Marksman marksman, bool reverse, float weight)
		: base(reverse, weight)
	{
		this.marksman = marksman;
	}

	public override float Calculate(CharacterBody self)
	{
		float num = Time.time - marksman.FireTimestamp;
		if (num >= 0.16f && num < 5f)
		{
			base.Source.type = ActionSource.Type.Marksman;
			return (!reverse) ? 1f : 0f;
		}
		return (!reverse) ? 0f : 1f;
	}
}

}
