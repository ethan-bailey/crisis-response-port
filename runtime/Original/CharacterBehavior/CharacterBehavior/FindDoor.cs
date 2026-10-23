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
	[TaskCategory("Character/Perception")]
	[TaskIcon("Assets/Editor/Behavior icons/door.png")]
	public class FindDoor : Conditional
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("How far ahead the door will be detected. In meters.")]
		public float distance = 1f;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("The door if any is present.")]
		public SharedDoor door;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			float num = 1f;
			Vector3 right = self.transform.right;
			Ray ray = new Ray(new Vector3(self.transform.position.x - num * self.Direction, 1f, self.transform.position.z), right);
			RaycastHit[] array = Physics.RaycastAll(ray, distance + num, Crisis.Runtime.FullSource.Masks.Instance.Interactables);
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				if (!(raycastHit.collider.tag != "Door"))
				{
					door.Value = raycastHit.collider.GetComponent<DoorController>();
					return TaskStatus.Success;
				}
			}
			return TaskStatus.Failure;
		}
	}
}
