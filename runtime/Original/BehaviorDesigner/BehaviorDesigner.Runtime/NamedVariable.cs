using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using System;
using UnityEngine;

namespace BehaviorDesigner.Runtime;

[Serializable]
public class NamedVariable : GenericVariable
{
	[SerializeField]
	public string name = string.Empty;
}
