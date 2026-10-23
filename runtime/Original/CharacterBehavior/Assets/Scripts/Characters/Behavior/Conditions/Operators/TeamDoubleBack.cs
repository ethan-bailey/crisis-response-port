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
	[TaskDescription("Sets the team movment position to the start of the level.")]
	public class TeamDoubleBack : Action
	{
		private TeamOperator self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<TeamOperator>();
		}

		public override TaskStatus OnUpdate()
		{
			self.TeamTask = TeamOperator.TeamActivity.Lead;
			self.Team.targetPosition = Global.Context.Mission.StartPosition;
			return TaskStatus.Success;
		}
	}
}
