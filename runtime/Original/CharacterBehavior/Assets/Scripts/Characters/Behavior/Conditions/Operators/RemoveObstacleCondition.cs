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
	public class RemoveObstacleCondition : Conditional
	{
		private CharacterBody self;

		private TeamOperator op;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			op = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.IsPlayer)
			{
				return (self.PendingAction != CharacterBody.CharacterAction.RemoveObstacle) ? TaskStatus.Failure : TaskStatus.Success;
			}
			BallisticObstacle ballisticObstacle = self.FindObstacle(2f);
			if (ballisticObstacle != null && ballisticObstacle.QueryInteraction(self))
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
