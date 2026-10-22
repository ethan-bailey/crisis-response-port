namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIFeedbackWindow : MonoBehaviour
{
	public Button openDiscordButton;

	public Button openDirectMessageButton;

	public Button doneButton;

	public GameObject panel;

	public UIFeedbackForm feedbackForm;

	public bool Visible
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	private void Start()
	{
		openDiscordButton.onClick.AddListener(OnPressedDiscordButton);
		openDirectMessageButton.onClick.AddListener(OnPressedDirectMessageButton);
		doneButton.onClick.AddListener(OnPressedDoneButton);
	}

	public void Show(bool show)
	{
		base.gameObject.SetActive(show);
	}

	private void OnPressedDiscordButton()
	{
		Application.OpenURL("https://discord.gg/rEzzjFH");
	}

	private void OnPressedDirectMessageButton()
	{
		panel.SetActive(false);
		feedbackForm.Show(OnClosedDirectMessageWindow);
	}

	private void OnClosedDirectMessageWindow()
	{
		panel.SetActive(true);
		if (feedbackForm.CurrentStatus == UIFeedbackForm.Status.Sent)
		{
			Show(false);
		}
	}

	private void OnPressedDoneButton()
	{
		base.gameObject.SetActive(false);
	}
}

}
