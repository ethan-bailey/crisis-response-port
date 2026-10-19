namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHotkeysIndicator : MonoBehaviour
{
	public Text entryTeamText;

	public Text marksmanText;

	public Image background;

	public float fadeTime = 0.25f;

	private InputHandler input;

	private List<Text> textElements;

	private float textAlpha;

	private float backgroundAlpha;

	private float timestamp;

	private bool appearing;

	private void Awake()
	{
		input = Global.Context.InputHandler;
		entryTeamText.text = string.Format("{0}: entry team", input.Keys.entryTeam.KeyName);
		marksmanText.text = string.Format("{0}: Marksman", input.Keys.marksman.KeyName);
		textAlpha = entryTeamText.color.a;
		backgroundAlpha = background.color.a;
		textElements = new List<Text> { entryTeamText, marksmanText };
		SetTextAlpha(0f);
		SetBackgroundAlpha(0f);
	}

	private void Update()
	{
		if (appearing)
		{
			SetTextAlpha(Mathf.Clamp01((Time.time - timestamp) / fadeTime));
			SetBackgroundAlpha(Mathf.Clamp01((Time.time - timestamp) / fadeTime));
		}
	}

	private void SetTextAlpha(float alpha)
	{
		foreach (Text textElement in textElements)
		{
			textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.b, textAlpha * alpha);
		}
	}

	private void SetBackgroundAlpha(float alpha)
	{
		background.color = new Color(background.color.r, background.color.g, background.color.b, backgroundAlpha * alpha);
	}

	public void Show()
	{
		appearing = true;
		timestamp = Time.time;
	}

	public void Hide()
	{
		appearing = false;
		SetTextAlpha(0f);
		SetBackgroundAlpha(0f);
	}
}

}
