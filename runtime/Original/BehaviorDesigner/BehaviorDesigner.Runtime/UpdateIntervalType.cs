using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
namespace BehaviorDesigner.Runtime;

public enum UpdateIntervalType
{
	EveryFrame,
	SpecifySeconds,
	Manual
}
