using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Characters.Behavior.Conditions.Operators
{
	[TaskCategory("Operator")]
	public class SecureWeaponCondition : Conditional
	{
		private CharacterBody self;

		private TeamOperator op;

		private Mission mission;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			op = gameObject.GetComponent<TeamOperator>();
			mission = Global.Context.Mission;
		}

		public override TaskStatus OnUpdate()
		{
			if (self.PendingAction == CharacterBody.CharacterAction.SecureWeapon)
			{
				return TaskStatus.Success;
			}
			WeaponBase weaponBase = null;
			if (mission.AllRoomsChecked && mission.Occupants.AllRenderedSafe)
			{
				float num = float.PositiveInfinity;
				foreach (Transform item in mission.WeaponsContainer)
				{
					WeaponBase component = item.GetComponent<WeaponBase>();
					if (!component.PendingPickup)
					{
						float num2 = Mathf.Abs(item.position.x - self.Position);
						if (num2 < num)
						{
							num = num2;
							weaponBase = component;
						}
					}
				}
			}
			else if (op.secureWeapons && op.TeamPosition > 2)
			{
				foreach (WeaponBase item2 in self.FindWeapons(self.VisibleRange))
				{
					if (op.Leader.Body.Position - item2.Position >= 1f)
					{
						weaponBase = item2;
						break;
					}
				}
			}
			if (weaponBase != null)
			{
				weaponBase.PendingPickup = true;
				self.Blackboard.SecureWeaponTarget = weaponBase;
				self.PendingAction = CharacterBody.CharacterAction.SecureWeapon;
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}
