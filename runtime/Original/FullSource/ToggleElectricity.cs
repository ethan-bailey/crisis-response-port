namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class ToggleElectricity : Interaction
{
	public bool electicityEnabled = true;

	public static string enabledString = "Disable building power";

	public static string disabledString = "Enable building power";

	private AudioSource audioSource;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		UpdateDescription();
	}

	private void UpdateDescription()
	{
		description = ((!electicityEnabled) ? disabledString : enabledString);
	}

	public override void Invoke(CharacterBody body)
	{
		electicityEnabled = !electicityEnabled;
		audioSource.Play();
		UpdateDescription();
		foreach (Room room in Global.Context.Mission.rooms)
		{
			room.SetPowerState(electicityEnabled);
		}
	}
}

}
