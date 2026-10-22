namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class NegotiationMode : MissionMode
{
	public UINegotiation UI;

	public AudioMixerSnapshot audioSnapshot;

	public UnityEvent eventRefreshUI = new UnityEvent();

	public UnityEvent eventPeacefulEnd = new UnityEvent();

	private int currentOccupant;

	private float phoneRingStartTimestamp;

	public Mission Mission { get; private set; }

	public CampaignManager CampaignManager { get; private set; }

	public CharacterBody CurrentOccupant
	{
		get
		{
			return Mission.Occupants.Occupant(currentOccupant);
		}
	}

	public bool PhoneDelivered { get; set; }

	public bool PhoneRinging { get; private set; }

	public bool CurrentIsNegotiable
	{
		get
		{
			CharacterBody characterBody = CurrentOccupant;
			return characterBody != null && characterBody.gameObject.activeInHierarchy && characterBody.Blackboard.negotiable;
		}
	}

	private void OnEnable()
	{
		SetCursorState();
	}

	public override void OnResume()
	{
		if (base.gameObject.activeInHierarchy)
		{
			SetCursorState();
		}
	}

	private void Awake()
	{
		Mission = Global.Context.Mission;
		CampaignManager = Global.Context.CampaignManager;
	}

	private void Start()
	{
		Mission.onPhoneDelivered.AddListener(OnPhoneDelivered);
		Next();
	}

	private void SetCursorState()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void Next()
	{
		int num = 0;
		do
		{
			num++;
			currentOccupant++;
			if (currentOccupant >= Mission.Occupants.Count)
			{
				currentOccupant = 0;
			}
		}
		while (!CurrentIsNegotiable && num < Mission.Occupants.Count);
		eventRefreshUI.Invoke();
	}

	public void Previous()
	{
		int num = 0;
		do
		{
			num++;
			currentOccupant--;
			if (currentOccupant < 0)
			{
				currentOccupant = Mission.Occupants.Count - 1;
			}
		}
		while (!CurrentIsNegotiable && num < Mission.Occupants.Count);
		eventRefreshUI.Invoke();
	}

	public void IncrementMorale()
	{
		float morale = CurrentOccupant.Blackboard.morale;
		CurrentOccupant.Blackboard.morale = Mathf.Clamp01(morale + 0.1f);
		eventRefreshUI.Invoke();
	}

	public void DecrementMorale()
	{
		float morale = CurrentOccupant.Blackboard.morale;
		CurrentOccupant.Blackboard.morale = Mathf.Clamp01(morale - 0.1f);
		eventRefreshUI.Invoke();
	}

	public void IncrementAggression()
	{
		float aggression = CurrentOccupant.Blackboard.aggression;
		CurrentOccupant.Blackboard.aggression = Mathf.Clamp01(aggression + 0.1f);
		eventRefreshUI.Invoke();
	}

	public void DecrementAggression()
	{
		float aggression = CurrentOccupant.Blackboard.aggression;
		CurrentOccupant.Blackboard.aggression = Mathf.Clamp01(aggression - 0.1f);
		eventRefreshUI.Invoke();
	}

	public void IncrementFear()
	{
		float fear = CurrentOccupant.Blackboard.fear;
		CurrentOccupant.Blackboard.fear = Mathf.Clamp01(fear + 0.1f);
		eventRefreshUI.Invoke();
	}

	public void DecrementFear()
	{
		float fear = CurrentOccupant.Blackboard.fear;
		CurrentOccupant.Blackboard.fear = Mathf.Clamp01(fear - 0.1f);
		eventRefreshUI.Invoke();
	}

	public void Surrender()
	{
		CurrentOccupant.gameObject.SetActive(false);
		eventRefreshUI.Invoke();
		TestMissionOver();
	}

	public void MobilizeEntryTeam()
	{
		Global.Context.Mission.Operators.Container.gameObject.SetActive(true);
		Global.Context.Mission.SetMode(Mission.MissionView.EntryTeam);
	}

	private void TestMissionOver()
	{
		if (Mission.Occupants.AllSurrendered())
		{
			eventPeacefulEnd.Invoke();
			eventRefreshUI.Invoke();
		}
	}

	public void SetCurrentOccupant(CharacterBody occupant)
	{
		for (int i = 0; i < Mission.Occupants.Container.childCount - 1; i++)
		{
			CharacterBody component = Mission.Occupants.Container.GetChild(i).GetComponent<CharacterBody>();
			if (component == occupant)
			{
				currentOccupant = i;
				break;
			}
		}
	}

	public void OnPhoneDelivered()
	{
		UI.OnPhoneDelivered();
	}

	public void StartCallingPhone()
	{
		CellPhone cellPhone = Global.Context.Mission.CellPhone;
		cellPhone.StartRinging();
		phoneRingStartTimestamp = Time.time;
		PhoneRinging = true;
	}

	public void EndPhoneCall()
	{
		CellPhone cellPhone = Global.Context.Mission.CellPhone;
		cellPhone.StopRinging();
		PhoneRinging = false;
	}

	public void OnPhoneAnswered(CharacterBody occupant)
	{
		SetCurrentOccupant(occupant);
		UI.OnPhoneAnswered();
		PhoneRinging = false;
	}
}

}
