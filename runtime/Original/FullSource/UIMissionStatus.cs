namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIMissionStatus : MonoBehaviour
{
	public Mission mission;

	public AnimationCurve fade;

	public float displayTime = 3f;

	[Header("Toggles")]
	public Toggle roomsChecked;

	public Toggle weaponsSecured;

	public Toggle occupantsDisabled;

	public Toggle occupantsRestrained;

	public Toggle occupantsStabilized;

	public Image background;

	private bool active;

	private bool permanent;

	private float timestamp;

	private float duration;

	private AudioPlayer audioPlayer;

	private void Awake()
	{
		SetAlpha(0f);
		audioPlayer = Global.Context.AudioPlayer;
	}

	private void Start()
	{
		mission = Global.Context.Mission;
		mission.UI.PauseMenu.onShow.AddListener(Show);
		mission.UI.PauseMenu.onHide.AddListener(Hide);
	}

	private void Update()
	{
		roomsChecked.isOn = mission.AllRoomsChecked;
		weaponsSecured.isOn = mission.AllWeaponsSecured;
		if (mission.Occupants != null)
		{
			occupantsDisabled.isOn = mission.Occupants.AllRenderedSafe;
			occupantsRestrained.isOn = mission.Occupants.AllRestrained;
			occupantsStabilized.isOn = false;
		}
		if (active && !permanent)
		{
			float alpha = fade.Evaluate(Mathf.Clamp((Time.realtimeSinceStartup - timestamp) / duration, 0f, 1f));
			SetAlpha(alpha);
		}
	}

	public void Indicate(float duration = 0f, bool audio = true)
	{
		if (duration == 0f)
		{
			duration = displayTime;
		}
		timestamp = Time.realtimeSinceStartup;
		this.duration = duration;
		active = true;
		if (base.gameObject.activeInHierarchy && audio)
		{
			audioPlayer.PlayDoubleBlip();
		}
	}

	public void Show()
	{
		SetAlpha(1f);
		active = (permanent = true);
	}

	public void Hide()
	{
		SetAlpha(0f);
		active = (permanent = false);
	}

	private void SetAlpha(float alpha)
	{
		Color color = new Color(1f, 1f, 1f, alpha);
		Color color2 = new Color(0f, 0f, 0f, alpha);
		background.color = color2;
		foreach (Transform item in base.transform)
		{
			Toggle component = item.GetComponent<Toggle>();
			ColorBlock colors = component.colors;
			colors.disabledColor = color;
			component.colors = colors;
			item.GetChild(1).GetComponent<Text>().color = color;
			item.GetChild(0).GetChild(0).GetComponent<Image>()
				.color = color;
		}
	}
}

}
