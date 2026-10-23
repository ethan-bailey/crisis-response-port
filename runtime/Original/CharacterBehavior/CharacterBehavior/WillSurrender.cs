using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Occupant")]
	public class WillSurrender : Conditional
	{
		private CharacterBody self;

		private OccupantMorale morale;

		private OccupantFear fear;

		private bool isMarksmanSandbox;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			morale = self.GetComponent<OccupantMorale>();
			fear = self.GetComponent<OccupantFear>();
			isMarksmanSandbox = Global.Context.SceneTransition.CurrentScene == SceneTransition.Scene.Marksman;
		}

		public override TaskStatus OnUpdate()
		{
			if (isMarksmanSandbox)
			{
				return TaskStatus.Failure;
			}
			if (self.IsRestrained)
			{
				return TaskStatus.Success;
			}
			if (fear.Value > 0.85f)
			{
				return TaskStatus.Failure;
			}
			if (self.IsCompliant)
			{
				return TaskStatus.Success;
			}
			if (morale.Value < 0.1f)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
