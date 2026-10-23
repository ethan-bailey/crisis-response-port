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
	public class SwitchWeaponCondition : Conditional
	{
		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			if (self.PendingAction == CharacterBody.CharacterAction.SwitchWeapon)
			{
				return TaskStatus.Success;
			}
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon == null || !activeWeapon.ChamberPermanentlyEmpty || !activeWeapon.Magazine.Empty || self.Equipment.HasAmmunitionForWeapon(activeWeapon))
			{
				return TaskStatus.Failure;
			}
			if (self.Equipment.GetNonEmptyWeapon() != null)
			{
				self.PendingAction = CharacterBody.CharacterAction.SwitchWeapon;
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
