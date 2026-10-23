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
	public class WorkBoltCondition : Conditional
	{
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
			if (activeWeapon.boltHandleTransform == null)
			{
				return TaskStatus.Failure;
			}
			if (!self.rightArm.OnTarget || !self.leftArm.OnTarget)
			{
				return TaskStatus.Failure;
			}
			if (self.IsPlayer)
			{
				return (self.PendingAction != CharacterBody.CharacterAction.WorkBolt) ? TaskStatus.Failure : TaskStatus.Success;
			}
			CyclingBolt cyclingBolt = activeWeapon.cyclingBolt;
			if (cyclingBolt != null && !cyclingBolt.Live && !cyclingBolt.IsMoving && (cyclingBolt.CarryingLiveRound || (activeWeapon.Magazine != null && !activeWeapon.Magazine.Empty)))
			{
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
