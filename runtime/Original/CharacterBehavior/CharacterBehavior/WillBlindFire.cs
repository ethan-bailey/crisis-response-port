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
	[TaskCategory("Character/Combat")]
	[TaskIcon("Assets/Editor/Behavior icons/skull.png")]
	[TaskDescription("Returns success when the character perceives audio that is close enough.")]
	public class WillBlindFire : Conditional
	{
		public SharedAudioStimuli audioStimuli;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (!self.Blackboard.WillBlindFire)
			{
				return TaskStatus.Failure;
			}
			if (audioStimuli.Value == null)
			{
				return TaskStatus.Failure;
			}
			float num = self.Blackboard.aggression * 10f;
			float num2 = Mathf.Abs(audioStimuli.Value.position - self.transform.position.x);
			if (num2 > self.Blackboard.BlindFireDistance)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
