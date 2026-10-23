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
	[TaskIcon("Assets/Editor/Behavior icons/magazine.png")]
	[TaskDescription("Test the state of the character's ammo for the current weapon.")]
	public class AmmoState : Conditional
	{
		public enum Type
		{
			HasMoreMagazines = 0,
			IsOutOfMagazines = 1
		}

		public Type type;

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
			bool flag = self.Equipment.HasAmmunitionForWeapon(activeWeapon);
			if (type == Type.HasMoreMagazines && flag)
			{
				return TaskStatus.Success;
			}
			if (type == Type.IsOutOfMagazines && !flag)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
