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
	[TaskCategory("Operator")]
	[TaskIcon("Assets/Editor/Behavior icons/door.png")]
	[TaskDescription("Gradually opens the targeted door enough that a stun can go through the opening.")]
	public class PrepareDoorForStun : Action
	{
		public SharedDoor door;

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
			if (door.Value.Angle >= 15f && door.Value.Angle < 30f)
			{
				return TaskStatus.Success;
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
				float num = Mathf.Sign(20f - door.Value.Angle);
				door.Value.MovePartial(self, 3f * num);
				return TaskStatus.Running;
			}
			return TaskStatus.Running;
		}
	}
}
