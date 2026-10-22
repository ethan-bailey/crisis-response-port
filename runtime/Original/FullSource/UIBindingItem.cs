namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIBindingItem : MonoBehaviour
{
	[Header("References")]
	public Text buttonText;

	public Text descriptionText;

	public Button button;

	public InputKey inputKey;

	private InputHandler handler;

	private static Color DoubleBindColor = new Color(1f, 0.5f, 0f);

	public void Initialize(InputHandler handler, InputKey key)
	{
		this.handler = handler;
		inputKey = key;
		buttonText.text = StringHelper.FriendlyName(key.KeyName);
		descriptionText.text = key.description;
		button.onClick.AddListener(InvokeStartBind);
		SetButtonColor();
	}

	public void SetKeyText(string text)
	{
		buttonText.text = StringHelper.FriendlyName(text);
	}

	public void InvokeStartBind()
	{
		handler.StartBinding(this);
		buttonText.text = "Binding...";
		buttonText.color = Color.green;
	}

	public void CancelBind()
	{
		buttonText.text = StringHelper.FriendlyName(inputKey.KeyName);
		buttonText.color = Color.black;
	}

	public void UpdateTexts()
	{
		buttonText.text = StringHelper.FriendlyName(inputKey.KeyName);
		buttonText.color = Color.black;
		SetButtonColor();
	}

	private void SetButtonColor()
	{
		Image component = button.GetComponent<Image>();
		if (inputKey.key == KeyCode.None && inputKey.axis == null)
		{
			component.color = Color.red;
		}
		else if (handler.FindKeyCodeBindings(inputKey.key).Count > 1)
		{
			component.color = DoubleBindColor;
		}
		else if (handler.FindAxisBindings(inputKey.axis, inputKey.axisDirection).Count > 1)
		{
			component.color = DoubleBindColor;
		}
		else
		{
			component.color = Color.white;
		}
	}
}

}
