namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class MissionMetaData : MonoBehaviour
{
	public Tutorial.Mode tutorial;

	public bool tutorialActive
	{
		get
		{
			return tutorial != Tutorial.Mode.None;
		}
	}

	public void ActivateTutorial(Tutorial.Mode mode)
	{
		tutorial = mode;
	}

	public void DeactivateTutorial()
	{
		tutorial = Tutorial.Mode.None;
	}
}

}
