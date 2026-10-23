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
	[TaskCategory("Character/Locomotion")]
	[TaskIcon("Assets/Editor/Behavior icons/feet.png")]
	public class MoveToDoor : Action
	{
		public SharedDoor door;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("How close to the door should I attempt to get? In meters.")]
		public float proximity = 0.25f;

		public SharedFloat tolerance = 0.1f;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("This variable is set to reference any GameObject that we collide with.")]
		public SharedGameObject collision;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Should I sprint this movement?")]
		public bool sprint;

		private CharacterBody self;

		private float position;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			self.IsSprinting = sprint;
		}

		public override TaskStatus OnUpdate()
		{
			if (door.Value == null)
			{
				return TaskStatus.Failure;
			}
			float num = Mathf.Abs(door.Value.transform.position.x - self.transform.position.x);
			position = door.Value.transform.position.x + num * -1f * proximity;
			float num2 = Mathf.Abs(position - self.transform.position.x);
			if (num2 <= tolerance.Value)
			{
				return TaskStatus.Success;
			}
			if (collision.Value != null)
			{
				if (collision.Value == door.Value.physicalDoor)
				{
					return TaskStatus.Success;
				}
				return TaskStatus.Failure;
			}
			float direction = Mathf.Sign(position - self.transform.position.x);
			self.Move(direction);
			return TaskStatus.Running;
		}

		public override void OnEnd()
		{
			self.IsSprinting = false;
		}

		public override void OnDrawGizmos()
		{
		}
	}
}
