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
	[TaskDescription("Returns success if the operator should face its leader during follow.")]
	public class CorrectFollowFacing : Conditional
	{
		private TeamOperator op;

		public override void OnAwake()
		{
			op = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			if (op.Leader == null)
			{
				return TaskStatus.Failure;
			}
			DoorController door = op.Leader.Door;
			float num = ((!(door == null)) ? Mathf.Abs(door.transform.position.x - op.Body.Position) : 0f);
			if (!op.IsRearGuard && door != null && num < 4f)
			{
				float num2 = Mathf.Sign(door.transform.position.x - op.Body.Position);
				return (op.Body.Direction != num2) ? TaskStatus.Failure : TaskStatus.Success;
			}
			float num3 = ((!op.Leader.Body.IsPlayer) ? op.Leader.Blackboard.MovePosition : op.Leader.Body.Position);
			float num4 = Mathf.Sign(num3 - op.transform.position.x);
			bool flag = Mathf.Abs(num3 - op.Body.Position) > 1.5f;
			float num5 = num4 * ((!op.IsRearGuard || op.Body.IsSprinting) ? 1f : (-1f));
			return (((!flag) ? num5 : num4) != op.Body.Direction) ? TaskStatus.Failure : TaskStatus.Success;
		}
	}
}
