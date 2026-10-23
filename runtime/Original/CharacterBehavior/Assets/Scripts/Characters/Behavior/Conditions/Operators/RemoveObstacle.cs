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
	public class RemoveObstacle : Action
	{
		private enum State
		{
			MoveIntoPosition = 0,
			LowerWeapons = 1,
			ReachForObstacle = 2,
			Wait = 3,
			Abort = 4
		}

		private const float ExecutionTime = 0.25f;

		private State state;

		private TeamOperator op;

		private CharacterBody self;

		private UIInteractionMenu interactionMenu;

		private UIInteriorModeTextIndicator indicator;

		private BallisticObstacle target;

		private float movePosition;

		private float startWaitTimestamp;

		public override void OnAwake()
		{
			op = gameObject.GetComponent<TeamOperator>();
			self = gameObject.GetComponent<CharacterBody>();
			indicator = Global.Context.Mission.UI.indicatorLowerLeft;
			interactionMenu = Global.Context.Mission.InteriorMode.UI.InteractionMenu;
		}

		public override void OnStart()
		{
			WeaponBase activeWeapon = self.ActiveWeapon;
			state = State.MoveIntoPosition;
			target = self.FindObstacle(2f);
			movePosition = target.transform.position.x;
			startWaitTimestamp = float.PositiveInfinity;
		}

		public override void OnEnd()
		{
			if (self.IsPlayer)
			{
				interactionMenu.SetActionComplete();
			}
			self.leftArm.SetOnWeapon(self.ActiveWeapon, 0.5f, false);
			self.ClearPendingAction();
		}

		public override TaskStatus OnUpdate()
		{
			if (target == null)
			{
				return TaskStatus.Success;
			}
			if (self.IsPlayer && (Input.anyKeyDown || self.PendingAction != CharacterBody.CharacterAction.RemoveObstacle))
			{
				state = State.Abort;
				interactionMenu.CancelAction();
			}
			switch (state)
			{
			case State.MoveIntoPosition:
			{
				float f = movePosition - self.Position;
				if (self.Blackboard.LastCollider != null && self.Blackboard.LastCollider.GetComponent<BallisticObstacle>() != null)
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
			case State.LowerWeapons:
				if (self.armController.RightSequencer.IsReady)
				{
					state++;
					self.leftArm.TargetTransition(target.transform, 0.25f, Crisis.Runtime.FullSource.Hand.Mode.ForwardAssist);
				}
				return TaskStatus.Running;
			case State.ReachForObstacle:
				if (self.rightArm.OnTarget)
				{
					state++;
					startWaitTimestamp = Time.time;
				}
				return TaskStatus.Running;
			case State.Wait:
				if (Time.time - startWaitTimestamp >= 0.25f)
				{
					if (target != null)
					{
						self.leftArm.ReturnTransforms();
						target.Remove(self);
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
