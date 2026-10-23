using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Locomotion")]
	[TaskIcon("Assets/Editor/Behavior icons/kneel.png")]
	[TaskDescription("Set the character's kneeling state.")]
	public class Kneel : Action
	{
		public bool kneel;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			self.IsKneeling = kneel;
			return TaskStatus.Success;
		}
	}
}
