namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class MissionEndMenu : MonoBehaviour
{
	public Text title;

	public Text restartButtonText;

	public Text finishButtonText;

	public Button finishButton;

	public Button restartButton;

	public bool Visible
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	private void Awake()
	{
		finishButton.onClick.RemoveAllListeners();
		finishButton.onClick.AddListener(Global.Context.Mission.ReturnToTitle);
		restartButton.onClick.RemoveAllListeners();
		restartButton.onClick.AddListener(Global.Context.Mission.Restart);
	}

	private void ShowCursor()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void Show()
	{
		ShowCursor();
		InteriorMode interiorMode = Global.Context.Mission.InteriorMode;
		if (interiorMode != null)
		{
			interiorMode.interiorCamera.EnableDepthOfField(true);
		}
		Global.Context.InputHandler.SuspendInput(true);
		base.gameObject.SetActive(true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	public void SetSuccess()
	{
		ShowCursor();
		title.text = "Mission conclusion";
		restartButtonText.text = "Play again";
		finishButtonText.text = "Finish";
	}

	public void SetFailure()
	{
		ShowCursor();
		title.text = "Mission failure";
		restartButtonText.text = "Try again";
		finishButtonText.text = "Main menu";
	}
}

}
