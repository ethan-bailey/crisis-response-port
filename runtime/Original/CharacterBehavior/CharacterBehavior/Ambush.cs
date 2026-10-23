using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Occupant")]
	public class Ambush : Action
	{
		private enum Action
		{
			Attack = 0,
			Ambush = 1
		}

		private CharacterBody self;

		private Action action;

		private DoorController ambushDoor;

		private float sourcePosition;

		private float targetPosition;

		private bool sprintFirstPart;

		private bool sprintAllTheWay;

		private bool isWaiting;

		private bool isFiring;

		private bool wasKneeling;

		private bool willKneel;

		private bool fireWhenDoorMoves;

		private bool fireWhenDoorUnlocks;

		private float startWaitTimestamp;

		private float waitPeriod;

		private float fireTimestamp;

		private float firePeriod;

		private float aimpointY;

		private float aimpointTimestamp;

		private float aimpointPeriod;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			action = ((!(Random.value < self.Blackboard.aggression * 0.75f)) ? Action.Ambush : Action.Attack);
			sourcePosition = self.Blackboard.AudioStimuli.position;
			wasKneeling = self.IsKneeling;
			sprintFirstPart = self.Blackboard.aggression >= Random.value;
			sprintAllTheWay = self.Blackboard.aggression * 0.5f >= Random.value;
			isWaiting = (isFiring = false);
			aimpointY = 1.5f;
			switch (action)
			{
			case Action.Attack:
				targetPosition = sourcePosition;
				break;
			case Action.Ambush:
			{
				float f = sourcePosition - self.Position;
				float num = Mathf.Sign(f);
				Room room = ((!(Mathf.Abs(f) < 10f)) ? self.Mission.RoomAtPosition(sourcePosition - num * 10f) : self.Room);
				if (room != null)
				{
					ambushDoor = room.NextDoor(num);
					targetPosition = room.NextEdge(num) - num * Random.Range(1f, 4f);
				}
				willKneel = Random.value >= 0.25f;
				fireWhenDoorMoves = self.Blackboard.aggression < Random.value;
				fireWhenDoorUnlocks = self.Blackboard.aggression * 0.85f < Random.value;
				break;
			}
			}
		}

		public override void OnEnd()
		{
			self.IsKneeling = wasKneeling;
			self.WeaponAimer.Mode = WeaponAimer.AimMode.Ready;
		}

		public override TaskStatus OnUpdate()
		{
			switch (action)
			{
			case Action.Attack:
				return RunAttack();
			case Action.Ambush:
				return RunAmbush();
			default:
				return TaskStatus.Failure;
			}
		}

		private TaskStatus RunAttack()
		{
			float f = targetPosition - self.Position;
			float num = Mathf.Abs(f);
			bool flag = sprintAllTheWay || (sprintFirstPart && num > 10f);
			if (StaticBehavior.OccupantHandleDoorWhileMoving(self, targetPosition, flag, 2f) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			float direction = Mathf.Sign(f);
			if (num > 0.5f)
			{
				if (!self.Facing(targetPosition))
				{
					StaticBehavior.TurnAround(self);
					return TaskStatus.Running;
				}
				self.Move(direction);
				self.IsSprinting = flag;
				return TaskStatus.Running;
			}
			return TaskStatus.Success;
		}

		private TaskStatus RunAmbush()
		{
			float f = targetPosition - self.Position;
			float num = Mathf.Abs(f);
			bool flag = sprintAllTheWay || (sprintFirstPart && num > 10f);
			if (StaticBehavior.OccupantHandleDoorWhileMoving(self, targetPosition, flag, 2f) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			float direction = Mathf.Sign(f);
			if (num > 0.5f)
			{
				if (!self.Facing(targetPosition))
				{
					StaticBehavior.TurnAround(self);
					return TaskStatus.Running;
				}
				self.Move(direction);
				self.IsSprinting = flag;
				return TaskStatus.Running;
			}
			if (!isWaiting)
			{
				isWaiting = true;
				waitPeriod = Random.Range(15f, 60f);
				startWaitTimestamp = Time.time;
				return TaskStatus.Running;
			}
			if (Time.time - startWaitTimestamp >= waitPeriod)
			{
				return TaskStatus.Failure;
			}
			return WaitInAmbush();
		}

		private TaskStatus WaitInAmbush()
		{
			if (!self.Facing(sourcePosition))
			{
				StaticBehavior.TurnAround(self);
				return TaskStatus.Running;
			}
			self.IsKneeling = willKneel;
			self.WeaponAimer.Mode = WeaponAimer.AimMode.AtWorldPoint;
			self.WeaponAimer.Point = new Vector3(self.Position + self.Direction * 3f, aimpointY, -1.5f);
			if (ambushDoor != null && ((fireWhenDoorMoves && ambushDoor.IsMoving) || (fireWhenDoorUnlocks && ambushDoor.lockState == DoorLockState.Unlocking)))
			{
				if (Time.time - aimpointTimestamp >= aimpointPeriod)
				{
					aimpointY = Random.Range(0.5f, 1.8f);
					aimpointTimestamp = Time.time;
					aimpointPeriod = Random.Range(0.3f, 1.5f);
				}
				if (!isFiring)
				{
					isFiring = true;
					fireTimestamp = Time.time;
					firePeriod = Random.Range(self.Blackboard.aggression * 0.5f, self.Blackboard.aggression * 2f);
				}
				else
				{
					if (!(Time.time - fireTimestamp < firePeriod))
					{
						return TaskStatus.Failure;
					}
					self.IsUsingPrecision = false;
					self.PullTrigger();
				}
			}
			return TaskStatus.Running;
		}
	}
}
