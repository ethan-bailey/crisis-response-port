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
	[TaskCategory("Perception")]
	public class FindWeapon : Conditional
	{
		private CharacterBody self;

		private const float updateInterval = 0.25f;

		private float timestamp;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.ActiveWeapon != null)
			{
				return TaskStatus.Failure;
			}
			if (Time.time - timestamp < 0.25f)
			{
				return (!(self.Blackboard.SecureWeaponTarget != null) || !(self.Blackboard.SecureWeaponTarget.Owner == null)) ? TaskStatus.Failure : TaskStatus.Success;
			}
			timestamp = Time.time;
			self.Blackboard.SecureWeaponTarget = self.FindWeapon(self.interactionRange);
			return (!(self.Blackboard.SecureWeaponTarget != null)) ? TaskStatus.Failure : TaskStatus.Success;
		}
	}
}
