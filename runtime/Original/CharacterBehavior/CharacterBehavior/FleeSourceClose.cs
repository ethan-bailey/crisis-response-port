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
	[TaskIcon("Assets/Editor/Behavior icons/flee.png")]
	public class FleeSourceClose : Conditional
	{
		public SharedFleeSource fleeSource;

		public float requiredDistance = 4f;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("Read-only")]
		public float currentDistance;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (fleeSource.Value == null)
			{
				currentDistance = float.PositiveInfinity;
				return TaskStatus.Failure;
			}
			currentDistance = 0f;
			switch (fleeSource.Value.type)
			{
			case ActionSource.Type.Character:
				currentDistance = Mathf.Abs(fleeSource.Value.transform.position.x - self.transform.position.x);
				break;
			case ActionSource.Type.Audio:
			case ActionSource.Type.Position:
				currentDistance = Mathf.Abs(fleeSource.Value.position - self.transform.position.x);
				break;
			}
			if (currentDistance <= requiredDistance)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
