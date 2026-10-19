namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class TeamController : MonoBehaviour
{
	public enum Command
	{
		MoveThere = 0,
		ClearRoom = 1,
		MoveOut = 2,
		HoldPosition = 3,
		FollowMe = 4
	}

	public const float NormalDistance = 1f;

	public const float KneelDistance = 0.6f;

	public const float CoverDistance = 0.5f;

	public const float StackDistance = 0.55f;

	public const float EngageDistance = 2.5f;

	public CharacterBody.Tempo tempo = CharacterBody.Tempo.Normal;

	[Header("Orders")]
	[Tooltip("The operator leading the team will move toward this position.")]
	public float targetPosition = 30f;

	[Tooltip("The operator leading the team will hold the current position when this is set.")]
	public bool holdPosition;

	private InputHandler input;

	private UIInteriorModeTextIndicator indicator;

	private List<CharacterBody> bodies = new List<CharacterBody>();

	public Mission Mission { get; private set; }

	public TeamOperator Leader { get; private set; }

	public TeamOperator RearGuard { get; private set; }

	public bool AllDisabled { get; private set; }

	public int ActiveMemberCount { get; private set; }

	public bool UseStunGrenades { get; set; }

	public List<CharacterBody> Bodies
	{
		get
		{
			return bodies;
		}
	}

	public LimbId RoeTargetLimb { get; private set; }

	public bool Empty
	{
		get
		{
			return Bodies.Count == 0;
		}
	}

	private void Start()
	{
		input = Global.Context.InputHandler;
		Mission = Global.Context.Mission;
		indicator = Mission.UI.indicatorUpperLeft;
		InitializeRulesOfEngagement();
		AllDisabled = false;
	}

	private void InitializeRulesOfEngagement()
	{
		switch (Global.Context.OperatorSpawnData.Team.behavior.teamEngageMode)
		{
		case RoePermission.Destroy:
			RoeTargetLimb = LimbId.Head;
			break;
		case RoePermission.Incapacitate:
			RoeTargetLimb = LimbId.Torso;
			break;
		case RoePermission.Preserve:
			RoeTargetLimb = LimbId.RightUpperLeg;
			break;
		}
		IssueRulesOfEngagement();
	}

	private void Update()
	{
		UpdateTeamData();
	}

	public void RegisterOperator(CharacterBody body)
	{
		bodies.Add(body);
	}

	public void CommandMoveTo(float position)
	{
		if (Leader != null && Mathf.Abs(targetPosition - position) > 2f)
		{
			Leader.Communication.PlayPhrase((!(Random.value < 0.5f)) ? Leader.Communication.going : Leader.Communication.affermative);
		}
		SetTargetPosition(position);
	}

	public void CommandMoveOut(float direction)
	{
		float num = ((direction != 1f) ? Mission.StartPosition : Mission.EndPosition);
		SetTargetPosition(num);
		if (Leader != null)
		{
			Leader.Communication.PlayPhrase((!(Random.value < 0.5f)) ? Leader.Communication.going : Leader.Communication.affermative);
		}
	}

	public void CommandHoldPosition()
	{
		holdPosition = true;
		if (Leader != null)
		{
			CharacterBody body = Leader.Body;
			targetPosition = body.Position + body.Direction * body.Velocity;
			Leader.Communication.PlayPhrase(Leader.Communication.affermative);
		}
	}

	public void CommandSetTempo(CharacterBody.Tempo tempo)
	{
		if (this.tempo != tempo)
		{
			this.tempo = tempo;
			ApplyTempo(this.tempo);
		}
	}

	public void CommandIncreaseTempo()
	{
		if (tempo == CharacterBody.Tempo.Slow)
		{
			tempo = CharacterBody.Tempo.Normal;
		}
		else if (tempo == CharacterBody.Tempo.Normal)
		{
			tempo = CharacterBody.Tempo.Quick;
		}
		ApplyTempo(tempo);
	}

	public void CommandDecreaseTempo()
	{
		if (tempo == CharacterBody.Tempo.Quick)
		{
			tempo = CharacterBody.Tempo.Normal;
		}
		else if (tempo == CharacterBody.Tempo.Normal)
		{
			tempo = CharacterBody.Tempo.Slow;
		}
		ApplyTempo(tempo);
	}

	public void SetStunGrenadeUsage(bool use)
	{
		UseStunGrenades = use;
	}

	private void ApplyTempo(CharacterBody.Tempo tempo)
	{
		foreach (Transform item in base.transform)
		{
			CharacterBody component = item.GetComponent<CharacterBody>();
			if (!(component == null))
			{
				component.tempo = tempo;
			}
		}
	}

	private void UpdateMissionState(TeamOperator op)
	{
		foreach (Target enemy in op.Blackboard.Enemies)
		{
			CharacterBody body = enemy.Body;
			if (body != null && !body.ReportedDown && body.IsRagdoll)
			{
				body.ReportedDown = true;
				op.ReportSuspectDown();
			}
		}
		foreach (CharacterBody friend in op.Blackboard.Friends)
		{
			if (friend.IsRagdoll && !friend.ReportedDown)
			{
				friend.ReportedDown = true;
				op.ReportOperatorDown();
			}
		}
		if (!(op.CurrentRoom == null))
		{
			op.CurrentRoom.Clear(op.Body.Direction, op.Body.WorldSpaceFurthestVision());
		}
	}

	private void UpdateTeamData()
	{
		Leader = null;
		RearGuard = null;
		AllDisabled = true;
		ActiveMemberCount = 0;
		TeamOperator teamOperator = null;
		int num = 1;
		foreach (Transform item in base.transform)
		{
			TeamOperator component = item.GetComponent<TeamOperator>();
			if (component.Body.IsRagdoll)
			{
				if (component.Follower != null)
				{
					component.Follower.Leader = null;
				}
				if (teamOperator != null)
				{
					teamOperator.Follower = null;
				}
				continue;
			}
			ActiveMemberCount++;
			component.TeamPosition = num++;
			AllDisabled = false;
			if (teamOperator != null)
			{
				component.Leader = teamOperator;
				teamOperator.Follower = component;
			}
			teamOperator = component;
			if (Leader == null)
			{
				SetGroupLeaderData(component);
			}
		}
		if (RearGuard == null && ActiveMemberCount > 2 && RearGuard != Leader)
		{
			RearGuard = teamOperator;
		}
	}

	private void UpdateTeamMovePosition()
	{
		if (!Mission.AllRoomsChecked)
		{
			if (Leader == null)
			{
				targetPosition = Mission.EndPosition;
			}
			else
			{
				targetPosition = Leader.CurrentRoom.RightEdge - ((!(Leader.NextRoom != null)) ? 0f : 2f);
			}
		}
		else if (!Mission.Occupants.AllRenderedSafe)
		{
			CharacterBody characterBody = Mission.Occupants.ClosestToPosition(Leader.Body.Position);
			targetPosition = ((!(characterBody != null)) ? Mission.StartPosition : characterBody.Position);
		}
		else if (Leader != null)
		{
			targetPosition = Leader.Body.Position;
		}
	}

	private void IssueRulesOfEngagement()
	{
		foreach (Transform item in base.transform)
		{
			item.GetComponent<CharacterBody>().PreferredTargetLimb = RoeTargetLimb;
		}
	}

	private void SetGroupLeaderData(TeamOperator op)
	{
		Leader = op;
		SetLeaderRoomData(op);
		UpdateMissionState(op);
		op.Door = op.Body.FindDoor(2f);
		if (op.Door == null || op.Door.Angle > 70f || !op.Door.InteractorInRange(op.Body))
		{
			op.FacingDoor = false;
			return;
		}
		float num = Mathf.Sign(op.Door.transform.position.x - op.Body.Position);
		op.FacingDoor = num == op.Body.Direction;
	}

	public void SetLeaderRoomData(TeamOperator op)
	{
		if (!(op.Body.Room == null))
		{
			Room room = ((!op.Body.FacingRight) ? op.Body.Room.leftRoom : op.Body.Room.rightRoom);
			op.NextRoom = ((!(room == null)) ? room : null);
		}
	}

	public void SetTargetPosition(float position)
	{
		targetPosition = position;
		holdPosition = false;
	}

	private void BringStunGrenadeUpFront()
	{
		CharacterBody characterBody = null;
		foreach (CharacterBody body in bodies)
		{
			if (body.Equipment.stunGrenades > 0)
			{
				characterBody = body;
				break;
			}
		}
		characterBody.transform.SetSiblingIndex(0);
	}

	public void AssignAsMember(TeamOperator op)
	{
		if (Leader == null)
		{
			Leader = op;
		}
		bool isGroupLeader = op.IsGroupLeader;
		TeamOperator follower = op.Follower;
		op.transform.SetParent(base.transform);
		op.Team = this;
		op.Leader = Leader;
		if (isGroupLeader && follower != null)
		{
			follower.Leader = null;
		}
	}
}

}
