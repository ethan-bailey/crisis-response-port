namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class TextFader : MonoBehaviour
{
	public float minAlpha;

	public float maxAlpha = 1f;

	public float period = 0.5f;

	public float offset;

	public bool repeat = true;

	private float time;

	private bool rising;

	private int cycles;

	private Text text;

	private void Start()
	{
		text = GetComponent<Text>();
		time = offset;
	}

	private void Update()
	{
		if (rising)
		{
			time = Mathf.Clamp(time + Time.unscaledDeltaTime, 0f, period);
		}
		else
		{
			time = Mathf.Clamp(time - Time.unscaledDeltaTime, 0f, period);
		}
		if (time >= period)
		{
			rising = !rising;
		}
		else if (time <= 0f)
		{
			rising = !rising;
			cycles++;
		}
		if (!repeat && cycles >= 2)
		{
			text.color = Color.clear;
			return;
		}
		float num = time / period;
		Color color = text.color;
		color.a = minAlpha + (maxAlpha - minAlpha) * num;
		text.color = color;
	}
}

}
