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
	[TaskIcon("Assets/Editor/Behavior icons/facing.png")]
	public class FacingAudio : Conditional
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
			float num = Mathf.Sign(audioStimuli.Value.position - self.transform.position.x);
			if (num == self.Direction)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
