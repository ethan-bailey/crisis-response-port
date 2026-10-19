namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIInteractionMenuItem : MonoBehaviour
{
	public Text actionText;

	public Text indexText;

	public string executionText;

	[Tooltip("Only set this for actions that represents actual actions.")]
	public bool executable;

	public bool disabled;

	[Tooltip("When enabled the action can't be invoked by the quick action key.")]
	public bool noQuickExecute;

	public UIInteractionMenu.InteractionCondition condition;

	public UIInteractionMenu.InteractionAction action;

	private int index;

	private bool available;

	private UIInteractionMenu menu;

	private Button button;

	public void Initialize(UIInteractionMenu menu, string text, string executionText, string indexText, UIInteractionMenu.InteractionAction action, UIInteractionMenu.InteractionCondition condition = null, bool executable = true, bool noQuickExecute = false)
	{
		actionText.text = text;
		this.indexText.text = indexText;
		this.executionText = executionText;
		this.menu = menu;
		this.condition = condition;
		this.action = action;
		this.executable = executable;
		this.noQuickExecute = noQuickExecute;
		button = GetComponent<Button>();
		if (action != null)
		{
			button.onClick.AddListener(ExecuteAction);
		}
	}

	public bool EvaluateAndSetAvailability()
	{
		available = condition == null || condition();
		base.gameObject.SetActive(available);
		button.interactable = !disabled;
		return available;
	}

	public bool EvaluateAvailability()
	{
		return condition == null || condition();
	}

	public void ExecuteAction()
	{
		if (action != null)
		{
			action();
			menu.ExecuteAction(this);
		}
	}

	private void Update()
	{
		EvaluateAndSetAvailability();
	}

	public void SetAvailable(bool available)
	{
		this.available = available;
	}

	public bool Available()
	{
		return available;
	}

	public void SetText(string text)
	{
		actionText.text = text;
	}

	public void SetIndex(int i)
	{
		index = i;
		indexText.text = string.Format("({0})", index);
	}
}

}
