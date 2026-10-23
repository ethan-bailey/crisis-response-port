using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Operator")]
	public class EngageCondition : Conditional
	{
		private CharacterBody self;

		private TeamOperator op;

		private float reactionTime = 0.52f;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			op = gameObject.GetComponent<TeamOperator>();
			reactionTime = 0.2f + 0.2f * (1f - self.Blackboard.weaponSkill);
		}

		public override TaskStatus OnUpdate()
		{
			Target target = self.Blackboard.Target;
			if (target == null || self.Blackboard.TargetVisibleTime < reactionTime)
			{
				return TaskStatus.Failure;
			}
			if (!self.Blackboard.Enemies.Contains(target))
			{
				return TaskStatus.Failure;
			}
			if (target.Body != null && target.Body.IsCompliantForInteraction)
			{
				return TaskStatus.Failure;
			}
			if (self.ActiveWeapon == null)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
