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
	public class Lead : Action
	{
		private TeamOperator self;

		private CharacterBody body;

		private const float ObstacleClearance = 1f;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
			body = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			Room currentRoom = self.CurrentRoom;
			if (currentRoom == null)
			{
				return TaskStatus.Running;
			}
			DoorController doorController = currentRoom.NextDoor(body.Direction);
			float targetPosition = self.Team.targetPosition;
			float f = targetPosition - body.Position;
			float num = Mathf.Abs(f);
			float num2 = Mathf.Sign(f);
			float num3 = ((!(num < 2.5f)) ? num2 : 1f);
			if (self.Leader != null)
			{
				return TaskStatus.Success;
			}
			if (doorController != null && HandleDoor(doorController, currentRoom, targetPosition) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			if (num <= ((!body.IsSprinting) ? 0.1f : 0.5f))
			{
				if (body.Direction != 1f)
				{
					StaticBehavior.TurnAround(body);
				}
				return TaskStatus.Running;
			}
			if (num3 != body.Direction && StaticBehavior.TurnAround(body) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			if (self.Follower != null && Mathf.Abs(self.Follower.Body.Position - body.Position) > ((!body.IsSprinting) ? 2f : 5f))
			{
				return TaskStatus.Running;
			}
			bool isSprinting = self.Team.tempo == CharacterBody.Tempo.Normal || (self.Team.tempo == CharacterBody.Tempo.Quick && num > 1.5f);
			body.IsUsingPrecision = self.Team.tempo == CharacterBody.Tempo.Normal;
			body.IsSprinting = isSprinting;
			body.Move(num2);
			return TaskStatus.Running;
		}

		private bool IsBlocked(float direction)
		{
			Ray ray = new Ray(new Vector3(self.transform.position.x - direction, 1f, self.transform.position.z), Vector3.right * direction);
			return Physics.Raycast(ray, out body.raycastHit, 1f + direction, Crisis.Runtime.FullSource.Masks.Instance.BallisticTargetsSansLimbs);
		}

		private TaskStatus HandleDoor(DoorController door, Room room, float targetPosition)
		{
			if (door.Angle >= 90f)
			{
				return TaskStatus.Success;
			}
			if ((body.Direction == 1f && targetPosition < door.Position) || (body.Direction == -1f && targetPosition > door.Position))
			{
				return TaskStatus.Success;
			}
			float f = door.Position - body.Position;
			float direction = Mathf.Sign(f);
			float num = Mathf.Abs(f);
			if (num > ((!body.IsSprinting) ? 1.5f : 3f))
			{
				return TaskStatus.Success;
			}
			if (self.Blackboard.LastCollider != door.physicalDoor)
			{
				body.IsSprinting = false;
				body.Move(direction);
			}
			else
			{
				if (door.lockState == DoorLockState.Locked || door.lockState == DoorLockState.Unlocking)
				{
					if (door.lockState == DoorLockState.Locked)
					{
						door.Unlock(body);
					}
					return TaskStatus.Running;
				}
				float direction2 = Mathf.Sign(targetPosition - body.Position);
				if (DeliverStunGrenade(door, room.NextRoom(direction2)) != TaskStatus.Success)
				{
					return TaskStatus.Running;
				}
				if (self.Team.tempo == CharacterBody.Tempo.Slow && door.openState == DoorOpenState.Closed)
				{
					door.MovePartial(body, 3f);
				}
				else if (door.openState != DoorOpenState.Opening)
				{
					door.Open(body);
				}
			}
			return TaskStatus.Running;
		}

		private TaskStatus DeliverStunGrenade(DoorController door, Room room)
		{
			if (!(self.Blackboard.ThrownGrenade != null) && (room == null || room.cleared || room.HasBeenStunned || !self.Team.UseStunGrenades || body.Equipment.stunGrenades == 0 || Mathf.Abs(body.Position - body.Blackboard.ThrowStunPosition) < 2f))
			{
				return TaskStatus.Success;
			}
			if (self.Blackboard.ThrownGrenade == null && !body.armController.IsThrowingGrenade)
			{
				if (!body.IsHoldingGrenade && !body.armController.IsPlaying)
				{
					body.armController.ReadyGrenade(Grenade.Type.Stun);
					return TaskStatus.Running;
				}
				if (door.Angle < 20f || door.Angle > 25f)
				{
					float num = Mathf.Sign(22.5f - door.Angle);
					door.MovePartial(body, 3f * num);
					return TaskStatus.Running;
				}
				if (!body.armController.IsThrowingGrenade)
				{
					body.InitiateGrenadeThrow();
				}
				return TaskStatus.Running;
			}
			if (self.Blackboard.StunDetonateTimestamp > 2f)
			{
				return TaskStatus.Running;
			}
			self.Blackboard.ThrowStunPosition = body.Position;
			room.HasBeenStunned = true;
			return TaskStatus.Success;
		}

		public override void OnEnd()
		{
			if (body.IsHoldingGrenade)
			{
				body.armController.ReturnGrenade();
			}
		}
	}
}
