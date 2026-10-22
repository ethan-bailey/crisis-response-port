using Object = UnityEngine.Object;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks;

[TaskDescription("Behavior Reference allows you to run another behavior tree within the current behavior tree.")]
[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=53")]
[TaskIcon("BehaviorTreeReferenceIcon.png")]
public abstract class BehaviorReference : Action
{
	[Tooltip("External behavior array that this task should reference")]
	public ExternalBehavior[] externalBehaviors;

	[Tooltip("Any variables that should be set for the specific tree")]
	public SharedNamedVariable[] variables;

	[HideInInspector]
	public bool collapsed;

	public virtual ExternalBehavior[] GetExternalBehaviors()
	{
		return externalBehaviors;
	}

	public override void OnReset()
	{
		externalBehaviors = null;
	}
}
