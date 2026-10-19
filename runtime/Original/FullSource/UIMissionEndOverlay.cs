namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIMissionEndOverlay : MonoBehaviour
{
	public Button previousTarget;

	public Button nextTarget;

	public Button tryAgainButton;

	public Text tryAgainButtonText;

	public Button toTitleButton;

	public Text previousButtonText;

	public Text nextButtonText;

	public Text targetText;

	public Transform headerTransform;

	public Transform opposingFlare;

	public GameObject menu;

	public Text continueTextFill;

	public Text continueTextOutline;

	[Header("Animation")]
	public AnimationCurve translation;

	public AnimationCurve scale;

	[Tooltip("Inversely scales the time it takes to complete one cycle of the animation. 1 = One second.")]
	public float period = 1f;

	private InteriorCamera interiorCamera;

	private InputHandler input;

	private bool initialized;

	private float headerTransformY;

	private float translationDistance;

	private bool showOnPauseMenuClosed;

	private bool showingMenu;

	public bool Visible
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	private void Initialize()
	{
		showingMenu = false;
		int width = 1920;
		int height = 0;
		Global.Context.GetCanvasResolution(ref width, ref height);
		translationDistance = (float)width + headerTransform.GetComponent<RectTransform>().rect.width;
		headerTransformY = headerTransform.localPosition.y;
		interiorCamera = Global.Context.Mission.InteriorMode.interiorCamera;
		interiorCamera.onSubjectChanged.AddListener(SetTargetText);
		Global.Context.Mission.UI.PauseMenu.onShow.AddListener(OnPauseMenuOpened);
		Global.Context.Mission.UI.PauseMenu.onHide.AddListener(OnPauseMenuClosed);
		input = Global.Context.InputHandler;
		tryAgainButtonText.text = string.Format("Try again <{0}>", input.Keys.interact.KeyName);
		previousButtonText.text = string.Format("Previous <{0}>", input.Keys.left.KeyName);
		nextButtonText.text = string.Format("Next <{0}>", input.Keys.right.KeyName);
		previousTarget.onClick.AddListener(() =>
		{
			interiorCamera.ChangeSubject(InteriorCamera.ChangeSubjectDirection.Previous);
		});
		nextTarget.onClick.AddListener(() =>
		{
			interiorCamera.ChangeSubject(InteriorCamera.ChangeSubjectDirection.Next);
		});
		tryAgainButton.onClick.AddListener(() =>
		{
			Global.Context.Mission.Restart();
		});
		toTitleButton.onClick.AddListener(() =>
		{
			Global.Context.Mission.ReturnToTitle();
		});
		string text = string.Format("Press {0} to continue.", input.Keys.interact.KeyName);
		Text text2 = continueTextFill;
		string text3 = text;
		continueTextOutline.text = text3;
		text2.text = text3;
		SetTargetText();
		initialized = true;
	}

	private void Update()
	{
		float num = Time.realtimeSinceStartup * period;
		headerTransform.localPosition = new Vector3(translation.Evaluate(num) * translationDistance, headerTransformY, 0f);
		headerTransform.localScale = Vector3.one * scale.Evaluate(num);
		opposingFlare.localPosition = new Vector3(translation.Evaluate(1f - num) * translationDistance, headerTransformY, 0f);
		opposingFlare.localScale = Vector3.one * scale.Evaluate(1f - num);
		if ((input.Keys.interact.Down || Input.GetKeyDown(KeyCode.Escape)) && !showingMenu)
		{
			showingMenu = true;
			menu.SetActive(true);
			headerTransform.gameObject.SetActive(false);
			opposingFlare.gameObject.SetActive(false);
			continueTextFill.gameObject.SetActive(false);
		}
		else if (showingMenu)
		{
			if (input.Keys.left.Down)
			{
				interiorCamera.ChangeSubject(InteriorCamera.ChangeSubjectDirection.Previous);
			}
			else if (input.Keys.right.Down)
			{
				interiorCamera.ChangeSubject(InteriorCamera.ChangeSubjectDirection.Next);
			}
			else if (input.Keys.interact.Down)
			{
				Global.Context.Mission.Restart();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				Global.Context.Mission.ReturnToTitle();
			}
		}
	}

	public void Show()
	{
		if (!initialized)
		{
			Initialize();
		}
		base.gameObject.SetActive(true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	private void SetTargetText()
	{
		targetText.text = string.Format("Viewing {0}", (!(interiorCamera.Subject != null)) ? "-" : interiorCamera.Subject.name);
	}

	private void OnPauseMenuOpened()
	{
		showOnPauseMenuClosed = Visible;
		if (Visible)
		{
			Hide();
		}
	}

	private void OnPauseMenuClosed()
	{
		if (showOnPauseMenuClosed)
		{
			Show();
		}
	}
}

}
