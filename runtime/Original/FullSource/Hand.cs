namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class Hand : MonoBehaviour
{
	public enum Mode
	{
		Trigger = 0,
		Fist = 1,
		ForwardAssist = 2
	}

	public Transform grabPoint;

	public GameObject fist;

	public GameObject trigger;

	public GameObject forwardAssist;

	public Mode CurrentMode { get; private set; }

	public void SetMode(Mode mode)
	{
		if (mode != CurrentMode)
		{
			CurrentMode = mode;
			switch (mode)
			{
			case Mode.Trigger:
				forwardAssist.SetActive(false);
				fist.SetActive(false);
				trigger.SetActive(true);
				break;
			case Mode.Fist:
				forwardAssist.SetActive(false);
				trigger.SetActive(false);
				fist.SetActive(true);
				break;
			case Mode.ForwardAssist:
				fist.SetActive(false);
				trigger.SetActive(false);
				forwardAssist.SetActive(true);
				break;
			}
		}
	}
}

}
