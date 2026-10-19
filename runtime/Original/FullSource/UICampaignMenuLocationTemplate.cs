namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UICampaignMenuLocationTemplate : MonoBehaviour
{
	public struct CampaignMenuLocationPair
	{
		public CampaignMenuLocation identifier;

		public UICampaignMenuLocation location;
	}

	public Text titleText;

	public Text currentDayText;

	public Image background;

	[Header("Locations")]
	public UICampaignMenuLocation headquarters;

	public UICampaignMenuLocation teamLeaderOffice;

	public UICampaignMenuLocation teamRoster;

	public UICampaignMenuLocation shiftSchedule;

	public UICampaignMenuLocation activitySchedule;

	public UICampaignMenuLocation mission;

	public UICampaignMenuLocation debrief;

	private void Awake()
	{
		Global.Context.CampaignManager.onDayChanged.RemoveListener(RefreshDayDisplay);
		Global.Context.CampaignManager.onDayChanged.AddListener(RefreshDayDisplay);
	}

	public void SetLocation(UICampaignMenuLocation location)
	{
		titleText.text = location.title;
		background.sprite = location.background;
	}

	public void RefreshDayDisplay()
	{
		currentDayText.text = string.Format("Day {0}", Global.Context.CampaignManager.data.day);
	}

	private void HideAll()
	{
		headquarters.gameObject.SetActive(false);
		teamLeaderOffice.gameObject.SetActive(false);
		teamRoster.gameObject.SetActive(false);
		shiftSchedule.gameObject.SetActive(false);
		activitySchedule.gameObject.SetActive(false);
		mission.gameObject.SetActive(false);
		debrief.gameObject.SetActive(false);
	}

	public void Goto(CampaignMenuLocation identifier)
	{
		HideAll();
		UICampaignMenuLocation uICampaignMenuLocation = null;
		switch (identifier)
		{
		case CampaignMenuLocation.Headquarters:
			uICampaignMenuLocation = headquarters;
			break;
		case CampaignMenuLocation.TeamLeader:
			uICampaignMenuLocation = teamLeaderOffice;
			break;
		case CampaignMenuLocation.TeamRoster:
			uICampaignMenuLocation = teamRoster;
			break;
		case CampaignMenuLocation.ShiftSchedule:
			uICampaignMenuLocation = shiftSchedule;
			break;
		case CampaignMenuLocation.ActivitySchedule:
			uICampaignMenuLocation = activitySchedule;
			break;
		case CampaignMenuLocation.Mission:
			uICampaignMenuLocation = mission;
			break;
		case CampaignMenuLocation.Debrief:
			uICampaignMenuLocation = debrief;
			break;
		}
		if (!(uICampaignMenuLocation == null))
		{
			titleText.text = uICampaignMenuLocation.title;
			background.sprite = uICampaignMenuLocation.background;
			uICampaignMenuLocation.gameObject.SetActive(true);
			RefreshDayDisplay();
			base.gameObject.SetActive(true);
		}
	}
}

}
