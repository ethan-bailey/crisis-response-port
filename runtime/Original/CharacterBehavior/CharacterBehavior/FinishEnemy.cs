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
	public class FinishEnemy : Action
	{
		private CharacterBody self;

		private bool shouldKneel;

		private bool wasKneeling;

		private float behaviorStart;

		private float delay;

		private float delayStart;

		private int rounds;

		private int maxRounds;

		private const float Timeout = 10f;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			rounds = 0;
			behaviorStart = Time.time;
			maxRounds = Random.Range(2, 8);
			wasKneeling = self.IsKneeling;
			shouldKneel = Random.value > 0.75f;
			float x = self.Blackboard.CharacterInteractionTarget.Head.transform.position.x;
			float num = Mathf.Sign(x - self.Position);
		}

		public override TaskStatus OnUpdate()
		{
			CharacterBody characterInteractionTarget = self.Blackboard.CharacterInteractionTarget;
			if (characterInteractionTarget == null || rounds > maxRounds || Time.time - behaviorStart > 10f)
			{
				return TaskStatus.Failure;
			}
			if (Move() != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			return Execute();
		}

		public override void OnEnd()
		{
			self.IsKneeling = wasKneeling;
		}

		private TaskStatus Move()
		{
			WeaponBase activeWeapon = self.ActiveWeapon;
			CharacterBody characterInteractionTarget = self.Blackboard.CharacterInteractionTarget;
			float num = ((!(self.Blackboard.aggression >= 0.9f)) ? characterInteractionTarget.Torso.AimPoint.x : characterInteractionTarget.Head.AimPoint.x);
			Vector3 position = activeWeapon.Muzzle.position;
			float f = num - self.Position;
			float num2 = Mathf.Abs(f) - activeWeapon.Length() * 1.5f;
			float num3 = Mathf.Sign(f);
			if (StaticBehavior.OccupantHandleDoorWhileMoving(self, num, false, 2f) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			if (num2 > 0.5f)
			{
				self.IsSprinting = false;
				if (!self.Facing(num))
				{
					StaticBehavior.TurnAround(self);
					return TaskStatus.Running;
				}
				self.Move(num3);
				return TaskStatus.Running;
			}
			if (num2 < 0.2f)
			{
				self.Move(num3 * -1f);
				return TaskStatus.Running;
			}
			if (!self.Facing(num))
			{
				StaticBehavior.TurnAround(self);
				return TaskStatus.Running;
			}
			self.IsKneeling = shouldKneel;
			return TaskStatus.Success;
		}

		private TaskStatus Execute()
		{
			CharacterBody characterInteractionTarget = self.Blackboard.CharacterInteractionTarget;
			self.IsUsingPrecision = true;
			self.IsKneeling = false;
			self.WeaponAimer.Mode = WeaponAimer.AimMode.AtWorldPoint;
			self.WeaponAimer.Point = ((!(self.Blackboard.aggression >= 0.9f)) ? characterInteractionTarget.Torso.AimPoint : characterInteractionTarget.Head.AimPoint);
			float num = self.WeaponAimAlignment();
			if (num > 3.5f)
			{
				return TaskStatus.Running;
			}
			if (Time.time - delayStart < delay)
			{
				return TaskStatus.Running;
			}
			self.PullTrigger();
			delayStart = Time.time;
			delay = Random.Range(0.1f, 0.6f);
			rounds++;
			return TaskStatus.Running;
		}
	}
}
