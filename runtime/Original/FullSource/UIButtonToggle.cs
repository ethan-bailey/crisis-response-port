namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIButtonToggle : MonoBehaviour
{
	public string enableText = "Enable";

	public string disableText = "Disable";

	public string enabledText = "Enabled";

	public string disabledText = "Disabled";

	public string enableTooltip = "Click to enable this setting.";

	public string disableTooltip = "Click to disable this setting.";

	public string enabledTooltip = "This setting is enabled.";

	public string disabledTooltip = "This setting is disabled.";

	public UnityEventBool onValueChanged = new UnityEventBool();

	[Header("References")]
	public Button enableButton;

	public Button disableButton;

	public Text enableButtonText;

	public Text disbleButtonText;

	public UITooltip enableButtonTooltip;

	public UITooltip disableButtonTooltip;

	private bool isEnabled;

	public bool isOn
	{
		get
		{
			return isEnabled;
		}
		set
		{
			if (value)
			{
				OnEnabled();
			}
			else
			{
				OnDisabled();
			}
		}
	}

	private void Awake()
	{
		enableButton.onClick.RemoveAllListeners();
		enableButton.onClick.AddListener(OnEnabled);
		disableButton.onClick.RemoveAllListeners();
		disableButton.onClick.AddListener(OnDisabled);
	}

	private void Start()
	{
		enableButtonText.text = ((!isOn) ? enableText : enabledText);
		disbleButtonText.text = ((!isOn) ? disabledText : disableText);
		enableButtonTooltip.text = ((!isOn) ? enableTooltip : enabledTooltip);
		disableButtonTooltip.text = ((!isOn) ? disabledTooltip : disableTooltip);
	}

	private void OnEnabled()
	{
		enableButton.interactable = false;
		disableButton.interactable = true;
		enableButtonText.text = enabledText;
		disbleButtonText.text = disableText;
		disableButtonTooltip.Set(disableTooltip);
		enableButtonTooltip.Set(enabledTooltip);
		isEnabled = true;
		onValueChanged.Invoke(true);
	}

	private void OnDisabled()
	{
		enableButton.interactable = true;
		disableButton.interactable = false;
		enableButtonText.text = enableText;
		disbleButtonText.text = disabledText;
		enableButtonTooltip.Set(enableTooltip);
		disableButtonTooltip.Set(disabledTooltip);
		isEnabled = false;
		onValueChanged.Invoke(false);
	}
}

}
