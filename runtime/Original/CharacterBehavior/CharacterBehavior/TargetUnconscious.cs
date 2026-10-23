using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Perception")]
	[TaskIcon("Assets/Editor/Behavior icons/dead.png")]
	public class TargetUnconscious : Conditional
	{
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
			CharacterBody body = target.Value.Body;
			if (body == null)
			{
				return TaskStatus.Failure;
			}
			if (!body.IsRagdoll)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
