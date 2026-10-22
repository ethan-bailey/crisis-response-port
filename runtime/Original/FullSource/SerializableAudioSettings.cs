namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.IO;
using FullSerializer;
using UnityEngine;
using UnityEngine.Events;

[fsObject("0_0_9_4792", new Type[] { typeof(SerializableAudioSettings_v2) }, MemberSerialization = fsMemberSerialization.OptIn)]
public class SerializableAudioSettings
{
	public class AudioSettingsChangedEvent : UnityEvent<SerializableAudioSettings>
	{
	}

	[fsProperty]
	public bool enableMenuMusic = true;

	[fsProperty]
	public bool enableMissionMusic = true;

	[fsProperty]
	public float menuMusicVolume = 0.4f;

	[fsProperty]
	public float missionMusicVolume = 0.4f;

	[fsProperty]
	public float effectsVolume = -4f;

	[fsProperty]
	public float voiceVolume = -12f;

	[fsProperty]
	public float ambienceVolume = -12f;

	[fsProperty]
	public float interfaceVolume = -12f;

	public AudioSettingsChangedEvent onChanged = new AudioSettingsChangedEvent();

	public UnityEventBool onEnableMenuMusicChanged = new UnityEventBool();

	public UnityEventBool onEnableMissionMusicChanged = new UnityEventBool();

	public UnityEventFloat onMenuMusicVolumeChanged = new UnityEventFloat();

	public UnityEventFloat onMissionMusicVolumeChanged = new UnityEventFloat();

	public UnityEventFloat onEffectsVolumeChanged = new UnityEventFloat();

	public UnityEventFloat onVoiceVolumeChanged = new UnityEventFloat();

	public UnityEventFloat onAmbienceVolumeChanged = new UnityEventFloat();

	public UnityEventFloat onInterfaceVolumeChanged = new UnityEventFloat();

	public const string Filename = "audio.dat";

	public SerializableAudioSettings()
	{
	}

	public SerializableAudioSettings(SerializableAudioSettings_v2 v2Data)
	{
		enableMenuMusic = v2Data.enableMenuMusic;
		enableMissionMusic = v2Data.enableMissionMusic;
		menuMusicVolume = v2Data.menuMusicVolume;
		missionMusicVolume = v2Data.missionMusicVolume;
		effectsVolume = v2Data.effectsVolume;
		voiceVolume = v2Data.voiceVolume;
		ambienceVolume = v2Data.ambienceVolume;
		interfaceVolume = v2Data.userInterfaceVolume;
	}

	public bool Different(SerializableAudioSettings other)
	{
		bool flag = false;
		flag |= enableMenuMusic != other.enableMenuMusic;
		flag |= enableMissionMusic != other.enableMissionMusic;
		flag |= menuMusicVolume != other.menuMusicVolume;
		flag |= missionMusicVolume != other.missionMusicVolume;
		flag |= effectsVolume != other.effectsVolume;
		flag |= voiceVolume != other.voiceVolume;
		flag |= ambienceVolume != other.ambienceVolume;
		return flag | (interfaceVolume != other.interfaceVolume);
	}

	public void Overwrite(SerializableAudioSettings other)
	{
		enableMenuMusic = other.enableMenuMusic;
		enableMissionMusic = other.enableMissionMusic;
		menuMusicVolume = other.menuMusicVolume;
		missionMusicVolume = other.missionMusicVolume;
		effectsVolume = other.effectsVolume;
		voiceVolume = other.voiceVolume;
		ambienceVolume = other.ambienceVolume;
		interfaceVolume = other.interfaceVolume;
	}

	public void Save()
	{
		if (!Serializer.SerializeToDisk(this, typeof(SerializableAudioSettings), Application.persistentDataPath + "/audio.dat"))
		{
			Debug.LogWarning("Failed to save audio settings.");
		}
		onChanged.Invoke(this);
	}

	public void Load()
	{
		SerializableAudioSettings serializableAudioSettings = null;
		try
		{
			serializableAudioSettings = (SerializableAudioSettings)Serializer.DeserializeFromDisk(typeof(SerializableAudioSettings), Application.persistentDataPath + "/audio.dat");
		}
		catch (IOException)
		{
		}
		catch (Exception)
		{
			Global.Context.ClearSettingsFile("audio.dat", "audio", false);
			Global.Context.MessageBox.ShowOK("Failed to load audio settings.\nDefaults have been restored.");
		}
		if (serializableAudioSettings == null)
		{
			serializableAudioSettings = new SerializableAudioSettings();
		}
		Overwrite(serializableAudioSettings);
	}

	public void Reset()
	{
		Overwrite(new SerializableAudioSettings());
		onChanged.Invoke(this);
	}
}

}
