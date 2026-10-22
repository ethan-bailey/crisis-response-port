namespace Crisis.Runtime.FullSource
{
public class HealthMeasure : CharacterPsychologyMeasure
{
	public float sensitivity = 1f;

	public HealthMeasure(float sensitivity, bool reverse, float weight)
		: base(reverse, weight)
	{
		this.sensitivity = sensitivity;
	}

	public override float Calculate(CharacterBody self)
	{
		return (1f - self.Physiology.BloodVolumeNormalized) * sensitivity;
	}
}

}
