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
	[TaskIcon("Assets/Editor/Behavior icons/flee audio.png")]
	public class FleeFromAudio : Conditional
	{
		public SharedAudioStimuli audioStimuli;

		private CharacterBody self;

		private float radius;

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
			AudioStimuliType type = audioStimuli.Value.type;
			float num = Mathf.Abs(audioStimuli.Value.position - self.transform.position.x);
			radius = self.Blackboard.fear * 20f;
			if (num < radius)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}

		public override void OnDrawGizmos()
		{
		}
	}
}
