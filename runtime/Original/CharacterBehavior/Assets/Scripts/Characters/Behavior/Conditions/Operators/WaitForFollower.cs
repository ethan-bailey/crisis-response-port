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
	[TaskDescription("Returns success when a follower is less than the specified distance behind. Returns running while the follower outside that distance until timeout has passed.")]
	public class WaitForFollower : Action
	{
		public float distance = 2f;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Return success if the follower doesn't catch up in this time.")]
		public float timeout = 5f;

		private TeamOperator self;

		private float timestamp;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
		}

		public override void OnStart()
		{
			timestamp = Time.time;
		}

		public override TaskStatus OnUpdate()
		{
			TeamOperator follower = self.Follower;
			if (follower == null)
			{
				return TaskStatus.Failure;
			}
			if (Time.time - timestamp >= timeout)
			{
				return TaskStatus.Success;
			}
			float num = Mathf.Abs(follower.Body.Position - self.Body.Position);
			if (num > distance)
			{
				return TaskStatus.Running;
			}
			return TaskStatus.Success;
		}
	}
}
