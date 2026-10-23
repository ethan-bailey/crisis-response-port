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
	[TaskIcon("Assets/Editor/Behavior icons/target range.png")]
	public class TestEngageDistance : Conditional
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("Test if the target is inside the minimum distance. Otherwise the inverse.")]
		public bool inside;

		public float minimumDistance = 1.5f;

		public SharedTarget target;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (target.Value == null)
			{
				return TaskStatus.Failure;
			}
			float num = Mathf.Abs(target.Value.transform.position.x - self.transform.position.x);
			if (num <= minimumDistance)
			{
				return (!inside) ? TaskStatus.Failure : TaskStatus.Success;
			}
			return inside ? TaskStatus.Failure : TaskStatus.Success;
		}
	}
}
