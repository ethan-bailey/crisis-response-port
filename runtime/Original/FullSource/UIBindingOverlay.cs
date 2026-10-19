namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIBindingOverlay : MonoBehaviour
{
	public Text inputNameText;

	public bool Visible
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	public void StartListening()
	{
		Global.Context.InputHandler.onStartBinding.AddListener(OnStartBinding);
	}

	public void StopListening()
	{
		Global.Context.InputHandler.onStartBinding.RemoveListener(OnStartBinding);
	}

	private void OnStartBinding(UIBindingItem item)
	{
		inputNameText.text = item.inputKey.name;
		base.gameObject.SetActive(true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}
}

}
