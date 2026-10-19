namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine;

public class BallisticHitEffect : MonoBehaviour
{
	[Tooltip("Moves the spawn position of the emission back in the direction the projectile came from. In meters. This is to prevent particles getting trapped inside the geometry the projectile hit.")]
	public float offset;

	[Tooltip("A random audio clip from this list will be played.")]
	public List<AudioClip> audioClips;

	private AudioSource audioSource;

	private float spawnTime;

	public float Lifetime { get; private set; }

	public void InitializeTransform(Transform parent, Vector3 worldPosition, Vector3 worldDirection)
	{
		base.transform.SetParent(parent);
		base.transform.position = worldPosition;
		base.transform.rotation = Quaternion.LookRotation(worldDirection);
	}

	public void PlayEntry(Projectile projectile, Vector3 parentVelocity = default(Vector3), AudioInstigator instigator = null)
	{
		spawnTime = Time.time;
		Lifetime = Global.Context.Settings.Performance.particlesLifetime;
		foreach (Transform item in base.transform)
		{
			SingleEmissionEffect component = item.GetComponent<SingleEmissionEffect>();
			if (!(component == null))
			{
				component.EntryEmission(projectile, parentVelocity);
			}
		}
		PlayAudio(instigator, Mathf.Clamp01(projectile.Energy / 300f));
	}

	public void PlayExit(Projectile projectile, Vector3 parentVelocity, AudioInstigator instigator)
	{
		spawnTime = Time.time;
		Lifetime = Global.Context.Settings.Performance.particlesLifetime;
		foreach (Transform item in base.transform)
		{
			SingleEmissionEffect component = item.GetComponent<SingleEmissionEffect>();
			if (!(component == null))
			{
				component.ExitEmission(projectile, parentVelocity);
			}
		}
		PlayAudio(instigator, Mathf.Clamp01(projectile.Energy / 300f));
	}

	public void PlayBounce(int count, float velocity)
	{
		spawnTime = Time.time;
		Lifetime = Global.Context.Settings.Performance.particlesLifetime;
		foreach (Transform item in base.transform)
		{
			SingleEmissionEffect component = item.GetComponent<SingleEmissionEffect>();
			if (!(component == null))
			{
				component.BounceEmission(count, velocity);
			}
		}
		PlayAudio(null, 0.5f);
	}

	private void PlayAudio(AudioInstigator instigator, float volumeScale = 1f)
	{
		if (audioClips.Count != 0)
		{
			audioSource = GetComponent<AudioSource>();
			audioSource.priority = 40;
			audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Count)], volumeScale);
		}
	}

	private void Update()
	{
		if (audioSource != null && !audioSource.isPlaying)
		{
			Object.Destroy(audioSource);
		}
		if (Time.time - spawnTime >= Lifetime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}

}
