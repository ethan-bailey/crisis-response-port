using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Occupants/Flee")]
	public class WillFlee : Conditional
	{
		private CharacterBody self;

		private OccupantFear fear;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			fear = self.GetComponent<OccupantFear>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.IsFleeing)
			{
				return TaskStatus.Success;
			}
			if (fear.Value > 0.5f)
			{
				self.IsFleeing = true;
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
