namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGraphicsSettings : MonoBehaviour
{
	private const string DropdownItemTextWhenResolutionUnavailable = "Select a new resolution";

	[Header("References")]
	[Tooltip("This needs to be a canvas that covers the entire window. It is used to detect the resolution in windowed mode.")]
	public Canvas canvas;

	public GameObject nonRuntimeLayout;

	public GameObject runtimeNotice;

	public Button applySettingsButton;

	public Button resetAmbientLightButton;

	[Header("Quality")]
	public UIButtonToggle windowedMode;

	public Dropdown resolutionDropdown;

	public Slider ambientLightSlider;

	[Header("Camera")]
	public Dropdown fieldOfViewDropdown;

	public UIRangeSlider dynamicFieldOfView;

	public Slider staticFieldOfView;

	public UIButtonToggle vsyncToggle;

	public UIButtonToggle frameRateToggle;

	public UIButtonToggle censorToggle;

	private Settings settings;

	private SerializableGraphicsSettings defaults;

	private SerializableGraphicsSettings configuration;

	private bool initialized;

	private void OnEnable()
	{
		GlobalContext context = Global.Context;
		settings = context.Settings;
		defaults = new SerializableGraphicsSettings();
		Deserialize(settings.Graphics);
		nonRuntimeLayout.SetActive(context.Mission == null);
		runtimeNotice.SetActive(context.Mission != null);
		initialized = true;
	}

	private void OnDisable()
	{
		Save();
	}

	private void Start()
	{
		settings = Global.Object.GetComponent<Settings>();
		configuration = settings.Graphics;
		applySettingsButton.onClick.AddListener(ApplySettings);
		frameRateToggle.onValueChanged.AddListener(OnShowFrameRateChanged);
		resolutionDropdown.onValueChanged.AddListener(SetResolution);
		vsyncToggle.onValueChanged.AddListener(SetVsync);
		windowedMode.onValueChanged.AddListener(SetWindowedMode);
		resetAmbientLightButton.onClick.AddListener(() =>
		{
			ambientLightSlider.value = defaults.ambientLightScale;
		});
		fieldOfViewDropdown.onValueChanged.AddListener((int index) =>
		{
			OnUseDynamicFieldOfViewChanged(index == 0);
		});
		dynamicFieldOfView.onValueChanged.AddListener((float min, float max) =>
		{
			configuration.onDynamicFieldOfViewChanged.Invoke(min, max);
		});
		staticFieldOfView.onValueChanged.AddListener((float angle) =>
		{
			configuration.onStaticFieldOfViewChanged.Invoke(angle);
		});
		ambientLightSlider.onValueChanged.AddListener((float scale) =>
		{
			configuration.onAmbientLightChanged.Invoke(scale);
		});
		censorToggle.onValueChanged.AddListener((bool enable) =>
		{
			configuration.onCensoringChanged.Invoke(enable);
		});
		OnUseDynamicFieldOfViewChanged(configuration.useDynamicFieldOfView);
		InitializeResolutionDropdown();
		InitializeWindowedButton();
		applySettingsButton.interactable = false;
	}

	public void Save()
	{
		if (initialized)
		{
			SerializableGraphicsSettings serializableGraphicsSettings = Serialize();
			if (serializableGraphicsSettings.Different(settings.Graphics))
			{
				settings.Graphics = serializableGraphicsSettings;
			}
		}
	}

	private SerializableGraphicsSettings Serialize()
	{
		SerializableGraphicsSettings serializableGraphicsSettings = new SerializableGraphicsSettings();
		serializableGraphicsSettings.vsync = vsyncToggle.isOn;
		serializableGraphicsSettings.ambientLightScale = ambientLightSlider.value;
		serializableGraphicsSettings.showFrameRate = frameRateToggle.isOn;
		serializableGraphicsSettings.applyCensoring = censorToggle.isOn;
		serializableGraphicsSettings.useDynamicFieldOfView = fieldOfViewDropdown.value == 0;
		serializableGraphicsSettings.dynamicFieldOfViewMin = dynamicFieldOfView.LowValue;
		serializableGraphicsSettings.dynamicFieldOfViewMax = dynamicFieldOfView.HighValue;
		serializableGraphicsSettings.staticFieldOfView = staticFieldOfView.value;
		return serializableGraphicsSettings;
	}

	private void Deserialize(SerializableGraphicsSettings data)
	{
		vsyncToggle.isOn = data.vsync;
		ambientLightSlider.value = data.ambientLightScale;
		frameRateToggle.isOn = data.showFrameRate;
		censorToggle.isOn = data.applyCensoring;
		fieldOfViewDropdown.value = ((!data.useDynamicFieldOfView) ? 1 : 0);
		dynamicFieldOfView.SetValues(data.dynamicFieldOfViewMin, data.dynamicFieldOfViewMax);
		staticFieldOfView.value = data.staticFieldOfView;
	}

	private void ApplySettings()
	{
		Save();
		bool flag = windowedMode.isOn != !Screen.fullScreen;
		bool flag2 = resolutionDropdown.value < Screen.resolutions.Length;
		bool flag3 = (vsyncToggle.isOn ? 1 : 0) != QualitySettings.vSyncCount;
		if (flag)
		{
			Debug.LogFormat("Windowed mode will change to: {0}", (!windowedMode.isOn) ? "Off" : "On");
		}
		Resolution resolution = ((!flag2) ? Screen.currentResolution : Screen.resolutions[resolutionDropdown.value]);
		configuration.Apply(resolution, !windowedMode.isOn);
		if (flag)
		{
			InitializeResolutionDropdown();
		}
		if (flag3)
		{
			configuration.ApplyVsyncCount();
		}
		if (flag || flag2)
		{
			configuration.onChangedResolution.Invoke(resolution.width, resolution.height);
		}
		applySettingsButton.interactable = false;
	}

	private void InitializeResolutionDropdown()
	{
		Debug.Log("Supported fullscreen resolutions:");
		resolutionDropdown.ClearOptions();
		List<string> list = new List<string>();
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			Resolution resolution = Screen.resolutions[i];
			string text = resolution.ToString();
			Debug.LogFormat(" {0}", text);
			list.Add(text);
		}
		resolutionDropdown.AddOptions(list);
		if (!Screen.fullScreen)
		{
			Debug.Log("Currently in windowed mode; can't detect fullscreen resolution.");
			resolutionDropdown.AddOptions(new List<string> { "Select a new resolution" });
			resolutionDropdown.value = resolutionDropdown.options.Count - 1;
			resolutionDropdown.options.RemoveAt(resolutionDropdown.options.Count - 1);
			resolutionDropdown.captionText.text = "Select a new resolution";
			return;
		}
		string text2 = Screen.currentResolution.ToString();
		Debug.LogFormat("Current resolution is: {0}", text2);
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] == text2)
			{
				resolutionDropdown.value = j;
				break;
			}
		}
	}

	private void InitializeWindowedButton()
	{
		windowedMode.isOn = !Screen.fullScreen;
	}

	private void OnUseDynamicFieldOfViewChanged(bool enable)
	{
		configuration.useDynamicFieldOfView = enable;
		dynamicFieldOfView.gameObject.SetActive(enable);
		staticFieldOfView.gameObject.SetActive(!enable);
		configuration.onUseDynamicFovChanged.Invoke(enable);
	}

	private void SetResolution(int value)
	{
		applySettingsButton.interactable = true;
	}

	private void SetWindowedMode(bool value)
	{
		applySettingsButton.interactable = true;
	}

	private void SetVsync(bool value)
	{
		applySettingsButton.interactable = true;
	}

	private void OnShowFrameRateChanged(bool value)
	{
		Global.Context.frameRateDisplay.gameObject.SetActive(value);
	}
}

}
