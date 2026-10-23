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
	public class Flee : Action
	{
		private enum State
		{
			SelectPosition = 0,
			Move = 1,
			Evaluate = 2
		}

		private ActionSource source;

		private CharacterBody self;

		private Occupant occupant;

		private OccupantFear fear;

		private State state;

		private const float Epsilon = 0.1f;

		private float movePosition;

		private bool lockDoors;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			occupant = gameObject.GetComponent<Occupant>();
			fear = gameObject.GetComponent<OccupantFear>();
			state = State.SelectPosition;
			lockDoors = !Global.Context.Settings.Extras.disableLocks;
		}

		public override TaskStatus OnUpdate()
		{
			switch (state)
			{
			case State.SelectPosition:
				return SelectPosition();
			case State.Move:
				return Move();
			case State.Evaluate:
				return Evaluate();
			default:
				return TaskStatus.Running;
			}
		}

		private TaskStatus SelectPosition()
		{
			source = ((fear.driver.TopMeasure == null) ? null : fear.driver.TopMeasure.Source);
			float num = 2f;
			float num2 = 1f;
			float num3 = 0f;
			float value = 0f;
			switch (source.type)
			{
			case ActionSource.Type.Character:
				if (source.transform != null)
				{
					num = Random.Range(2f, 6f);
					num3 = source.transform.position.x;
					num2 = Mathf.Sign(self.Position - num3);
					value = self.Position + num2 * num;
				}
				break;
			case ActionSource.Type.Audio:
			{
				float num4 = 1f - Mathf.Clamp(Mathf.Abs(source.position - self.Position), 1f, 20f) / 20f;
				num = 2f + num4 * 10f;
				num3 = self.Position;
				num2 = Mathf.Sign(self.Position - source.position);
				value = self.Position + num2 * num;
				break;
			}
			case ActionSource.Type.Grenade:
				if (source.transform != null)
				{
					num = Random.Range(4f, 10f);
					num3 = source.transform.position.x;
					num2 = Mathf.Sign(self.Position - num3);
					value = self.Position + num2 * num;
				}
				break;
			case ActionSource.Type.Marksman:
				num = Random.Range(4f, 10f);
				num3 = self.Position;
				num2 = ((!(Random.value > 0.5f)) ? (-1f) : 1f);
				value = self.Position + num2 * num;
				break;
			case ActionSource.Type.Other:
			{
				Target target = self.Blackboard.Target;
				if (target != null && target.Body != null)
				{
					num3 = target.Body.Position;
					num2 = Mathf.Sign(self.Position - num3);
				}
				else
				{
					num2 = self.LastDirectionAttackedFrom * -1f;
				}
				num = Random.Range(2f, 6f);
				value = self.Position + num2 * num;
				break;
			}
			}
			value = Mathf.Clamp(value, occupant.walkableAreaLeft, occupant.walkableAreaRight);
			Ray ray = new Ray(self.Pelvis.transform.position - Vector3.right * num2 * 0.5f, Vector3.right * num2);
			RaycastHit hitInfo = default(RaycastHit);
			bool flag = Physics.Raycast(ray, num + 1f, Crisis.Runtime.FullSource.Masks.Instance.ObstaclesProps);
			bool flag2 = Physics.Raycast(ray, out hitInfo, num + 1f, Crisis.Runtime.FullSource.Masks.Instance.Doors);
			DoorController doorController = ((!flag2) ? null : hitInfo.transform.GetComponent<DoorReference>().door);
			bool flag3 = flag2 && (doorController.IsBusy() || (doorController.lockState == DoorLockState.Locked && !doorController.CanOperateLock(self)));
			if (flag || flag3)
			{
				if (source.type == ActionSource.Type.Audio)
				{
					movePosition = self.Position - num2 * Random.Range(0.5f, 2f);
				}
				else
				{
					if (Mathf.Abs(num3 - self.Position) > self.VisionRange * 0.75f)
					{
						return TaskStatus.Failure;
					}
					movePosition = num3 - num2 * num;
				}
			}
			else
			{
				movePosition = value;
			}
			state = State.Move;
			return TaskStatus.Running;
		}

		private TaskStatus Move()
		{
			float f = movePosition - self.Position;
			float num = Mathf.Abs(f);
			if (num <= 0.1f)
			{
				state = State.Evaluate;
				return TaskStatus.Running;
			}
			if (StaticBehavior.OccupantHandleDoorWhileMoving(self, movePosition, true, 2f) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			float f2 = ((!(source.transform != null)) ? float.PositiveInfinity : (source.transform.position.x - self.Position));
			float num2 = ((source.type != ActionSource.Type.Character) ? float.PositiveInfinity : Mathf.Abs(f2));
			bool flag = source.type == ActionSource.Type.Character && Mathf.Sign(f2) == Mathf.Sign(f);
			bool flag2 = source.type == ActionSource.Type.Character && num2 < 3f;
			if (!flag && !flag2 && StaticBehavior.OccupantCloseDoorWhileMoving(self, movePosition, true, lockDoors) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			if (!self.Facing(movePosition))
			{
				StaticBehavior.TurnAround(self);
				return TaskStatus.Running;
			}
			self.IsSprinting = true;
			self.Move(Mathf.Sign(f));
			return TaskStatus.Running;
		}

		private TaskStatus Evaluate()
		{
			if (fear.Value < 0.2f)
			{
				self.IsFleeing = false;
				return TaskStatus.Failure;
			}
			state = State.SelectPosition;
			return TaskStatus.Running;
		}
	}
}
