namespace Crisis.Runtime.FullSource
{
using System;

public class AudioStimuli
{
	public float timestamp;

	public float position;

	public float power;

	public AudioStimuliType type;

	public AudioInstigator instigator;

	public Guid guid;

	public AudioStimuli()
	{
		timestamp = -100f;
		position = -100f;
		power = 0f;
		type = AudioStimuliType.Equipment;
		instigator = null;
		guid = Guid.NewGuid();
	}

	public AudioStimuli(float timestamp, float position, float power, AudioStimuliType type, AudioInstigator instigator)
	{
		this.timestamp = timestamp;
		this.position = position;
		this.power = power;
		this.type = type;
		this.instigator = instigator;
		guid = Guid.NewGuid();
	}
}

}
