using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class IKArmSequencer
{
	private IKArmController controller;

	private IKArmAction currentAction;

	private ArmIKSolver solver;

	private IKArmType armType;

	private CharacterBody self;

	private IKControllerInvoke callback;

	private IKArmSequencer sibling;

	private bool frameDone;

	private float timestamp;

	private float frameTime;

	private int waitForSiblingFrame;

	public List<IKArmAnimationFrame> Sequence { get; set; }

	public bool IsPlaying
	{
		get
		{
			return Sequence != null;
		}
	}

	public bool IsReady
	{
		get
		{
			return Sequence == null;
		}
	}

	public int CurrentFrame { get; private set; }

	public IKArmSequencer(IKArmController controller, CharacterBody body, IKArmType armType, ArmIKSolver solver)
	{
		this.controller = controller;
		this.armType = armType;
		this.solver = solver;
		self = body;
	}

	public void SetSibling(IKArmSequencer sibling)
	{
		this.sibling = sibling;
	}

	public void Update()
	{
		if (Sequence == null)
		{
			return;
		}
		if (frameDone)
		{
			if (callback != IKControllerInvoke.None)
			{
				Invoke(callback);
				callback = IKControllerInvoke.None;
			}
			NewFrame(++CurrentFrame);
			return;
		}
		switch (currentAction)
		{
		case IKArmAction.Wait:
			frameDone = Time.time - timestamp >= frameTime;
			break;
		case IKArmAction.Transition:
			frameDone = solver.OnTarget;
			break;
		case IKArmAction.WaitUntilLowered:
			frameDone = self.WeaponAimAlignment() <= 10f;
			break;
		case IKArmAction.WaitForSiblingFrame:
			frameDone = sibling.HasFinishedFrame(waitForSiblingFrame);
			break;
		}
	}

	public void Play(List<IKArmAnimationFrame> sequence, int abort = 0)
	{
		Sequence = sequence;
		CurrentFrame = 0;
		NewFrame();
	}

	private void NewFrame(int index = 0)
	{
		if (index >= Sequence.Count)
		{
			Sequence = null;
			CurrentFrame = 0;
			return;
		}
		frameDone = false;
		timestamp = Time.time;
		IKArmAnimationFrame iKArmAnimationFrame = Sequence[index];
		frameTime = iKArmAnimationFrame.time;
		callback = iKArmAnimationFrame.callback;
		currentAction = iKArmAnimationFrame.action;
		waitForSiblingFrame = iKArmAnimationFrame.waitForSiblingFrame;
		Vector3 offset = Vector3.zero;
		IKArmAction action = iKArmAnimationFrame.action;
		if (action == IKArmAction.Transition)
		{
			Transform transform = TargetTransform(iKArmAnimationFrame.transform, armType, out offset);
			if (transform != null)
			{
				if (iKArmAnimationFrame.offset != Vector3.zero)
				{
					offset = transform.TransformVector(iKArmAnimationFrame.offset);
				}
				solver.TargetTransition(transform, offset, iKArmAnimationFrame.time);
			}
		}
		else
		{
			solver.StartWait();
		}
	}

	private Transform TargetTransform(IKArmTarget target, IKArmType armType, out Vector3 offset)
	{
		offset = Vector3.zero;
		switch (target)
		{
		case IKArmTarget.CurrentWeapon:
		{
			WeaponBase weaponBase = self.ActiveWeapon;
			if (weaponBase == null)
			{
				weaponBase = controller.PreviousWeapon;
			}
			return (armType != IKArmType.Right) ? weaponBase.leftHandAnchor : weaponBase.rightHandAnchor;
		}
		case IKArmTarget.PreviousWeapon:
			if (controller.PreviousWeapon == null)
			{
				return null;
			}
			return (armType != IKArmType.Right) ? controller.PreviousWeapon.leftHandAnchor : controller.PreviousWeapon.rightHandAnchor;
		case IKArmTarget.PendingWeapon:
			if (controller.PendingWeapon == null)
			{
				return null;
			}
			return (armType != IKArmType.Right) ? controller.PendingWeapon.leftHandAnchor : controller.PendingWeapon.rightHandAnchor;
		case IKArmTarget.ThighHolster:
			return self.holster;
		case IKArmTarget.GrenadeAnchor:
			return self.stunGrenadeAnchor;
		case IKArmTarget.MagazinePocket:
			return self.chestRigIKTarget;
		case IKArmTarget.PistolIK:
			return self.pistolIKTarget;
		case IKArmTarget.Wound:
			return self.ApplyGauzePosition(out offset);
		case IKArmTarget.MedicPouch:
		{
			TeamOperator component = self.GetComponent<TeamOperator>();
			return (!(component != null)) ? null : component.medicPouchTransform;
		}
		case IKArmTarget.Head:
			return self.Head.transform;
		case IKArmTarget.Pelvis:
			return self.Pelvis.transform;
		default:
			return null;
		}
	}

	private void Invoke(IKControllerInvoke method)
	{
		switch (method)
		{
		case IKControllerInvoke.ActivatePendingWeapon:
			controller.IK_ActivatePendingWeapon();
			break;
		case IKControllerInvoke.ActivatePreviousWeapon:
			controller.IK_ActivatePreviousWeapon();
			break;
		case IKControllerInvoke.HolsterWeapon:
			controller.IK_HolsterWeapon();
			break;
		case IKControllerInvoke.LowerWeapon:
			controller.IK_LowerWeapon();
			break;
		case IKControllerInvoke.PickupGrenade:
			controller.IK_PickupGrenade();
			break;
		case IKControllerInvoke.ReturnGrenade:
			controller.IK_ReturnGrenade();
			break;
		case IKControllerInvoke.MovePendingWeaponToHand:
			controller.IK_MovePendingWeaponToHand();
			break;
		case IKControllerInvoke.PickupGauze:
			controller.IK_PickupGauze();
			break;
		case IKControllerInvoke.ApplyGauze:
			controller.IK_ApplyGauze();
			break;
		case IKControllerInvoke.GauzeFinished:
			controller.IK_FinishGauze();
			break;
		case IKControllerInvoke.FireWeapon:
			controller.IK_FireWeapon();
			break;
		case IKControllerInvoke.Restrain:
			controller.IK_Restrain();
			break;
		}
	}

	public bool HasFinishedFrame(int frame)
	{
		return Sequence == null || CurrentFrame > frame;
	}
}

}
