using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Flee")]
	[TaskDescription("If we handle a door during fleeing we update the flee position to be past that door.")]
	public class SelectFleeDoorPosition : Action
	{
		public SharedDoor door;

		public SharedFleeSource fleeSource;

		public SharedFloat position;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (door.Value == null || fleeSource.Value == null)
			{
				return TaskStatus.Success;
			}
			float num = Mathf.Abs(door.Value.transform.position.x - position.Value);
			if (num < 5f)
			{
				float num2 = Mathf.Sign(fleeSource.Value.position - self.transform.position.x);
				position = door.Value.transform.position.x - num2 * 0.5f;
			}
			return TaskStatus.Success;
		}
	}
}
