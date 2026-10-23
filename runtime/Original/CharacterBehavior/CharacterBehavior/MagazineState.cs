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
	[TaskDescription("Test the state of the character's weapon magazine.")]
	public class MagazineState : Conditional
	{
		[Tooltip("Check if the magazine is empty? If not set will check if it is not empty.")]
		public bool checkIfEmpty = true;

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
			if (checkIfEmpty)
			{
				if (activeWeapon.Magazine == null || activeWeapon.Magazine.Empty)
				{
					return TaskStatus.Success;
				}
			}
			else if (activeWeapon.Magazine != null && !activeWeapon.Magazine.Empty)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
