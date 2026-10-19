namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UINewVersionNotice : MonoBehaviour
{
	public GameObject containerObject;

	public Text newVersionText;

	public Transform downloadTextTransform;

	private void Start()
	{
		bool active = !string.IsNullOrEmpty(Global.Context.downloadURL);
		containerObject.SetActive(active);
		if (Global.NewerVersionAvailable())
		{
			Display(PaddedVersionString(Global.Context.AvailableVersion));
		}
	}

	public void Display(string newVersionString)
	{
		newVersionText.text = string.Format("Version {0} is available", PaddedVersionString(newVersionString));
		containerObject.SetActive(true);
	}

	public void OpenDownloadURL()
	{
		Global.Context.Usage.session.clicksDL++;
		Global.Context.Usage.PersistSessionData();
		Application.OpenURL(Global.Context.downloadURL);
	}

	public void OnPointerEnter()
	{
		downloadTextTransform.localScale = new Vector3(1.1f, 1.1f, 1f);
	}

	public void OnPointerExit()
	{
		downloadTextTransform.localScale = Vector3.one;
	}

	private string PaddedVersionString(string version)
	{
		string[] array = version.Split('.');
		if (array.Length < 4)
		{
			return "0.0.0.0";
		}
		return string.Format("{0}.{1}.{2}.{3}", array[0], array[1], array[2], array[3].PadLeft(3, '0'));
	}
}

}
