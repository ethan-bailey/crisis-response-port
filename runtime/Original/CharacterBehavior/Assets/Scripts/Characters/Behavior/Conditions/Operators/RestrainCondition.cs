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
	public class RestrainCondition : Conditional
	{
		private CharacterBody self;

		private TeamOperator op;

		private Mission mission;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			op = gameObject.GetComponent<TeamOperator>();
			mission = Global.Context.Mission;
		}

		public override TaskStatus OnUpdate()
		{
			if (!self.Equipment.plasticuffs)
			{
				return TaskStatus.Failure;
			}
			if (self.Blackboard.PendingRestrainTarget != null)
			{
				return TaskStatus.Success;
			}
			CharacterBody characterBody = null;
			if (mission.AllRoomsChecked && mission.Occupants.AllRenderedSafe)
			{
				float num = float.PositiveInfinity;
				foreach (Transform item in mission.Occupants.Container)
				{
					CharacterBody component = item.GetComponent<CharacterBody>();
					if (!component.IsRestrained && component.IsCompliantForInteraction && !component.PendingRestrain && !component.PendingGauze)
					{
						float num2 = Mathf.Abs(item.position.x - self.Position);
						if (num2 < num)
						{
							num = num2;
							characterBody = component;
						}
					}
				}
			}
			else if (op.restrainOccupants && op.TeamPosition > 2)
			{
				foreach (CharacterBody item2 in self.FindBodies(false, self.VisibleRange, true, false))
				{
					if (!item2.IsRestrained && item2.IsCompliantForInteraction && !item2.PendingRestrain && !item2.PendingGauze && op.Leader.Body.Position - item2.Position >= 1f)
					{
						characterBody = item2;
						break;
					}
				}
			}
			if (characterBody != null)
			{
				characterBody.PendingRestrain = true;
				self.Blackboard.PendingRestrainTarget = characterBody;
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
