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
	[TaskCategory("Occupant")]
	public class IdleMove : Action
	{
		private enum State
		{
			Move = 0,
			Wait = 1
		}

		private State state;

		private CharacterBody self;

		private Occupant occupant;

		private float movePosition;

		private float waitTimestamp;

		private float waitPeriod;

		private bool lockDoors;

		private const float Epsilon = 0.1f;

		private const float MinWaitPeriod = 3f;

		private const float MaxWaitPeriod = 10f;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			occupant = gameObject.GetComponent<Occupant>();
			lockDoors = !Global.Context.Settings.Extras.disableLocks;
		}

		public override void OnStart()
		{
			SetNextPosition();
		}

		public override TaskStatus OnUpdate()
		{
			switch (state)
			{
			case State.Move:
				if (MoveToPosition() == TaskStatus.Success)
				{
					state = State.Wait;
					waitTimestamp = Time.time;
					waitPeriod = Random.Range(3f, 10f);
				}
				return TaskStatus.Running;
			case State.Wait:
				if (Time.time - waitTimestamp >= waitPeriod)
				{
					SetNextPosition();
				}
				return TaskStatus.Running;
			default:
				return TaskStatus.Failure;
			}
		}

		private TaskStatus MoveToPosition()
		{
			if (Mathf.Abs(movePosition - self.Position) <= 0.1f)
			{
				return TaskStatus.Success;
			}
			if (StaticBehavior.OccupantHandleDoorWhileMoving(self, movePosition, false, 2f) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			if (StaticBehavior.OccupantCloseDoorWhileMoving(self, movePosition, false, lockDoors) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			if (!self.Facing(movePosition))
			{
				StaticBehavior.TurnAround(self);
			}
			self.IsSprinting = false;
			self.Move(Mathf.Sign(movePosition - self.Position));
			return TaskStatus.Running;
		}

		private void SetNextPosition()
		{
			float num = Random.Range(occupant.patrolAreaLeft, occupant.patrolAreaRight);
			float f = num - self.Position;
			float num2 = Mathf.Abs(f);
			float num3 = Mathf.Sign(f);
			Ray ray = new Ray(self.Pelvis.transform.position - Vector3.right * num3 * 0.5f, Vector3.right * num3);
			RaycastHit hitInfo = default(RaycastHit);
			bool flag = Physics.Raycast(ray, out hitInfo, num2 + 1f, Crisis.Runtime.FullSource.Masks.Instance.ObstaclesProps);
			movePosition = ((!flag) ? num : (hitInfo.point.x - num3 * -1f));
			state = State.Move;
		}
	}
}
