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
	[TaskDescription("Returns success when the follower is less than a meter behind and is holding its weapon high, or there is no follower. Otherwise failure.")]
	public class HasCover : Conditional
	{
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
				return TaskStatus.Success;
			}
			if (!follower.CoveringLeader())
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
