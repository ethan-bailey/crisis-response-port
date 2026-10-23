using System.Collections.Generic;
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
	[TaskIcon("Assets/Editor/Behavior icons/ear.png")]
	public class AuditoryPerceive : Action
	{
		public SharedAudioStimuli audioStimuli;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("For how long has the current target not been seen? In seconds.")]
		public SharedFloat audioDecay;

		public float audioDecayTime = 3f;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			AudioStimuli audioStimuli = new AudioStimuli();
			List<AudioStimuli> list = self.Audio.Propagator.Perceivable(self);
			foreach (AudioStimuli item in list)
			{
				if (item.type > audioStimuli.type || Mathf.Abs(transform.position.x - item.position) < Mathf.Abs(transform.position.x - audioStimuli.position))
				{
					audioStimuli = item;
				}
			}
			if (this.audioStimuli.Value == null || audioStimuli.guid != this.audioStimuli.Value.guid)
			{
				this.audioStimuli.Value = audioStimuli;
			}
			if (this.audioStimuli.Value != null)
			{
				audioDecay = Time.time - this.audioStimuli.Value.timestamp;
				self.LastActionTimestamp = Time.time;
			}
			if (audioDecay.Value > audioDecayTime)
			{
				this.audioStimuli.Value = null;
				audioDecay.Value = float.NegativeInfinity;
			}
			return TaskStatus.Running;
		}
	}
}
