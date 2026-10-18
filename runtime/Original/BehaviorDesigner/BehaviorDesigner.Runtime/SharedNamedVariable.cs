using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using System;

namespace BehaviorDesigner.Runtime;

[Serializable]
public class SharedNamedVariable : SharedVariable<NamedVariable>
{
	public SharedNamedVariable()
	{
		mValue = new NamedVariable();
	}

	public static implicit operator SharedNamedVariable(NamedVariable value)
	{
		SharedNamedVariable sharedNamedVariable = new SharedNamedVariable();
		sharedNamedVariable.mValue = value;
		return sharedNamedVariable;
	}
}
