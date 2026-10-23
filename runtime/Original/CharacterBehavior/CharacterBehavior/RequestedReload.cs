using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Player")]
	[TaskIcon("Assets/Editor/Behavior icons/magazine.png")]
	public class RequestedReload : Conditional
	{
		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.PendingAction == CharacterBody.CharacterAction.SwitchMagazine)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
