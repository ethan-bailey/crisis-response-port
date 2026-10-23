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
	public class FireAtAudioCondition : Conditional
	{
		private CharacterBody self;

		private bool declinedAttack;

		private float engagementEndTime;

		public override void OnAwake()
		{
			engagementEndTime = float.NegativeInfinity;
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.Blackboard.aggression < 0.4f)
			{
				return TaskStatus.Failure;
			}
			if (Global.Context.Mission.Occupants.FirstConscious != self)
			{
				return TaskStatus.Failure;
			}
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon == null || (!activeWeapon.IsCycling && !activeWeapon.ReadyToFire))
			{
				return TaskStatus.Failure;
			}
			float num = engagementEndTime - Time.time;
			if (num > 0f)
			{
				return TaskStatus.Success;
			}
			if (declinedAttack)
			{
				if (self.Blackboard.AudioStimuli == null)
				{
					declinedAttack = false;
				}
				return TaskStatus.Failure;
			}
			if (self.Blackboard.AudioStimuli != null && num <= 0f)
			{
				if (Random.value > self.Blackboard.aggression)
				{
					declinedAttack = true;
					return TaskStatus.Failure;
				}
				if (self.CanSeePosition(self.Blackboard.AudioStimuli.position))
				{
					return TaskStatus.Failure;
				}
				engagementEndTime = Time.time + Random.Range(self.Blackboard.aggression * 3f, self.Blackboard.aggression * 10f);
			}
			return (!(engagementEndTime - Time.time > 0f)) ? TaskStatus.Failure : TaskStatus.Success;
		}
	}
}
