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
	[TaskDescription("Returns success when the operator faces the direction intended for leading.")]
	public class CorrectLeadFacing : Conditional
	{
		public float sensitivity = 0.1f;

		private TeamOperator self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			float f = self.Team.targetPosition - self.transform.position.x;
			if (Mathf.Abs(f) < sensitivity)
			{
				return TaskStatus.Success;
			}
			float num = Mathf.Sign(f);
			return (num != self.Body.Direction) ? TaskStatus.Failure : TaskStatus.Success;
		}
	}
}
