using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Operators
{
	[TaskCategory("Operator")]
	[TaskDescription("For the team leader to detect when the team movement target position is reached.")]
	public class HasReachedTeamTargetPosition : Conditional
	{
		public float tolerance = 0.1f;

		private TeamOperator self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			float num = Mathf.Abs(self.Team.targetPosition - self.Body.Position);
			if (num > tolerance)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
