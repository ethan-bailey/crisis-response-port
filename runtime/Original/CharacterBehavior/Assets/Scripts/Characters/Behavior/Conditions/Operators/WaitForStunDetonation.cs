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
	[TaskCategory("Operator")]
	public class WaitForStunDetonation : Action
	{
		private TeamOperator self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			Grenade thrownGrenade = self.Blackboard.ThrownGrenade;
			if (thrownGrenade != null)
			{
				return TaskStatus.Running;
			}
			if (Time.time - self.Blackboard.StunDetonateTimestamp < 0.25f)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Running;
		}
	}
}
