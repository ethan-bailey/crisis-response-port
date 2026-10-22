namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class TextScaler : MonoBehaviour
{
	public Vector3 minScale = Vector3.one * 0.75f;

	public Vector3 maxScale = Vector3.one;

	public float period = 0.5f;

	public float phase;

	public bool repeat = true;

	[Tooltip("Run only one cycle after this is enabled. Disables repeat.")]
	public bool once;

	private float time;

	private bool falling;

	private Text text;

	private void Awake()
	{
		text = GetComponent<Text>();
		time = period - phase;
	}

	private void Update()
	{
		if (falling)
		{
			time = Mathf.Clamp(time - Time.unscaledDeltaTime, 0f, period);
		}
		else
		{
			time = Mathf.Clamp(time + Time.unscaledDeltaTime, 0f, period);
		}
		if (time < 0f)
		{
			falling = !falling;
			if (once)
			{
				once = (repeat = false);
				base.transform.localScale = Vector3.one;
			}
		}
		else if (time > period)
		{
			falling = !falling;
		}
		float t = time / period;
		text.transform.localScale = Vector3.Lerp(minScale, maxScale, t);
	}

	public void FireOnce()
	{
		once = true;
		base.transform.localScale = Vector3.one;
		falling = false;
		time = 0f;
	}
}

}
