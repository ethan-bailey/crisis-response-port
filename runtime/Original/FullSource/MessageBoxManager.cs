namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;

public class MessageBoxManager : MonoBehaviour
{
	public Canvas canvas;

	public UIMessageBox messageBoxPrefab;

	public void ShowOK(string text, UnityAction okCallback = null, bool modal = true)
	{
		UIMessageBox uIMessageBox = CreateMessageBox("MessageBox");
		uIMessageBox.InitializeOK(text, okCallback, modal);
	}

	public void ShowOKCancel(string text, UnityAction okCallback = null, UnityAction cancelCallback = null, bool modal = true)
	{
		UIMessageBox uIMessageBox = CreateMessageBox("OK/Cancel");
		uIMessageBox.InitializeOKCancel(text, okCallback, cancelCallback, modal);
	}

	private UIMessageBox CreateMessageBox(string objectName)
	{
		UIMessageBox uIMessageBox = Object.Instantiate(messageBoxPrefab);
		uIMessageBox.name = objectName;
		uIMessageBox.transform.SetParent(canvas.transform, false);
		return uIMessageBox;
	}
}

}
