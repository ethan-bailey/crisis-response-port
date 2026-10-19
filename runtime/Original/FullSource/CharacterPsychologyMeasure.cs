namespace Crisis.Runtime.FullSource
{
public abstract class CharacterPsychologyMeasure
{
	public float period;

	public bool reverse;

	public float weight = 1f;

	protected float value;

	private float timestamp;

	public ActionSource Source { get; protected set; }

	public CharacterPsychologyMeasure(bool reverse, float weight)
	{
		Source = new ActionSource();
		this.reverse = reverse;
		this.weight = weight;
	}

	public abstract float Calculate(CharacterBody self);

	public float Value(float time, CharacterBody self)
	{
		if (time - timestamp < period)
		{
			return value;
		}
		timestamp = time;
		if (reverse)
		{
			value = (1f - Calculate(self)) * weight;
		}
		else
		{
			value = Calculate(self) * weight;
		}
		return value;
	}
}

}
