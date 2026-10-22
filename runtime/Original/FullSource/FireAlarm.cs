namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class FireAlarm : MonoBehaviour
{
	public Light lightSource;

	public AudioClip alarmAudio;

	[Tooltip("Interval between light flashes in standby. In seconds.")]
	public float standByFlashInterval = 15f;

	[Tooltip("Interval between light flashes when in alarm mode. In seconds.")]
	public float alarmFlashInterval = 0.25f;

	[Tooltip("For how long the light source is enabled when it flashes. In seconds.")]
	public float lightOnPeriod = 0.1f;

	[Tooltip("For how long the alarm should sound when triggered. In seconds.")]
	public float alarmPeriod = 15f;

	public bool alarm;

	[Tooltip("GameObjects in this list are activated when the alarm is destroyed.")]
	public List<GameObject> pieces = new List<GameObject>();

	private AudioSource audioSource;

	private BallisticMaterial material;

	private float flashTimestamp;

	private float triggerTimestamp;

	private bool lightEnabled;

	private bool disabledBySetting;

	private const float CheckInterval = 3f;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		material = GetComponent<BallisticMaterial>();
		lightSource.enabled = false;
		flashTimestamp = Time.time + standByFlashInterval * Random.value;
		material.onInteractionBegin.AddListener(OnBallisticHit);
	}

	private void Start()
	{
		SetDisabledBySetting(Global.Context.Settings.Extras.disableFireAlarms);
		Global.Context.Settings.Extras.onChanged.AddListener(OnExtrasChanged);
	}

	private void Update()
	{
		if (disabledBySetting)
		{
			return;
		}
		float num = flashTimestamp - Time.time;
		if (alarm && Time.time - triggerTimestamp >= alarmPeriod)
		{
			SetState(false);
		}
		if (num >= 0f && num < lightOnPeriod)
		{
			if (!lightEnabled)
			{
				lightSource.enabled = true;
				lightEnabled = true;
			}
		}
		else if (num < 0f && lightEnabled)
		{
			lightSource.enabled = false;
			lightEnabled = false;
			flashTimestamp = Time.time + ((!alarm) ? standByFlashInterval : alarmFlashInterval);
		}
	}

	private void OnExtrasChanged(SerializableExtraSettings newSettings)
	{
		SetDisabledBySetting(newSettings.disableFireAlarms);
	}

	public void SetDisabledBySetting(bool disable)
	{
		disabledBySetting = disable;
		if (disable)
		{
			SetState(false);
		}
	}

	public void ActivateIn(float seconds)
	{
		if (!disabledBySetting)
		{
			Coroutine.DelayedAction(seconds, () =>
			{
				SetState(true);
			}, this);
		}
	}

	private void SetState(bool enabled)
	{
		if (enabled)
		{
			triggerTimestamp = Time.time;
		}
		if (alarm != enabled)
		{
			alarm = enabled;
			flashTimestamp = Time.time + ((!alarm) ? standByFlashInterval : alarmFlashInterval);
			if (enabled)
			{
				audioSource.Play();
				return;
			}
			audioSource.Stop();
			bool flag = false;
			lightSource.enabled = flag;
			lightEnabled = flag;
		}
	}

	private void OnBallisticHit(Projectile projectile)
	{
		foreach (GameObject piece in pieces)
		{
			piece.transform.SetParent(null);
			piece.SetActive(true);
			Rigidbody component = piece.GetComponent<Rigidbody>();
			component.AddForce(Random.onUnitSphere * projectile.Energy * 0.001f, ForceMode.Impulse);
		}
		Object.Destroy(base.gameObject);
	}
}

}
