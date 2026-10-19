namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class AudioMeasure : CharacterPsychologyMeasure
{
	private float explosionFactor;

	private float otherFactor;

	private const float MaxDistance = 20f;

	public AudioMeasure(float explosionFactor, float otherFactor, bool reverse, float weight)
		: base(reverse, weight)
	{
		this.explosionFactor = explosionFactor;
		this.otherFactor = otherFactor;
	}

	public override float Calculate(CharacterBody self)
	{
		AudioStimuli audioStimuli = self.Blackboard.AudioStimuli;
		if (audioStimuli != null)
		{
			base.Source.type = ActionSource.Type.Audio;
			base.Source.position = audioStimuli.position;
			bool flag = audioStimuli.type == AudioStimuliType.Explosion || audioStimuli.type == AudioStimuliType.Firearm;
			float num = Mathf.Clamp(Mathf.Abs(audioStimuli.position - self.Position), 1f, 20f) / 20f;
			float num2 = ((!reverse) ? (1f - num) : num);
			return num2 * ((!flag) ? otherFactor : explosionFactor);
		}
		return (!reverse) ? 0f : 1f;
	}
}

}
