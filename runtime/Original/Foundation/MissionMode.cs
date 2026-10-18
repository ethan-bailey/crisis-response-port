using UnityEngine;

public class MissionMode : MonoBehaviour
{
	public virtual void Initialize()
	{
	}

	public virtual void OnShow()
	{
	}

	public virtual void OnHide()
	{
	}

	public virtual void OnPause()
	{
	}

	public virtual void OnResume()
	{
	}

	public void Show()
	{
		base.gameObject.SetActive(true);
		OnShow();
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
		OnHide();
	}
}
