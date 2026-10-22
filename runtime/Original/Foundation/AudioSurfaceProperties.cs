using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine;

public class AudioSurfaceProperties : MonoBehaviour
{
	[Tooltip("Footstep samples for this surface type.")]
	public List<AudioClip> footstepSamples;

	[Tooltip("Grenade bounce samples for this surface type.")]
	public List<AudioClip> grenadeBounceSamples;

	public AudioClip RandomFootstep()
	{
		if (footstepSamples.Count == 0)
		{
			return null;
		}
		return footstepSamples[Random.Range(0, footstepSamples.Count)];
	}

	public AudioClip RandomGrenadeBounce()
	{
		if (grenadeBounceSamples.Count == 0)
		{
			return null;
		}
		return grenadeBounceSamples[Random.Range(0, grenadeBounceSamples.Count)];
	}
}
