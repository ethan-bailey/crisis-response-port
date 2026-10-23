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
	[TaskCategory("Operator")]
	public class Engage : Action
	{
		private CharacterBody self;

		private TeamOperator op;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			op = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			CharacterBody targetBody = self.Blackboard.TargetBody;
			if (targetBody == null)
			{
				return TaskStatus.Failure;
			}
			Move(targetBody);
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (self.WeaponAimAlignment() > 5f)
			{
				return TaskStatus.Running;
			}
			if (!targetBody.IsRagdoll && !targetBody.IsCompliant)
			{
				TriggerPull(activeWeapon);
			}
			return TaskStatus.Running;
		}

		private TaskStatus Move(CharacterBody target)
		{
			if (!self.Facing(target.Position))
			{
				StaticBehavior.TurnAround(self);
				return TaskStatus.Running;
			}
			self.IsSprinting = false;
			float f = target.Position - self.Position;
			float num = Mathf.Sign(f);
			float num2 = 0f;
			num2 = ((!(op.Leader != null)) ? (target.Position - num * 2.5f) : ((!op.Leader.Body.Facing(target.Position)) ? (op.Leader.Body.Position + num * 0.55f) : (op.Leader.Body.Position - num * 0.55f)));
			float f2 = num2 - self.Position;
			float direction = Mathf.Sign(f2);
			if (Mathf.Abs(f2) > 0.1f)
			{
				self.Move(direction);
				return TaskStatus.Running;
			}
			return TaskStatus.Success;
		}

		private void TriggerPull(WeaponBase weapon)
		{
			if (self.Blackboard.ReleaseTriggerTimestamp == 0f && self.WeaponAimer.ClearFireSolution)
			{
				Vector2 vector = ((weapon.ActiveFireMode != WeaponBase.FireMode.Auto) ? self.SemiTriggerReleaseTime : self.AutoTriggerReleaseTime);
				self.Blackboard.ReleaseTriggerTimestamp = Time.time + Random.Range(vector.x, vector.y);
				self.PullTrigger();
				return;
			}
			float num = self.Blackboard.ReleaseTriggerTimestamp - Time.time;
			if (num <= 0f)
			{
				self.Blackboard.ReleaseTriggerTimestamp = 0f;
			}
			else if (weapon.ActiveFireMode == WeaponBase.FireMode.Auto && self.WeaponAimer.ClearFireSolution)
			{
				self.PullTrigger();
			}
		}
	}
}
