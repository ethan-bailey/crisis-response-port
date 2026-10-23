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
	public class ReadyStunGrenade : Action
	{
		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.IsHoldingGrenade && !self.armController.IsPlaying)
			{
				return TaskStatus.Success;
			}
			if (!self.IsHoldingGrenade && !self.armController.IsPlaying)
			{
				self.armController.ReadyGrenade(Grenade.Type.Stun);
			}
			return TaskStatus.Running;
		}
	}
}
