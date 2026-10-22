namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class MarksmanMode : MissionMode
{
	public static float DeflectionModifier = 0.15f;

	public MarksmanCamera marksmanCamera;

	public MarksmanBoltActionVisualization boltActionVisualization;

	public Text entryTeamText;

	private void OnEnable()
	{
		SetCursorState();
	}

	public override void OnResume()
	{
		SetCursorState();
	}

	private void Start()
	{
		if (Global.Context.Mission.InteriorMode != null)
		{
			entryTeamText.text = string.Format("{0}: entry team", Global.Context.InputHandler.Keys.entryTeam.KeyName);
		}
		else
		{
			entryTeamText.gameObject.SetActive(false);
		}
	}

	private void SetCursorState()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}

}
