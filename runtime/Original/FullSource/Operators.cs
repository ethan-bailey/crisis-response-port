namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Operators : MonoBehaviour
{
	public UnityEventVoid allDisabledEvent = new UnityEventVoid();

	private UIInteriorModeTextIndicator indicatorUpperLeft;

	private UIInteriorModeTextIndicator indicatorUpperRight;

	private List<CharacterBody> bodies = new List<CharacterBody>();

	private UIMissionStatus status;

	private InputHandler input;

	private Marksman marksman;

	private Mission mission;

	private bool allDisabledEventFired;

	public Transform Container { get; private set; }

	public CharacterBody First
	{
		get
		{
			return (bodies.Count != 0) ? bodies[0] : null;
		}
	}

	public TeamController MainGroup { get; private set; }

	public TeamController SupportGroup { get; private set; }

	public List<CharacterBody> Bodies
	{
		get
		{
			return bodies;
		}
	}

	public PlayerController PlayerController { get; set; }

	public EngagementRules EngagementRules { get; private set; }

	public bool AllDisabled { get; private set; }

	private void Awake()
	{
		GlobalContext context = Global.Context;
		input = context.InputHandler;
		mission = context.Mission;
		Container = new GameObject("Operators").transform;
		Container.SetParent(mission.Containers, true);
		MainGroup = Container.gameObject.AddComponent<TeamController>();
		status = mission.UI.missionStatusIndicator;
		indicatorUpperLeft = mission.UI.indicatorUpperLeft;
		indicatorUpperRight = mission.UI.indicatorUpperRight;
		GameObject gameObject = new GameObject("Task group", typeof(TeamController));
		gameObject.transform.SetParent(base.transform.parent);
		SupportGroup = gameObject.GetComponent<TeamController>();
		mission.onMissionBegin.AddListener(OnMissionBegin);
	}

	private void Start()
	{
		marksman = mission.Marksman;
	}

	private void Update()
	{
		if (input.Keys.teamMoveTo.Down)
		{
			input.Keys.teamMoveTo.DownHandled = true;
			CommandMoveTo();
		}
		else if (input.Keys.teamClearNext.Down)
		{
			input.Keys.teamClearNext.DownHandled = true;
			CommandClearNextRoom();
		}
		else if (input.Keys.teamMoveOut.Down)
		{
			input.Keys.teamMoveOut.DownHandled = true;
			CommandMoveOut();
		}
		else if (input.Keys.teamHold.Down)
		{
			input.Keys.teamHold.DownHandled = true;
			CommandHoldPosition();
		}
		else if (input.Keys.teamFollow.Down)
		{
			input.Keys.teamFollow.DownHandled = true;
			CommandFollowMe();
		}
		else if (input.Keys.teamIncreaseTempo.Down)
		{
			input.Keys.teamIncreaseTempo.DownHandled = true;
			CommandIncreaseTempo();
		}
		else if (input.Keys.teamDecreaseTempo.Down)
		{
			input.Keys.teamDecreaseTempo.DownHandled = true;
			CommandDecreaseTempo();
		}
		else if (input.Keys.teamToggleStun.Down)
		{
			input.Keys.teamToggleStun.DownHandled = true;
			CommandToggleStunUsage();
		}
		if (marksman != null)
		{
			if (input.Keys.marksmanHold.Down)
			{
				marksman.ComputerCommand(Marksman.ComputerMode.Hold);
				input.Keys.marksmanHold.DownHandled = true;
			}
			else if (input.Keys.marksmanEngage.Down)
			{
				marksman.ComputerCommand(Marksman.ComputerMode.Engage);
				input.Keys.marksmanEngage.DownHandled = true;
			}
			else if (input.Keys.marksmanFree.Down)
			{
				marksman.ComputerCommand(Marksman.ComputerMode.Free);
				input.Keys.marksmanFree.DownHandled = true;
			}
		}
		else if (input.Keys.marksmanHold.Down || input.Keys.marksmanEngage.Down || input.Keys.marksmanFree.Down)
		{
			indicatorUpperRight.Indicate("No marksman", 2f);
		}
		if (!allDisabledEventFired && MainGroup.AllDisabled && (SupportGroup == null || SupportGroup.AllDisabled))
		{
			allDisabledEventFired = true;
			allDisabledEvent.Invoke();
		}
		if (input.Keys.objectives.Down)
		{
			status.Indicate(3f, false);
		}
	}

	private void OnMissionBegin()
	{
		SerializableTeamBehavior serializableTeamBehavior = ((!(mission.TutorialMode != null) || !mission.TutorialMode.Active) ? Global.Context.OperatorSpawnData.Team.behavior : Global.Context.Mission.TutorialMode.Tutorial.Behavior());
		switch (serializableTeamBehavior.teamOrder)
		{
		case TeamController.Command.MoveOut:
			CommandMoveOut(true);
			break;
		case TeamController.Command.HoldPosition:
			CommandHoldPosition();
			break;
		case TeamController.Command.FollowMe:
			CommandFollowMe();
			break;
		}
		CommandSetTempo(serializableTeamBehavior.teamTempo);
		SetStunUsage(serializableTeamBehavior.teamUseStunGrenades);
		AssignPlayerControl();
	}

	public void StandDown()
	{
		Container.gameObject.SetActive(false);
	}

	public void AssignPlayerControl()
	{
		SerializableExtraSettings extras = Global.Context.Settings.Extras;
		if (((bool)mission.TutorialMode && mission.TutorialMode.Active) || !extras.playerSpectator)
		{
			if (First != null)
			{
				GameObject gameObject = First.gameObject;
				PlayerController playerController = gameObject.gameObject.AddComponent<PlayerController>();
				playerController.Body.Invisible = extras.playerInvisible;
				gameObject.GetComponent<Physiology>().invulnerable = extras.playerInvulnerable;
				playerController.OnMissionBegin();
			}
			else
			{
				Debug.LogWarning("No operator available for player control! One must be ready at mission start.");
			}
		}
	}

	private void IndicateCommand(TeamController.Command command, bool audio = true)
	{
		if (audio && PlayerController != null)
		{
			if (MainGroup.ActiveMemberCount <= 1 && SupportGroup.ActiveMemberCount == 0)
			{
				return;
			}
			Communication component = PlayerController.GetComponent<Communication>();
			switch (command)
			{
			case TeamController.Command.MoveOut:
				component.PlayPhrase(component.moveOut, 0f);
				break;
			case TeamController.Command.ClearRoom:
				component.PlayPhrase(component.moveOut, 0f);
				break;
			case TeamController.Command.MoveThere:
				component.PlayPhrase(component.moveThere, 0f);
				break;
			case TeamController.Command.HoldPosition:
				component.PlayPhrase(component.holdPosition, 0f);
				break;
			case TeamController.Command.FollowMe:
				component.PlayPhrase(component.followMe, 0f);
				break;
			}
		}
		switch (command)
		{
		case TeamController.Command.MoveOut:
			indicatorUpperLeft.Indicate("Move out", 2f, false);
			break;
		case TeamController.Command.ClearRoom:
			indicatorUpperLeft.Indicate("Clear it", 2f, false);
			break;
		case TeamController.Command.MoveThere:
			indicatorUpperLeft.Indicate("Go there", 2f, false);
			break;
		case TeamController.Command.HoldPosition:
			indicatorUpperLeft.Indicate("Hold position", 2f, false);
			break;
		case TeamController.Command.FollowMe:
			indicatorUpperLeft.Indicate("On me", 2f, false);
			break;
		}
	}

	public bool IsPlayer(CharacterBody body)
	{
		return PlayerController.Body == body;
	}

	public TeamController FormTaskGroup()
	{
		return MakeTaskGroup(1, MainGroup.ActiveMemberCount - 1);
	}

	public void RegisterBody(CharacterBody body)
	{
		bodies.Add(body);
	}

	private TeamController MakeTaskGroup(int first, int count)
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in MainGroup.transform)
		{
			if (!item.GetComponent<CharacterBody>().IsRagdoll)
			{
				list.Add(item);
			}
		}
		List<Transform> list2 = new List<Transform>();
		for (int i = 0; i < count; i++)
		{
			int num = first + i;
			if (num > list.Count)
			{
				break;
			}
			list2.Add(list[num]);
		}
		bool flag = true;
		foreach (Transform item2 in list2)
		{
			TeamOperator component = item2.GetComponent<TeamOperator>();
			SupportGroup.AssignAsMember(component);
			if (flag)
			{
				component.Leader = null;
				flag = false;
			}
		}
		return SupportGroup;
	}

	public void MergeGroups()
	{
		if (SupportGroup == null)
		{
			return;
		}
		List<TeamOperator> list = new List<TeamOperator>();
		foreach (Transform item in SupportGroup.transform)
		{
			list.Add(item.GetComponent<TeamOperator>());
		}
		foreach (TeamOperator item2 in list)
		{
			MainGroup.AssignAsMember(item2);
		}
	}

	public void CommandMoveTo()
	{
		float num = 0f;
		if (marksman != null && mission.View == Mission.MissionView.Marksman)
		{
			num = marksman.targetPosition.position.x;
		}
		else
		{
			RaycastHit hitInfo = default(RaycastHit);
			Ray ray = mission.InteriorMode.interiorCamera.interiorCamera.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(ray, out hitInfo, 100f, Masks.Instance.Aim))
			{
				return;
			}
			num = hitInfo.point.x;
		}
		MoveToPosition(num);
	}

	private void MoveToPosition(float position)
	{
		IndicateCommand(TeamController.Command.MoveThere, PlayerController != null);
		if (PlayerController != null)
		{
			FormTaskGroup().CommandMoveTo(position);
		}
		else
		{
			MainGroup.CommandMoveTo(position);
		}
	}

	public void CommandClearNextRoom()
	{
		TeamOperator teamOperator = ((!(SupportGroup.Leader != null)) ? MainGroup.Leader : SupportGroup.Leader);
		if (teamOperator == null)
		{
			return;
		}
		Room room = null;
		if (PlayerController.Body.Direction != teamOperator.Body.Direction)
		{
			teamOperator = PlayerController.Operator;
		}
		float num = ((teamOperator.Body.Direction != 1f) ? teamOperator.Body.Room.RightEdge : teamOperator.Body.Room.LeftEdge);
		room = (((teamOperator.Body.Direction != 1f || !(teamOperator.Body.Position - num < 1f)) && (teamOperator.Body.Direction != -1f || !(num - teamOperator.Body.Position < 1f))) ? teamOperator.NextRoom : teamOperator.CurrentRoom);
		if (room == null)
		{
			return;
		}
		float targetPosition = ((!(teamOperator.Body.Direction > 0f)) ? (room.LeftEdge + 1.5f) : (room.RightEdge - 1.5f));
		IndicateCommand(TeamController.Command.ClearRoom, PlayerController != null);
		if (PlayerController != null)
		{
			TeamController teamController = FormTaskGroup();
			teamController.SetTargetPosition(targetPosition);
			if (teamController.Leader != null)
			{
				Communication communication = teamController.Leader.Communication;
				communication.PlayPhrase((!(Random.value < 0.5f)) ? communication.going : communication.affermative);
			}
		}
		else
		{
			MainGroup.SetTargetPosition(targetPosition);
		}
	}

	public void CommandMoveOut(bool overrideCursor = false)
	{
		IndicateCommand(TeamController.Command.MoveOut, PlayerController != null);
		if (PlayerController != null)
		{
			FormTaskGroup().CommandMoveOut(PlayerController.Body.Direction);
			return;
		}
		float direction = 1f;
		if (!overrideCursor)
		{
			CharacterBody characterBody = ((!(MainGroup.Leader != null)) ? null : MainGroup.Leader.Body);
			if (characterBody == null)
			{
				characterBody = ((!(SupportGroup.Leader != null)) ? null : SupportGroup.Leader.Body);
			}
			float num = ((!(characterBody != null)) ? mission.EndPosition : characterBody.Position);
			if (marksman != null && mission.View == Mission.MissionView.Marksman)
			{
				direction = Mathf.Sign(marksman.targetPosition.position.x - num);
			}
			else
			{
				RaycastHit hitInfo = default(RaycastHit);
				Ray ray = mission.InteriorMode.interiorCamera.interiorCamera.ScreenPointToRay(Input.mousePosition);
				if (!Physics.Raycast(ray, out hitInfo, 100f, Masks.Instance.Aim))
				{
					return;
				}
				direction = Mathf.Sign(hitInfo.point.x - num);
			}
		}
		MainGroup.CommandMoveOut(direction);
	}

	public void CommandFollowMe()
	{
		if (SupportGroup.Leader != null)
		{
			Communication communication = SupportGroup.Leader.Communication;
			communication.PlayPhrase(communication.affermative);
		}
		MergeGroups();
		if (PlayerController != null)
		{
			IndicateCommand(TeamController.Command.FollowMe);
		}
	}

	public void CommandHoldPosition()
	{
		IndicateCommand(TeamController.Command.HoldPosition, PlayerController != null);
		if (PlayerController != null)
		{
			FormTaskGroup().CommandHoldPosition();
		}
		else
		{
			MainGroup.CommandHoldPosition();
		}
	}

	public void CommandSetTempo(CharacterBody.Tempo tempo)
	{
		MainGroup.CommandSetTempo(tempo);
		SupportGroup.CommandSetTempo(tempo);
	}

	public void CommandIncreaseTempo()
	{
		MainGroup.CommandIncreaseTempo();
		SupportGroup.CommandIncreaseTempo();
		IndicateTempo(MainGroup.tempo);
	}

	public void CommandDecreaseTempo()
	{
		MainGroup.CommandDecreaseTempo();
		SupportGroup.CommandDecreaseTempo();
		IndicateTempo(MainGroup.tempo);
	}

	public void CommandToggleStunUsage()
	{
		SetStunUsage(!MainGroup.UseStunGrenades);
		indicatorUpperLeft.Indicate((!MainGroup.UseStunGrenades) ? "No stun" : "Use stun");
	}

	public void SetStunUsage(bool use)
	{
		MainGroup.SetStunGrenadeUsage(use);
		SupportGroup.SetStunGrenadeUsage(use);
	}

	private void IndicateTempo(CharacterBody.Tempo tempo)
	{
		string empty = string.Empty;
		switch (tempo)
		{
		case CharacterBody.Tempo.Quick:
			empty = "Make haste";
			break;
		case CharacterBody.Tempo.Slow:
			empty = "Move carefully";
			break;
		default:
			empty = "Normal pace";
			break;
		}
		indicatorUpperLeft.Indicate(empty, 3f, false);
	}
}

}
