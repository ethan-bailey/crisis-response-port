using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

using Crisis.Runtime.FullSource;
using Object = UnityEngine.Object;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace CharacterBehavior
{
	[TaskCategory("Character/Combat")]
	[TaskIcon("Assets/Editor/Behavior icons/muzzle.png")]
	[TaskDescription("Returns failure if the distance from the active weapon's muzzle to the aimpoint is too small.")]
	public class MuzzleFree : Conditional
	{
		[BehaviorDesigner.Runtime.Tasks.Tooltip("The least clearance required. In meters.")]
		public float minimumClearance = 0.5f;

		public SharedTarget target;

		public float required;

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
			Vector3 vector = target.Value.transform.position;
			CharacterBody body = target.Value.Body;
			if (body != null)
			{
				vector = ((!self.Blackboard.preferHeadTarget) ? body.Torso.AimPoint : body.Head.AimPoint);
			}
			Vector3 to = vector - self.RightUpperArm.transform.position;
			float angle = Vector3.Angle(self.transform.right, to) + 90f;
			float num = self.ActiveWeapon.RequiredCleareance(angle) + minimumClearance;
			float num2 = Mathf.Abs(to.x);
			if (num2 < num)
			{
				Debug.DrawLine(self.RightUpperArm.transform.position, self.AimPoint, Color.red);
				return TaskStatus.Failure;
			}
			Debug.DrawLine(self.RightUpperArm.transform.position, self.AimPoint, Color.green);
			return TaskStatus.Success;
		}
	}
}
