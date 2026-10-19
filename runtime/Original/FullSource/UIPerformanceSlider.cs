namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIPerformanceSlider : MonoBehaviour
{
	public Text labelText;

	public Slider slider;

	public Button toggleLimitButton;

	public GameObject toggleLimitCheckmark;

	public UITooltip toggleLimitTooltip;

	public Button resetButton;

	public Text resetButtonText;

	public Text valueText;

	public GameObject warningIcon;

	public Color limitDisabledColor;

	public UnityEventFloat onValueChanged = new UnityEventFloat();

	public UnityEventVoid onReset = new UnityEventVoid();

	private Color limitEnabledColor;

	private Color limitEnabledHoverColor;

	private float internalValue;

	private bool initialized;

	private bool enableSliderEvents = true;

	public bool Unlimited { get; private set; }

	public float value
	{
		get
		{
			return internalValue;
		}
		set
		{
			OnValueChanged(value, false);
		}
	}

	public int intValue
	{
		get
		{
			return Mathf.RoundToInt(internalValue);
		}
		set
		{
			this.value = value;
		}
	}

	public float minValue
	{
		get
		{
			return slider.minValue;
		}
	}

	public float maxValue
	{
		get
		{
			return slider.maxValue;
		}
	}

	private void Start()
	{
		initialized = true;
		limitEnabledColor = toggleLimitButton.colors.normalColor;
		limitEnabledHoverColor = toggleLimitButton.colors.highlightedColor;
		SetVisualState(false);
		toggleLimitButton.onClick.AddListener(OnLimitButtonPressed);
		slider.onValueChanged.AddListener(OnSliderValueChanged);
		resetButton.onClick.AddListener(OnResetButtonClicked);
	}

	private void OnLimitButtonPressed()
	{
		Unlimited = !Unlimited;
		value = ((!Unlimited) ? slider.maxValue : (-1f));
		OnValueChanged(value, false);
	}

	private void OnValueChanged(float newValue, bool bySliderMove)
	{
		internalValue = newValue;
		Unlimited = !bySliderMove && internalValue < 0f;
		if (initialized)
		{
			SetVisualState(bySliderMove);
			onValueChanged.Invoke(internalValue);
		}
	}

	private void SetVisualState(bool bySliderMove)
	{
		if (!bySliderMove)
		{
			enableSliderEvents = false;
			slider.value = ((!Unlimited) ? value : slider.maxValue);
			enableSliderEvents = true;
		}
		bool flag = value == slider.minValue;
		bool flag2 = Unlimited || value == slider.maxValue;
		Text text = labelText;
		Color color = new Color(0f, 0f, 0f, (!flag) ? 1f : 0.6f);
		valueText.color = color;
		text.color = color;
		toggleLimitButton.gameObject.SetActive(flag2);
		toggleLimitCheckmark.SetActive(Unlimited);
		toggleLimitTooltip.Set((!Unlimited) ? "Click to disable the limit of this setting." : "Click to re-enable the limit of this setting.");
		resetButtonText.text = ((!flag2) ? "Reset" : "R");
		valueText.text = ((!flag) ? string.Format("{0}", value) : "Disabled");
		valueText.gameObject.SetActive(!Unlimited);
		warningIcon.SetActive(Unlimited);
		SetColors();
	}

	private void SetColors()
	{
		ColorBlock colors = toggleLimitButton.colors;
		colors.normalColor = ((!Unlimited) ? limitEnabledColor : limitDisabledColor);
		colors.highlightedColor = ((!Unlimited) ? limitEnabledHoverColor : limitDisabledColor);
		toggleLimitButton.colors = colors;
	}

	private void OnSliderValueChanged(float value)
	{
		if (enableSliderEvents)
		{
			OnValueChanged(value, true);
		}
	}

	private void OnResetButtonClicked()
	{
		onReset.Invoke();
	}
}

}
