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
	public class CheckVitals : Action
	{
		private enum State
		{
			MoveIntoPosition = 0,
			LowerWeapons = 1,
			ReachForNeck = 2,
			Wait = 3,
			DisplayResult = 4,
			Abort = 5
		}

		private State state;

		private CharacterBody self;

		private CharacterBody target;

		private UIInteractionMenu interactionMenu;

		private UIInteriorModeTextIndicator indicator;

		private bool wasKneeling;

		private bool targetIsOnFloor;

		private float movePosition;

		private float startWaitTimestamp;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			indicator = Global.Context.Mission.UI.indicatorLowerLeft;
			interactionMenu = Global.Context.Mission.InteriorMode.UI.InteractionMenu;
		}

		public override void OnStart()
		{
			WeaponBase activeWeapon = self.ActiveWeapon;
			state = State.MoveIntoPosition;
			target = self.Blackboard.CharacterInteractionTarget;
			targetIsOnFloor = target.Neck.transform.position.y < 0.7f;
			movePosition = target.Neck.transform.position.x - self.Direction * ((!targetIsOnFloor) ? 0.6f : 0.3f);
			wasKneeling = self.IsKneeling;
			startWaitTimestamp = float.PositiveInfinity;
		}

		public override TaskStatus OnUpdate()
		{
			if (target == null)
			{
				return TaskStatus.Failure;
			}
			if (self.IsPlayer)
			{
				bool flag = state < State.ReachForNeck && Input.anyKeyDown;
				bool flag2 = state >= State.ReachForNeck && Mathf.Abs(self.LeftHand.transform.position.x - target.Neck.transform.position.x) > 0.5f;
				if (flag || flag2)
				{
					state = State.Abort;
					interactionMenu.CancelAction();
				}
			}
			switch (state)
			{
			case State.MoveIntoPosition:
			{
				float f = movePosition - self.Position;
				if (Mathf.Abs(f) > 0.1f)
				{
					self.Move(Mathf.Sign(f));
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
					self.IsKneeling = targetIsOnFloor;
					self.leftArm.TargetTransition(target.Neck.transform, 0.5f, Crisis.Runtime.FullSource.Hand.Mode.ForwardAssist);
				}
				return TaskStatus.Running;
			case State.ReachForNeck:
				if (self.rightArm.OnTarget)
				{
					state++;
					startWaitTimestamp = Time.time;
				}
				return TaskStatus.Running;
			case State.Wait:
				if (Time.time - startWaitTimestamp >= 2f)
				{
					state++;
				}
				return TaskStatus.Running;
			case State.DisplayResult:
			{
				state++;
				string text = target.Physiology.Summary();
				indicator.Indicate(text, Mathf.Clamp((float)text.Length * 0.1f, 1f, 4f));
				interactionMenu.SetActionComplete();
				return TaskStatus.Running;
			}
			case State.Abort:
				self.IsKneeling = wasKneeling;
				self.leftArm.ReturnTransforms();
				self.leftArm.SetOnWeapon(self.ActiveWeapon, 0.5f, false);
				self.ClearPendingAction();
				return TaskStatus.Success;
			default:
				return TaskStatus.Success;
			}
		}
	}
}
