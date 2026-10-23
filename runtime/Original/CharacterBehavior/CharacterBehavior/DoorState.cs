using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Perception")]
	[TaskIcon("Assets/Editor/Behavior icons/door.png")]
	public class DoorState : Conditional
	{
		public enum Type
		{
			Open = 0,
			Closed = 1
		}

		public SharedDoor door;

		public Type state;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (door.Value == null)
			{
				return TaskStatus.Failure;
			}
			switch (state)
			{
			case Type.Open:
				if (door.Value.Angle >= 90f)
				{
					return TaskStatus.Success;
				}
				break;
			case Type.Closed:
				if (door.Value.Angle <= 0f)
				{
					return TaskStatus.Success;
				}
				break;
			}
			return TaskStatus.Failure;
		}
	}
}
