namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
	[Header("Effects")]
	public AudioClip blip;

	public AudioClip toggle;

	public AudioClip doubleBlip;

	public AudioClip notice;

	public AudioClip benchmarkSuccess;

	public AudioClip benchmarkFailure;

	private AudioSource audioSource;

	private float uiAudioTimestamp;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public void Play(AudioClip clip)
	{
		audioSource.PlayOneShot(clip);
	}

	public void PlayBlip()
	{
		if (Time.realtimeSinceStartup - uiAudioTimestamp > 0.25f)
		{
			audioSource.PlayOneShot(blip);
			uiAudioTimestamp = Time.time;
		}
	}

	public void PlayToggle()
	{
		if (Time.realtimeSinceStartup - uiAudioTimestamp > 0.25f)
		{
			audioSource.PlayOneShot(toggle);
			uiAudioTimestamp = Time.time;
		}
	}

	public void PlayDoubleBlip()
	{
		if (Time.realtimeSinceStartup - uiAudioTimestamp > 0.25f)
		{
			audioSource.PlayOneShot(doubleBlip);
			uiAudioTimestamp = Time.time;
		}
	}

	public void PlaySelect()
	{
	}

	public void PlayDeselect()
	{
	}

	public void PlayOpen()
	{
	}

	public void PlayClose()
	{
	}

	public void PlayNotice()
	{
		if (Time.realtimeSinceStartup - uiAudioTimestamp > 0.25f)
		{
			audioSource.PlayOneShot(notice);
			uiAudioTimestamp = Time.time;
		}
	}

	public void PlayBenchmarkResult(bool success)
	{
		audioSource.PlayOneShot((!success) ? benchmarkFailure : benchmarkSuccess);
	}
}

}
