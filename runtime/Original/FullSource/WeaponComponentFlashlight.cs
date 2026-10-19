namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class WeaponComponentFlashlight : MonoBehaviour
{
	public Light lightSource;

	private AudioSource audioSource;

	private bool active;

	private bool suspended;

	public bool IsOn
	{
		get
		{
			return lightSource.enabled;
		}
	}

	public bool IsActive
	{
		get
		{
			return active;
		}
	}

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public void Toggle()
	{
		if (!suspended)
		{
			active = !active;
			lightSource.enabled = active;
			audioSource.Play();
		}
	}

	public void SetActive(bool active)
	{
		this.active = active;
	}

	public void Suspend(bool suspend)
	{
		suspended = suspend;
		bool flag = !suspend && active;
		if (flag != IsOn)
		{
			audioSource.Play();
		}
		lightSource.enabled = flag;
	}

	public void SetState(bool enable = true, bool audio = true)
	{
		if (enable != active)
		{
			active = enable;
			lightSource.enabled = active;
			if (audio)
			{
				audioSource.Play();
			}
		}
	}
}

}
