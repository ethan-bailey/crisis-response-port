using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Locomotion")]
	public class FaceAudio : Action
	{
		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			AudioStimuli audioStimuli = self.Blackboard.AudioStimuli;
			if (audioStimuli == null)
			{
				return TaskStatus.Failure;
			}
			if (!self.Facing(audioStimuli.position) && StaticBehavior.TurnAround(self) != TaskStatus.Success)
			{
				return TaskStatus.Running;
			}
			return TaskStatus.Failure;
		}
	}
}
