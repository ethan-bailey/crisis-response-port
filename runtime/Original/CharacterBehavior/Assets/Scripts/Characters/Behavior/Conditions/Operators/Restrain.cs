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
	[TaskDescription("Sets the operator's aim mode.")]
	public class Restrain : Action
	{
		private enum State
		{
			MoveIntoPosition = 0,
			GrabCuff = 1,
			CollectLeftHand = 2,
			GrabLeftHand = 3,
			MoveToRightHand = 4,
			CollectRightHand = 5,
			GrabRightHand = 6,
			MoveBack = 7,
			CuffHands = 8,
			Abort = 9
		}

		private State state;

		private TeamOperator op;

		private CharacterBody self;

		private CharacterBody target;

		private UIInteractionMenu interactionMenu;

		private SpringJoint leftSpring;

		private SpringJoint rightSpring;

		private bool wasKneeling;

		public override void OnAwake()
		{
			self = gameObject.GetComponent<CharacterBody>();
			op = gameObject.GetComponent<TeamOperator>();
			interactionMenu = Global.Context.Mission.InteriorMode.UI.InteractionMenu;
		}

		public override void OnStart()
		{
			state = State.MoveIntoPosition;
			target = self.Blackboard.PendingRestrainTarget;
			target.PendingRestrain = true;
			wasKneeling = self.IsKneeling;
		}

		public override void OnEnd()
		{
			if (target != null)
			{
				target.PendingRestrain = false;
				target.armController.Unarmed();
				target = null;
			}
			if (leftSpring != null)
			{
				Object.Destroy(leftSpring);
			}
			if (rightSpring != null)
			{
				Object.Destroy(rightSpring);
			}
			self.HeadMode = CharacterBody.HeadLookMode.AimPoint;
			op.DestroyPlasticuffInHand();
			self.armController.ResumePreviousWeapon();
			self.Blackboard.PendingRestrainTarget = null;
			self.IsKneeling = wasKneeling;
			if (self.IsPlayer)
			{
				interactionMenu.CancelAction();
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (target.IsRestrained)
			{
				return TaskStatus.Failure;
			}
			if (target.IsMoving)
			{
				state = State.Abort;
			}
			if (self.IsPlayer && Input.anyKeyDown)
			{
				state = State.Abort;
			}
			switch (state)
			{
			case State.MoveIntoPosition:
			{
				float x = target.LeftHand.transform.position.x;
				float num = Mathf.Sign(x - self.Position);
				float num2 = x - num * 0.5f;
				float f = num2 - self.Position;
				if (!self.Facing(num2) && StaticBehavior.TurnAround(self) != TaskStatus.Success)
				{
					return TaskStatus.Running;
				}
				if (Mathf.Abs(f) > 0.1f)
				{
					self.Move(Mathf.Sign(f));
					self.IsSprinting = Mathf.Abs(f) > 2f && op.Team.tempo != CharacterBody.Tempo.Slow;
				}
				else
				{
					state++;
					self.armController.LowerCurrentWeapon();
					self.leftArm.TargetTransition(op.plasticuffsVisual.transform, 0.3f);
				}
				return TaskStatus.Running;
			}
			case State.GrabCuff:
				if (self.armController.RightSequencer.IsReady && self.leftArm.OnTarget)
				{
					state++;
					op.InstantiatePlasticuffInHand();
				}
				return TaskStatus.Running;
			case State.CollectLeftHand:
				if (!self.Facing(target.LeftHand.transform.position.x) && StaticBehavior.TurnAround(self) != TaskStatus.Success)
				{
					return TaskStatus.Running;
				}
				if (target.IsRagdoll)
				{
					self.HeadMode = CharacterBody.HeadLookMode.RightHand;
				}
				else
				{
					target.RestrainArm(self.LeftHand.transform, true);
				}
				state++;
				if (target.LeftHand.transform.position.y < 0.5f)
				{
					self.IsKneeling = true;
				}
				self.leftArm.TargetTransition(target.LeftHand.transform, 0.5f);
				return TaskStatus.Running;
			case State.GrabLeftHand:
				if (self.leftArm.OnTarget)
				{
					state++;
					if (target.IsRagdoll)
					{
						leftSpring = AddSpring(target.LeftHand.gameObject, self.LeftHand.Rigidbody, new Vector3(0f, -0.08f, 0f), 1000f);
					}
					target.leftArm.hand.SetMode(Crisis.Runtime.FullSource.Hand.Mode.Fist);
					target.InstantiatePlasticuffOnLeftHand();
					op.DestroyPlasticuffInHand();
					self.Audio.Plasticuffs();
					float num2 = target.RightHand.transform.position.x - self.Direction * 0.25f;
				}
				return TaskStatus.Running;
			case State.MoveToRightHand:
			{
				self.HeadMode = CharacterBody.HeadLookMode.AimPoint;
				float x = target.RightHand.transform.position.x;
				float num = Mathf.Sign(x - self.Position);
				float num2 = x - num * 0.5f;
				float f = num2 - self.Position;
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
			case State.CollectRightHand:
				if (!self.Facing(target.RightHand.transform.position.x) && StaticBehavior.TurnAround(self) != TaskStatus.Success)
				{
					return TaskStatus.Running;
				}
				state++;
				if (target.RightHand.transform.position.y < 0.5f)
				{
					self.IsKneeling = true;
				}
				self.rightArm.TargetTransition(target.RightHand.transform, 0.5f);
				if (target.IsRagdoll)
				{
					self.HeadMode = CharacterBody.HeadLookMode.RightHand;
				}
				else
				{
					target.RestrainArm(self.RightHand.transform, false);
				}
				return TaskStatus.Running;
			case State.GrabRightHand:
				if (self.rightArm.OnTarget)
				{
					state++;
					self.IsKneeling = false;
					float num2 = target.Torso.transform.position.x - self.Direction * -0.25f;
					if (target.IsRagdoll)
					{
						rightSpring = AddSpring(target.RightHand.gameObject, self.RightHand.Rigidbody, new Vector3(0f, -0.08f, 0f), 1000f);
					}
					Vector3 vector4 = target.Torso.transform.TransformVector(new Vector3(-0.3f, 0f, 0f));
					target.rightArm.hand.SetMode(Crisis.Runtime.FullSource.Hand.Mode.Fist);
				}
				return TaskStatus.Running;
			case State.MoveBack:
			{
				state++;
				self.HeadMode = CharacterBody.HeadLookMode.AimPoint;
				Vector3 worldSpaceOffset = target.Torso.transform.TransformVector(new Vector3(-0.3f, 0f, 0f));
				self.rightArm.TargetTransition(target.Torso.transform, worldSpaceOffset, 0.5f);
				self.leftArm.TargetTransition(target.Torso.transform, worldSpaceOffset, 0.5f);
				return TaskStatus.Running;
			}
			case State.CuffHands:
			{
				Vector3 vector = target.LeftHand.transform.position - target.RightHand.transform.position;
				Vector3 vector2 = target.RightHand.transform.position - self.RightHand.transform.position;
				Vector3 vector3 = target.LeftHand.transform.position - self.LeftHand.transform.position;
				if (target.IsRagdoll)
				{
					self.HeadMode = CharacterBody.HeadLookMode.RightHand;
				}
				else
				{
					target.armController.Restrained();
				}
				target.Restrain();
				self.Audio.Plasticuffs();
				if (self.IsPlayer)
				{
					interactionMenu.SetActionComplete();
				}
				return TaskStatus.Success;
			}
			case State.Abort:
				return TaskStatus.Success;
			default:
				return TaskStatus.Failure;
			}
		}

		private SpringJoint AddSpring(GameObject gameObject, Rigidbody source, Vector3 anchorOffset, float force)
		{
			SpringJoint springJoint = gameObject.AddComponent<SpringJoint>();
			springJoint.autoConfigureConnectedAnchor = false;
			springJoint.connectedAnchor = anchorOffset;
			springJoint.connectedBody = source;
			springJoint.spring = force;
			return springJoint;
		}
	}
}
