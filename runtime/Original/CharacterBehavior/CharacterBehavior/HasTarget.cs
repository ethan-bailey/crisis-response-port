using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Perception")]
	[TaskIcon("Assets/Editor/Behavior icons/target.png")]
	public class HasTarget : Conditional
	{
		public SharedTarget target;

		[Tooltip("Unconscious targets will not be included.")]
		public bool onlyConscious;

		public bool onlyVisible;

		private CharacterBody self;

		private float reactionTime = 0.52f;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			reactionTime = 0.2f + 0.2f * (1f - self.Blackboard.weaponSkill);
		}

		public override TaskStatus OnUpdate()
		{
			if (target.Value == null || self.Blackboard.TargetVisibleTime < reactionTime)
			{
				return TaskStatus.Failure;
			}
			if (!target.Value.active)
			{
				return TaskStatus.Failure;
			}
			if (onlyVisible && !self.Blackboard.Enemies.Contains(target.Value))
			{
				return TaskStatus.Failure;
			}
			if (target.Value.Body != null && target.Value.Body.IsCompliant)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
