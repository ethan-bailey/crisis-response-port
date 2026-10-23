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
	[TaskDescription("Returns success when my follower is less than the specified distance behind.")]
	public class FollowerWithinDistance : Conditional
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("The distance the follower must be within for the task to return success.")]
		public float distance = 2f;

		private TeamOperator self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			TeamOperator follower = self.Follower;
			if (follower == null)
			{
				return TaskStatus.Failure;
			}
			float num = Mathf.Abs(follower.Body.Position - self.Body.Position);
			if (num > distance)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
