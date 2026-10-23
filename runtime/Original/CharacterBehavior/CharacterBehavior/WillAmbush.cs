using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Occupants")]
	public class WillAmbush : Conditional
	{
		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.Blackboard.aggression < 0.4f)
			{
				return TaskStatus.Failure;
			}
			AudioStimuli audioStimuli = self.Blackboard.AudioStimuli;
			if (audioStimuli == null)
			{
				return TaskStatus.Failure;
			}
			CharacterBody characterBody = ((!(audioStimuli.instigator != null)) ? null : audioStimuli.instigator.Body);
			if (characterBody != null)
			{
				if (self.CanSeePosition(characterBody.Position) && characterBody.IsRagdoll)
				{
					return TaskStatus.Failure;
				}
				float num = Mathf.Abs(characterBody.Position - self.Position);
				float num2 = 3f + Mathf.Clamp01(self.Blackboard.aggression - 0.4f) * 15f;
				if (num > num2)
				{
					return TaskStatus.Failure;
				}
			}
			return TaskStatus.Success;
		}
	}
}
