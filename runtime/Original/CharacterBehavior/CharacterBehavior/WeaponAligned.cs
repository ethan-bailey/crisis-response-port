using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Combat")]
	[TaskIcon("Assets/Editor/Behavior icons/target.png")]
	public class WeaponAligned : Conditional
	{
		[Tooltip("Override the current aimpoint by setting it to the current target position?")]
		public bool useTarget;

		public SharedTarget target;

		[Tooltip("Read-only")]
		public float angle;

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
			if (self.weaponMode == WeaponMode.Lowered)
			{
				return TaskStatus.Failure;
			}
			if (useTarget && target.Value == null)
			{
				return TaskStatus.Failure;
			}
			float num = self.WeaponAimAlignment();
			if (num > self.WeaponAimTolerance)
			{
				return TaskStatus.Failure;
			}
			if (!self.WeaponAimer.ClearFireSolution)
			{
				return TaskStatus.Failure;
			}
			return TaskStatus.Success;
		}
	}
}
