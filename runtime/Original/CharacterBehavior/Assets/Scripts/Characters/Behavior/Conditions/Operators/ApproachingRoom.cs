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
	[TaskDescription("Returns success when the operator is close to and facing a room that is different from the current one.")]
	public class ApproachingRoom : Conditional
	{
		public enum ClearedStatus
		{
			Cleared = 0,
			NotCleared = 1
		}

		public bool useClearedStatus;

		public ClearedStatus clearedStatus;

		private TeamOperator self;

		private float distance = 2f;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			Room nextRoom = self.NextRoom;
			if (nextRoom == null)
			{
				return TaskStatus.Failure;
			}
			float num = ((self.Body.Direction != 1f) ? nextRoom.RightEdge : nextRoom.LeftEdge);
			float num2 = Mathf.Abs(self.Body.Position - num);
			if (num2 > distance)
			{
				return TaskStatus.Failure;
			}
			if (useClearedStatus && ((clearedStatus == ClearedStatus.Cleared && !nextRoom.cleared) || (clearedStatus == ClearedStatus.NotCleared && nextRoom.cleared)))
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
