using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class IKArmController : MonoBehaviour
{
	public AnimationCurve armStrength;

	[Header("Sequences")]
	public IKArmSequence holsterPistol;

	public IKArmSequence unholsterPistol;

	public IKArmSequence lowerShoulderWeapon;

	public IKArmSequence returnShoulderWeapon;

	public IKArmSequence pickupGrenade;

	public IKArmSequence returnGrenade;

	public IKArmSequence applyGauze;

	private CharacterBody self;

	private float grenadeThrowTime;

	public IKArmSequencer RightSequencer { get; private set; }

	public IKArmSequencer LeftSequencer { get; private set; }

	public WeaponBase PreviousWeapon { get; private set; }

	public WeaponBase PendingWeapon { get; private set; }

	public bool IsPlaying
	{
		get
		{
			return LeftSequencer.Sequence != null || RightSequencer.Sequence != null;
		}
	}

	public bool IsThrowingGrenade { get; private set; }

	public int RightFrame { get; private set; }

	public int LeftFrame { get; private set; }

	private void Awake()
	{
		self = GetComponent<CharacterBody>();
		RightSequencer = new IKArmSequencer(this, self, IKArmType.Right, self.rightArm);
		LeftSequencer = new IKArmSequencer(this, self, IKArmType.Left, self.leftArm);
		RightSequencer.SetSibling(LeftSequencer);
		LeftSequencer.SetSibling(RightSequencer);
	}

	private void Update()
	{
		RightSequencer.Update();
		LeftSequencer.Update();
		UpdateHoldGrenade();
	}

	public void Play(IKArmSequenceRuntime sequence)
	{
		RightSequencer.Play(sequence.right);
		LeftSequencer.Play(sequence.left);
	}

	public void InitiateGrenadeThrow()
	{
		IsThrowingGrenade = true;
		grenadeThrowTime = 0.05f;
	}

	private void UpdateHoldGrenade()
	{
		if (!self.IsHoldingGrenade)
		{
			return;
		}
		Vector3 vector = self.Torso.transform.position + self.Torso.transform.TransformVector(Vector3.up * 0.25f);
		Vector3 vector2 = ((!self.IsPlayer) ? (Vector3.right * self.Direction) : (self.AimPoint - vector).normalized);
		float num = 0f;
		if (IsThrowingGrenade)
		{
			grenadeThrowTime -= Time.deltaTime;
			float num2 = 1f - Mathf.Clamp(grenadeThrowTime / 0.05f, 0f, 1f);
			num = Mathf.Pow(num2, 2f);
			if (num2 == 1f)
			{
				grenadeThrowTime = 0f;
				IsThrowingGrenade = false;
				self.ThrowGrenade((self.AimPoint - self.RightUpperArm.transform.position).normalized);
			}
		}
		float num3 = 0.3f + num * 0.15f;
		self.pistolIKTarget.position = vector + vector2 * num3;
	}

	public void SwitchWeapons()
	{
		if (!self.IsHoldingGrenade)
		{
			PendingWeapon = self.InactiveWeapon;
		}
		PreviousWeapon = self.ActiveWeapon;
		if (PendingWeapon == null)
		{
			if (PreviousWeapon != null)
			{
				HolsterWeapon();
			}
			else
			{
				Unarmed();
			}
			return;
		}
		IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
		if (self.IsHoldingGrenade)
		{
			iKArmSequenceRuntime.Append(returnGrenade);
			iKArmSequenceRuntime.right.Add(new IKArmAnimationFrame
			{
				action = IKArmAction.WaitForSiblingFrame,
				waitForSiblingFrame = 0
			});
		}
		else if (PreviousWeapon != null)
		{
			if (PreviousWeapon.type == WeaponBrace.Shoulder)
			{
				iKArmSequenceRuntime.Append(lowerShoulderWeapon.Sequence());
			}
			else
			{
				iKArmSequenceRuntime.Append(holsterPistol.Sequence());
				iKArmSequenceRuntime.left.Add(new IKArmAnimationFrame
				{
					action = IKArmAction.Transition,
					transform = IKArmTarget.PendingWeapon,
					time = 0.3f
				});
			}
		}
		IKArmSequenceRuntime iKArmSequenceRuntime2 = new IKArmSequenceRuntime();
		if (PendingWeapon.type == WeaponBrace.Shoulder)
		{
			iKArmSequenceRuntime2.Append(returnShoulderWeapon.Sequence());
		}
		else
		{
			iKArmSequenceRuntime2.Append(unholsterPistol.Sequence());
		}
		iKArmSequenceRuntime.Append(iKArmSequenceRuntime2);
		Play(iKArmSequenceRuntime);
	}

	public void HolsterWeapon()
	{
		PreviousWeapon = self.ActiveWeapon;
		IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
		if (self.IsHoldingGrenade)
		{
			iKArmSequenceRuntime.Append(returnGrenade);
			iKArmSequenceRuntime.right.Add(new IKArmAnimationFrame
			{
				action = IKArmAction.WaitForSiblingFrame,
				waitForSiblingFrame = 0
			});
		}
		else if (PreviousWeapon != null)
		{
			if (PreviousWeapon.type == WeaponBrace.Shoulder)
			{
				iKArmSequenceRuntime.Append(lowerShoulderWeapon.Sequence());
			}
			else
			{
				iKArmSequenceRuntime.Append(holsterPistol.Sequence());
				iKArmSequenceRuntime.left.Add(new IKArmAnimationFrame
				{
					action = IKArmAction.Transition,
					transform = IKArmTarget.PendingWeapon,
					time = 0.3f
				});
			}
		}
		IKArmAnimationFrame iKArmAnimationFrame = new IKArmAnimationFrame();
		iKArmAnimationFrame.action = IKArmAction.Transition;
		iKArmAnimationFrame.transform = IKArmTarget.Pelvis;
		iKArmAnimationFrame.offset = new Vector3(0.25f, 0.15f, 0f);
		iKArmAnimationFrame.time = 0.3f;
		IKArmAnimationFrame item = iKArmAnimationFrame;
		iKArmSequenceRuntime.left.Add(item);
		iKArmSequenceRuntime.right.Add(item);
		Play(iKArmSequenceRuntime);
	}

	public void ReadyGrenade(Grenade.Type type)
	{
		PendingWeapon = self.ActiveWeapon;
		self.PendingGrenadeType = type;
		IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
		if (PendingWeapon != null)
		{
			iKArmSequenceRuntime.Append((PendingWeapon.type != WeaponBrace.Shoulder) ? holsterPistol.Sequence() : lowerShoulderWeapon.Sequence());
		}
		iKArmSequenceRuntime.Append(pickupGrenade);
		Play(iKArmSequenceRuntime);
	}

	public void ReturnGrenade()
	{
		if (self.IsHoldingGrenade)
		{
			IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime(returnGrenade);
			iKArmSequenceRuntime.right.Add(new IKArmAnimationFrame
			{
				action = IKArmAction.WaitForSiblingFrame,
				waitForSiblingFrame = 0
			});
			if (PendingWeapon != null)
			{
				iKArmSequenceRuntime.Append((PendingWeapon.type != WeaponBrace.Shoulder) ? unholsterPistol.Sequence() : returnShoulderWeapon.Sequence());
			}
			Play(iKArmSequenceRuntime);
		}
	}

	public void LowerCurrentWeapon()
	{
		PendingWeapon = self.ActiveWeapon;
		IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
		if (PendingWeapon != null)
		{
			iKArmSequenceRuntime.Append((PendingWeapon.type != WeaponBrace.Shoulder) ? holsterPistol.Sequence() : lowerShoulderWeapon.Sequence());
			Play(iKArmSequenceRuntime);
		}
	}

	public bool ResumePreviousWeapon()
	{
		IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
		if (PendingWeapon != null)
		{
			iKArmSequenceRuntime.Append((PendingWeapon.type != WeaponBrace.Shoulder) ? unholsterPistol.Sequence() : returnShoulderWeapon.Sequence());
			Play(iKArmSequenceRuntime);
			return true;
		}
		SwitchWeapons();
		return false;
	}

	public void ActivateWeapon(WeaponBase weapon)
	{
		if (!(weapon == null))
		{
			PendingWeapon = weapon;
			IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
			iKArmSequenceRuntime.Append((PendingWeapon.type != WeaponBrace.Shoulder) ? unholsterPistol.Sequence() : returnShoulderWeapon.Sequence());
			Play(iKArmSequenceRuntime);
		}
	}

	public void EngageSelf()
	{
		WeaponBase activeWeapon = self.ActiveWeapon;
		if (!(activeWeapon == null) && activeWeapon.type == WeaponBrace.Freehand && !(activeWeapon.Length() > 0.3f))
		{
			IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
			Vector3 offset = new Vector3(0.45f, -0.08f, 0f);
			IKArmAnimationFrame iKArmAnimationFrame = new IKArmAnimationFrame();
			iKArmAnimationFrame.action = IKArmAction.Transition;
			iKArmAnimationFrame.transform = IKArmTarget.Head;
			iKArmAnimationFrame.offset = offset;
			iKArmAnimationFrame.time = Random.Range(0.5f, 3f);
			iKArmAnimationFrame.callback = IKControllerInvoke.FireWeapon;
			IKArmAnimationFrame item = iKArmAnimationFrame;
			iKArmSequenceRuntime.right.Add(item);
			iKArmSequenceRuntime.left.Add(item);
			self.FreeAimActive = true;
			Play(iKArmSequenceRuntime);
		}
	}

	public void Surrender()
	{
		IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
		Vector3 vector = new Vector3(Random.Range(0.2f, 0.3f), Random.Range(0.2f, 0.45f), 0f);
		Vector3 vector2 = vector + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0f);
		Vector3 offset = vector + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0f);
		float num = Random.Range(0.5f, 5f);
		IKArmAnimationFrame iKArmAnimationFrame = new IKArmAnimationFrame();
		iKArmAnimationFrame.action = IKArmAction.Transition;
		iKArmAnimationFrame.transform = IKArmTarget.Head;
		iKArmAnimationFrame.offset = vector;
		iKArmAnimationFrame.time = num * Random.Range(0.75f, 1.25f);
		IKArmAnimationFrame item = iKArmAnimationFrame;
		iKArmAnimationFrame = new IKArmAnimationFrame();
		iKArmAnimationFrame.action = IKArmAction.Transition;
		iKArmAnimationFrame.transform = IKArmTarget.Head;
		iKArmAnimationFrame.offset = offset;
		iKArmAnimationFrame.time = num * Random.Range(0.75f, 1.25f);
		IKArmAnimationFrame item2 = iKArmAnimationFrame;
		iKArmSequenceRuntime.right.Add(item2);
		iKArmSequenceRuntime.left.Add(item);
		Play(iKArmSequenceRuntime);
	}

	public void Unarmed()
	{
		IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
		Vector3 offset = new Vector3(0.25f, 0.15f, 0f);
		float num = 0.3f;
		IKArmAnimationFrame iKArmAnimationFrame = new IKArmAnimationFrame();
		iKArmAnimationFrame.action = IKArmAction.Transition;
		iKArmAnimationFrame.transform = IKArmTarget.Pelvis;
		iKArmAnimationFrame.offset = offset;
		iKArmAnimationFrame.time = num * Random.Range(0.75f, 1.25f);
		IKArmAnimationFrame item = iKArmAnimationFrame;
		iKArmSequenceRuntime.right.Add(item);
		iKArmSequenceRuntime.left.Add(item);
		Play(iKArmSequenceRuntime);
	}

	public void Restrained()
	{
		IKArmSequenceRuntime iKArmSequenceRuntime = new IKArmSequenceRuntime();
		Vector3 vector = new Vector3(-0.08f, 0.025f, 0f);
		Vector3 vector2 = vector + new Vector3(Random.Range(-0.025f, 0.025f), Random.Range(-0.025f, 0.025f), 0f);
		Vector3 offset = vector + new Vector3(Random.Range(-0.025f, 0.025f), Random.Range(-0.025f, 0.025f), 0f);
		IKArmAnimationFrame iKArmAnimationFrame = new IKArmAnimationFrame();
		iKArmAnimationFrame.action = IKArmAction.Transition;
		iKArmAnimationFrame.transform = IKArmTarget.Pelvis;
		iKArmAnimationFrame.offset = vector;
		iKArmAnimationFrame.time = 1f;
		IKArmAnimationFrame item = iKArmAnimationFrame;
		iKArmAnimationFrame = new IKArmAnimationFrame();
		iKArmAnimationFrame.action = IKArmAction.Transition;
		iKArmAnimationFrame.transform = IKArmTarget.Pelvis;
		iKArmAnimationFrame.offset = offset;
		iKArmAnimationFrame.time = 1.25f;
		iKArmAnimationFrame.callback = IKControllerInvoke.Restrain;
		IKArmAnimationFrame item2 = iKArmAnimationFrame;
		iKArmSequenceRuntime.right.Add(item2);
		iKArmSequenceRuntime.left.Add(item);
		Play(iKArmSequenceRuntime);
	}

	public void IK_LowerWeapon()
	{
		self.IsLoweringWeapon = true;
	}

	public void IK_HolsterWeapon()
	{
		self.HolsterWeapon(self.ActiveWeapon);
	}

	public void IK_ActivatePreviousWeapon()
	{
		self.ActivateWeapon(PreviousWeapon);
	}

	public void IK_ActivatePendingWeapon()
	{
		self.ActivateWeapon(PendingWeapon, 0.05f);
	}

	public void IK_MovePendingWeaponToHand()
	{
		if (!(PendingWeapon == null))
		{
			Transform grabPoint = self.rightArm.hand.grabPoint;
			PendingWeapon.transform.SetParent(grabPoint);
			PendingWeapon.transform.localPosition = Vector3.zero;
		}
	}

	public void IK_PickupGrenade()
	{
		self.InstantiateGrenadeInHand(self.PendingGrenadeType);
	}

	public void IK_ReturnGrenade()
	{
		self.ReturnGrenadeToEquipment();
	}

	public void IK_PickupGauze()
	{
		TeamOperator component = GetComponent<TeamOperator>();
		if (component != null)
		{
			component.InstantiateGauzeInHand();
		}
	}

	public void IK_ApplyGauze()
	{
		self.StabilizeBody(self);
	}

	public void IK_FinishGauze()
	{
		TeamOperator component = GetComponent<TeamOperator>();
		if (component != null)
		{
			component.RemoveGauzeFromHand();
		}
	}

	public void IK_FireWeapon()
	{
		self.PullTrigger();
	}

	public void IK_Restrain()
	{
		self.Restrain();
		self.InstantiatePlasticuffOnLeftHand();
	}
}

}
