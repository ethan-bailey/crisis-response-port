namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIMarksmanIndicator : MonoBehaviour
{
	public Image icon;

	public Sprite holdSprite;

	public Sprite engageOneSprite;

	public Sprite engangeFreelySprite;

	private void Start()
	{
		if (Global.Context.Mission.Marksman == null)
		{
			base.gameObject.SetActive(false);
		}
	}

	public void SetMode(Marksman.ComputerMode mode)
	{
		switch (mode)
		{
		case Marksman.ComputerMode.Hold:
			icon.sprite = holdSprite;
			break;
		case Marksman.ComputerMode.Engage:
			icon.sprite = engageOneSprite;
			break;
		case Marksman.ComputerMode.Free:
			icon.sprite = engangeFreelySprite;
			break;
		}
	}
}

}
