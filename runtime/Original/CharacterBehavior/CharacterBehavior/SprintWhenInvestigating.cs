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
	[TaskCategory("Character")]
	public class SprintWhenInvestigating : Conditional
	{
		public SharedAudioStimuli audioStimuli;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (audioStimuli.Value == null)
			{
				return TaskStatus.Failure;
			}
			float num = Mathf.Abs(audioStimuli.Value.position - self.transform.position.x);
			if (num <= self.Blackboard.aggression * 10f)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
