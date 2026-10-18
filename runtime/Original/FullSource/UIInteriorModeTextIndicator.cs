namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;
using UnityEngine.UI;

public class UIInteriorModeTextIndicator : MonoBehaviour
{
	public Text text;

	public Image background;

	public AnimationCurve fadeIn;

	public AnimationCurve fadeOut;

	private bool active;

	private bool indefinite;

	private bool hide;

	private float timestamp;

	private float duration;

	private float textAlpha;

	private float backgroundAlpha;

	private float fadeInTime;

	private float fadeOutTime;

	private AudioPlayer audioPlayer;

	private void Awake()
	{
		textAlpha = text.color.a;
		backgroundAlpha = background.color.a;
		SetTextAlpha(0f);
		SetBackgroundAlpha(0f);
		fadeInTime = fadeIn.keys[fadeIn.length - 1].time;
		fadeOutTime = fadeOut.keys[fadeOut.length - 1].time;
		duration = Mathf.Max(duration, fadeInTime + fadeOutTime);
		audioPlayer = Global.Context.AudioPlayer;
	}

	public void Indicate(string statusText, float duration = 1f, bool audio = true)
	{
		timestamp = Time.realtimeSinceStartup;
		this.duration = duration;
		text.text = statusText;
		active = true;
		indefinite = false;
		hide = false;
		if (base.gameObject.activeInHierarchy && audio)
		{
			audioPlayer.PlayBlip();
		}
	}

	public void Indicate(string statusText, AudioClip clip, float duration = 1f)
	{
		timestamp = Time.realtimeSinceStartup;
		this.duration = duration;
		text.text = statusText;
		active = true;
		indefinite = false;
		hide = false;
		if (base.gameObject.activeInHierarchy && clip != null)
		{
			audioPlayer.Play(clip);
		}
	}

	public void Display(string statusText, bool audio = true)
	{
		timestamp = Time.realtimeSinceStartup;
		text.text = statusText;
		active = true;
		indefinite = true;
		hide = false;
		if (base.gameObject.activeInHierarchy && audio)
		{
			audioPlayer.PlayBlip();
		}
	}

	public void Hide()
	{
		timestamp = Time.realtimeSinceStartup;
		hide = true;
	}

	public void SetText(string statusText)
	{
		text.text = statusText;
	}

	private void Update()
	{
		if (active)
		{
			if (indefinite)
			{
				UpdateIndefinite();
			}
			else
			{
				UpdateLimited();
			}
		}
	}

	private void UpdateIndefinite()
	{
		float num = Time.realtimeSinceStartup - timestamp;
		float num2 = 1f;
		num2 = ((!hide) ? ((!(num < fadeInTime)) ? 1f : fadeIn.Evaluate(num / fadeInTime)) : ((!(num < fadeOutTime)) ? 0f : fadeOut.Evaluate(num / fadeOutTime)));
		SetTextAlpha(num2);
		SetBackgroundAlpha(num2);
		if (num2 <= 0f)
		{
			active = false;
		}
	}

	private void UpdateLimited()
	{
		float num = Time.realtimeSinceStartup - timestamp;
		float num2 = 1f;
		num2 = ((num < fadeInTime) ? fadeIn.Evaluate(num / fadeInTime) : ((!(num < duration - fadeOutTime)) ? fadeOut.Evaluate((num - duration + fadeOutTime) / fadeOutTime) : 1f));
		SetTextAlpha(num2);
		SetBackgroundAlpha(num2);
		if (num2 <= 0f)
		{
			active = false;
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
