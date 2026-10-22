namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIBulletTimeEntry : MonoBehaviour
{
	public Toggle entryEnabled;

	public Text label;

	public UINonLinearSlider rate;

	public UINonLinearSlider easeIn;

	public UINonLinearSlider duration;

	public UINonLinearSlider easeOut;

	public GameObject slidersGroup;

	public bool Enabled
	{
		get
		{
			return entryEnabled.isOn;
		}
	}

	public float Rate
	{
		get
		{
			return rate.TransformedValue;
		}
	}

	public float EaseIn
	{
		get
		{
			return easeIn.TransformedValue;
		}
	}

	public float Duration
	{
		get
		{
			return duration.TransformedValue;
		}
	}

	public float EaseOut
	{
		get
		{
			return easeOut.TransformedValue;
		}
	}

	private void Awake()
	{
		entryEnabled.onValueChanged.AddListener((bool value) =>
		{
			OnEntryEnabled(value);
		});
		rate.UpdateLabel();
	}

	public void Deserialize(SerializableBulletTimeEntry data)
	{
		if (data != null)
		{
			entryEnabled.isOn = data.enabled;
			rate.SetValue(data.rate);
			easeIn.SetValue(data.easeIn);
			duration.SetValue(data.duration);
			easeOut.SetValue(data.easeOut);
			slidersGroup.SetActive(data.enabled);
		}
	}

	public SerializableBulletTimeEntry Serialize()
	{
		SerializableBulletTimeEntry serializableBulletTimeEntry = new SerializableBulletTimeEntry();
		serializableBulletTimeEntry.enabled = entryEnabled.isOn;
		serializableBulletTimeEntry.rate = rate.TransformValue(rate.slider.value);
		serializableBulletTimeEntry.easeIn = easeIn.TransformValue(easeIn.slider.value);
		serializableBulletTimeEntry.duration = duration.TransformValue(duration.slider.value);
		serializableBulletTimeEntry.easeOut = easeOut.TransformValue(easeOut.slider.value);
		return serializableBulletTimeEntry;
	}

	private void OnEntryEnabled(bool value)
	{
		slidersGroup.SetActive(value);
	}
}

}
