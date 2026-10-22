namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIResumeControlBanner : MonoBehaviour
{
	private enum Mode
	{
		ResumeControl = 0,
		ExtractionZone = 1
	}

	public GameObject banner;

	public Text text;

	private InputHandler input;

	private PauseMenu pauseMenu;

	private Operators operators;

	private MissionEndMenu endMenu;

	private List<CharacterBody> operatorBodies = new List<CharacterBody>();

	private InteriorCamera interiorCamera;

	private static float updateInterval = 0.5f;

	private float lastUpdate = -100f;

	private bool active;

	private void Start()
	{
		input = Global.Context.InputHandler;
		operators = Global.Context.Mission.Operators;
		pauseMenu = Global.Context.Mission.UI.PauseMenu;
		endMenu = Global.Context.Mission.UI.missionEndMenu;
		operatorBodies = Global.Context.Mission.Operators.Bodies;
		interiorCamera = Global.Context.Mission.InteriorMode.interiorCamera;
	}

	private void Update()
	{
		if (active)
		{
			if (interiorCamera != null)
			{
				if (input.Keys.left.Down)
				{
					interiorCamera.ChangeSubject(InteriorCamera.ChangeSubjectDirection.Previous, true);
				}
				else if (input.Keys.right.Down)
				{
					interiorCamera.ChangeSubject(InteriorCamera.ChangeSubjectDirection.Next, true);
				}
			}
			if (input.Keys.interact.Down)
			{
				input.Keys.interact.DownHandled = true;
				CharacterBody characterBody = NextAvailableOperator();
				if (characterBody != null)
				{
					operators.PlayerController = characterBody.gameObject.AddComponent<PlayerController>();
					interiorCamera.Subject = characterBody;
				}
				return;
			}
		}
		if (!(Time.realtimeSinceStartup - lastUpdate < updateInterval))
		{
			lastUpdate = Time.realtimeSinceStartup;
			CharacterBody characterBody2 = NextAvailableOperator();
			active = !pauseMenu.gameObject.activeInHierarchy && !endMenu.Visible && operators.PlayerController == null && characterBody2 != null;
			if (active)
			{
				text.text = string.Format("Press {0} to take control", input.Keys.interact.KeyName);
			}
			banner.SetActive(active);
		}
	}

	private CharacterBody NextAvailableOperator()
	{
		foreach (CharacterBody operatorBody in operatorBodies)
		{
			if (!operatorBody.IsRagdoll)
			{
				return operatorBody;
			}
		}
		return null;
	}
}

}
