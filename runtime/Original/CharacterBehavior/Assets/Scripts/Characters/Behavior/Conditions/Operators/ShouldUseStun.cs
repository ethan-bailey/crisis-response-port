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
	[TaskDescription("Returns success when the operator has at least one stun and the team is set to use them and the target room has not been stunned in the last 10 seconds.")]
	public class ShouldUseStun : Conditional
	{
		private TeamOperator self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			if (!self.Team.UseStunGrenades)
			{
				return TaskStatus.Failure;
			}
			if (self.Body.Equipment.stunGrenades == 0)
			{
				return TaskStatus.Failure;
			}
			if (self.NextRoom != null && (self.NextRoom.cleared || self.NextRoom.HasBeenStunned))
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
