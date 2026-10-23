using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Flee")]
	[TaskIcon("Assets/Editor/Behavior icons/flee.png")]
	[TaskDescription("Compare against the type of the current flee source. Returns failure when there is no source.")]
	public class FleeSourceType : Conditional
	{
		public ActionSource.Type type;

		public SharedFleeSource fleeSource;

		[Tooltip("Return success when the type is different. Otherwise equal.")]
		public bool different;

		public override TaskStatus OnUpdate()
		{
			if (fleeSource.Value == null)
			{
				return TaskStatus.Failure;
			}
			if (!different && fleeSource.Value.type == type)
			{
				return TaskStatus.Success;
			}
			if (different && fleeSource.Value.type != type)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
