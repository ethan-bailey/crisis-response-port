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
	public class GetWeapon : Action
	{
		private enum State
		{
			MoveIntoPosition = 0,
			LowerWeapons = 1,
			FacePosition = 2,
			CollectWeapon = 3,
			Finish = 4
		}

		private State state;

		private WeaponBase target;

		private CharacterBody self;

		private bool wasKneeling;

		private float movePosition;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			state = State.MoveIntoPosition;
			target = self.Blackboard.SecureWeaponTarget;
			movePosition = target.transform.position.x - self.Direction * 0.25f;
			wasKneeling = self.IsKneeling;
		}

		public override TaskStatus OnUpdate()
		{
			if (target == null || target.Owner != null)
			{
				return TaskStatus.Failure;
			}
			switch (state)
			{
			case State.MoveIntoPosition:
			{
				float f = movePosition - self.Position;
				if (Mathf.Abs(f) > 0.1f)
				{
					self.Move(Mathf.Sign(f));
				}
				else
				{
					state++;
				}
				return TaskStatus.Running;
			}
			case State.LowerWeapons:
				if (self.armController.RightSequencer.IsReady)
				{
					state++;
					self.IsKneeling = true;
					self.leftArm.TargetTransition(target.leftHandAnchor, 0.5f);
				}
				return TaskStatus.Running;
			case State.FacePosition:
				if (!self.Facing(target.Position))
				{
					StaticBehavior.TurnAround(self);
				}
				else
				{
					state++;
				}
				return TaskStatus.Running;
			case State.CollectWeapon:
				if (self.KneelRatio >= 1f && self.leftArm.OnTarget)
				{
					state++;
					self.leftArm.ReturnTransforms();
					self.PickupAndEquipWeapon(target, true);
				}
				return TaskStatus.Running;
			case State.Finish:
				self.IsKneeling = wasKneeling;
				self.Blackboard.SecureWeaponTarget = null;
				return TaskStatus.Success;
			default:
				return TaskStatus.Success;
			}
		}
	}
}
