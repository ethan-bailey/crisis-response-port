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
	public class MoveToPosition : Action
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("The direction that the agent should move.")]
		public SharedFloat position;

		public SharedFloat tolerance = 0.1f;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("If an obstacle gets closer than this I will stop.")]
		public float obstacleClearance = 1f;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("This variable is set to reference any GameObject that we collide with.")]
		public SharedGameObject collision;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Should I sprint this movement?")]
		public bool sprint;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Should kneeling status be cleared?")]
		public bool standUp = true;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			self.IsSprinting = sprint;
			if (standUp)
			{
				self.IsKneeling = false;
			}
		}

		public override TaskStatus OnUpdate()
		{
			float f = position.Value - self.transform.position.x;
			float num = Mathf.Abs(f);
			if (num <= tolerance.Value)
			{
				return TaskStatus.Success;
			}
			float direction = Mathf.Sign(f);
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

		public override void OnDrawGizmos()
		{
		}

		private bool IsBlocked(float direction)
		{
			float num = 1f;
			Ray ray = new Ray(new Vector3(self.transform.position.x - num * direction, 1f, self.transform.position.z), Vector3.right * direction);
			return Physics.Raycast(ray, out self.raycastHit, obstacleClearance + num, Crisis.Runtime.FullSource.Masks.Instance.BallisticTargetsSansLimbs);
		}
	}
}
