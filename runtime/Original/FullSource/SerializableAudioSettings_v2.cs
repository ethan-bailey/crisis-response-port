namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("v2", new Type[] { typeof(SerializableAudioSettings_v1) })]
public class SerializableAudioSettings_v2
{
	public float effectsVolume = -4f;

	public float voiceVolume = -12f;

	public float ambienceVolume = -12f;

	public float missionMusicVolume = 0.4f;

	public float menuMusicVolume = 0.4f;

	public bool enableMissionMusic = true;

	public bool enableMenuMusic = true;

	public float userInterfaceVolume = -12f;

	public SerializableAudioSettings_v2(SerializableAudioSettings_v1 v1Data)
	{
		effectsVolume = v1Data.effectsVolume;
		voiceVolume = v1Data.voiceVolume;
		ambienceVolume = v1Data.ambienceVolume;
		missionMusicVolume = v1Data.missionMusicVolume;
		menuMusicVolume = v1Data.menuMusicVolume;
		enableMissionMusic = v1Data.enableMissionMusic;
		enableMenuMusic = v1Data.enableMenuMusic;
		userInterfaceVolume = -12f;
	}
}

}
