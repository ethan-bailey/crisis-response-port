namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UISettingsPanel : MonoBehaviour
{
	public Text headerText;

	public Text ribbonText;

	public ScrollRect scrollRect;

	public GameObject performanceLegend;

	public GameObject restartNotice;

	public Button doneButton;

	public UnityEventVoid onDone = new UnityEventVoid();

	private GameObject currentSection;

	private void Start()
	{
		doneButton.onClick.AddListener(OnDoneButtonPressed);
	}

	public void Open(string header, string description, int childIndex = -1)
	{
		int index = ((childIndex != -1) ? childIndex : 0);
		GameObject section = ((base.transform.childCount <= 0) ? null : base.transform.GetChild(index).gameObject);
		Open(section, header, description);
	}

	public void Open(GameObject section, string header, string description)
	{
		if (!(section == null))
		{
			headerText.text = header;
			ribbonText.text = description;
			currentSection = section;
			scrollRect.content = currentSection.GetComponent<RectTransform>();
			scrollRect.verticalNormalizedPosition = 1f;
			currentSection.SetActive(true);
			base.gameObject.SetActive(true);
		}
	}

	public void ShowPerformanceLegend(bool show)
	{
		performanceLegend.SetActive(show);
	}

	public void ShowRestartNotice(bool show)
	{
		restartNotice.SetActive(show);
	}

	private void OnDoneButtonPressed()
	{
		base.gameObject.SetActive(false);
		if (currentSection != null)
		{
			currentSection.SetActive(false);
		}
		onDone.Invoke();
	}
}

}
