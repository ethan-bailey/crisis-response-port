using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Weapon")]
	public class SwitchWeapon : Action
	{
		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			self.armController.SwitchWeapons();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.armController.IsPlaying)
			{
				return TaskStatus.Running;
			}
			return TaskStatus.Failure;
		}

		public override void OnEnd()
		{
			self.PendingAction = CharacterBody.CharacterAction.None;
		}
	}
}
