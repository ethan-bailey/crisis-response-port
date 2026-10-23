using System.Collections.Generic;
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
	[TaskCategory("Occupants")]
	public class WillFinishEnemy : Conditional
	{
		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.Blackboard.aggression < 0.75f)
			{
				return TaskStatus.Failure;
			}
			CharacterBody targetBody = self.Blackboard.TargetBody;
			if (targetBody != null)
			{
				return TaskStatus.Failure;
			}
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon == null || (!activeWeapon.cyclingBolt.CarryingLiveRound && !activeWeapon.cyclingBolt.IsMoving))
			{
				return TaskStatus.Failure;
			}
			List<CharacterBody> list = self.FindBodies(false, self.VisibleRange, false);
			self.Blackboard.CharacterInteractionTarget = null;
			foreach (CharacterBody item in list)
			{
				if (item.IsRagdoll && ((self.Blackboard.aggression < 0.9f && item.Conscious) || (self.Blackboard.aggression >= 0.9f && item.ShowsSignsOfLife)))
				{
					self.Blackboard.CharacterInteractionTarget = item;
					break;
				}
			}
			if (self.Blackboard.CharacterInteractionTarget == null)
			{
				return TaskStatus.Failure;
			}
			if (Time.time - self.Blackboard.CharacterInteractionTarget.Physiology.UnconsciousTimestamp < 2f)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
