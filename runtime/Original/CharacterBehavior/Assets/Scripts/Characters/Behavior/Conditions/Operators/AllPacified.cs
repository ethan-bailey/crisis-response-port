using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Operators
{
	[TaskCategory("Mission")]
	[TaskDescription("Returns success when all rooms are cleared and all occupants are pacified.")]
	public class AllPacified : Conditional
	{
		public override TaskStatus OnUpdate()
		{
			Mission mission = Global.Context.Mission;
			if (!mission.AllRoomsChecked)
			{
				return TaskStatus.Failure;
			}
			if (!mission.Occupants.AllRenderedSafe)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
