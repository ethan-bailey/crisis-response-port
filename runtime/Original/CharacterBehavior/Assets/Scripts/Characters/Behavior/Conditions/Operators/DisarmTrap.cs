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
	public class DisarmTrap : Action
	{
		private enum State
		{
			MoveIntoPosition = 0,
			LowerWeapon = 1,
			PerformStep1 = 2,
			PerformStep2 = 3,
			PerformStep3 = 4,
			Finish = 5,
			Abort = 6
		}

		private State state;

		private TeamOperator op;

		private CharacterBody self;

		private DoorTrap targetTrap;

		private DoorController targetDoor;

		private UIInteractionMenu interactionMenu;

		private bool wasKneeling;

		private float movePosition;

		private float timestamp;

		private float executePeriod;

		public override void OnAwake()
		{
			op = gameObject.GetComponent<TeamOperator>();
			self = gameObject.GetComponent<CharacterBody>();
			executePeriod = ((!self.Equipment.multiTool) ? 3f : 0.75f);
			interactionMenu = Global.Context.Mission.InteriorMode.UI.InteractionMenu;
		}

		public override void OnStart()
		{
			wasKneeling = self.IsKneeling;
			state = State.MoveIntoPosition;
			targetDoor = self.Room.NextDoor(self.Direction);
			targetTrap = ((!(targetDoor != null)) ? null : targetDoor.Trap);
			if (targetDoor != null)
			{
				float num = Mathf.Sign(targetDoor.Position - self.Position);
				movePosition = targetDoor.Position - 0.3f * num;
			}
		}

		public override void OnEnd()
		{
			self.IsKneeling = wasKneeling;
			self.ClearPendingAction();
			self.armController.ResumePreviousWeapon();
			self.HeadMode = CharacterBody.HeadLookMode.AimPoint;
			if (self.IsPlayer)
			{
				interactionMenu.CancelAction();
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (targetDoor == null)
			{
				return TaskStatus.Success;
			}
			if (self.IsPlayer && (Input.anyKeyDown || self.PendingAction != CharacterBody.CharacterAction.DisarmTrap))
			{
				state = State.Abort;
			}
			switch (state)
			{
			case State.MoveIntoPosition:
			{
				float f = movePosition - self.Position;
				if (self.Blackboard.LastCollider == targetDoor.physicalDoor)
				{
					state++;
				}
				else if (Mathf.Abs(f) > 0.1f)
				{
					self.Move(Mathf.Sign(f));
					self.IsSprinting = Mathf.Abs(f) > 2f && op.Team.tempo != CharacterBody.Tempo.Slow;
				}
				else
				{
					state++;
				}
				return TaskStatus.Running;
			}
			case State.LowerWeapon:
				state++;
				self.armController.LowerCurrentWeapon();
				return TaskStatus.Running;
			case State.PerformStep1:
				if (self.armController.RightSequencer.IsReady)
				{
					state++;
					self.HeadMode = CharacterBody.HeadLookMode.RightHand;
					self.IsKneeling = targetTrap.transform.position.y < 1f;
					self.armController.LowerCurrentWeapon();
					self.leftArm.TargetTransition(targetTrap.transform, 0.5f);
					self.rightArm.TargetTransition(targetTrap.transform, 0.5f);
				}
				return TaskStatus.Running;
			case State.PerformStep2:
				if (!self.Facing(targetDoor.Position) && StaticBehavior.TurnAround(self) != TaskStatus.Success)
				{
					return TaskStatus.Running;
				}
				if (self.leftArm.OnTarget)
				{
					state++;
					timestamp = Time.time;
					targetTrap.PlayDisarmAudio();
				}
				return TaskStatus.Running;
			case State.PerformStep3:
				if (Time.time - timestamp >= executePeriod)
				{
					state++;
				}
				return TaskStatus.Running;
			case State.Finish:
				targetTrap.Disarm();
				if (self.IsPlayer)
				{
					interactionMenu.SetActionComplete();
				}
				return TaskStatus.Success;
			case State.Abort:
				targetTrap.StopDisarmAudio();
				return TaskStatus.Success;
			default:
				return TaskStatus.Failure;
			}
		}
	}
}
