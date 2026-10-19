namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGeneralSettings : MonoBehaviour
{
	public Dropdown grenadeTimerDropdown;

	public Dropdown pistolFireModeDropdown;

	public Dropdown smgFireModeDropdown;

	public Dropdown shotgunFireModeDropdown;

	public Dropdown rifleFireModeDropdown;

	public UIButtonToggle flashLightDefaultMode;

	public UIButtonToggle laserDefaultMode;

	public UIButtonToggle interactionMarkers;

	public UIButtonToggle automaticBolt;

	public UIButtonToggle showMarksmanTargetIndication;

	public UIButtonToggle enableMarksmanBobbing;

	public Slider interiorAimSensitivity;

	public Slider marksmanAimSensitivity;

	public Slider marksmanReloadSensitivity;

	private Settings settings;

	private SerializableGeneralSettings unmodified;

	private SerializableGeneralSettings configuration;

	private bool initialized;

	private void OnEnable()
	{
		InitializeFireModeDropdowns();
		settings = Global.Context.Settings;
		unmodified = settings.General.Copy();
		Deserialize(unmodified);
		initialized = true;
	}

	private void OnDisable()
	{
		Save();
	}

	private void Save()
	{
		if (initialized)
		{
			SerializableGeneralSettings serializableGeneralSettings = Serialize();
			if (serializableGeneralSettings.Different(unmodified))
			{
				settings.General = serializableGeneralSettings;
			}
		}
	}

	private void Start()
	{
		settings = Global.Object.GetComponent<Settings>();
		configuration = settings.General;
		pistolFireModeDropdown.onValueChanged.AddListener((int value) =>
		{
			configuration.pistolFireMode = (WeaponBase.FireMode)value;
		});
		smgFireModeDropdown.onValueChanged.AddListener((int value) =>
		{
			configuration.smgFireMode = (WeaponBase.FireMode)value;
		});
		shotgunFireModeDropdown.onValueChanged.AddListener((int value) =>
		{
			configuration.shotgunFireMode = (WeaponBase.FireMode)value;
		});
		rifleFireModeDropdown.onValueChanged.AddListener((int value) =>
		{
			configuration.rifleFireMode = (WeaponBase.FireMode)value;
		});
		interactionMarkers.onValueChanged.AddListener((bool show) =>
		{
			configuration.onInteractionMarkersChanged.Invoke(show);
		});
		showMarksmanTargetIndication.onValueChanged.AddListener((bool show) =>
		{
			configuration.onMarksmanIndicationChanged.Invoke(show);
		});
	}

	private SerializableGeneralSettings Serialize()
	{
		SerializableGeneralSettings serializableGeneralSettings = new SerializableGeneralSettings();
		serializableGeneralSettings.lightOn = flashLightDefaultMode.isOn;
		serializableGeneralSettings.laserOn = laserDefaultMode.isOn;
		serializableGeneralSettings.pistolFireMode = (WeaponBase.FireMode)pistolFireModeDropdown.value;
		serializableGeneralSettings.smgFireMode = (WeaponBase.FireMode)smgFireModeDropdown.value;
		serializableGeneralSettings.shotgunFireMode = (WeaponBase.FireMode)shotgunFireModeDropdown.value;
		serializableGeneralSettings.rifleFireMode = (WeaponBase.FireMode)rifleFireModeDropdown.value;
		serializableGeneralSettings.showInteractionMarkers = interactionMarkers.isOn;
		serializableGeneralSettings.reloadHelp = automaticBolt.isOn;
		serializableGeneralSettings.showMarksmanTargetIndication = showMarksmanTargetIndication.isOn;
		serializableGeneralSettings.enableMarksmanBobbing = enableMarksmanBobbing.isOn;
		serializableGeneralSettings.interiorAimSensitivity = interiorAimSensitivity.value;
		serializableGeneralSettings.marksmanAimSensitivity = marksmanAimSensitivity.value;
		serializableGeneralSettings.marksmanReloadSensitivity = marksmanReloadSensitivity.value;
		serializableGeneralSettings.grenadeTimerMode = grenadeTimerDropdown.value;
		return serializableGeneralSettings;
	}

	private void Deserialize(SerializableGeneralSettings data)
	{
		flashLightDefaultMode.isOn = data.lightOn;
		laserDefaultMode.isOn = data.laserOn;
		pistolFireModeDropdown.value = (int)data.pistolFireMode;
		smgFireModeDropdown.value = (int)data.smgFireMode;
		shotgunFireModeDropdown.value = (int)data.shotgunFireMode;
		rifleFireModeDropdown.value = (int)data.rifleFireMode;
		interactionMarkers.isOn = data.showInteractionMarkers;
		automaticBolt.isOn = data.reloadHelp;
		showMarksmanTargetIndication.isOn = data.showMarksmanTargetIndication;
		enableMarksmanBobbing.isOn = data.enableMarksmanBobbing;
		interiorAimSensitivity.value = data.interiorAimSensitivity;
		marksmanAimSensitivity.value = data.marksmanAimSensitivity;
		marksmanReloadSensitivity.value = data.marksmanReloadSensitivity;
		grenadeTimerDropdown.value = data.grenadeTimerMode;
	}

	private void InitializeFireModeDropdowns()
	{
		pistolFireModeDropdown.ClearOptions();
		smgFireModeDropdown.ClearOptions();
		shotgunFireModeDropdown.ClearOptions();
		rifleFireModeDropdown.ClearOptions();
		Array values = Enum.GetValues(typeof(WeaponBase.FireMode));
		List<string> list = new List<string>();
		foreach (object item in values)
		{
			list.Add(StringHelper.FriendlyName(item.ToString()));
		}
		pistolFireModeDropdown.AddOptions(list);
		smgFireModeDropdown.AddOptions(list);
		shotgunFireModeDropdown.AddOptions(list);
		rifleFireModeDropdown.AddOptions(list);
	}
}

}
