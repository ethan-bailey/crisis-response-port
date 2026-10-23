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
	[TaskIcon("Assets/Editor/Behavior icons/ear stimuli.png")]
	[TaskDescription("Test if the character wants to investigate the current audio stimuli.")]
	public class InvestigateAudio : Conditional
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
			if (self.Blackboard.aggression < self.Blackboard.fear)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
