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
	public class ApplyGauze : Action
	{
		private enum State
		{
			MoveIntoPosition = 0,
			LowerWeapon = 1,
			FinishedLowerWeapon = 2,
			GrabGauze = 3,
			OnWound = 4,
			ApplyGauze = 5,
			Abort = 6
		}

		private static float ApplyPeriod = 3f;

		private CharacterBody self;

		private TeamOperator op;

		private CharacterBody target;

		private State state;

		private Transform wound;

		private Vector3 woundOffset;

		private Crisis.Runtime.FullSource.InputHandler input;

		private UIInteractionMenu interactionMenu;

		private bool wasKneeling;

		private bool shouldKneel;

		private bool targetIsSelf;

		private float targetPosition;

		private float movePosition;

		private float holdPosition;

		private float applyTime;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			op = gameObject.GetComponent<TeamOperator>();
			input = Global.Context.InputHandler;
			interactionMenu = Global.Context.Mission.InteriorMode.UI.InteractionMenu;
		}

		public override void OnStart()
		{
			wasKneeling = self.IsKneeling;
			target = self.Blackboard.PendingGauzeTarget;
			targetIsSelf = target == self;
			state = (targetIsSelf ? State.LowerWeapon : State.MoveIntoPosition);
			if (target == self)
			{
				if (self.Head.LimbPhysiology.Wounded)
				{
					wound = self.Head.transform;
					woundOffset = new Vector3(0.05f, Random.Range(0f, 0.2f), 0f) * self.Direction;
				}
				else if (self.Neck.LimbPhysiology.Wounded)
				{
					wound = self.Neck.transform;
					woundOffset = new Vector3(Random.Range(0.05f, 0.15f), 0f, 0f) * self.Direction;
				}
				else if (self.Torso.LimbPhysiology.Wounded || self.Abdomen.LimbPhysiology.Wounded || self.Pelvis.LimbPhysiology.Wounded)
				{
					wound = self.Torso.transform;
					woundOffset = new Vector3(Random.Range(0.2f, 0.25f), Random.Range(0f, 0.3f), 0f) * self.Direction;
				}
				else
				{
					wound = self.LeftLowerLeg.transform;
					woundOffset = Vector3.zero;
				}
			}
			else
			{
				wound = target.ApplyGauzePosition(out woundOffset);
			}
			targetPosition = wound.position.x;
			shouldKneel = wound.position.y < 0.6f;
			float num = Mathf.Sign(targetPosition - self.Position);
			movePosition = wound.position.x - num * 0.75f;
		}

		public override void OnEnd()
		{
			if (target != null)
			{
				target.PendingGauze = false;
				target = null;
			}
			op.RemoveGauzeFromHand();
			self.HeadMode = CharacterBody.HeadLookMode.AimPoint;
			self.Blackboard.PendingGauzeTarget = null;
			self.armController.ResumePreviousWeapon();
			self.IsKneeling = wasKneeling;
			if (self.IsPlayer)
			{
				interactionMenu.CancelAction();
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (self.IsPlayer && Input.anyKeyDown)
			{
				state = State.Abort;
			}
			switch (state)
			{
			case State.MoveIntoPosition:
			{
				if (!self.Facing(targetPosition) && StaticBehavior.TurnAround(self) != TaskStatus.Success)
				{
					return TaskStatus.Running;
				}
				float f = movePosition - self.Position;
				if (Mathf.Abs(f) > 0.1f)
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
			case State.FinishedLowerWeapon:
				if (self.armController.RightSequencer.IsReady)
				{
					state++;
					self.rightArm.TargetTransition(op.medicPouchTransform, 0.3f);
				}
				return TaskStatus.Running;
			case State.GrabGauze:
				if (self.rightArm.OnTarget)
				{
					state++;
					op.InstantiateGauzeInHand();
					self.IsKneeling = shouldKneel;
					self.rightArm.TargetTransition(wound, woundOffset, 0.3f);
					self.leftArm.TargetTransition(wound, woundOffset, 0.3f);
				}
				return TaskStatus.Running;
			case State.OnWound:
			{
				float f = self.rightArm.TransitionTarget.position.x - self.rightArm.hand.grabPoint.position.x;
				if (Mathf.Abs(f) > 0.05f)
				{
					self.Move(Mathf.Sign(f));
				}
				if (self.rightArm.OnTarget)
				{
					state++;
					applyTime = 0f;
					holdPosition = self.Position;
				}
				return TaskStatus.Running;
			}
			case State.ApplyGauze:
				if (self.IsKneeling != shouldKneel || Mathf.Abs(holdPosition - self.Position) > 0.3f)
				{
					state = State.Abort;
					return TaskStatus.Running;
				}
				applyTime += Time.deltaTime;
				if (applyTime >= ApplyPeriod)
				{
					self.RemoveGauzeItem();
					self.StabilizeBody(target);
					if (self.IsPlayer)
					{
						interactionMenu.SetActionComplete();
					}
					return TaskStatus.Success;
				}
				return TaskStatus.Running;
			case State.Abort:
				return TaskStatus.Success;
			default:
				return TaskStatus.Success;
			}
		}
	}
}
