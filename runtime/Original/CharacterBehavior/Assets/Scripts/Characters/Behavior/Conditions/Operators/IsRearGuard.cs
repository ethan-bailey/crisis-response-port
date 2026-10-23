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
	public class IsRearGuard : Conditional
	{
		private TeamOperator op;

		public override void OnAwake()
		{
			op = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			return (!op.IsRearGuard) ? TaskStatus.Failure : TaskStatus.Success;
		}
	}
}
