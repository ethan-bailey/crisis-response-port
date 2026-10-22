#if CRISIS_FOUNDATION_TRANSPORT
using CharacterBody = Crisis.Runtime.Weapons.CharacterBody;
#else
using CharacterBody = Crisis.Runtime.FullSource.CharacterBody;
#endif
using UnityEngine;

public class ArmsControl : MonoBehaviour
{
	[Header("Arms")]
	public ArmController rightArm;

	public ArmController leftArm;

	[Header("Reference transforms")]
	public Transform holster;

	private void Start()
	{
		CharacterBody component = GetComponent<CharacterBody>();
		rightArm.Initialize(this, component);
		leftArm.Initialize(this, component);
	}

	private void Update()
	{
		rightArm.Update();
		leftArm.Update();
	}

	public void Relax()
	{
		rightArm.Relax();
		leftArm.Relax();
	}

	public void SetOnWeapon()
	{
		rightArm.SetOnWeapon();
		leftArm.SetOnWeapon();
	}

	public void SetOnTarget()
	{
		rightArm.SetOnTarget();
		leftArm.SetOnTarget();
	}

	public void SetOnRelativePosition()
	{
		rightArm.SetOnRelativePosition();
		leftArm.SetOnRelativePosition();
	}

	public void SetOnHolster()
	{
		rightArm.SetOnTarget(holster, 10f, 0.1f);
		leftArm.SetOnTarget();
	}
}
