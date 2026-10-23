using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Settings")]
	public class TestSetting : Conditional
	{
		public enum Setting
		{
			DisableLocks = 0
		}

		public Setting setting;

		[Tooltip("Test whether the setting is enabled or disabled?")]
		public bool enabled = true;

		public override TaskStatus OnUpdate()
		{
			if (setting == Setting.DisableLocks && Global.Context.Settings.Extras.disableLocks == enabled)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
