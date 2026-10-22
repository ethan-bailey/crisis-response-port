namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIMagazineIndicator : MonoBehaviour
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

	public void Indicate(Magazine magazine, bool chamberOpen, bool chamberEmpty)
	{
		string text = "Heavy magazine";
		if (magazine.Ratio == 0f)
		{
			text = "Empty magazine";
		}
		else if (magazine.Ratio < 0.25f)
		{
			text = "Light magazine";
		}
		else if (magazine.Ratio < 0.65f)
		{
			text = "Medium magazine";
		}
		if (chamberOpen)
		{
			text += " + Chamber open";
		}
		else if (chamberEmpty)
		{
			text += " + Chamber empty";
		}
		timestamp = Time.realtimeSinceStartup;
		this.text.text = text;
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
