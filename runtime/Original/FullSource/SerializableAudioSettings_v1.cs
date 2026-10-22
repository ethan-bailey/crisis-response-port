namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("1", new Type[] { })]
public class SerializableAudioSettings_v1
{
	public float effectsVolume = -4f;

	public float voiceVolume = -12f;

	public float ambienceVolume = -12f;

	public float missionMusicVolume = 0.4f;

	public float menuMusicVolume = 0.4f;

	public bool enableMissionMusic = true;

	public bool enableMenuMusic = true;
}

}
