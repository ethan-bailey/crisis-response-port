using System;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using BehaviorDesigner.Runtime;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedTarget : SharedVariable<Target>
	{
		public static implicit operator SharedTarget(Target value)
		{
			SharedTarget sharedTarget = new SharedTarget();
			sharedTarget.Value = value;
			return sharedTarget;
		}
	}
}
