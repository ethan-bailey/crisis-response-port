namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIPhysiologyIndicator : MonoBehaviour
{
	[Header("Circulation")]
	public Image circulationImage;

	[Header("Shock/trauma")]
	public Image shockImage;

	public float fadeInPeriod = 0.05f;

	public float fadeOutPeriod = 0.2f;

	private InteriorMode interiorMode;

	private InteriorCamera interiorCamera;

	private Material oxygenOverlayMaterial;

	private AudioSource heartBeatAudioSource;

	private bool activeState;

	private float flashBuffer;

	private float fadeInTime;

	private float fadeOutTime;

	private void Awake()
	{
		oxygenOverlayMaterial = circulationImage.GetComponent<Image>().material;
		heartBeatAudioSource = GetComponent<AudioSource>();
		heartBeatAudioSource.priority = 180;
	}

	private void Start()
	{
		interiorMode = Global.Context.Mission.InteriorMode;
		interiorCamera = interiorMode.interiorCamera;
	}

	private void Update()
	{
		CharacterBody subject = interiorCamera.Subject;
		bool flag = subject != null && !subject.IsRagdoll && subject.IsPlayer && !subject.Physiology.invulnerable;
		if (flag != activeState)
		{
			if (flag)
			{
				subject.Physiology.HeartBeatAudio = heartBeatAudioSource;
			}
			else
			{
				oxygenOverlayMaterial.SetFloat("_Opacity", 0f);
				shockImage.color = Color.clear;
			}
			activeState = flag;
		}
		if (flag)
		{
			UpdateCirculation(subject);
			UpdateShock(subject);
		}
	}

	private void UpdateCirculation(CharacterBody subject)
	{
		float bloodOxygen = subject.Physiology.BloodOxygen;
		float num = Mathf.Clamp(bloodOxygen - 0.5f, 0f, 0.3f) / 0.3f;
		float num2 = 1f - num;
		float num3 = Mathf.Clamp(0.2f * subject.Physiology.HeartBeatCycle * (0.85f - bloodOxygen), 0f, 1f);
		oxygenOverlayMaterial.SetFloat("_Direction", subject.Direction);
		oxygenOverlayMaterial.SetFloat("_Opacity", num2 + num3);
	}

	private void UpdateShock(CharacterBody subject)
	{
		if (subject == null || subject.Physiology.invulnerable)
		{
			return;
		}
		float num = 0f;
		if (flashBuffer > 0f)
		{
			float num2 = Mathf.Min(Time.deltaTime, flashBuffer);
			fadeInTime += num2;
			flashBuffer -= num2;
			num = Mathf.Lerp(0f, 1f, fadeInTime / fadeInPeriod);
		}
		else if (fadeOutTime > 0f)
		{
			float num3 = Mathf.Min(Time.deltaTime, fadeOutTime);
			fadeOutTime -= num3;
			num = Mathf.Lerp(0f, 1f, fadeOutTime / fadeOutPeriod);
			if (num == 0f)
			{
				fadeInTime = 0f;
			}
		}
		shockImage.color = new Color(1f, 1f, 1f, num);
	}

	public void Flash()
	{
		if (flashBuffer >= 1f)
		{
			flashBuffer = 1f;
			return;
		}
		flashBuffer += 0.1f;
		fadeOutTime = fadeOutPeriod;
	}
}

}
