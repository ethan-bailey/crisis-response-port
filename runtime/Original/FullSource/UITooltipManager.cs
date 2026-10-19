namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UITooltipManager : MonoBehaviour
{
	[Header("Configuration")]
	[Range(0f, 1f)]
	[Tooltip("The cursor has to move less than this distance for the period of time defined above before the tooltip appears.")]
	public float appearSensitivity = 10f;

	public Vector2 offset = new Vector2(10f, -10f);

	[Header("References")]
	[Tooltip("The transform that contains the tooltip text and is moved to the cursor.")]
	public Transform box;

	[Tooltip("The canvas that renders the tooltip.")]
	public Canvas canvas;

	[Tooltip("The text element that renders the text.")]
	public Text text;

	private RectTransform rectTransform;

	private Vector3 lastCursorPosition;

	private float timestamp;

	public bool Visible
	{
		get
		{
			return box.gameObject.activeInHierarchy;
		}
	}

	public UITooltip Current { get; private set; }

	private void Awake()
	{
		text.text = string.Empty;
		rectTransform = box.GetComponent<RectTransform>();
	}

	public void Set(UITooltip tip)
	{
		text.text = tip.text;
		Current = tip;
		if (string.IsNullOrEmpty(text.text))
		{
			box.gameObject.SetActive(false);
		}
	}

	private void Show()
	{
		if (!string.IsNullOrEmpty(text.text))
		{
			box.gameObject.SetActive(true);
		}
	}

	public void Hide()
	{
		box.gameObject.SetActive(false);
		text.text = string.Empty;
	}

	private void Update()
	{
		Vector3 mousePosition = Input.mousePosition;
		if ((lastCursorPosition - mousePosition).magnitude >= appearSensitivity)
		{
			timestamp = Time.realtimeSinceStartup;
		}
		if (Current != null && Time.realtimeSinceStartup - timestamp >= Current.delay)
		{
			Show();
		}
		lastCursorPosition = mousePosition;
		if (Visible)
		{
			box.position = new Vector3((!(mousePosition.x + offset.x + rectTransform.sizeDelta.x * canvas.scaleFactor > (float)Screen.currentResolution.width)) ? (mousePosition.x + offset.x) : ((float)Screen.currentResolution.width - rectTransform.sizeDelta.x * canvas.scaleFactor), (!(mousePosition.y + offset.y - rectTransform.sizeDelta.y * canvas.scaleFactor < 0f)) ? (mousePosition.y + offset.y) : (rectTransform.sizeDelta.y * canvas.scaleFactor), box.position.z);
		}
	}
}

}
