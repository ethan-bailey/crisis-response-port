namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIBindingConflictDialog : MonoBehaviour
{
	public Text existingKeyText;

	public Text existingActionText;

	public Text pendingActionText;

	public Button replaceButton;

	public Button useBothButton;

	public Button cancelButton;

	private InputHandler inputHandler;

	private UIBindingsTab bindingsDialog;

	private InputKey existingBind;

	private InputKey pendingBind;

	public bool Visible
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	private void Start()
	{
		replaceButton.onClick.AddListener(Replace);
		useBothButton.onClick.AddListener(UseBoth);
		cancelButton.onClick.AddListener(Cancel);
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Cancel();
		}
	}

	public void Initialize(UIBindingsTab bindingsTab)
	{
		bindingsDialog = bindingsTab;
		inputHandler = Global.Context.InputHandler;
	}

	public void Show(InputKey existing, InputKey pending)
	{
		existingBind = existing;
		pendingBind = pending;
		existingKeyText.text = existing.KeyName;
		existingActionText.text = existing.name;
		pendingActionText.text = pending.name;
		inputHandler.StopActiveBind();
		base.gameObject.SetActive(true);
	}

	public void Replace()
	{
		UIBindingItem uIBindingItem = bindingsDialog.FindBindingUIItemByInputName(pendingBind.name);
		UIBindingItem uIBindingItem2 = bindingsDialog.FindBindingUIItemByInputName(existingBind.name);
		pendingBind.key = existingBind.key;
		pendingBind.axis = existingBind.axis;
		pendingBind.axisDirection = existingBind.axisDirection;
		existingBind.key = KeyCode.None;
		existingBind.axis = null;
		existingBind.axisDirection = 0f;
		uIBindingItem.UpdateTexts();
		uIBindingItem2.UpdateTexts();
		base.gameObject.SetActive(false);
	}

	private void UseBoth()
	{
		UIBindingItem uIBindingItem = bindingsDialog.FindBindingUIItemByInputName(pendingBind.name);
		UIBindingItem uIBindingItem2 = bindingsDialog.FindBindingUIItemByInputName(existingBind.name);
		pendingBind.key = existingBind.key;
		pendingBind.axis = existingBind.axis;
		pendingBind.axisDirection = existingBind.axisDirection;
		uIBindingItem.UpdateTexts();
		uIBindingItem2.UpdateTexts();
		base.gameObject.SetActive(false);
	}

	private void Cancel()
	{
		base.gameObject.SetActive(false);
	}

	public void Hide()
	{
		Cancel();
	}
}

}
