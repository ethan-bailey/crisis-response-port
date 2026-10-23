using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using BehaviorDesigner.Runtime;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Locomotion")]
	[TaskIcon("Assets/Editor/Behavior icons/feet.png")]
	public class TurnAround : Action
	{
		private CharacterBody self;

		private bool resumeLookThroughSights;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			self.IsLoweringWeapon = true;
		}

		public override TaskStatus OnUpdate()
		{
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon != null && !activeWeapon.IsBlocked)
			{
				self.AimPoint = new Vector3(self.transform.position.x + activeWeapon.armConfiguration.retractDistance * self.Direction, 0f, self.transform.position.z);
				Vector3 position = activeWeapon.Muzzle.position;
				Vector3 right = activeWeapon.transform.right;
				Vector3 vector = self.AimPoint - position;
				float num = Vector3.Angle(vector, right);
				if (num > 10f)
				{
					return TaskStatus.Running;
				}
			}
			self.Turn();
			return TaskStatus.Success;
		}

		public override void OnEnd()
		{
			self.IsLoweringWeapon = false;
		}
	}
}
