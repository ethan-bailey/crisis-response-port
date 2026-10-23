using BehaviorDesigner.Runtime.Tasks;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Combat")]
	[TaskDescription("Returns success while the weapon is not aligned or the muzzle doesn't have enough clearance to the aimpoint.")]
	[TaskIcon("Assets/Editor/Behavior icons/target.png")]
	public class WeaponNotAligned : Conditional
	{
		public float minimumClearance = 0.5f;

		[Tooltip("Read-only")]
		public float clearance;

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
			clearance = (self.WeaponAlignPoint.x - self.ActiveWeapon.Muzzle.position.x) * self.Direction;
			if (clearance < minimumClearance)
			{
				return TaskStatus.Success;
			}
			angle = self.WeaponAimAlignment();
			if (angle > self.WeaponAimTolerance)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
