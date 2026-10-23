using System;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using BehaviorDesigner.Runtime;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedGameObject : SharedVariable<GameObject>
	{
		public static implicit operator SharedGameObject(GameObject value)
		{
			SharedGameObject sharedGameObject = new SharedGameObject();
			sharedGameObject.mValue = value;
			return sharedGameObject;
		}
	}
}
