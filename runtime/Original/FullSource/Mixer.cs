namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Mixer : MonoBehaviour
{
	private class Transition
	{
		public AudioSource source;

		public float startVolume;

		public float targetVolume;

		public float startTime;

		public float duration;

		public Transition(AudioSource source, float startVolume, float targetVolume, float duration)
		{
			this.source = source;
			this.startVolume = startVolume;
			this.targetVolume = targetVolume;
			startTime = Time.realtimeSinceStartup;
			this.duration = duration;
		}
	}

	public AudioMixer audioMixer;

	[Header("Tracks")]
	public AudioClip successTrack;

	public AudioClip failureTrack;

	[Header("Sources")]
	public AudioSource titleSource;

	public AudioSource tensionSource;

	public AudioSource actionSource;

	public AudioSource endedSource;

	[Header("Sample sources")]
	public AudioSource effectSample;

	public AudioSource voiceSample;

	public AudioSource ambienceSample;

	public AudioSource interfaceSample;

	[Header("Snapshots")]
	public AudioMixerSnapshot titleSnapshot;

	public AudioMixerSnapshot missionSnapshot;

	public AudioMixerSnapshot pausedSnapshot;

	public AudioMixerSnapshot endedSnapshot;

	public AudioMixerSnapshot noMusicSnapshot;

	private const float MaxPooledActionMusic = 10f;

	private SerializableAudioSettings settings;

	private List<Transition> transitions;

	private float actionMusicPeriod;

	private bool pendingResumeTensionMusic;

	private void Start()
	{
		settings = Global.Context.Settings.Audio;
		transitions = new List<Transition>();
		AudioSource audioSource = titleSource;
		float menuMusicVolume = settings.menuMusicVolume;
		endedSource.volume = menuMusicVolume;
		audioSource.volume = menuMusicVolume;
		AudioSource audioSource2 = tensionSource;
		menuMusicVolume = settings.missionMusicVolume;
		actionSource.volume = menuMusicVolume;
		audioSource2.volume = menuMusicVolume;
		audioMixer.SetFloat("EffectsVolume", settings.effectsVolume);
		audioMixer.SetFloat("VoiceVolume", settings.voiceVolume);
		audioMixer.SetFloat("AmbienceVolume", settings.ambienceVolume);
		audioMixer.SetFloat("InterfaceVolume", settings.interfaceVolume);
		settings.onEnableMenuMusicChanged.AddListener(OnEnableMenuMusic);
		settings.onEnableMissionMusicChanged.AddListener(OnEnableMissionMusic);
		settings.onMenuMusicVolumeChanged.AddListener((float volume) =>
		{
			titleSource.volume = volume;
		});
		settings.onMissionMusicVolumeChanged.AddListener((float volume) =>
		{
			AudioSource audioSource3 = tensionSource;
			actionSource.volume = volume;
			audioSource3.volume = volume;
		});
		settings.onEffectsVolumeChanged.AddListener((float volume) =>
		{
			audioMixer.SetFloat("EffectsVolume", volume);
			PlaySample(effectSample);
		});
		settings.onVoiceVolumeChanged.AddListener((float volume) =>
		{
			audioMixer.SetFloat("VoiceVolume", volume);
			PlaySample(voiceSample);
		});
		settings.onAmbienceVolumeChanged.AddListener((float volume) =>
		{
			audioMixer.SetFloat("AmbienceVolume", volume);
			PlaySample(ambienceSample);
		});
		settings.onInterfaceVolumeChanged.AddListener((float volume) =>
		{
			audioMixer.SetFloat("InterfaceVolume", volume);
			PlaySample(interfaceSample);
		});
		if (Version.Instance.Staging)
		{
			LoadMusicOverrides();
		}
	}

	private void Update()
	{
		if (actionMusicPeriod > 0f)
		{
			actionMusicPeriod -= Time.deltaTime;
			if (actionMusicPeriod <= 0f)
			{
				actionMusicPeriod = 0f;
				ResumeTensionMusic();
			}
		}
		UpdateTransitions();
	}

	private void UpdateTransitions()
	{
		for (int num = transitions.Count - 1; num >= 0; num--)
		{
			Transition transition = transitions[num];
			float num2 = ((transition.duration != 0f) ? ((Time.realtimeSinceStartup - transition.startTime) / transition.duration) : 1f);
			transition.source.volume = Mathf.Lerp(transition.startVolume, transition.targetVolume, num2);
			if (num2 >= 1f)
			{
				transitions.RemoveAt(num);
			}
		}
	}

	public void StartTitleMusic()
	{
		titleSnapshot.TransitionTo(0f);
		titleSource.Stop();
		if (settings.enableMenuMusic)
		{
			titleSource.Play();
		}
	}

	public void StartMissionMusic()
	{
		missionSnapshot.TransitionTo(2.5f);
		tensionSource.Stop();
		actionSource.Stop();
		if (settings.enableMissionMusic)
		{
			tensionSource.volume = settings.missionMusicVolume;
			tensionSource.Play();
		}
	}

	public void StartEndedMusic(bool success)
	{
		pendingResumeTensionMusic = false;
		endedSnapshot.TransitionTo(0f);
		endedSource.Stop();
		if (settings.enableMissionMusic)
		{
			endedSource.PlayOneShot((!success) ? failureTrack : successTrack);
		}
	}

	public void PlayActionMusic(float seconds)
	{
		if (settings.enableMissionMusic && Global.Context.Mission != null && !Global.Context.Mission.missionEnded)
		{
			transitions.Clear();
			transitions.Add(new Transition(actionSource, 0f, settings.missionMusicVolume, 0.1f));
			transitions.Add(new Transition(tensionSource, tensionSource.volume, 0f, 0.25f));
			pendingResumeTensionMusic = true;
			if (!actionSource.isPlaying)
			{
				actionSource.Play();
			}
			actionMusicPeriod = Mathf.Min(actionMusicPeriod + seconds, 10f);
		}
	}

	private void ResumeTensionMusic()
	{
		if (pendingResumeTensionMusic && settings.enableMissionMusic && Global.Context.Mission != null && !Global.Context.Mission.missionEnded)
		{
			transitions.Clear();
			transitions.Add(new Transition(tensionSource, 0f, settings.missionMusicVolume, 3f));
			transitions.Add(new Transition(actionSource, actionSource.volume, 0f, 3f));
			pendingResumeTensionMusic = false;
		}
	}

	public void SetPaused(bool pause)
	{
		if (settings.enableMissionMusic)
		{
			((!pause) ? missionSnapshot : pausedSnapshot).TransitionTo(0.25f);
		}
	}

	public void OnLoadMission()
	{
		noMusicSnapshot.TransitionTo(5f);
	}

	public void OnMissionUnload()
	{
		pendingResumeTensionMusic = false;
		noMusicSnapshot.TransitionTo(1f);
	}

	public void OnGameQuit()
	{
		pendingResumeTensionMusic = false;
		noMusicSnapshot.TransitionTo(3f);
	}

	private void OnEnableMenuMusic(bool enable)
	{
		if (Global.Context.MainMenu != null)
		{
			if (enable)
			{
				titleSource.volume = settings.menuMusicVolume;
				titleSource.Play();
			}
			else
			{
				titleSource.Stop();
			}
		}
	}

	private void OnEnableMissionMusic(bool enable)
	{
		if (Global.Context.Mission != null)
		{
			AudioSource audioSource = ((!pendingResumeTensionMusic) ? tensionSource : actionSource);
			if (enable)
			{
				audioSource.volume = settings.missionMusicVolume;
				audioSource.Play();
			}
			else
			{
				audioSource.Stop();
			}
		}
	}

	private void OnLeaveAudioSettings()
	{
	}

	private void PlaySample(AudioSource source)
	{
		if (source != null && !source.isPlaying)
		{
			source.Play();
		}
	}

	private void LoadMusicOverrides()
	{
		Debug.Log("Load music overrides...");
		string text = string.Empty;
		string text2 = Application.dataPath.Replace("/", "\\") + "\\title.wav";
		Debug.LogFormat("Looking for: {0}", text2);
		if (FileBrowser.FileExists(text2))
		{
			Debug.Log("Found title override. Loading...");
			text += string.Format("{0}Title", (!string.IsNullOrEmpty(text)) ? ", " : string.Empty);
			ResourceManager.Instance.GetAudioClip(text2, OverrideTitleMusic);
		}
		else
		{
			Debug.Log("No title override.");
		}
		string text3 = Application.dataPath.Replace("/", "\\") + "\\tension.wav";
		Debug.LogFormat("Looking for: {0}", text3);
		if (FileBrowser.FileExists(text3))
		{
			Debug.Log("Found tension override. Loading...");
			text += string.Format("{0}Tension", (!string.IsNullOrEmpty(text)) ? ", " : string.Empty);
			ResourceManager.Instance.GetAudioClip(text3, OverrideTensionMusic);
		}
		else
		{
			Debug.Log("No tension override.");
		}
		string text4 = Application.dataPath.Replace("/", "\\") + "\\action.wav";
		Debug.LogFormat("Looking for: {0}", text4);
		if (FileBrowser.FileExists(text4))
		{
			Debug.Log("Found action override. Loading...");
			text += string.Format("{0}Action", (!string.IsNullOrEmpty(text)) ? ", " : string.Empty);
			ResourceManager.Instance.GetAudioClip(text4, OverrideActionMusic);
		}
		else
		{
			Debug.Log("No action override.");
		}
		string text5 = Application.dataPath.Replace("/", "\\") + "\\success.wav";
		Debug.LogFormat("Looking for: {0}", text5);
		if (FileBrowser.FileExists(text5))
		{
			Debug.Log("Found success override. Loading...");
			text += string.Format("{0}Success", (!string.IsNullOrEmpty(text)) ? ", " : string.Empty);
			ResourceManager.Instance.GetAudioClip(text5, OverrideSuccessMusic);
		}
		else
		{
			Debug.Log("No success override.");
		}
		string text6 = Application.dataPath.Replace("/", "\\") + "\\failure.wav";
		Debug.LogFormat("Looking for: {0}", text6);
		if (FileBrowser.FileExists(text6))
		{
			Debug.Log("Found failure override. Loading...");
			text += string.Format("{0}Failure", (!string.IsNullOrEmpty(text)) ? ", " : string.Empty);
			ResourceManager.Instance.GetAudioClip(text6, OverrideFailureMusic);
		}
		else
		{
			Debug.Log("No failure override.");
		}
		if (!string.IsNullOrEmpty(text))
		{
			Global.Context.MessageBox.ShowOK(string.Format("Overridden music tracks:\n{0}", text));
		}
	}

	private void OverrideTitleMusic(AudioClip clip)
	{
		if (clip != null)
		{
			titleSource.clip = clip;
			Debug.Log("Title music override loaded.");
		}
	}

	private void OverrideTensionMusic(AudioClip clip)
	{
		if (clip != null)
		{
			tensionSource.clip = clip;
			Debug.Log("Tension music override loaded.");
		}
	}

	private void OverrideActionMusic(AudioClip clip)
	{
		if (clip != null)
		{
			actionSource.clip = clip;
			Debug.Log("Action music override loaded.");
		}
	}

	private void OverrideSuccessMusic(AudioClip clip)
	{
		if (clip != null)
		{
			successTrack = clip;
			Debug.Log("Success music override loaded.");
		}
	}

	private void OverrideFailureMusic(AudioClip clip)
	{
		if (clip != null)
		{
			failureTrack = clip;
			Debug.Log("Failure music override loaded.");
		}
	}
}

}
