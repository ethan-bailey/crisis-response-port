using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Weapon")]
	[TaskIcon("Assets/Editor/Behavior icons/bolt.png")]
	public class BoltState : Conditional
	{
		[Tooltip("Return success when bolt is in which state?")]
		public CyclingBolt.State state;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon == null)
			{
				return TaskStatus.Failure;
			}
			if (activeWeapon.BoltStatus == state)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
