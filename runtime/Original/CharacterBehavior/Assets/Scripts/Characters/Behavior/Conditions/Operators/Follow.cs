using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Operators
{
	[TaskCategory("Operator")]
	public class Follow : Action
	{
		private TeamOperator self;

		private CharacterBody body;

		private bool catchingUp;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
			body = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			catchingUp = false;
		}

		public override TaskStatus OnUpdate()
		{
			Room currentRoom = self.CurrentRoom;
			if (currentRoom == null)
			{
				return TaskStatus.Running;
			}
			DoorController doorController = currentRoom.NextDoor(body.Direction);
			TeamOperator leader = self.Leader;
			if (leader == null)
			{
				return TaskStatus.Failure;
			}
			float num = TargetPosition(leader);
			float num2 = Mathf.Sign(leader.Body.Position - body.Position);
			float f = num - body.Position;
			float num3 = Mathf.Abs(f);
			if (((!self.IsRearGuard) ? (body.Direction != num2) : (num2 * ((!(num3 > 1f)) ? (-1f) : 1f) != body.Direction)) && StaticBehavior.TurnAround(body) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			if (doorController != null && StaticBehavior.OperatorHandleDoor(self, doorController, num, false) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			float num4 = ((self.Team.tempo != CharacterBody.Tempo.Slow) ? 2f : 1.5f);
			if (num3 > num4)
			{
				catchingUp = true;
			}
			if (num3 > 0.1f)
			{
				body.IsSprinting = catchingUp || leader.Body.IsSprinting;
				body.IsUsingPrecision = leader.Body.IsUsingPrecision;
				body.Move(Mathf.Sign(f));
				if (num3 < 1f)
				{
					catchingUp = false;
				}
			}
			return TaskStatus.Running;
		}

		private float TargetPosition(TeamOperator leader)
		{
			float result = leader.Body.Position - self.FollowDistance(leader) * leader.Body.Direction;
			if (self.TeamPosition == 2 && self.Team.Leader.FacingDoor)
			{
				DoorController door = self.Team.Leader.Door;
				float num = Mathf.Sign(door.transform.position.x - body.Position);
				if (num == door.OpeningDirection)
				{
					result = leader.Body.Position - self.FollowDistance(leader) * leader.Body.Direction;
				}
				else
				{
					float num2 = Mathf.Max(0.5f, (num != 1f) ? door.OpenClearanceRight : door.OpenClearanceLeft);
					float num3 = 0.25f + ((!(body.ActiveWeapon != null)) ? 0f : body.ActiveWeapon.Length());
					result = door.transform.position.x - (num2 + num3) * num;
				}
			}
			return result;
		}
	}
}
