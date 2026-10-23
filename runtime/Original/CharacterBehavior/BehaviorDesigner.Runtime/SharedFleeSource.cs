using System;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using BehaviorDesigner.Runtime;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedFleeSource : SharedVariable<ActionSource>
	{
		public static implicit operator SharedFleeSource(ActionSource value)
		{
			SharedFleeSource sharedFleeSource = new SharedFleeSource();
			sharedFleeSource.Value = value;
			return sharedFleeSource;
		}
	}
}
