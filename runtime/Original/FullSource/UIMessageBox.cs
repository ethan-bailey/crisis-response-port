namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMessageBox : MonoBehaviour
{
	private enum Type
	{
		OK = 0,
		OK_Cancel = 1
	}

	public Text message;

	public Button okButton;

	public Button cancelButton;

	public Image background;

	private UnityAction onOK;

	private UnityAction onCancel;

	private Type type;

	public void InitializeOK(string text, UnityAction okCallback = null, bool modal = false)
	{
		message.text = text;
		onOK = okCallback;
		type = Type.OK;
		background.enabled = modal;
		okButton.onClick.RemoveAllListeners();
		okButton.onClick.AddListener(ClickedOK);
		cancelButton.onClick.RemoveAllListeners();
		cancelButton.gameObject.SetActive(false);
	}

	public void InitializeOKCancel(string text, UnityAction okCallback = null, UnityAction cancelCallback = null, bool modal = false)
	{
		message.text = text;
		onOK = okCallback;
		onCancel = cancelCallback;
		type = Type.OK_Cancel;
		background.enabled = modal;
		okButton.onClick.RemoveAllListeners();
		okButton.onClick.AddListener(ClickedOK);
		cancelButton.onClick.RemoveAllListeners();
		cancelButton.onClick.AddListener(ClickedCancel);
	}

	private void Update()
	{
		if (type == Type.OK)
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
			{
				ClickedOK();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Return))
		{
			ClickedOK();
		}
		else if (Input.GetKeyDown(KeyCode.Escape))
		{
			ClickedCancel();
		}
	}

	private void ClickedOK()
	{
		if (onOK != null)
		{
			onOK();
		}
		OnFinished();
	}

	private void ClickedCancel()
	{
		if (onCancel != null)
		{
			onCancel();
		}
		OnFinished();
	}

	private void OnFinished()
	{
		Object.Destroy(base.gameObject);
	}
}

}
