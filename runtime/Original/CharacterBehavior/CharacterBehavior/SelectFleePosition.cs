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
	public class SelectFleePosition : Action
	{
		public SharedFleeSource fleeSource;

		public SharedTarget target;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("The selected position.")]
		public SharedFloat position;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("How far to dash. Is randomized from this range. In meters.")]
		public Vector2 randomDistance = new Vector2(4f, 8f);

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Should I dash away from the source or towards and past?")]
		public bool awayFromSource = true;

		private CharacterBody self;

		private float xMin;

		private float xMax;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			Occupant component = self.GetComponent<Occupant>();
			xMin = component.walkableAreaLeft;
			xMax = component.walkableAreaRight;
		}

		public override TaskStatus OnUpdate()
		{
			if (fleeSource.Value == null)
			{
				return TaskStatus.Failure;
			}
			float num = ((!(fleeSource.Value.transform == null)) ? fleeSource.Value.transform.position.x : fleeSource.Value.position);
			float num2 = Mathf.Sign(num - self.transform.position.x) * ((!awayFromSource) ? 1f : (-1f));
			float num3 = ((!awayFromSource) ? Mathf.Abs(num - self.transform.position.x) : 0f);
			float num4 = Random.Range(randomDistance.x, randomDistance.y) + num3;
			position.Value = Mathf.Clamp(self.transform.position.x + num4 * num2, xMin + 0.5f, xMax - 0.5f);
			return TaskStatus.Success;
		}
	}
}
