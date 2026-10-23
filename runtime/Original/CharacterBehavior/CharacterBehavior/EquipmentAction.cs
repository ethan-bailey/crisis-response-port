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
	public class EquipmentAction : Action
	{
		public enum Action
		{
			Ready = 0,
			Return = 1,
			SwitchWeapons = 2
		}

		public enum Target
		{
			None = 0,
			Primary = 1,
			Secondary = 2,
			Grenade = 3
		}

		public bool isPlayer;

		public Action action;

		public Target next;

		private CharacterBody self;

		private ArmIKSolver right;

		private ArmIKSolver left;

		private Target previous;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			right = self.rightArm;
			left = self.leftArm;
		}

		public override void OnStart()
		{
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon == null)
			{
				previous = (self.IsHoldingGrenade ? Target.Grenade : Target.None);
			}
			else
			{
				previous = (activeWeapon.IsPrimary ? Target.Primary : Target.Secondary);
			}
		}

		public override TaskStatus OnUpdate()
		{
			return TaskStatus.Running;
		}
	}
}
