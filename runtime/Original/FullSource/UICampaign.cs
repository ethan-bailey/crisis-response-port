namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICampaign : MonoBehaviour
{
	public static string ObjectName = "Campaign UI";

	public GameObject controls;

	public GameObject startScreen;

	public UICampaignMenuLocationTemplate locationTemplate;

	public GameObject debriefScreen;

	[Header("Continue section")]
	public GameObject continueSection;

	public UIContinueCampaignEntry continueEntryPrefab;

	public Transform continueContainer;

	[Header("New campaign panel")]
	public Button createCampaign;

	public InputField campaignName;

	private void Awake()
	{
		RefreshContinueList();
		createCampaign.onClick.AddListener(CreateCampaign);
	}

	private void Start()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		CampaignMenuLocation menuLocation = Global.Context.CampaignManager.MenuLocation;
		startScreen.SetActive(menuLocation == CampaignMenuLocation.Start);
		locationTemplate.Goto(menuLocation);
	}

	public void RefreshContinueList()
	{
		foreach (Transform item in continueContainer)
		{
			Object.Destroy(item.gameObject);
		}
		List<CampaignFileInfo> list = Global.Context.CampaignManager.CampaignsOnDisk();
		foreach (CampaignFileInfo item2 in list)
		{
			UIContinueCampaignEntry uIContinueCampaignEntry = Object.Instantiate(continueEntryPrefab);
			uIContinueCampaignEntry.transform.SetParent(continueContainer, false);
			uIContinueCampaignEntry.Initialize(item2, OnLoaded);
		}
		continueSection.SetActive(list.Count > 0);
	}

	private void CreateCampaign()
	{
		Global.Context.CampaignManager.CreateCampaign(campaignName.text);
		startScreen.SetActive(false);
		locationTemplate.Goto(CampaignMenuLocation.Headquarters);
	}

	public void OnLoaded()
	{
		startScreen.SetActive(false);
		locationTemplate.Goto(CampaignMenuLocation.Headquarters);
		locationTemplate.RefreshDayDisplay();
	}

	public void ReturnToTitle()
	{
		Global.Context.CampaignManager.ReturnToTitle();
	}

	public void SaveAndReturnToTitle()
	{
		Global.Context.CampaignManager.SaveAndReturnToTitle();
	}
}

}
