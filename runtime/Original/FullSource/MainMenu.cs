namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public static string ObjectName = "Main menu";

	[Header("Screens and panels")]
	public GameObject titleScreen;

	public GameObject oppositionScreen;

	public GameObject equipmentScreen;

	public UIPreferencesMenu preferencesMenu;

	public GameObject creditsScreen;

	public UIBindingsTab bindingsPanel;

	public UINewVersionNotice newVersionNotice;

	public GameObject candidateNotice;

	[Header("References")]
	public Transform equipmentContainer;

	public Image fadeToBlack;

	public GameObject flare1;

	public Text versionText;

	public Text copyrightText;

	public Text creditText;

	public UITitleTextAnimator titleTextAnimator;

	public GameObject sandboxSubtitle;

	public UIChildSequenceAnimator operatorsAnimator;

	public UIChildSequenceAnimator titleButtonsAnimator;

	public UIChildSequenceAnimator backgroundAnimator;

	public ImageColorFlasher backgroundFlasher;

	public UITitleSpriteFlasherSequence operatorsFlasher;

	public Button campaignButton;

	public Button feedbackButton;

	public Button discordButton;

	public WeaponRenderer weaponRenderer;

	public float flarePeriod = 10f;

	public float flare1Radius = 100f;

	public float flare2Radius = 50f;

	private float fadeStartTime;

	private float fadeOutPeriod = 1f;

	private float flareTime;

	private bool pendingLoadMission;

	private void Awake()
	{
		Cursor.visible = true;
		ShowStaticAnimationElements(false);
	}

	private void Start()
	{
		Global.Context.Usage.receivedFinalSessionDataCallback.AddListener(OnServerSessionClosed);
		versionText.text = string.Format("Version {0}", Version.Instance.VersionString);
		feedbackButton.onClick.RemoveAllListeners();
		feedbackButton.onClick.AddListener(OnPressedFeedbackButton);
		discordButton.onClick.RemoveAllListeners();
		discordButton.onClick.AddListener(OnPressedDiscordButton);
		preferencesMenu.onHide.AddListener(() =>
		{
			titleScreen.SetActive(true);
		});
		Global.Context.Mixer.StartTitleMusic();
	}

	public void Begin()
	{
		Time.timeScale = 1f;
		PlayEnterAnimation();
	}

	private void Update()
	{
		flareTime += Time.deltaTime;
		float num = flareTime / flarePeriod;
		if (flareTime >= flarePeriod)
		{
			flareTime = 0f;
		}
		if (flare1 != null)
		{
			flare1.transform.position = Input.mousePosition + new Vector3(Mathf.Sin(num * 2f * (float)Math.PI), Mathf.Cos(num * 2f * (float)Math.PI)) * flare1Radius;
		}
		if (pendingLoadMission && fadeToBlack != null)
		{
			float num2 = Time.realtimeSinceStartup - fadeStartTime;
			fadeToBlack.color = new Color(0f, 0f, 0f, num2 / fadeOutPeriod);
			if (num2 >= fadeOutPeriod)
			{
				fadeToBlack.color = Color.black;
				SceneManager.LoadScene("Transition");
			}
		}
	}

	public void StartSandbox()
	{
		TransitionToScene(SceneTransition.Scene.Mission);
	}

	public void StartMarksmanSandbox()
	{
		TransitionToScene(SceneTransition.Scene.Marksman);
	}

	public void StartCampaign()
	{
		TransitionToScene(SceneTransition.Scene.Campaign);
	}

	public void StartTutorial()
	{
		TransitionToScene(SceneTransition.Scene.Mission);
	}

	public void StartMarksmanTutorial()
	{
		Global.Context.MissionMetaData.ActivateTutorial(Tutorial.Mode.Marksman);
		TransitionToScene(SceneTransition.Scene.Marksman);
	}

	public void StartBenchmark()
	{
		TransitionToScene(SceneTransition.Scene.Benchmark);
	}

	private void OnPressedFeedbackButton()
	{
		Global.Context.FeedbackWindow.Show(true);
	}

	private void OnPressedDiscordButton()
	{
		Application.OpenURL("https://discord.gg/rEzzjFH");
	}

	public void QuitWithAnimation()
	{
		PlayExitAnimation(true);
		if (Global.Context.Usage.RecordUsage && Global.Context.Connection.communicationEnabled)
		{
			Global.Context.Usage.PersistSessionClose();
			Coroutine.DelayedAction(3f, Quit, this);
		}
		else
		{
			Coroutine.DelayedAction(1f, Quit, this);
		}
	}

	private void ShowStaticAnimationElements(bool show)
	{
		sandboxSubtitle.SetActive(show);
		versionText.gameObject.SetActive(show);
		copyrightText.gameObject.SetActive(show);
		creditText.gameObject.SetActive(show);
		feedbackButton.gameObject.SetActive(show);
		discordButton.gameObject.SetActive(show);
		newVersionNotice.gameObject.SetActive(show);
		candidateNotice.gameObject.SetActive(show && Version.Instance.Candidate);
	}

	public void PlayEnterAnimation()
	{
		titleTextAnimator.PlayStartAnimation();
		operatorsAnimator.PlayAppear();
		backgroundAnimator.PlayAppear();
		titleButtonsAnimator.PlayAppear();
		backgroundFlasher.FlashIn(1.35f, 4);
		operatorsFlasher.FlashIn(0.75f);
		Coroutine.DelayedAction(1f, () =>
		{
			ShowStaticAnimationElements(true);
		}, this);
	}

	public void PlayExitAnimation(bool pendingTermination)
	{
		if (pendingTermination)
		{
			Global.Context.Mixer.OnGameQuit();
		}
		else
		{
			Global.Context.Mixer.OnLoadMission();
		}
		Global.Context.frameRateDisplay.gameObject.SetActive(false);
		titleTextAnimator.PlayQuitAnimation();
		operatorsAnimator.PlayDisappear();
		backgroundAnimator.PlayDisappear();
		titleButtonsAnimator.PlayDisappear();
		Coroutine.DelayedAction(0.25f, () =>
		{
			ShowStaticAnimationElements(false);
		}, this);
	}

	public void Quit()
	{
		Application.Quit();
	}

	private void OnServerSessionClosed(bool closedOK)
	{
		if (closedOK)
		{
			StartCoroutine(FlashBackgroundAndQuit(10, 1f));
		}
		else
		{
			StartCoroutine(FlashBackgroundAndQuit(0, 1f));
		}
	}

	private IEnumerator FlashBackgroundAndQuit(int followUpFlashes, float duration)
	{
		backgroundFlasher.FlashIn(0f, followUpFlashes);
		yield return new WaitForSeconds(duration);
		Quit();
	}

	public void StartSandboxMission()
	{
		titleScreen.SetActive(false);
		oppositionScreen.SetActive(true);
	}

	public void OppositionNavigate(int direction)
	{
		oppositionScreen.SetActive(false);
		titleScreen.SetActive(direction == -1);
		equipmentScreen.SetActive(direction == 1);
	}

	public void TransitionToScene(SceneTransition.Scene scene)
	{
		PlayExitAnimation(false);
		StartTransitionFadeOut();
		Global.Object.GetComponent<SceneTransition>().targetScene = scene;
	}

	private void StartTransitionFadeOut()
	{
		pendingLoadMission = true;
		fadeStartTime = Time.realtimeSinceStartup;
		fadeToBlack.gameObject.SetActive(true);
	}
}

}
