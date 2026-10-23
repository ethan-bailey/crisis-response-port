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
	[TaskCategory("Character/Combat")]
	[TaskIcon("Assets/Editor/Behavior icons/skull.png")]
	public class FireWeapon : Action
	{
		public bool usePrecision = true;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon == null || !activeWeapon.cyclingBolt.Live || activeWeapon.IsBlocked)
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
