namespace Crisis.Runtime.FullSource
{
using System;
using UnityEngine;

[Serializable]
public class PropagatingAudioSample
{
	public AudioClip sample;

	public float intensity = 1f;

	[Range(0f, 1f)]
	public float sampleVolume = 1f;

	public AudioStimuliType type;

	private AudioSource source;

	public PropagatingAudioSample(float intensity = 1f, AudioStimuliType type = AudioStimuliType.Equipment, float sampleVolume = 1f)
	{
		this.intensity = intensity;
		this.type = type;
		this.sampleVolume = sampleVolume;
	}

	public AudioSource Initialize(AudioSource audioSource)
	{
		source = audioSource;
		return audioSource;
	}

	public void Play(float position, AudioInstigator instigator, bool onlyOneSimultaneous = false)
	{
		if (sample != null && source != null)
		{
			if (onlyOneSimultaneous && source.isPlaying)
			{
				return;
			}
			source.PlayOneShot(sample, sampleVolume);
		}
		AudioPropagator.Instance.Emit(position, intensity, type, instigator);
	}

	public void PlayCustom(float position, AudioInstigator instigator, float customIntensity, float customSampleVolume, bool onlyOneSimultaneous = false)
	{
		if (sample != null && source != null)
		{
			if (onlyOneSimultaneous && source.isPlaying)
			{
				return;
			}
			source.PlayOneShot(sample, customSampleVolume);
		}
		AudioPropagator.Instance.Emit(position, customIntensity, type, instigator);
	}
}

}
