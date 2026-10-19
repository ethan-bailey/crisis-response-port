namespace Crisis.Runtime.FullSource
{
using System;

[Serializable]
public class SerializableOperatorAttributes
{
	public float perception = 0.5f;

	public float empathy = 0.5f;

	public float communication = 0.5f;

	public float strictness = 0.5f;

	public float stressTolerance = 0.5f;

	public SerializableOperatorAttributes()
	{
	}

	public SerializableOperatorAttributes(SerializableOperatorAttributes data)
	{
		Overwrite(data);
	}

	public void Overwrite(SerializableOperatorAttributes data)
	{
		perception = data.perception;
		empathy = data.empathy;
		communication = data.communication;
		strictness = data.strictness;
		stressTolerance = data.stressTolerance;
	}
}

}
