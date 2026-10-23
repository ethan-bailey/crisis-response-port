using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Operators
{
	[TaskCategory("Operator")]
	public class HandleDoor : Action
	{
		private enum Sequence
		{
			MoveToDoor = 0,
			ReadyStun = 1,
			WaitUntilReady = 2,
			OpenDoorSlightly = 3,
			ThrowStun = 4,
			ThrowFinished = 5,
			WaitForDetonation = 6,
			OpenDoor = 7,
			WaitUntilOpen = 8,
			MovePast = 9,
			Finished = 10
		}

		public SharedDoor sharedDoor;

		public SharedGameObject collision;

		private TeamOperator self;

		private CharacterBody body;

		private TeamController team;

		private DoorController door;

		private Sequence step;

		private bool useStun;

		private bool doorAlreadyOpen;

		private float direction;

		private float doorOpenAngle;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
			body = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			door = sharedDoor.Value;
			if (!(door == null))
			{
				team = self.Team;
				useStun = team.UseStunGrenades && body.Equipment.stunGrenades > 0 && !door.RoomOppositeFrom(body.Position).cleared;
				doorOpenAngle = ((team.tempo != CharacterBody.Tempo.Quick) ? 90f : 45f);
				doorAlreadyOpen = door.Angle >= 90f;
				direction = Mathf.Sign(sharedDoor.Value.transform.position.x - body.Position);
				float num = Time.time - self.Blackboard.ThrowStunTimestamp;
				if (num > 1.5f || float.IsInfinity(num))
				{
					step = Sequence.MoveToDoor;
				}
				else
				{
					step = ((!(num > 1.2f)) ? Sequence.WaitForDetonation : Sequence.OpenDoor);
				}
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (door == null)
			{
				return TaskStatus.Failure;
			}
			float x = door.transform.position.x;
			if (Mathf.Abs(self.Team.targetPosition - x) < 0.5f)
			{
				return TaskStatus.Failure;
			}
			if ((body.Direction != 1f) ? (body.Position < x) : (body.Position > x))
			{
				if (body.IsHoldingGrenade)
				{
					body.armController.ReturnGrenade();
				}
				return TaskStatus.Failure;
			}
			switch (step)
			{
			case Sequence.MoveToDoor:
			{
				bool flag = collision.Value != null && collision.Value == door.physicalDoor;
				float num3 = door.transform.position.x - direction * ((!doorAlreadyOpen) ? 0.5f : 2.5f);
				bool flag2 = ((body.Direction != 1f) ? (body.Position < num3) : (body.Position > num3));
				if (flag || flag2)
				{
					step = (useStun ? Sequence.ReadyStun : Sequence.OpenDoor);
				}
				else
				{
					body.Move(direction);
				}
				break;
			}
			case Sequence.ReadyStun:
				if (!body.IsHoldingGrenade && !body.armController.IsPlaying)
				{
					body.armController.ReadyGrenade(Grenade.Type.Stun);
				}
				if (body.IsHoldingGrenade && !body.armController.IsPlaying)
				{
					step++;
				}
				break;
			case Sequence.WaitUntilReady:
				if (!body.armController.IsPlaying)
				{
					if (body.InstantiateGrenadeInHand(Grenade.Type.Stun) == null)
					{
						return TaskStatus.Failure;
					}
					step = ((!doorAlreadyOpen) ? Sequence.OpenDoorSlightly : Sequence.ThrowStun);
				}
				break;
			case Sequence.OpenDoorSlightly:
				if (door.Angle >= 15f && door.Angle < 30f)
				{
					step++;
				}
				if (door.lockState == DoorLockState.Locked)
				{
					door.Unlock(body);
				}
				if (door.openState != DoorOpenState.Opening)
				{
					float num2 = Mathf.Sign(20f - door.Angle);
					door.MovePartial(body, 3f * num2);
				}
				break;
			case Sequence.ThrowStun:
				body.InitiateGrenadeThrow();
				step++;
				break;
			case Sequence.ThrowFinished:
				if (body.Blackboard.ThrownGrenade != null)
				{
					step++;
				}
				break;
			case Sequence.WaitForDetonation:
				if (Time.time - self.Blackboard.ThrowStunTimestamp > 1.2f)
				{
					step = ((!doorAlreadyOpen) ? Sequence.OpenDoor : Sequence.MovePast);
				}
				break;
			case Sequence.OpenDoor:
				if (door.Angle >= 90f)
				{
					step = Sequence.MovePast;
					break;
				}
				door.Open(body);
				step = Sequence.WaitUntilOpen;
				break;
			case Sequence.WaitUntilOpen:
				if (door.Angle >= doorOpenAngle)
				{
					step++;
				}
				break;
			case Sequence.MovePast:
			{
				body.Move(direction);
				float num = door.transform.position.x + direction * 0.5f;
				if (Mathf.Abs(num - body.Position) <= 0.1f)
				{
					step++;
				}
				break;
			}
			case Sequence.Finished:
				return TaskStatus.Success;
			}
			return TaskStatus.Running;
		}
	}
}
