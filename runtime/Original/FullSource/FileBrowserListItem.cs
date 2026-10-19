namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class FileBrowserListItem : MonoBehaviour
{
	public Text text;

	private Button button;

	private string value;

	public void Initialize(string text, string value, Transform parent, UnityEventString callback)
	{
		this.value = value;
		this.text.text = text;
		base.transform.SetParent(parent);
		base.transform.localScale = Vector3.one;
		button = GetComponent<Button>();
		button.onClick.AddListener(() =>
		{
			callback.Invoke(value);
		});
	}
}

}
