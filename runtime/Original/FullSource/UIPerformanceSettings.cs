namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIPerformanceSettings : MonoBehaviour
{
	public Dropdown presetsDropdown;

	public Dropdown qualityDropdown;

	public UIButtonToggle cameraGrainToggle;

	public UIButtonToggle cameraBloomToggle;

	public UIButtonToggle cameraBlurToggle;

	public UIButtonToggle woundDecals;

	public Dropdown shadowsDropdown;

	public Dropdown impactEmissionsModeDropdown;

	public Dropdown dynamicLimbDecalModeDropdown;

	public Dropdown particlesLifetimeDropdown;

	public Dropdown woundStreakLifetimeDropdown;

	public Dropdown surfaceDecalsDropdown;

	[Header("Sliders")]
	public UIPerformanceSlider envHitEffectsSlider;

	public UIPerformanceSlider envHitMeshesSlider;

	public UIPerformanceSlider goreHitEffectsSlider;

	public UIPerformanceSlider soakSpotsSlider;

	public UIPerformanceSlider woundStreaksSlider;

	public UIPerformanceSlider bloodStreamsSlider;

	public UIPerformanceSlider wallStreaksSlider;

	public UIPerformanceSlider floorStreaksSlider;

	public UIPerformanceSlider floorPoolsSlider;

	public UIPerformanceSlider bloodParticlesSlider;

	public UIPerformanceSlider gibsSlider;

	[Header("Local reference")]
	public Button startBenchmarkButton;

	private Settings settings;

	private SerializablePerformanceSettings defaults;

	private GameObject performanceLegend;

	private GameObject restartNotice;

	private bool initialized;

	private void OnEnable()
	{
		settings = Global.Context.Settings;
		defaults = new SerializablePerformanceSettings();
		Deserialize(settings.Performance);
		if (performanceLegend != null)
		{
			performanceLegend.SetActive(true);
		}
		initialized = true;
		if (startBenchmarkButton != null && Global.Context.MainMenu != null)
		{
			startBenchmarkButton.gameObject.SetActive(true);
		}
	}

	private void OnDisable()
	{
		if (performanceLegend != null)
		{
			performanceLegend.SetActive(false);
		}
		if (restartNotice != null)
		{
			restartNotice.SetActive(false);
		}
		Save();
		if (startBenchmarkButton != null && Global.Context.MainMenu != null)
		{
			startBenchmarkButton.gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		SerializablePerformanceSettings configuration = settings.Performance;
		presetsDropdown.onValueChanged.AddListener(OnPresetSelected);
		qualityDropdown.onValueChanged.AddListener((int level) =>
		{
			QualityLevels.SetQualityLevel(level);
		});
		cameraGrainToggle.onValueChanged.AddListener((bool enable) =>
		{
			configuration.onCameraGrainChanged.Invoke(enable);
		});
		cameraBloomToggle.onValueChanged.AddListener((bool enable) =>
		{
			configuration.onCameraBloomChanged.Invoke(enable);
		});
		cameraBlurToggle.onValueChanged.AddListener((bool enable) =>
		{
			configuration.onCameraBlurChanged.Invoke(enable);
		});
		woundDecals.onValueChanged.AddListener((bool enable) =>
		{
			restartNotice.SetActive(RuntimeRestartRequired());
		});
		shadowsDropdown.onValueChanged.AddListener((int type) =>
		{
			configuration.onShadowsChanged.Invoke(type);
		});
		surfaceDecalsDropdown.onValueChanged.AddListener((int value) =>
		{
			restartNotice.SetActive(RuntimeRestartRequired());
		});
		envHitEffectsSlider.onReset.AddListener(() =>
		{
			envHitEffectsSlider.value = defaults.maxEnvironmentHitEffects;
		});
		envHitMeshesSlider.onReset.AddListener(() =>
		{
			envHitMeshesSlider.value = defaults.maxEnvironmentHitMeshes;
		});
		goreHitEffectsSlider.onReset.AddListener(() =>
		{
			goreHitEffectsSlider.value = defaults.maxGoreHitEffects;
		});
		soakSpotsSlider.onReset.AddListener(() =>
		{
			soakSpotsSlider.value = defaults.maxSoakSpots;
		});
		woundStreaksSlider.onReset.AddListener(() =>
		{
			woundStreaksSlider.value = defaults.maxWoundStreaks;
		});
		bloodStreamsSlider.onReset.AddListener(() =>
		{
			bloodStreamsSlider.value = defaults.maxBloodStreams;
		});
		wallStreaksSlider.onReset.AddListener(() =>
		{
			wallStreaksSlider.value = defaults.maxWallStreaks;
		});
		floorStreaksSlider.onReset.AddListener(() =>
		{
			floorStreaksSlider.value = defaults.maxFloorStreaks;
		});
		floorPoolsSlider.onReset.AddListener(() =>
		{
			floorPoolsSlider.value = defaults.maxFloorPools;
		});
		bloodParticlesSlider.onReset.AddListener(() =>
		{
			bloodParticlesSlider.value = defaults.maxBloodParticles;
		});
		gibsSlider.onReset.AddListener(() =>
		{
			gibsSlider.value = defaults.maxGibs;
		});
		if (startBenchmarkButton != null && Global.Context.MainMenu != null)
		{
			startBenchmarkButton.onClick.AddListener(OnStartBenchmarkClicked);
		}
	}

	public void Initialize(UISettingsPanel settingsPanel)
	{
		performanceLegend = settingsPanel.performanceLegend;
		restartNotice = settingsPanel.restartNotice;
	}

	public void Show(UISettingsPanel settingsPanel)
	{
		base.gameObject.SetActive(true);
		if (settingsPanel != null)
		{
			Mission mission = Global.Context.Mission;
		}
	}

	public void Save()
	{
		if (initialized)
		{
			SerializablePerformanceSettings serializablePerformanceSettings = Serialize();
			if (serializablePerformanceSettings.Different(settings.Performance))
			{
				settings.Performance = serializablePerformanceSettings;
			}
		}
	}

	private SerializablePerformanceSettings Serialize()
	{
		SerializablePerformanceSettings serializablePerformanceSettings = new SerializablePerformanceSettings();
		serializablePerformanceSettings.preset = presetsDropdown.value;
		serializablePerformanceSettings.qualityLevel = qualityDropdown.value;
		serializablePerformanceSettings.cameraGrain = cameraGrainToggle.isOn;
		serializablePerformanceSettings.cameraBloom = cameraBloomToggle.isOn;
		serializablePerformanceSettings.cameraBlur = cameraBlurToggle.isOn;
		serializablePerformanceSettings.woundDecals = woundDecals.isOn;
		serializablePerformanceSettings.shadows = shadowsDropdown.value;
		serializablePerformanceSettings.paintTexturePixelsPerMeter = SerializablePerformanceSettings.PaintTextureResolutionValue(surfaceDecalsDropdown.value);
		serializablePerformanceSettings.impactEmissionsMode = impactEmissionsModeDropdown.value;
		serializablePerformanceSettings.particlesLifetime = SerializablePerformanceSettings.ParticlesLifetimeValue(particlesLifetimeDropdown.value);
		serializablePerformanceSettings.woundStreakLifetime = SerializablePerformanceSettings.WoundStreakLifetimeValue(woundStreakLifetimeDropdown.value);
		serializablePerformanceSettings.dynamicLimbDecalMode = dynamicLimbDecalModeDropdown.value;
		serializablePerformanceSettings.maxEnvironmentHitEffects = envHitEffectsSlider.intValue;
		serializablePerformanceSettings.maxEnvironmentHitMeshes = envHitMeshesSlider.intValue;
		serializablePerformanceSettings.maxGoreHitEffects = goreHitEffectsSlider.intValue;
		serializablePerformanceSettings.maxSoakSpots = soakSpotsSlider.intValue;
		serializablePerformanceSettings.maxWoundStreaks = woundStreaksSlider.intValue;
		serializablePerformanceSettings.maxBloodStreams = bloodStreamsSlider.intValue;
		serializablePerformanceSettings.maxWallStreaks = wallStreaksSlider.intValue;
		serializablePerformanceSettings.maxFloorStreaks = floorStreaksSlider.intValue;
		serializablePerformanceSettings.maxFloorPools = floorPoolsSlider.intValue;
		serializablePerformanceSettings.maxBloodParticles = bloodParticlesSlider.intValue;
		serializablePerformanceSettings.maxGibs = gibsSlider.intValue;
		return serializablePerformanceSettings;
	}

	private void Deserialize(SerializablePerformanceSettings data)
	{
		presetsDropdown.onValueChanged.RemoveAllListeners();
		presetsDropdown.value = data.preset;
		qualityDropdown.value = data.qualityLevel;
		cameraGrainToggle.isOn = data.cameraGrain;
		cameraBloomToggle.isOn = data.cameraBloom;
		cameraBlurToggle.isOn = data.cameraBlur;
		woundDecals.isOn = data.woundDecals;
		shadowsDropdown.value = data.shadows;
		surfaceDecalsDropdown.value = SerializablePerformanceSettings.PaintTextureResolutionIndex(data.paintTexturePixelsPerMeter);
		impactEmissionsModeDropdown.value = data.impactEmissionsMode;
		particlesLifetimeDropdown.value = SerializablePerformanceSettings.ParticlesLifetimeIndex(data.particlesLifetime);
		woundStreakLifetimeDropdown.value = SerializablePerformanceSettings.WoundStreakLifetimeIndex(data.woundStreakLifetime);
		dynamicLimbDecalModeDropdown.value = data.dynamicLimbDecalMode;
		envHitEffectsSlider.intValue = data.maxEnvironmentHitEffects;
		envHitMeshesSlider.intValue = data.maxEnvironmentHitMeshes;
		goreHitEffectsSlider.intValue = data.maxGoreHitEffects;
		soakSpotsSlider.intValue = data.maxSoakSpots;
		woundStreaksSlider.intValue = data.maxWoundStreaks;
		bloodStreamsSlider.intValue = data.maxBloodStreams;
		wallStreaksSlider.intValue = data.maxWallStreaks;
		floorStreaksSlider.intValue = data.maxFloorStreaks;
		floorPoolsSlider.intValue = data.maxFloorPools;
		bloodParticlesSlider.intValue = data.maxBloodParticles;
		gibsSlider.intValue = data.maxGibs;
		SetDefaults(data.preset);
		if (initialized)
		{
			presetsDropdown.onValueChanged.AddListener(OnPresetSelected);
		}
	}

	private bool RuntimeRestartRequired()
	{
		if (Global.Context.Mission == null)
		{
			return false;
		}
		return woundDecals.isOn != settings.Performance.woundDecals || surfaceDecalsDropdown.value != settings.Performance.paintTexturePixelsPerMeter;
	}

	private void SetDefaults(int presetIndex)
	{
		defaults = SerializablePerformanceSettings.GetPreset(presetIndex);
	}

	private void OnPresetSelected(int index)
	{
		SetDefaults(index);
		Deserialize(defaults);
	}

	private void OnStartBenchmarkClicked()
	{
		if (Global.Context.MainMenu != null)
		{
			Global.Context.MainMenu.StartBenchmark();
		}
	}
}

}
