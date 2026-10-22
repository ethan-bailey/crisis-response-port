namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class TutorialMode : MonoBehaviour
{
	public Tutorial operatorTutorial;

	public Tutorial teamTutorial;

	public Tutorial marksmanTutorial;

	private Tutorial.Mode scheduled;

	public Tutorial Tutorial { get; private set; }

	public bool Active
	{
		get
		{
			return Tutorial != null;
		}
	}

	private void Awake()
	{
		switch (Global.Context.MissionMetaData.tutorial)
		{
		case Tutorial.Mode.Operator:
			if (operatorTutorial != null)
			{
				Tutorial = operatorTutorial;
				operatorTutorial.gameObject.SetActive(true);
			}
			break;
		case Tutorial.Mode.Team:
			if (teamTutorial != null)
			{
				Tutorial = teamTutorial;
				teamTutorial.gameObject.SetActive(true);
			}
			break;
		case Tutorial.Mode.Marksman:
			if (marksmanTutorial != null)
			{
				Tutorial = marksmanTutorial;
				marksmanTutorial.gameObject.SetActive(true);
			}
			break;
		}
		Global.Context.Mission.marksmen.SetMarksmanDisabledForTutorial(Active && !Tutorial.marksmanEnabled);
	}

	public void Schedule(Tutorial.Mode mode)
	{
		scheduled = mode;
	}

	public void Deativate()
	{
		Tutorial = null;
		scheduled = Tutorial.Mode.None;
	}

	public bool MarksmanEnabled()
	{
		Tutorial tutorial = null;
		switch (scheduled)
		{
		case Tutorial.Mode.Operator:
			tutorial = operatorTutorial;
			break;
		case Tutorial.Mode.Team:
			tutorial = teamTutorial;
			break;
		case Tutorial.Mode.Marksman:
			tutorial = marksmanTutorial;
			break;
		}
		return tutorial != null && tutorial.marksmanEnabled;
	}
}

}
