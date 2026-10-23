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
	[TaskIcon("Assets/Editor/Behavior icons/barrel.png")]
	[TaskDescription("Aims the character's weapon. If continuos is not set, it returns Success when the weapon has reached the target angle. If Aggression is selected, the character's aggression value determines the angle. If Hold Height is selected it will use the specified height.")]
	public class WeaponAngle : Action
	{
		public enum Type
		{
			Aggression = 0,
			Down = 1,
			HoldHeight = 2
		}

		public Type type;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("The behavior will run continuosly, returning status Running. Otherwise it will return Success when weapon has reached the target angle.")]
		public bool continuos;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("When type Hold Height is selected, the weapon muzzle will be held toward this height 3 meters out.")]
		public float holdHeight;

		[BehaviorDesigner.Runtime.Tasks.Tooltip("If the action is not continuos and runs for longer than this time it will return failure. In seconds.")]
		public float failAfterSeconds = 3f;

		private CharacterBody self;

		private float startTime;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
		}

		public override void OnStart()
		{
			startTime = Time.time;
		}

		public override TaskStatus OnUpdate()
		{
			if (!continuos && Time.time - startTime >= failAfterSeconds)
			{
				return TaskStatus.Failure;
			}
			WeaponBase activeWeapon = self.ActiveWeapon;
			if (activeWeapon == null)
			{
				return TaskStatus.Failure;
			}
			float num = 0f;
			float y = 0f;
			switch (type)
			{
			case Type.Aggression:
				num = self.ActiveWeapon.armConfiguration.retractDistance + self.Blackboard.aggression * 5f;
				break;
			case Type.Down:
				num = self.ActiveWeapon.armConfiguration.retractDistance;
				break;
			case Type.HoldHeight:
				num = 3f;
				y = holdHeight;
				break;
			}
			self.AimPoint = new Vector3(self.transform.position.x + num * self.Direction, y, self.transform.position.z);
			if (continuos || self.WeaponAimAlignment() > 10f)
			{
				return TaskStatus.Running;
			}
			return TaskStatus.Success;
		}
	}
}
