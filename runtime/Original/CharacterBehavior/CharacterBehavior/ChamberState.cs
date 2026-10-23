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
	[TaskIcon("Assets/Editor/Behavior icons/cartridge.png")]
	[TaskDescription("Test the state of the current weapon's chamber.")]
	public class ChamberState : Conditional
	{
		[Tooltip("Which state to test for?")]
		public WeaponBase.ChamberState state;

		[Tooltip("Set this if the return result should be overridden while the bolt is in motion.")]
		public bool overrideWhileMoving;

		[Tooltip("Which result to return while the bolt is in motion.")]
		public TaskStatus boltMovingResult;

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
			if (overrideWhileMoving && activeWeapon.IsCycling)
			{
				return boltMovingResult;
			}
			if (activeWeapon.ChamberStatus == state)
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
