namespace Crisis.Runtime.FullSource
{
public class ShockMeasure : CharacterPsychologyMeasure
{
	public float sensitivity;

	public ShockMeasure(float sensitivity, bool reverse, float weight)
		: base(reverse, weight)
	{
		this.sensitivity = sensitivity;
	}

	public override float Calculate(CharacterBody self)
	{
		return self.Physiology.ShockNormalized * sensitivity;
	}
}

}
