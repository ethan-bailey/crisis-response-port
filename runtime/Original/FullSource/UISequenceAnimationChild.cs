namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UISequenceAnimationChild : MonoBehaviour
{
	private RectTransform rect;

	private bool initialized;

	private bool animateTranslation;

	private bool animateScale;

	private float translationDelay;

	private float scalingDelay;

	public Vector3 StartPosition { get; private set; }

	public Vector3 Translation { get; private set; }

	public void Initialize(Vector3 translation, bool animateTranslation, float translationDelay, bool animateScale, float scalingDelay, float startScale = 1f)
	{
		rect = GetComponent<RectTransform>();
		this.animateTranslation = animateTranslation;
		this.animateScale = animateScale;
		Translation = translation;
		StartPosition = rect.anchoredPosition3D + Translation;
		rect.anchoredPosition3D = StartPosition;
		SetDelays(translationDelay, scalingDelay);
		if (animateScale)
		{
			rect.localScale = Vector3.one * startScale;
		}
		initialized = true;
	}

	public void SetDelays(float translationDelay, float scalingDelay)
	{
		this.translationDelay = translationDelay;
		this.scalingDelay = scalingDelay;
	}

	public void Animate(float translation, float scale)
	{
		if (initialized)
		{
			if (animateTranslation)
			{
				rect.anchoredPosition3D = StartPosition - Translation * translation;
			}
			if (animateScale)
			{
				rect.localScale = Vector3.one * scale;
			}
		}
	}

	public float TranslationPosition(float time, float period)
	{
		return Mathf.Clamp((time - translationDelay) / period, 0f, 1f);
	}

	public float ScalingPosition(float time, float period)
	{
		return Mathf.Max(time - scalingDelay, 0f) / period;
	}

	public void DisableButtonComponent()
	{
		Button component = GetComponent<Button>();
		if (!(component == null))
		{
			component.enabled = false;
		}
	}

	public void EndAnimation()
	{
		Button component = GetComponent<Button>();
		if (!(component == null))
		{
			component.enabled = true;
		}
	}
}

}
