using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Operators
{
	[TaskCategory("Operator")]
	public class ThrowStunThroughDoor : Action
	{
		private CharacterBody self;

		private bool thrown;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			thrown = false;
		}

		public override TaskStatus OnUpdate()
		{
			if (!thrown)
			{
				thrown = true;
				self.InitiateGrenadeThrow();
				return TaskStatus.Running;
			}
			if (self.Blackboard.ThrownGrenade == null)
			{
				return TaskStatus.Running;
			}
			return TaskStatus.Success;
		}
	}
}
