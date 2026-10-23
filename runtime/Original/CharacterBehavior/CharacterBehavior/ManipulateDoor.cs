using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character")]
	[TaskIcon("Assets/Editor/Behavior icons/door.png")]
	public class ManipulateDoor : Action
	{
		public enum Type
		{
			Open = 0,
			Close = 1,
			Lock = 2,
			Unlock = 3
		}

		public SharedDoor door;

		public Type type;

		[Tooltip("If set the character will not wait for the door to reach its destination angle on open and close manipulations.")]
		public bool quick;

		protected CharacterBody self;

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
			if (!door.Value.InteractorInRange(self))
			{
				return TaskStatus.Failure;
			}
			switch (type)
			{
			case Type.Open:
			{
				if (door.Value.Angle >= 90f)
				{
					return TaskStatus.Success;
				}
				CharacterBody interactor = door.Value.interactor;
				if (interactor != null && interactor != self)
				{
					return TaskStatus.Failure;
				}
				if (door.Value.lockState == DoorLockState.Locked)
				{
					door.Value.Unlock(self);
				}
				if (door.Value.lockState == DoorLockState.Unlocking)
				{
					return TaskStatus.Running;
				}
				if (door.Value.openState != DoorOpenState.Opening)
				{
					if (quick)
					{
						door.Value.Open(self);
						return TaskStatus.Success;
					}
					door.Value.Open(self);
					return TaskStatus.Running;
				}
				break;
			}
			case Type.Close:
				if (door.Value.Angle <= 0f)
				{
					return TaskStatus.Success;
				}
				if (door.Value.openState != DoorOpenState.Closing)
				{
					if (quick)
					{
						door.Value.Close(self);
						return TaskStatus.Success;
					}
					door.Value.Close(self);
					return TaskStatus.Running;
				}
				break;
			case Type.Lock:
				if (door.Value.lockState == DoorLockState.Locked)
				{
					return TaskStatus.Success;
				}
				if (door.Value.openState != DoorOpenState.Closed)
				{
					return TaskStatus.Failure;
				}
				if (door.Value.lockState != DoorLockState.Locking)
				{
					door.Value.Lock(self);
					return TaskStatus.Running;
				}
				break;
			case Type.Unlock:
				if (door.Value.lockState == DoorLockState.Unlocked)
				{
					return TaskStatus.Success;
				}
				if (door.Value.lockState != DoorLockState.Unlocking)
				{
					door.Value.Unlock(self);
					return TaskStatus.Running;
				}
				break;
			}
			return TaskStatus.Running;
		}
	}
}
