namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIInteriorModeInteractionIndicator : MonoBehaviour
{
	public Text text;

	public Image background;

	public float fadeTime = 0.25f;

	private InputHandler inputHandler;

	private UIInteractionMenu interactionMenu;

	private float textAlpha;

	private float backgroundAlpha;

	private void Awake()
	{
		textAlpha = text.color.a;
		backgroundAlpha = background.color.a;
		SetTextAlpha(0f);
		SetBackgroundAlpha(0f);
	}

	private void Start()
	{
		inputHandler = Global.Context.InputHandler;
		interactionMenu = Global.Context.Mission.InteriorMode.UI.InteractionMenu;
	}

	private void Update()
	{
		if (!interactionMenu.Visible)
		{
			if (interactionMenu.QuickActionVisualizationDisabled)
			{
				SetBackgroundAlpha(0f);
				SetTextAlpha(0f);
			}
			else if (interactionMenu.CancelledAction != null)
			{
				text.color = Color.white;
				text.text = "Cancelled";
				SetBackgroundAlpha(0.75f);
				SetTextAlpha(0.3f);
			}
			else if (interactionMenu.ExecutingAction != null)
			{
				text.color = Color.white;
				text.text = interactionMenu.ExecutingAction.executionText;
				SetBackgroundAlpha(0.75f);
				SetTextAlpha(1f);
			}
			else if (interactionMenu.AvailableAction != null)
			{
				text.color = Color.white;
				text.text = string.Format("{0}: {1}", inputHandler.Keys.interact.KeyName, interactionMenu.AvailableAction.actionText.text);
				SetBackgroundAlpha(0.75f);
				SetTextAlpha(0.7f);
			}
			else
			{
				SetBackgroundAlpha(0f);
				SetTextAlpha(0f);
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
