namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIContinueCampaignEntry : MonoBehaviour
{
	public Button continueButton;

	public Button deleteButton;

	public Text continueText;

	private UnityAction loadedCallback;

	public string Name { get; set; }

	public string FilePath { get; set; }

	private void Awake()
	{
		continueButton.onClick.AddListener(Continue);
		deleteButton.onClick.AddListener(Delete);
	}

	public void Initialize(CampaignFileInfo info, UnityAction loadCallback)
	{
		string text = info.name;
		continueText.text = text;
		Name = text;
		FilePath = info.path;
		loadedCallback = loadCallback;
	}

	private void Continue()
	{
		if (Global.Context.CampaignManager.ContinueCampaign(FilePath))
		{
			loadedCallback();
		}
	}

	private void Delete()
	{
		Global.Context.CampaignManager.AskDeleteCampaign(FilePath);
	}
}

}
