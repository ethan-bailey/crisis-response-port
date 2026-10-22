namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class InteriorMode : MissionMode
{
	public InteriorCamera interiorCamera;

	public InteriorModeUI uiPrefab;

	public Transform audioListenerTransform;

	public AudioMixerSnapshot audioSnapshot;

	public AlternateCamera alternateCameraPrefab;

	private AudioSource marksmanAudioSource;

	private InteriorModeUI interiorModeUI;

	public AudioSource AudioSource
	{
		get
		{
			return marksmanAudioSource;
		}
	}

	public InteriorModeUI UI
	{
		get
		{
			return interiorModeUI;
		}
	}

	public AlternateCamera AlternateCamera { get; private set; }

	public override void Initialize()
	{
		base.Initialize();
		marksmanAudioSource = GetComponent<AudioSource>();
		marksmanAudioSource.priority = 15;
		if (interiorModeUI == null)
		{
			interiorModeUI = Object.Instantiate(uiPrefab);
			interiorModeUI.transform.SetParent(base.transform);
		}
	}

	private void OnEnable()
	{
		UI.weaponPointer.Show();
	}

	private void Start()
	{
		Global.Context.Mission.Occupants.allDisabledEvent.AddListener(ShowInteractionMarkers);
		Global.Context.Settings.General.onInteractionMarkersChanged.AddListener(ApplyInteractionMarkersVisibility);
	}

	public override void OnPause()
	{
		UI.weaponPointer.Hide();
	}

	public override void OnResume()
	{
		PlayerController playerController = Global.Context.Mission.Operators.PlayerController;
		if (!(playerController == null))
		{
			UI.weaponPointer.Show();
		}
	}

	public void ScheduleMarksmanDetonation(Marksman marksman)
	{
		List<AudioClip> list = ((!(interiorCamera.Subject != null) || !(interiorCamera.Subject.Room != null) || !interiorCamera.Subject.Room.exterior) ? marksman.interiorDetonate : marksman.exteriorDetonate);
		AudioClip clip = list[Random.Range(0, list.Count)];
		float delay = Mathf.Abs(-1.5f - marksman.transform.position.z) / 343f;
		Coroutine.DelayedAction(delay, () =>
		{
			PlayMarksmanDetonation(clip);
		}, Global.Context);
	}

	private void PlayMarksmanDetonation(AudioClip clip)
	{
		marksmanAudioSource.PlayOneShot(clip);
	}

	public bool ToggleAlternateCamera()
	{
		bool flag = AlternateCamera == null || !AlternateCamera.enabled;
		if (AlternateCamera == null)
		{
			AlternateCamera = Object.Instantiate(alternateCameraPrefab);
			AlternateCamera.name = alternateCameraPrefab.name;
		}
		interiorCamera.interiorCamera.enabled = !flag;
		AlternateCamera alternateCamera = AlternateCamera;
		bool flag2 = flag;
		AlternateCamera.Camera.enabled = flag2;
		alternateCamera.enabled = flag2;
		return flag;
	}

	private void ShowInteractionMarkers()
	{
		if (Global.Context.Settings.General.showInteractionMarkers)
		{
			ApplyInteractionMarkersVisibility(true);
		}
	}

	private void ApplyInteractionMarkersVisibility(bool visible)
	{
		Mission mission = Global.Context.Mission;
		foreach (Transform item in mission.WeaponsContainer)
		{
			WeaponBase weaponBase = item.GetComponent<WeaponBase>();
			if (weaponBase == null)
			{
				weaponBase = ((!(item.GetComponent<DroppedWeaponProxy>() != null)) ? null : item.GetComponent<DroppedWeaponProxy>().weapon);
			}
			if (weaponBase != null)
			{
				weaponBase.ShowInteractionMarker(visible);
			}
		}
		foreach (CharacterBody body in mission.Occupants.Bodies)
		{
			body.ShowRestrainMarker(visible);
			body.ShowStabilizeMarker(visible);
		}
	}
}

}
