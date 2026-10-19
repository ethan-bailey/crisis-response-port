namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class Marksmen : MonoBehaviour
{
	public MarksmanMode marksmanModePrefab;

	public Marksman marksmanPrefab;

	public Vector3 upperLeft;

	public Vector3 lowerRight;

	public Vector3 marksmanPosition;

	[Tooltip("The maximum distance marksmen can be positioned at. In meters relative to the position above.")]
	public float maxDistance = 300f;

	[Tooltip("The maximum elevation marksmen can be positioned at. In meters relative to the position above.")]
	public float maxElevation = 10f;

	private Mission mission;

	private bool disabledForTutorial;

	public Marksman Marksman { get; private set; }

	private void Awake()
	{
		mission = Global.Context.Mission;
		SerializableMarksman marksman = Global.Context.OperatorSpawnData.Team.marksman;
		bool flag = marksman.enabled;
		bool flag2 = Global.Context.SceneTransition.CurrentScene == SceneTransition.Scene.Marksman;
		bool tutorialActive = Global.Context.MissionMetaData.tutorialActive;
		bool flag3 = !tutorialActive && !flag && !flag2;
		if (!(marksmanModePrefab == null) && !flag3 && (!tutorialActive || !disabledForTutorial))
		{
			MarksmanMode marksmanMode = Object.Instantiate(marksmanModePrefab);
			marksmanMode.name = marksmanModePrefab.name;
			marksmanMode.Initialize();
			mission.MarksmanMode = marksmanMode;
			Marksman = Object.Instantiate(marksmanPrefab, base.transform);
			Marksman.transform.position = marksmanPosition;
			Marksman.name = string.Format("{0} {1}", marksmanPrefab.name, base.transform.childCount);
			Marksman.Initialize(this);
			mission.Marksman = Marksman;
			SerializableExtraSettings extras = Global.Context.Settings.Extras;
		}
	}

	private void Start()
	{
		if (!(Marksman == null))
		{
			SerializableMarksman marksman = Global.Context.OperatorSpawnData.Team.marksman;
			Vector3 position = ((!Global.Context.MissionMetaData.tutorialActive) ? (marksmanPosition + new Vector3((marksman.normalizedOffset - 0.25f) * mission.LevelLength, marksman.elevation * maxElevation, marksman.normalizedDistance * (0f - maxDistance))) : marksmanPosition);
			Marksman.InitializePosition(position);
		}
	}

	public void SetMarksmanDisabledForTutorial(bool disabled)
	{
		disabledForTutorial = disabled;
	}
}

}
