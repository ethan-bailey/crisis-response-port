using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Physiology")]
	[TaskIcon("Assets/Editor/Behavior icons/alive.png")]
	public class IsConscious : Conditional
	{
		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (!self.IsRagdoll)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
