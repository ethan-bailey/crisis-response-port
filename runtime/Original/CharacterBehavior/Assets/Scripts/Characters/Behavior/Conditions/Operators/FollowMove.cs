using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Operators
{
	[TaskCategory("Operator")]
	public class FollowMove : Action
	{
		public SharedFloat position;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("How close to the target position I need to get.")]
		public float tolerance = 0.1f;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("When the target position is further away than this I will sprint.")]
		public float leastSprintDistance = 0.85f;

		private TeamOperator self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			float movePosition = self.Blackboard.MovePosition;
			if (Mathf.Abs(movePosition - self.transform.position.x) <= tolerance)
			{
				return TaskStatus.Success;
			}
			float targetDistance = Mathf.Abs(movePosition - self.transform.position.x);
			self.Body.IsSprinting = ShouldSprint(targetDistance);
			self.Body.Move(Mathf.Sign(movePosition - self.transform.position.x));
			return TaskStatus.Running;
		}

		private bool ShouldSprint(float targetDistance)
		{
			bool flag = self.Team.tempo != CharacterBody.Tempo.Slow;
			bool flag2 = targetDistance > leastSprintDistance;
			return flag && flag2;
		}
	}
}
