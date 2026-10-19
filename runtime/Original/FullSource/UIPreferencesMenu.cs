namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPreferencesMenu : MonoBehaviour
{
	[Header("Prefabs")]
	public UIGeneralSettings generalPrefab;

	public UIBindingsTab controlsPrefab;

	public UIPerformanceSettings performancePrefab;

	public UIGraphicsSettings graphicsPrefab;

	public UIAudioSettings audioPrefab;

	public UIExtraSettings extrasPrefab;

	public UISlowMotionSettings timePrefab;

	[Header("References")]
	public GameObject menuPanel;

	public GameObject backgroundDim;

	public UISettingsPanel sectionPanel;

	public UIBindingOverlay bindingOverlay;

	public UIBindingConflictDialog bindingConflictDialog;

	public Transform container;

	public Button generalButton;

	public Button controlsButton;

	public Button graphicsButton;

	public Button audioButton;

	public Button performanceButton;

	public Button timeButton;

	public Button extrasButton;

	public Button backButton;

	[Header("Instances (title screen only)")]
	public UIGeneralSettings general;

	public UIBindingsTab controls;

	public UIPerformanceSettings performance;

	public UIGraphicsSettings graphics;

	public UIAudioSettings audio;

	public UIExtraSettings extras;

	public UISlowMotionSettings time;

	public UnityEvent onHide = new UnityEvent();

	public bool Visible
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	private void OnEnable()
	{
		backgroundDim.SetActive(Global.Context.Mission == null);
	}

	private void Awake()
	{
		if (general == null)
		{
			general = Object.Instantiate(generalPrefab);
			general.transform.SetParent(container, false);
		}
		if (controls == null)
		{
			controls = Object.Instantiate(controlsPrefab);
			controls.transform.SetParent(container, false);
		}
		if (performance == null)
		{
			performance = Object.Instantiate(performancePrefab);
			performance.transform.SetParent(container, false);
		}
		if (graphics == null)
		{
			graphics = Object.Instantiate(graphicsPrefab);
			graphics.transform.SetParent(container, false);
		}
		if (audio == null)
		{
			audio = Object.Instantiate(audioPrefab);
			audio.transform.SetParent(container, false);
		}
		if (time == null)
		{
			time = Object.Instantiate(timePrefab);
			time.transform.SetParent(container, false);
		}
		if (extras == null)
		{
			extras = Object.Instantiate(extrasPrefab);
			extras.transform.SetParent(container, false);
		}
		general.gameObject.SetActive(false);
		controls.gameObject.SetActive(false);
		performance.gameObject.SetActive(false);
		graphics.gameObject.SetActive(false);
		audio.gameObject.SetActive(false);
		time.gameObject.SetActive(false);
		extras.gameObject.SetActive(false);
		bindingConflictDialog.Initialize(controls);
		Global.Context.InputHandler.Initialize(bindingOverlay, bindingConflictDialog);
		controls.Initialize(sectionPanel, bindingOverlay);
		performance.Initialize(sectionPanel);
		backButton.onClick.RemoveAllListeners();
		backButton.onClick.AddListener(Hide);
	}

	private void Start()
	{
		generalButton.onClick.AddListener(() =>
		{
			OpenSettings(general.gameObject, "General", "Configure general settings.");
		});
		controlsButton.onClick.AddListener(() =>
		{
			OpenSettings(controls.gameObject, "Controls", "Click any item to start binding.");
		});
		performanceButton.onClick.AddListener(() =>
		{
			OpenSettings(performance.gameObject, "Performance", "Configure performance settings.");
		});
		graphicsButton.onClick.AddListener(() =>
		{
			OpenSettings(graphics.gameObject, "Video", "Configure video settings.");
		});
		performanceButton.onClick.AddListener(() =>
		{
			OpenSettings(performance.gameObject, "Performance", "Configure performance settings.");
		});
		audioButton.onClick.AddListener(() =>
		{
			OpenSettings(audio.gameObject, "Audio", "Configure audio settings.");
		});
		timeButton.onClick.AddListener(() =>
		{
			OpenSettings(time.gameObject, "Time control", "Configure bullet time and slow motion.");
		});
		extrasButton.onClick.AddListener(() =>
		{
			OpenSettings(extras.gameObject, "extras", "Configure extra features and modifiers.");
		});
		backButton.onClick.AddListener(Hide);
		sectionPanel.onDone.AddListener(() =>
		{
			menuPanel.SetActive(true);
		});
	}

	private void OpenSettings(GameObject section, string header, string description)
	{
		menuPanel.SetActive(false);
		sectionPanel.Open(section, header, description);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
		onHide.Invoke();
	}
}

}
