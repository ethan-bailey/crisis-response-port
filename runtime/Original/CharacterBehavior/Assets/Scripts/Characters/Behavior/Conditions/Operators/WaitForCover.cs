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
	[TaskDescription("Returns success when the follower is providing cover or there is no follower. If the follower is not in position it returns running.")]
	public class WaitForCover : Action
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("Will return success after waiting for this long.")]
		public float waitTimeoutSeconds = 5f;

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
				return TaskStatus.Success;
			}
			if (Time.time - timestamp >= waitTimeoutSeconds)
			{
				return TaskStatus.Success;
			}
			if (!follower.CoveringLeader())
			{
				return TaskStatus.Running;
			}
			return TaskStatus.Success;
		}
	}
}
