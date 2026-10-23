using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Perception")]
	[TaskIcon("Assets/Editor/Behavior icons/wall.png")]
	public class HasObstacle : Conditional
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("How far to trace for obstacles. In meters.")]
		public float distance = 1f;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Should the check be to my rear or front?")]
		public bool checkBehind;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Should doors be included?")]
		public bool includeDoors;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			float num = 0.5f;
			Vector3 direction = self.transform.right * ((!checkBehind) ? 1f : (-1f));
			Ray ray = new Ray(new Vector3(self.transform.position.x - num * Mathf.Sign(direction.x), 1f, self.transform.position.z), direction);
			int layerMask = ((!includeDoors) ? Crisis.Runtime.FullSource.Masks.Instance.Obstacles : Crisis.Runtime.FullSource.Masks.Instance.BallisticTargetsSansLimbs);
			if (Physics.Raycast(ray, distance + num, layerMask))
			{
				return TaskStatus.Success;
			}
			if (includeDoors)
			{
				RaycastHit[] array = Physics.RaycastAll(ray, distance + num, Crisis.Runtime.FullSource.Masks.Instance.Interactables);
				RaycastHit[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					RaycastHit raycastHit = array2[i];
					if (!(raycastHit.collider.tag != "Door"))
					{
						DoorController component = raycastHit.collider.GetComponent<DoorController>();
						if (component.Angle < 90f)
						{
							return TaskStatus.Success;
						}
					}
				}
			}
			return TaskStatus.Failure;
		}
	}
}
