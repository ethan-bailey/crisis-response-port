namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.EventSystems;

public class UITitleLetter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	private RectTransform rect;

	private UITitleSpriteFlasher flasher;

	private float translationDelay;

	private float scalingDelay;

	private bool flashed;

	public Vector3 StartPosition { get; private set; }

	public Vector3 Translation { get; private set; }

	public void Initialize(Vector3 translation, float translationDelay, float scalingDelay)
	{
		rect = GetComponent<RectTransform>();
		flasher = GetComponent<UITitleSpriteFlasher>();
		Translation = translation;
		StartPosition = rect.anchoredPosition3D + Translation;
		rect.anchoredPosition3D = StartPosition;
		this.translationDelay = translationDelay;
		this.scalingDelay = scalingDelay;
	}

	public void Animate(float translation, float scale)
	{
		rect.anchoredPosition3D = StartPosition - Translation * translation;
		rect.localScale = Vector3.one * scale;
		if (!flashed && scale >= 1.1f)
		{
			flashed = true;
			flasher.Flash();
		}
	}

	public float TranslationPosition(float time, float period)
	{
		return Mathf.Max(time - translationDelay, 0f) / period;
	}

	public float ScalingPosition(float time, float period)
	{
		return Mathf.Max(time - scalingDelay, 0f) / period;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}
}

}
