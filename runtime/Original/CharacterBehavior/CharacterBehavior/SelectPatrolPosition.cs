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
	[TaskCategory("Occupant")]
	public class SelectPatrolPosition : Action
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("The selected position.")]
		public SharedFloat position;

		private CharacterBody self;

		private Occupant occupant;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			occupant = gameObject.GetComponent<Occupant>();
		}

		public override TaskStatus OnUpdate()
		{
			position.Value = Random.Range(occupant.patrolAreaLeft, occupant.patrolAreaRight);
			return TaskStatus.Success;
		}
	}
}
