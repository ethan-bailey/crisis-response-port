namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class PauseMenu : MonoBehaviour
{
	[Header("References")]
	public GameObject menuPanel;

	public UIPreferencesMenu preferencesPrefab;

	public UIMissionConfirmDialog confirmStandDownDialog;

	public UIMissionConfirmDialog confirmRestartDialog;

	public UIMissionConfirmDialog confirmAbortDialog;

	[Header("Buttons")]
	public Button resumeButton;

	public Button preferencesButton;

	public Button feedbackButton;

	public Button standDownButton;

	public Button restartButton;

	public Button abortButton;

	[Header("Events")]
	public UnityEvent onShow = new UnityEvent();

	public UnityEvent onHide = new UnityEvent();

	private Image background;

	private Camera resumeCamera;

	private UIFeedbackWindow feedbackWindow;

	private bool hideMenuOnCancelStandDown;

	public UIPreferencesMenu PreferencesMenu { get; private set; }

	private void Awake()
	{
		background = base.transform.Find("Background").GetComponent<Image>();
		Mission mission = Global.Context.Mission;
		confirmStandDownDialog.Initialize(() =>
		{
			mission.EndMission(0f);
		}, OnConfirmationDialogClosed);
		confirmRestartDialog.Initialize(mission.Restart, OnConfirmationDialogClosed);
		confirmAbortDialog.Initialize(mission.ReturnToTitle, OnConfirmationDialogClosed);
		feedbackWindow = Global.Context.FeedbackWindow;
		if (Global.Context.MissionMetaData.tutorialActive)
		{
			standDownButton.gameObject.SetActive(false);
			restartButton.gameObject.SetActive(false);
		}
		else if (Global.Context.SceneTransition.CurrentScene == SceneTransition.Scene.Marksman)
		{
			standDownButton.gameObject.SetActive(false);
		}
		if (PreferencesMenu == null)
		{
			PreferencesMenu = Object.Instantiate(preferencesPrefab);
			PreferencesMenu.transform.SetParent(base.transform, false);
			PreferencesMenu.transform.SetParent(base.transform, false);
			PreferencesMenu.name = preferencesPrefab.name;
			PreferencesMenu.gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		resumeButton.onClick.AddListener(Global.Context.Mission.ResumeAndHideMenu);
		preferencesButton.onClick.AddListener(OnPressedPreferencesButton);
		feedbackButton.onClick.AddListener(OnPressedFeedbackButton);
		standDownButton.onClick.AddListener(AskConfirmStandDown);
		restartButton.onClick.AddListener(AskConfirmRestart);
		abortButton.onClick.AddListener(AskConfirmAbort);
		PreferencesMenu.onHide.AddListener(() =>
		{
			menuPanel.SetActive(true);
		});
	}

	private void Update()
	{
		if (!Input.GetKeyUp(KeyCode.Escape))
		{
			return;
		}
		if (feedbackWindow.Visible)
		{
			feedbackWindow.Show(false);
		}
		else if (!PreferencesMenu.bindingOverlay.Visible && !PreferencesMenu.bindingConflictDialog.Visible)
		{
			if (PreferencesMenu.Visible)
			{
				PreferencesMenu.Hide();
			}
			else if (confirmStandDownDialog.Visible)
			{
				confirmStandDownDialog.Cancel();
			}
			else if (confirmRestartDialog.Visible)
			{
				confirmRestartDialog.Cancel();
			}
			else if (confirmAbortDialog.Visible)
			{
				confirmAbortDialog.Cancel();
			}
			else
			{
				Global.Context.Mission.ResumeAndHideMenu();
			}
		}
	}

	public void Show()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			EnableCameraEffects();
			base.gameObject.SetActive(true);
			menuPanel.SetActive(true);
			onShow.Invoke();
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
	}

	public void Hide()
	{
		DisableCameraEffects();
		base.gameObject.SetActive(false);
		if (PreferencesMenu != null)
		{
			PreferencesMenu.Hide();
		}
		Global.Context.Mission.UI.missionStatusIndicator.Hide();
		onHide.Invoke();
	}

	private void EnableCameraEffects()
	{
		resumeCamera = Camera.main;
		if (!(resumeCamera == null))
		{
			DepthOfField component = resumeCamera.GetComponent<DepthOfField>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
	}

	private void DisableCameraEffects()
	{
		if (!(resumeCamera == null))
		{
			DepthOfField component = resumeCamera.GetComponent<DepthOfField>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	private void OnPressedFeedbackButton()
	{
		Global.Context.FeedbackWindow.Show(true);
	}

	private void OnPressedPreferencesButton()
	{
		PreferencesMenu.gameObject.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void ConfirmStandDownInteraction()
	{
		hideMenuOnCancelStandDown = !base.gameObject.activeInHierarchy;
		Coroutine.DelayedAction(0.05f, DelayedConfirmStandDown, Global.Context.Mission);
	}

	private void DelayedConfirmStandDown()
	{
		Global.Context.Mission.PauseAndShowMenu();
		menuPanel.SetActive(false);
		confirmStandDownDialog.Show();
		Global.Context.Mission.UI.missionStatusIndicator.Show();
	}

	private void AskConfirmStandDown()
	{
		hideMenuOnCancelStandDown = !base.gameObject.activeInHierarchy;
		confirmStandDownDialog.Show();
	}

	private void AskConfirmRestart()
	{
		hideMenuOnCancelStandDown = !base.gameObject.activeInHierarchy;
		confirmRestartDialog.Show();
	}

	private void AskConfirmAbort()
	{
		hideMenuOnCancelStandDown = !base.gameObject.activeInHierarchy;
		confirmAbortDialog.Show();
	}

	private void OnConfirmationDialogClosed()
	{
		if (hideMenuOnCancelStandDown)
		{
			Global.Context.Mission.ResumeAndHideMenu();
		}
	}
}

}
