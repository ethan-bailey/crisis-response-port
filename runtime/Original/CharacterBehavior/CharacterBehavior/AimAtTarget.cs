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
	[TaskDescription("Aim my weapon at my target. Returns success when aligned on target.")]
	public class AimAtTarget : Action
	{
		public SharedTarget target;

		[Tooltip("If not set, will return status Running until target is aligned.")]
		public bool continuos;

		private CharacterBody self;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override TaskStatus OnUpdate()
		{
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon == null || target.Value == null)
			{
				return TaskStatus.Failure;
			}
			self.IsLoweringWeapon = false;
			if (continuos)
			{
				float num = self.WeaponAimAlignment();
				if (num > self.WeaponAimTolerance)
				{
					return TaskStatus.Running;
				}
			}
			return TaskStatus.Success;
		}
	}
}
