namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class UITitleTextAnimator : MonoBehaviour
{
	[Header("Translation")]
	public Vector3 movement;

	public float translationDelay = 0.5f;

	public float translationSpacing = 0.1f;

	public float translationPeriod = 0.25f;

	public AnimationCurve translation;

	public AnimationCurve exitTranslation;

	[Header("Scaling")]
	public float scalingDelay = 1.5f;

	public float scalingSpacing = 0.1f;

	public float scalingPeriod = 0.25f;

	public AnimationCurve scaling;

	[Header("References")]
	public List<UITitleLetter> letters;

	[Header("Quit")]
	[Tooltip("Time to wait after quit has been clicked before quitting. In seconds.")]
	public float quitTime = 1f;

	private bool starting;

	private bool exiting;

	private float timestamp;

	private void Awake()
	{
		InitializePositions();
	}

	private void Update()
	{
		if (starting)
		{
			UpdateStartAnimation();
		}
		else if (exiting)
		{
			UpdateQuitAnimation();
		}
	}

	private void InitializePositions()
	{
		int num = 0;
		foreach (UITitleLetter letter in letters)
		{
			letter.Initialize(movement, (float)num * translationSpacing, (float)num * scalingSpacing);
			if (num == 5)
			{
				num++;
			}
			num++;
		}
	}

	public void PlayStartAnimation()
	{
		starting = true;
		exiting = false;
		timestamp = Time.realtimeSinceStartup;
	}

	private void UpdateStartAnimation()
	{
		float num = Time.realtimeSinceStartup - timestamp - translationDelay;
		float time = Time.realtimeSinceStartup - timestamp - scalingDelay;
		if (num <= 0f)
		{
			return;
		}
		foreach (UITitleLetter letter in letters)
		{
			float num2 = translation.Evaluate(letter.TranslationPosition(num, translationPeriod));
			float scale = scaling.Evaluate(letter.ScalingPosition(time, scalingPeriod));
			letter.Animate(num2, scale);
		}
	}

	private void UpdateQuitAnimation()
	{
		float time = Time.realtimeSinceStartup - timestamp;
		foreach (UITitleLetter letter in letters)
		{
			float num = exitTranslation.Evaluate(letter.TranslationPosition(time, translationPeriod));
			letter.Animate(num, 1f);
		}
	}

	public void PlayQuitAnimation()
	{
		exiting = true;
		starting = false;
		timestamp = Time.realtimeSinceStartup;
	}
}

}
