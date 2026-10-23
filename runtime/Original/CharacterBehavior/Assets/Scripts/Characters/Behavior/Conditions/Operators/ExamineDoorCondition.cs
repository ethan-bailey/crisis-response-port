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
	public class ExamineDoorCondition : Conditional
	{
		private TeamOperator op;

		private CharacterBody self;

		public override void OnAwake()
		{
			op = gameObject.GetComponent<TeamOperator>();
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.IsPlayer)
			{
				return (self.PendingAction != CharacterBody.CharacterAction.ExamineDoor) ? TaskStatus.Failure : TaskStatus.Success;
			}
			if (op.IsGroupLeader && op.Team.tempo == CharacterBody.Tempo.Slow)
			{
				DoorController doorController = ((!(self.Room != null)) ? null : self.Room.NextDoor(self.Direction));
				if (doorController != null && Mathf.Abs(doorController.Position - self.Position) < 1.5f)
				{
					return ((doorController.openState != DoorOpenState.Closed || doorController.KnownLockState != DoorLockState.Unknown) && (!(doorController.Angle < 30f) || doorController.TrapState != DoorController.DoorTrapState.Unknown)) ? TaskStatus.Failure : TaskStatus.Success;
				}
			}
			return TaskStatus.Failure;
		}
	}
}
