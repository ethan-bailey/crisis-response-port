using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime;

public interface IVariableSource
{
	SharedVariable GetVariable(string name);

	List<SharedVariable> GetAllVariables();

	void SetVariable(string name, SharedVariable sharedVariable);

	void UpdateVariableName(SharedVariable sharedVariable, string name);

	void SetAllVariables(List<SharedVariable> variables);
}
