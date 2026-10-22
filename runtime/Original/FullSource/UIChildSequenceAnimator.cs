namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class UIChildSequenceAnimator : MonoBehaviour
{
	public bool reverseChildStartOrder;

	public bool reverseChildExitOrder;

	public List<UISequenceAnimationChild> children = new List<UISequenceAnimationChild>();

	[Header("Translation")]
	public bool animateTranslation = true;

	public Vector3 movement;

	public float translationDelay = 0.5f;

	public float translationSpacing = 0.1f;

	public float translationPeriod = 0.25f;

	public AnimationCurve translation;

	public AnimationCurve exitTranslation;

	[Header("Scaling")]
	public bool animateScale = true;

	public float scalingDelay = 1.5f;

	public float scalingSpacing = 0.1f;

	public float scalingPeriod = 0.25f;

	public AnimationCurve scaling;

	public AnimationCurve exitScaling;

	private float timestamp;

	private bool appearing;

	private bool disappearing;

	private void Awake()
	{
		InitializePositions();
	}

	private void InitializePositions()
	{
		int num = 0;
		float startScale = ((!animateScale || scaling.keys.Length <= 0) ? 1f : scaling.keys[0].value);
		foreach (UISequenceAnimationChild child in children)
		{
			float num2 = ((!reverseChildStartOrder) ? ((float)num) : ((float)(base.transform.childCount - num)));
			child.Initialize(movement, animateTranslation, num2 * translationSpacing, animateScale, num2 * scalingSpacing, startScale);
			child.DisableButtonComponent();
			num++;
		}
	}

	public void PlayAppear()
	{
		appearing = true;
		disappearing = false;
		timestamp = Time.realtimeSinceStartup;
	}

	private void UpdateAppearAnimation()
	{
		float num = Time.realtimeSinceStartup - timestamp - translationDelay;
		float time = Time.realtimeSinceStartup - timestamp - scalingDelay;
		if (num <= 0f)
		{
			return;
		}
		foreach (UISequenceAnimationChild child in children)
		{
			float num2 = child.TranslationPosition(num, translationPeriod);
			float num3 = translation.Evaluate(num2);
			float scale = scaling.Evaluate(child.ScalingPosition(time, scalingPeriod));
			child.Animate(num3, scale);
			if (num2 >= 1f)
			{
				child.EndAnimation();
			}
		}
	}

	public void PlayDisappear()
	{
		appearing = false;
		disappearing = true;
		timestamp = Time.realtimeSinceStartup;
		int num = 0;
		foreach (UISequenceAnimationChild child in children)
		{
			float num2 = ((!reverseChildExitOrder) ? ((float)num) : ((float)(base.transform.childCount - num)));
			child.SetDelays(num2 * translationSpacing, num2 * scalingSpacing);
			child.DisableButtonComponent();
			num++;
		}
	}

	private void UpdateDisappearAnimation()
	{
		float time = Time.realtimeSinceStartup - timestamp;
		float time2 = Time.realtimeSinceStartup - timestamp;
		foreach (UISequenceAnimationChild child in children)
		{
			float num = child.TranslationPosition(time, translationPeriod);
			float num2 = exitTranslation.Evaluate(num);
			float scale = exitScaling.Evaluate(child.ScalingPosition(time2, scalingPeriod));
			child.Animate(num2, scale);
			if (num >= 1f)
			{
				child.EndAnimation();
			}
		}
	}

	private void Update()
	{
		if (appearing)
		{
			UpdateAppearAnimation();
		}
		else if (disappearing)
		{
			UpdateDisappearAnimation();
		}
	}
}

}
