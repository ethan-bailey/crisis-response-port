namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIFireModeIndicator : MonoBehaviour
{
	public Text text;

	public Image background;

	public AnimationCurve fade;

	private bool active;

	private float timestamp;

	private float textAlpha;

	private float backgroundAlpha;

	private void Awake()
	{
		textAlpha = text.color.a;
		backgroundAlpha = background.color.a;
		SetTextAlpha(0f);
		SetBackgroundAlpha(0f);
	}

	public void Indicate(WeaponBase.FireMode mode)
	{
		timestamp = Time.realtimeSinceStartup;
		text.text = mode.ToString();
		active = true;
	}

	private void Update()
	{
		if (active)
		{
			float num = fade.Evaluate(Time.realtimeSinceStartup - timestamp);
			SetTextAlpha(num);
			SetBackgroundAlpha(num);
			if (num <= 0f)
			{
				active = false;
			}
		}
	}

	private void SetTextAlpha(float alpha)
	{
		text.color = new Color(text.color.r, text.color.g, text.color.b, textAlpha * alpha);
	}

	private void SetBackgroundAlpha(float alpha)
	{
		background.color = new Color(background.color.r, background.color.g, background.color.b, backgroundAlpha * alpha);
	}
}

}
