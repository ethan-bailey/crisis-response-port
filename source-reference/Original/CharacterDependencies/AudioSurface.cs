using UnityEngine;

public class AudioSurface : MonoBehaviour
{
	public AudioSurfaceProperties surfaceProperties;

	public AudioClip RandomFootstep()
	{
		return surfaceProperties.RandomFootstep();
	}

	public AudioClip RandomGrenadeBounce()
	{
		return surfaceProperties.RandomGrenadeBounce();
	}
}
