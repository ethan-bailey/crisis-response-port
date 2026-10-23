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
	[TaskCategory("Weapon")]
	[TaskIcon("Assets/Editor/Behavior icons/bolt.png")]
	public class WorkBolt : Action
	{
		private enum Action
		{
			Open = 0,
			Close = 1
		}

		private CharacterBody self;

		private WeaponBase weapon;

		private CyclingBolt bolt;

		private Action action;

		private ArmIKSolver arm;

		private bool manipulationStarted;

		private float waitTimestamp;

		private float skillDelay;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			skillDelay = (1f - self.Blackboard.weaponSkill) * 1f;
		}

		public override void OnStart()
		{
			waitTimestamp = Time.time;
			weapon = self.ActiveWeapon;
			bolt = weapon.cyclingBolt;
			manipulationStarted = false;
			action = (bolt.IsOpen ? Action.Close : Action.Open);
			if (weapon.boltManipulationTransitionTime == 0f)
			{
				return;
			}
			if (action == Action.Close)
			{
				arm = ((!weapon.leftHandBoltClose) ? self.rightArm : self.leftArm);
				switch (weapon.boltReleaseAnimation)
				{
				case WeaponBase.BoltReleaseAnimation.NoAnimation:
					return;
				case WeaponBase.BoltReleaseAnimation.ControlPosition:
				{
					Vector3 worldSpaceOffset = weapon.transform.TransformVector(weapon.boltReleasePosition);
					arm.TargetTransition(weapon.transform, worldSpaceOffset, weapon.boltManipulationTransitionTime);
					break;
				}
				case WeaponBase.BoltReleaseAnimation.BoltHandle:
					arm.TargetTransition(weapon.boltHandleTransform, weapon.boltManipulationTransitionTime);
					break;
				}
			}
			else
			{
				arm = ((!weapon.leftHandBoltManipulation) ? self.rightArm : self.leftArm);
				arm.TargetTransition(weapon.boltHandleTransform, weapon.boltManipulationTransitionTime);
			}
			self.IsManipulatingEquipment = true;
		}

		public override void OnEnd()
		{
			self.PendingAction = CharacterBody.CharacterAction.None;
			self.leftArm.SetOnWeapon(weapon, weapon.boltManipulationTransitionTime, false);
			self.rightArm.SetOnWeapon(weapon, weapon.boltManipulationTransitionTime);
			self.IsManipulatingEquipment = false;
		}

		public override TaskStatus OnUpdate()
		{
			if (self.IsPlayer && self.PendingAction != CharacterBody.CharacterAction.WorkBolt)
			{
				return TaskStatus.Failure;
			}
			if (!self.IsPlayer && Time.time - waitTimestamp < skillDelay)
			{
				return TaskStatus.Running;
			}
			if (manipulationStarted && bolt.Status != CyclingBolt.State.Opening)
			{
				return TaskStatus.Failure;
			}
			if ((arm != null && !arm.OnTarget) || bolt.Status == CyclingBolt.State.Opening)
			{
				return TaskStatus.Running;
			}
			if (!manipulationStarted)
			{
				manipulationStarted = true;
				switch (action)
				{
				case Action.Open:
					bolt.Open(true);
					break;
				case Action.Close:
					if (arm != null)
					{
						arm.SetOnWeapon(weapon, weapon.boltManipulationTransitionTime, !weapon.leftHandBoltManipulation);
					}
					bolt.Close(true);
					break;
				}
			}
			return TaskStatus.Running;
		}
	}
}
