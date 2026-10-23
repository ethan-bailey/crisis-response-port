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
	[TaskCategory("Character/Locomotion")]
	[TaskIcon("Assets/Editor/Behavior icons/feet.png")]
	public class Step : Action
	{
		public bool backwards;

		public bool sprint;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("If an obstacle gets closer than this I will stop and return failure.")]
		public float obstacleClearance = 1f;

		private CharacterBody self;

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
			float direction = self.Direction * ((!backwards) ? 1f : (-1f));
			if (IsBlocked(direction))
			{
				return TaskStatus.Failure;
			}
			self.Move(direction);
			return TaskStatus.Running;
		}

		public override void OnEnd()
		{
			self.IsSprinting = false;
		}

		private bool IsBlocked(float direction)
		{
			float num = 1f;
			Ray ray = new Ray(new Vector3(self.transform.position.x - num * direction, 1f, self.transform.position.z), Vector3.right * direction);
			return Physics.Raycast(ray, out self.raycastHit, obstacleClearance + num, Crisis.Runtime.FullSource.Masks.Instance.BallisticTargetsSansLimbs);
		}
	}
}
