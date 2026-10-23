using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Operators
{
	[TaskCategory("Operator")]
	public class RoomBeyondDoorIsChecked : Conditional
	{
		public SharedDoor door;

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
			float num = Mathf.Sign(door.Value.transform.position.x - self.transform.position.x);
			Room room = self.Mission.RoomAtPosition(door.Value.transform.position.x + num);
			if (room == null)
			{
				return TaskStatus.Failure;
			}
			if (!room.cleared)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
