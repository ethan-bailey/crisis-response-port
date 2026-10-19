namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IEventSystemHandler
{
	[Tooltip("The button transform is scaled to these values when highlighted.")]
	public Vector2 highlightScale = new Vector2(1.1f, 1f);

	[Tooltip("Curve used to interpolate from scale of one to the highlight scale. X: normalized position of the interpolation period. Y: Normalized scale.")]
	public AnimationCurve interpolation;

	[Tooltip("Period of time the interpolation will take to complete.")]
	public float interpolationPeriod = 0.2f;

	public bool disablePointerEvents;

	private float timestamp;

	private bool animating;

	private bool highlighted;

	private void OnDisable()
	{
		animating = false;
		highlighted = false;
		base.transform.localScale = Vector3.one;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!disablePointerEvents)
		{
			animating = (highlighted = true);
			timestamp = Time.realtimeSinceStartup;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!disablePointerEvents)
		{
			highlighted = false;
			timestamp = Time.realtimeSinceStartup;
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
	}

	private void Update()
	{
		if (animating && !disablePointerEvents)
		{
			float num = (Time.realtimeSinceStartup - timestamp) / interpolationPeriod;
			float num2 = interpolation.Evaluate((!highlighted) ? (interpolationPeriod - num) : num);
			base.transform.localScale = new Vector3(1f + (highlightScale.x - 1f) * num2, 1f + (highlightScale.y - 1f) * num2, 1f);
			if (!highlighted && num >= 1f)
			{
				animating = false;
			}
		}
	}
}

}
