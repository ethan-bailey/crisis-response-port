namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMissionConfirmDialog : MonoBehaviour
{
	public Button cancel;

	public Button confirm;

	private UnityAction onConfirm;

	private UnityAction onCancel;

	public bool Visible
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	private void Awake()
	{
		cancel.onClick.AddListener(Cancel);
		confirm.onClick.AddListener(Confirm);
	}

	public void Initialize(UnityAction confirmCallback, UnityAction cancelCallback = null)
	{
		onConfirm = confirmCallback;
		onCancel = cancelCallback;
	}

	public void Show()
	{
		base.gameObject.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	public void Confirm()
	{
		Hide();
		if (onConfirm != null)
		{
			Global.Context.Mission.ActionScheduler.Schedule(() =>
			{
				onConfirm();
			}, 0.05f, ActionScheduler.DelayType.UnscaledTime);
		}
	}

	public void Cancel()
	{
		Hide();
		if (onCancel != null)
		{
			onCancel();
		}
	}
}

}
