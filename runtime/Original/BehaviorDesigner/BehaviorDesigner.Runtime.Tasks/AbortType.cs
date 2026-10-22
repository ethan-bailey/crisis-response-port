using Object = UnityEngine.Object;
namespace BehaviorDesigner.Runtime.Tasks;

public enum AbortType
{
	None,
	Self,
	LowerPriority,
	Both
}
