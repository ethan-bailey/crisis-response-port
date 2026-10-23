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
	[TaskCategory("Character/Perception")]
	[TaskIcon("Assets/Editor/Behavior icons/facing.png")]
	public class FacingMovePosition : Conditional
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("The position the agent is moving towards.")]
		public SharedFloat position;

		protected CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			float num = Mathf.Sign(position.Value - self.transform.position.x);
			return (self.Direction != num) ? TaskStatus.Failure : TaskStatus.Success;
		}
	}
}
