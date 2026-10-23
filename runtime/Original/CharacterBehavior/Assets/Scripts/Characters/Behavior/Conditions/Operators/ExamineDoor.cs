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
	public class ExamineDoor : Action
	{
		private enum State
		{
			MoveIntoPosition = 0,
			LowerWeapon = 1,
			PerformStep1 = 2,
			PerformStep2 = 3,
			PerformStep3 = 4,
			PerformStep4 = 5,
			PerformStep5 = 6,
			Finish = 7,
			Abort = 8
		}

		private State state;

		private TeamOperator op;

		private CharacterBody self;

		private DoorController target;

		private UIInteractionMenu interactionMenu;

		private bool wasKneeling;

		private float movePosition;

		private float timestamp;

		private const float ExamineStepPeriod = 1.25f;

		public override void OnAwake()
		{
			op = gameObject.GetComponent<TeamOperator>();
			self = gameObject.GetComponent<CharacterBody>();
			interactionMenu = Global.Context.Mission.InteriorMode.UI.InteractionMenu;
		}

		public override void OnStart()
		{
			wasKneeling = self.IsKneeling;
			state = State.MoveIntoPosition;
			target = self.Room.NextDoor(self.Direction);
			if (target != null)
			{
				float x = target.Knob(self.Position).position.x;
				float num = Mathf.Sign(x - self.Position);
				movePosition = x - 0.3f * num;
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
			if (target == null)
			{
				return TaskStatus.Success;
			}
			if (self.IsPlayer && (Input.anyKeyDown || self.PendingAction != CharacterBody.CharacterAction.ExamineDoor))
			{
				state = State.Abort;
			}
			switch (state)
			{
			case State.MoveIntoPosition:
			{
				float f = movePosition - self.Position;
				if (self.Blackboard.LastCollider == target.physicalDoor)
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
				self.armController.LowerCurrentWeapon();
				state++;
				return TaskStatus.Running;
			case State.PerformStep1:
				if (self.armController.RightSequencer.IsReady)
				{
					state++;
					self.HeadMode = CharacterBody.HeadLookMode.RightHand;
					self.IsKneeling = true;
					self.leftArm.TargetTransition(target.Knob(self.Position), new Vector3(0f, -0.1f, 0f), 1.25f);
					self.rightArm.TargetTransition(target.Knob(self.Position), new Vector3(0f, -0.5f, 0f), 1.25f);
				}
				return TaskStatus.Running;
			case State.PerformStep2:
				if (!self.Facing(target.Knob(self.Position).position.x) && StaticBehavior.TurnAround(self) != TaskStatus.Success)
				{
					return TaskStatus.Running;
				}
				if (self.KneelRatio >= 1f && self.leftArm.OnTarget)
				{
					state++;
					self.leftArm.TargetTransition(target.Knob(self.Position), new Vector3(0f, 0.2f, 0f), 1.25f);
					self.rightArm.TargetTransition(target.Knob(self.Position), new Vector3(0f, 1f, 0f), 1.25f);
					self.IsKneeling = false;
					timestamp = Time.time;
				}
				return TaskStatus.Running;
			case State.PerformStep3:
				if (Time.time - timestamp >= 0.625f)
				{
					state++;
				}
				return TaskStatus.Running;
			case State.PerformStep4:
				if (Time.time - timestamp >= 0.625f)
				{
					self.leftArm.TargetTransition(target.Knob(self.Position), new Vector3(0f, -0.2f, 0f), 1.25f);
					self.rightArm.TargetTransition(target.Knob(self.Position), new Vector3(0f, -1f, 0f), 1.25f);
					self.IsKneeling = true;
					timestamp = Time.time;
					state++;
				}
				return TaskStatus.Running;
			case State.PerformStep5:
				if (Time.time - timestamp >= 1.25f)
				{
					state++;
				}
				return TaskStatus.Running;
			case State.Finish:
				if (self.IsPlayer)
				{
					interactionMenu.TextIndicator.Indicate(string.Format("The door is {0}", target.ExamineAndReturnStatusString()), 2f, false);
					interactionMenu.SetActionComplete();
				}
				else
				{
					target.Examine();
				}
				return TaskStatus.Success;
			case State.Abort:
				return TaskStatus.Success;
			default:
				return TaskStatus.Failure;
			}
		}
	}
}
