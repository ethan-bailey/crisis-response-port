using System;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using BehaviorDesigner.Runtime;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedGrenade : SharedVariable<Grenade>
	{
		public static implicit operator SharedGrenade(Grenade value)
		{
			SharedGrenade sharedGrenade = new SharedGrenade();
			sharedGrenade.Value = value;
			return sharedGrenade;
		}
	}
}
