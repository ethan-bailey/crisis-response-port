namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class ComplianceMeasure : CharacterPsychologyMeasure
{
	private float minimumSeconds;

	private float fadeSeconds;

	public ComplianceMeasure(float minimumSeconds, float fadeSeconds)
		: base(false, 1f)
	{
		this.minimumSeconds = minimumSeconds;
		this.fadeSeconds = fadeSeconds;
	}

	public override float Calculate(CharacterBody self)
	{
		float num = Time.time - self.ComplianceStateTimestamp;
		if (num <= minimumSeconds)
		{
			return 1f;
		}
		if (num > minimumSeconds + fadeSeconds)
		{
			return 0f;
		}
		return (num - minimumSeconds) / fadeSeconds;
	}
}

}
