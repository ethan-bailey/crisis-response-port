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
	[TaskCategory("Character/Flee")]
	[TaskIcon("Assets/Editor/Behavior icons/skull.png")]
	public class EngageWhenCornered : Conditional
	{
		public SharedFleeSource fleeSource;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (fleeSource.Value.transform == null || self.IsMoving)
			{
				return TaskStatus.Failure;
			}
			float num = Mathf.Abs(fleeSource.Value.transform.position.x - self.transform.position.x);
			float num2 = 5f - (1f - self.Blackboard.aggression) * 3.5f;
			if (num <= num2)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
