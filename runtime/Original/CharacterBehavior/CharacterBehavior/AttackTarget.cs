using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Combat")]
	[TaskIcon("Assets/Editor/Behavior icons/skull.png")]
	public class AttackTarget : Action
	{
		public SharedTarget target;

		public bool usePrecision = true;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon == null || target.Value == null)
			{
				return TaskStatus.Failure;
			}
			if (self.Blackboard.ReleaseTriggerTimestamp == 0f && self.WeaponAimer.ClearFireSolution)
			{
				Vector2 vector = ((activeWeapon.ActiveFireMode != WeaponBase.FireMode.Auto) ? self.SemiTriggerReleaseTime : self.AutoTriggerReleaseTime);
				self.Blackboard.ReleaseTriggerTimestamp = Time.time + Random.Range(vector.x, vector.y);
				self.PullTrigger();
				return TaskStatus.Running;
			}
			float num = self.Blackboard.ReleaseTriggerTimestamp - Time.time;
			if (num <= 0f)
			{
				if (target.Value.Body == null)
				{
					return TaskStatus.Success;
				}
				if (target.Value.Body.IsRagdoll)
				{
					return TaskStatus.Success;
				}
				self.Blackboard.ReleaseTriggerTimestamp = 0f;
			}
			else if (activeWeapon.ActiveFireMode == WeaponBase.FireMode.Auto && self.WeaponAimer.ClearFireSolution)
			{
				self.PullTrigger();
			}
			return TaskStatus.Running;
		}
	}
}
