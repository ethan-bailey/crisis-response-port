namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class CharacterPsychologyDriver
{
	private List<CharacterPsychologyMeasure> measures = new List<CharacterPsychologyMeasure>();

	private float latestValue;

	private float timestamp;

	private float period = 0.2f;

	public CharacterPsychologyMeasure TopMeasure { get; private set; }

	public float Calculate(CharacterBody character)
	{
		if (Time.time - timestamp < period)
		{
			return latestValue;
		}
		float num = 0f;
		float num2 = float.NegativeInfinity;
		foreach (CharacterPsychologyMeasure measure in measures)
		{
			float num3 = measure.Value(Time.time, character);
			num += num3;
			if (num3 > num2)
			{
				TopMeasure = measure;
				num2 = num3;
			}
		}
		latestValue = Mathf.Clamp01(num);
		timestamp = Time.time;
		return latestValue;
	}

	public void Add(CharacterPsychologyMeasure measure)
	{
		measures.Add(measure);
	}
}

}
