using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using System;
using UnityEngine;

namespace BehaviorDesigner.Runtime;

[Serializable]
public class GenericVariable
{
	[SerializeField]
	public string type = "SharedString";

	[SerializeField]
	public SharedVariable value;

	public GenericVariable()
	{
		value = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
	}
}
