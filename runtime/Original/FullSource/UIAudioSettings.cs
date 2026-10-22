namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIAudioSettings : MonoBehaviour
{
	public Slider effectsVolume;

	public Slider voiceVolume;

	public Slider ambienceVolume;

	public Slider missionMusicVolume;

	public Slider menuMusicVolume;

	public Slider userInterfaceVolume;

	public UIButtonToggle enableMenuMusic;

	public UIButtonToggle enableMissionMusic;

	public Image menuMusicSliderImage;

	public Image missionMusicSliderImage;

	private Settings settings;

	private Mixer mixer;

	private SerializableAudioSettings configuration;

	private bool initialized;

	private void OnEnable()
	{
		settings = Global.Context.Settings;
		Deserialize(settings.Audio);
		initialized = true;
		enableMenuMusic.onValueChanged.AddListener(EnableMenuMusic);
	}

	private void OnDisable()
	{
		Save();
		initialized = false;
		enableMenuMusic.onValueChanged.RemoveAllListeners();
	}

	private void Start()
	{
		mixer = Global.Context.Mixer;
		configuration = Global.Context.Settings.Audio;
		enableMissionMusic.onValueChanged.AddListener(EnableMissionMusic);
		menuMusicVolume.onValueChanged.AddListener((float volume) =>
		{
			configuration.onMenuMusicVolumeChanged.Invoke(volume);
		});
		missionMusicVolume.onValueChanged.AddListener((float volume) =>
		{
			configuration.onMissionMusicVolumeChanged.Invoke(volume);
		});
		effectsVolume.onValueChanged.AddListener((float volume) =>
		{
			configuration.onEffectsVolumeChanged.Invoke(volume);
		});
		voiceVolume.onValueChanged.AddListener((float volume) =>
		{
			configuration.onVoiceVolumeChanged.Invoke(volume);
		});
		ambienceVolume.onValueChanged.AddListener((float volume) =>
		{
			configuration.onAmbienceVolumeChanged.Invoke(volume);
		});
		userInterfaceVolume.onValueChanged.AddListener((float volume) =>
		{
			configuration.onInterfaceVolumeChanged.Invoke(volume);
		});
	}

	private void Save()
	{
		if (initialized)
		{
			SerializableAudioSettings serializableAudioSettings = Serialize();
			if (serializableAudioSettings.Different(settings.Audio))
			{
				settings.Audio = serializableAudioSettings;
			}
		}
	}

	private SerializableAudioSettings Serialize()
	{
		SerializableAudioSettings serializableAudioSettings = new SerializableAudioSettings();
		serializableAudioSettings.effectsVolume = effectsVolume.value;
		serializableAudioSettings.voiceVolume = voiceVolume.value;
		serializableAudioSettings.ambienceVolume = ambienceVolume.value;
		serializableAudioSettings.interfaceVolume = userInterfaceVolume.value;
		serializableAudioSettings.missionMusicVolume = missionMusicVolume.value;
		serializableAudioSettings.menuMusicVolume = menuMusicVolume.value;
		serializableAudioSettings.enableMissionMusic = enableMissionMusic.isOn;
		serializableAudioSettings.enableMenuMusic = enableMenuMusic.isOn;
		return serializableAudioSettings;
	}

	private void Deserialize(SerializableAudioSettings data)
	{
		effectsVolume.value = data.effectsVolume;
		voiceVolume.value = data.voiceVolume;
		ambienceVolume.value = data.ambienceVolume;
		userInterfaceVolume.value = data.interfaceVolume;
		missionMusicVolume.value = data.missionMusicVolume;
		menuMusicVolume.value = data.menuMusicVolume;
		enableMissionMusic.isOn = data.enableMissionMusic;
		enableMenuMusic.isOn = data.enableMenuMusic;
	}

	private void EnableMenuMusic(bool enable)
	{
		menuMusicVolume.interactable = enable;
		Color color = menuMusicSliderImage.color;
		color.a = ((!enable) ? 0.5f : 1f);
		menuMusicSliderImage.color = color;
		configuration.onEnableMenuMusicChanged.Invoke(enable);
	}

	private void EnableMissionMusic(bool enable)
	{
		missionMusicVolume.interactable = enable;
		Color color = missionMusicSliderImage.color;
		color.a = ((!enable) ? 0.5f : 1f);
		missionMusicSliderImage.color = color;
		configuration.onEnableMissionMusicChanged.Invoke(enable);
	}
}

}
