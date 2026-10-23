using System;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using BehaviorDesigner.Runtime;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedDoor : SharedVariable<DoorController>
	{
		public static implicit operator SharedDoor(DoorController value)
		{
			SharedDoor sharedDoor = new SharedDoor();
			sharedDoor.Value = value;
			return sharedDoor;
		}
	}
}
