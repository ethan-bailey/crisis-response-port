namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UINonLinearSlider : MonoBehaviour
{
	[Serializable]
	public class FormatPrecision
	{
		public float threshold;

		public float multiplier = 1f;

		public int decimals;

		public string suffix;
	}

	[Header("Value range")]
	public float min;

	public float max = 5f;

	[Header("Remapping")]
	[Tooltip("Exponent used for remapping. Above 1: High resolution in low range. Below 1: High resolution in high range. 1: Linear (no) remap.")]
	public float exponent = 2f;

	public string prefix;

	[Tooltip("Text to display when the slider is disabled (deactive).")]
	public string disabledText;

	[Tooltip("List of thresholds for formatting levels. These are based on the values after the Display Multiplier is applied. Each level adds one decimal place.")]
	public List<FormatPrecision> precisionLevels = new List<FormatPrecision>();

	[Header("References")]
	public Slider slider;

	public Text label;

	public float TransformedValue
	{
		get
		{
			return min + Mathf.Pow(slider.value, exponent) * MaxAddition;
		}
	}

	private float MaxAddition
	{
		get
		{
			return max - min;
		}
	}

	private void Awake()
	{
		slider.onValueChanged.AddListener((float sliderValue) =>
		{
			UpdateLabel(sliderValue);
		});
	}

	private void OnEnable()
	{
		UpdateLabel(slider.value);
	}

	private void OnDisable()
	{
		label.text = disabledText;
	}

	public void SetCallback(UnityAction<float> onValueChanged)
	{
		slider.onValueChanged.AddListener(onValueChanged);
	}

	public void SetValue(float settingValue)
	{
		float num = InverseTransformValue(settingValue);
		slider.value = num;
		UpdateLabel(num);
	}

	public float TransformValue(float sliderValue)
	{
		return min + Mathf.Pow(sliderValue, exponent) * MaxAddition;
	}

	public float InverseTransformValue(float settingValue)
	{
		return Mathf.Pow(settingValue / MaxAddition - min, 1f / exponent);
	}

	public void UpdateLabel()
	{
		UpdateLabel(slider.value);
	}

	private void UpdateLabel(float sliderValue)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			label.text = disabledText;
			return;
		}
		string text = string.Empty;
		float num = TransformValue(sliderValue);
		foreach (FormatPrecision precisionLevel in precisionLevels)
		{
			if (!(num > precisionLevel.threshold))
			{
				string format = "{0}{1:F" + string.Format("{0}", precisionLevel.decimals) + "} {2}";
				text = string.Format(format, prefix, num * precisionLevel.multiplier, precisionLevel.suffix);
			}
		}
		label.text = text;
	}
}

}
