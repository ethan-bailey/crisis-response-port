namespace Crisis.Runtime.FullSource
{
public class ConcussionMeasure : CharacterPsychologyMeasure
{
	public float sensitivity;

	public ConcussionMeasure(float sensitivity, bool reverse, float weight)
		: base(reverse, weight)
	{
		this.sensitivity = sensitivity;
	}

	public override float Calculate(CharacterBody self)
	{
		return self.Physiology.Concussion * sensitivity;
	}
}

}
