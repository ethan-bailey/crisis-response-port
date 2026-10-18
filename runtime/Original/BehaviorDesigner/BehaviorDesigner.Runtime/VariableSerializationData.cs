using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime;

[Serializable]
public class VariableSerializationData
{
	[SerializeField]
	public List<int> variableStartIndex = new List<int>();

	[SerializeField]
	public string JSONSerialization = string.Empty;

	[SerializeField]
	public FieldSerializationData fieldSerializationData = new FieldSerializationData();
}
