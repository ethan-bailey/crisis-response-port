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
	public class ApplyGauzeCondition : Conditional
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
			if (self.Equipment.gauze == 0)
			{
				return TaskStatus.Failure;
			}
			if (self.Blackboard.PendingGauzeTarget != null)
			{
				return TaskStatus.Success;
			}
			CharacterBody characterBody = null;
			if (mission.AllRoomsChecked && mission.Occupants.AllRenderedSafe)
			{
				float num = float.PositiveInfinity;
				foreach (CharacterBody body in mission.Operators.Bodies)
				{
					if (!body.Conscious && body.CanBeGauzed && !body.PendingGauze && !body.PendingRestrain)
					{
						float num2 = Mathf.Abs(body.Position - self.Position);
						if (num2 < num)
						{
							num = num2;
							characterBody = body;
						}
					}
				}
				if (characterBody == null)
				{
					foreach (Transform item in mission.Occupants.Container)
					{
						CharacterBody component = item.GetComponent<CharacterBody>();
						if (component.CanBeGauzed && !component.PendingGauze && !component.PendingRestrain)
						{
							float num3 = Mathf.Abs(item.position.x - self.Position);
							if (num3 < num)
							{
								num = num3;
								characterBody = component;
							}
						}
					}
				}
			}
			else if (op.administerFirstAid && op.TeamPosition > 2)
			{
				foreach (CharacterBody item2 in self.FindBodies(false, self.VisibleRange))
				{
					if (item2.CanBeGauzed && !item2.PendingRestrain && !item2.PendingGauze && op.Leader.Body.Position - item2.Position >= 1f)
					{
						characterBody = item2;
						break;
					}
				}
			}
			if (characterBody != null)
			{
				characterBody.PendingGauze = true;
				self.Blackboard.PendingGauzeTarget = characterBody;
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
