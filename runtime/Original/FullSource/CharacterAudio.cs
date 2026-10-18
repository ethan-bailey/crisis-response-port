namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
	[Header("Sources")]
	public AudioSource impactAudio;

	public AudioSource footstepAudio;

	public AudioSource equipmentAudio;

	public AudioSource communicationAudio;

	[Header("Samples")]
	public List<AudioClip> impactExhaleSamples;

	public List<AudioClip> impactArmorSamples;

	public List<AudioClip> hurtSamples;

	[Tooltip("This is played when characters have obstructed respiration.")]
	public List<AudioClip> snoreSamples;

	[Tooltip("Characters may cough when they for instance have punctured lungs.")]
	public List<AudioClip> coughSamples;

	public List<AudioClip> squirtSamples;

	[Header("Footstep")]
	[Tooltip("This sample will be played if the character's current surface fails to return a sample.")]
	public AudioClip defaultFootstep;

	public List<AudioClip> locomotionFrictionSamples;

	[Header("Equipment")]
	[Tooltip("Volume modifier for equipment audio.")]
	[Range(0f, 1f)]
	public float equipmentVolume = 0.5f;

	[Tooltip("Propagation intensity for equipment audio.")]
	[Range(0f, 5f)]
	public float equipmentPropagation = 10f;

	public AudioClip throwGrenade;

	public AudioClip unholster;

	public AudioClip holster;

	public List<AudioClip> plasticuffSamples;

	private CharacterBody body;

	private float weaponMovementTimestamp;

	private float snoreTimestamp;

	private float hurtTimestamp;

	public AudioPropagator Propagator { get; private set; }

	private float NormalizedLocomotionIntensity
	{
		get
		{
			return Mathf.Pow(Mathf.Max(0f, Mathf.Abs(body.Velocity) - body.walkSpeed) / (body.sprintSpeed - body.walkSpeed), 2f);
		}
	}

	private float LocomotionIntensity
	{
		get
		{
			return AudioPropagator.LocomotionAudioPower(NormalizedLocomotionIntensity);
		}
	}

	public bool VoicePlaying
	{
		get
		{
			return communicationAudio.isPlaying;
		}
	}

	private void Awake()
	{
		body = GetComponent<CharacterBody>();
		Propagator = AudioPropagator.Instance;
		weaponMovementTimestamp = Time.time;
		communicationAudio.priority = 120;
		impactAudio.priority = 60;
		equipmentAudio.priority = 140;
		footstepAudio.priority = 160;
	}

	public void StopVoice()
	{
		communicationAudio.Stop();
	}

	public void ArmorImpact()
	{
		impactAudio.PlayOneShot(impactArmorSamples[Random.Range(0, impactArmorSamples.Count)]);
	}

	public void ImpactExhale()
	{
		impactAudio.PlayOneShot(impactExhaleSamples[Random.Range(0, impactExhaleSamples.Count)]);
	}

	public void Hurt(float delay = 0f)
	{
		if (hurtSamples.Count != 0)
		{
			if (delay > 0f)
			{
				Coroutine.DelayedAction(delay, PlayHurt, this);
			}
			else
			{
				PlayHurt();
			}
		}
	}

	private void PlayHurt()
	{
		if (body.Conscious && !(Time.time - hurtTimestamp < 3f))
		{
			communicationAudio.PlayOneShot(hurtSamples[Random.Range(0, hurtSamples.Count)]);
			Propagator.Emit(body.Position, 30f, AudioStimuliType.Equipment, body.AudioInstigator);
			hurtTimestamp = Time.time;
		}
	}

	public void Snore()
	{
		if (!(Time.time - snoreTimestamp < 1.5f))
		{
			communicationAudio.PlayOneShot(snoreSamples[Random.Range(0, snoreSamples.Count)]);
			Propagator.Emit(body.Position, 20f, AudioStimuliType.Equipment, body.AudioInstigator);
			snoreTimestamp = Time.time;
		}
	}

	public void Cough()
	{
		communicationAudio.PlayOneShot(coughSamples[Random.Range(0, coughSamples.Count)]);
		Propagator.Emit(body.Position, 30f, AudioStimuliType.Equipment, body.AudioInstigator);
	}

	public void BloodSquirt(float volume = 1f)
	{
		equipmentAudio.PlayOneShot(squirtSamples[Random.Range(0, squirtSamples.Count)], volume);
	}

	public void Footstep()
	{
		if (!(body.AudioSurface == null))
		{
			AudioClip audioClip = body.AudioSurface.RandomFootstep();
			footstepAudio.PlayOneShot((!(audioClip != null)) ? defaultFootstep : audioClip, 0.25f + 0.5f * NormalizedLocomotionIntensity);
			Propagator.Emit(base.transform.position.x, LocomotionIntensity, AudioStimuliType.Footstep, body.AudioInstigator);
		}
	}

	public void Holster()
	{
		equipmentAudio.PlayOneShot(holster, equipmentVolume);
		Propagator.Emit(base.transform.position.x, equipmentPropagation, AudioStimuliType.Equipment, body.AudioInstigator);
	}

	public void Unholster()
	{
		equipmentAudio.PlayOneShot(unholster, equipmentVolume);
		Propagator.Emit(base.transform.position.x, equipmentPropagation, AudioStimuliType.Equipment, body.AudioInstigator);
	}

	public void ThrowGrenade()
	{
		equipmentAudio.PlayOneShot(throwGrenade, equipmentVolume);
		Propagator.Emit(base.transform.position.x, equipmentPropagation, AudioStimuliType.Equipment, body.AudioInstigator);
	}

	public void Plasticuffs()
	{
		if (plasticuffSamples.Count != 0)
		{
			equipmentAudio.PlayOneShot(plasticuffSamples[Random.Range(0, plasticuffSamples.Count)], 1f);
			Propagator.Emit(base.transform.position.x, equipmentPropagation, AudioStimuliType.Equipment, body.AudioInstigator);
		}
	}
}

}
