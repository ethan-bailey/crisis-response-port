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
	[TaskCategory("Character/Combat")]
	[TaskIcon("Assets/Editor/Behavior icons/skull.png")]
	[TaskDescription("Returns success if there is a friendly in the line of fire. The test assumes precision mode is used.")]
	public class FriendlyInLineOfFire : Conditional
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
			float weaponSkill = self.Blackboard.weaponSkill;
			Ray ray = new Ray(activeWeapon.Muzzle.position + activeWeapon.Muzzle.forward * activeWeapon.PrecisionBegin, activeWeapon.Muzzle.forward);
			RaycastHit[] array = Physics.RaycastAll(ray, activeWeapon.PrecisionDistance * weaponSkill, Crisis.Runtime.FullSource.Masks.Instance.Limbs);
			Debug.DrawLine(ray.origin, ray.origin + activeWeapon.Muzzle.forward * activeWeapon.PrecisionDistance, Color.red, 0.5f);
			RaycastHit[] array2 = array;
			foreach (RaycastHit raycastHit in array2)
			{
				Limb component = raycastHit.collider.GetComponent<Limb>();
				if (!(component == null) && component.Body.team == self.team)
				{
					return TaskStatus.Success;
				}
			}
			return TaskStatus.Failure;
		}
	}
}
