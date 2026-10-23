using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Occupants
{
	[TaskCategory("Occupant")]
	public class Surrender : Action
	{
		public enum SurrenderAction
		{
			None = 0,
			Move = 1
		}

		private CharacterBody self;

		private OccupantMorale moraleDriver;

		private SurrenderAction action;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			moraleDriver = self.GetComponent<OccupantMorale>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.IsCompliant)
			{
				return RunComply();
			}
			if (moraleDriver.Value < 0.1f)
			{
				return DoSurrender();
			}
			return TaskStatus.Running;
		}

		private TaskStatus DoSurrender()
		{
			action = SurrenderAction.None;
			self.Blackboard.MovePosition = self.Position;
			self.DropWeapons(false);
			self.armController.Surrender();
			self.armsControl.leftArm.hand.SetMode(Crisis.Runtime.FullSource.Hand.Mode.Fist);
			self.armsControl.rightArm.hand.SetMode(Crisis.Runtime.FullSource.Hand.Mode.Fist);
			self.IsCompliant = true;
			return TaskStatus.Running;
		}

		private TaskStatus RunComply()
		{
			MoveToPosition();
			return TaskStatus.Running;
		}

		private TaskStatus MoveToPosition()
		{
			float movePosition = self.Blackboard.MovePosition;
			if (Mathf.Abs(movePosition - self.Position) <= 0.1f)
			{
				return TaskStatus.Success;
			}
			self.IsSprinting = false;
			self.Move(Mathf.Sign(movePosition - self.Position));
			return TaskStatus.Running;
		}
	}
}
