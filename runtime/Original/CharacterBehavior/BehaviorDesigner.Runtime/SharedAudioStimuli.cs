using System;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using BehaviorDesigner.Runtime;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedAudioStimuli : SharedVariable<AudioStimuli>
	{
		public static implicit operator SharedAudioStimuli(AudioStimuli value)
		{
			SharedAudioStimuli sharedAudioStimuli = new SharedAudioStimuli();
			sharedAudioStimuli.Value = value;
			return sharedAudioStimuli;
		}
	}
}
