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
	public class SecureWeapon : Action
	{
		private enum State
		{
			MoveIntoPosition = 0,
			LowerWeapons = 1,
			CollectWeapon = 2,
			Abort = 3
		}

		private State state;

		private TeamOperator op;

		private WeaponBase target;

		private CharacterBody self;

		private UIInteractionMenu interactionMenu;

		private bool wasKneeling;

		private float movePosition;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			op = gameObject.GetComponent<TeamOperator>();
			interactionMenu = Global.Context.Mission.InteriorMode.UI.InteractionMenu;
		}

		public override void OnStart()
		{
			wasKneeling = self.IsKneeling;
			state = State.MoveIntoPosition;
			target = self.Blackboard.SecureWeaponTarget;
			if (target != null)
			{
				target.PendingPickup = true;
				movePosition = target.rightHandAnchor.position.x - self.Direction * 0.5f;
				if (self.Blackboard.EquipCollectedWeapon && !self.HasFreeWeaponSlot)
				{
					self.DropWeapon(self.ActiveWeapon);
				}
			}
		}

		public override void OnEnd()
		{
			if (self.Blackboard.SecureWeaponTarget != null)
			{
				self.Blackboard.SecureWeaponTarget.PendingPickup = false;
				self.Blackboard.SecureWeaponTarget = null;
			}
			self.IsKneeling = wasKneeling;
			self.leftArm.SetOnWeapon(self.ActiveWeapon, 0.5f, false);
			self.ClearPendingAction();
			if (self.IsPlayer)
			{
				interactionMenu.CancelAction();
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (target == null)
			{
				return TaskStatus.Success;
			}
			if (self.IsPlayer && (Input.anyKeyDown || self.PendingAction != CharacterBody.CharacterAction.SecureWeapon))
			{
				state = State.Abort;
			}
			switch (state)
			{
			case State.MoveIntoPosition:
			{
				float f = movePosition - self.Position;
				float num = Mathf.Abs(f);
				if (num > 0.1f)
				{
					self.Move(Mathf.Sign(f));
					self.IsSprinting = num > 2f && op.Team.tempo != CharacterBody.Tempo.Slow;
				}
				else
				{
					state++;
				}
				return TaskStatus.Running;
			}
			case State.LowerWeapons:
				if (self.armController.RightSequencer.IsReady)
				{
					state++;
					self.IsKneeling = true;
					self.leftArm.TargetTransition(target.leftHandAnchor, 0.5f);
				}
				return TaskStatus.Running;
			case State.CollectWeapon:
				if (!self.Facing(target.rightHandAnchor.position.x) && StaticBehavior.TurnAround(self) != TaskStatus.Success)
				{
					return TaskStatus.Running;
				}
				if (self.KneelRatio >= 1f && self.leftArm.OnTarget)
				{
					state++;
					self.leftArm.ReturnTransforms();
					if (self.Blackboard.EquipCollectedWeapon)
					{
						self.PickupAndEquipWeapon(target, self.ActiveWeapon == null);
					}
					else
					{
						Object.Destroy(target.gameObject);
					}
					if (self.IsPlayer)
					{
						interactionMenu.SetActionComplete();
					}
				}
				return TaskStatus.Running;
			case State.Abort:
				return TaskStatus.Success;
			default:
				return TaskStatus.Failure;
			}
		}
	}
}
