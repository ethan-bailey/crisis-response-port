using System;
using UnityEngine;

public class WeaponAimer : MonoBehaviour
{
	public enum AimMode
	{
		Lowered = 0,
		SemiReady = 1,
		Ready = 2,
		Sweeping = 3,
		AtWorldPoint = 4
	}

	public const float SelfProximity = 0.35f;

	public const float MinRecoilCompensation = 5f;

	[Tooltip("The character will look through the sights if possible when this is set.")]
	public bool useSights = true;

	private Blackboard blackboard;

	private Vector3 targetAimOffset;

	private float idleAimOffset = 0.1f;

	private float idleAimPhase;

	private float sweepSpread = 0.5f;

	private bool isOperator;

	private bool isAimingLow;

	public Vector3 Point { get; set; }

	public bool IsUsingSights { get; set; }

	public CharacterBody Body { get; private set; }

	public AimMode Mode { get; set; }

	public bool ClearFireSolution { get; private set; }

	private void Awake()
	{
		Body = GetComponent<CharacterBody>();
		blackboard = Body.Blackboard;
		idleAimPhase = UnityEngine.Random.value * (float)Math.PI * 2f;
		Mode = AimMode.Ready;
		targetAimOffset = new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), 0f);
		sweepSpread = UnityEngine.Random.Range(0.3f, 0.6f);
	}

	private void Start()
	{
		isOperator = Body.IsOperator;
		if (!isOperator)
		{
			SetOccupantPreferredTargetLimb();
		}
	}

	private void Update()
	{
		UpdateHeadState();
		WeaponBase activeWeapon = Body.ActiveWeapon;
		if (activeWeapon == null)
		{
			return;
		}
		UpdateLineOfFire(activeWeapon);
		if (Body.IsPlayer)
		{
			return;
		}
		Target target = Body.Blackboard.Target;
		if (Body.weaponMode == WeaponMode.Lowered || Mode == AimMode.Lowered)
		{
			Body.AimPoint = new Vector3(Body.Position + Body.Direction, 0f, Body.transform.position.z);
			Body.IsUsingPrecision = false;
		}
		else if (target != null)
		{
			float num = ((!(target.Body != null) || !target.Body.Conscious) ? target.transform.position.x : target.Body.RightUpperArm.transform.position.x);
			if (Mathf.Sign((num - Body.RightUpperArm.transform.position.x - Body.Direction * 0.35f) * Body.Direction) > 0f)
			{
				AimAtTarget(activeWeapon);
			}
			else
			{
				HoldAtReady(activeWeapon, Mode == AimMode.SemiReady);
			}
		}
		else if (Mode == AimMode.Sweeping)
		{
			SweepingAim();
		}
		else if (Mode == AimMode.AtWorldPoint)
		{
			AimAtWorldPoint(activeWeapon);
		}
		else if (!Body.FreeAimActive)
		{
			HoldAtReady(activeWeapon, Mode == AimMode.SemiReady);
		}
	}

	private void UpdateHeadState()
	{
		WeaponBase activeWeapon = Body.ActiveWeapon;
		if (activeWeapon == null)
		{
			IsUsingSights = false;
			return;
		}
		bool flag = activeWeapon.IsBlocked || Body.IsLoweringWeapon;
		if (Body.IsUsingPrecision)
		{
			IsUsingSights = !flag;
			return;
		}
		if (Body.ActiveWeapon != null)
		{
			if (!isAimingLow && activeWeapon.Angle < -40f)
			{
				isAimingLow = true;
			}
			else if (isAimingLow && activeWeapon.Angle > -30f)
			{
				isAimingLow = false;
			}
		}
		IsUsingSights = useSights && !isAimingLow && !flag && !Body.IsSprinting;
	}

	private void AimAtTarget(WeaponBase weapon)
	{
		if (isOperator)
		{
			OperatorAimAtTarget(weapon);
		}
		else
		{
			OccupantAimAtTarget(weapon);
		}
	}

	private void AimAtWorldPoint(WeaponBase weapon)
	{
		float num = Mathf.Abs(Point.x - weapon.Muzzle.position.x);
		Vector3 vector = Mathf.Min(num / 3f, 1f) * targetAimOffset;
		Body.AimPoint = Point + vector;
	}

	private void HoldAtReady(WeaponBase weapon, bool semi)
	{
		if (isOperator)
		{
			OperatorHoldAtReady(weapon);
		}
		else
		{
			OccupantHoldAtReady(weapon);
		}
		Body.IsUsingPrecision = false;
	}

	private void OperatorAimAtTarget(WeaponBase weapon)
	{
		Target target = blackboard.Target;
		Vector3 vector = target.transform.position;
		if (target.Body != null)
		{
			CharacterBody body = target.Body;
			bool flag = BodyHelpers.TargetFacesSubject(Body, body);
			float num = Mathf.Abs(BodyHelpers.TargetWeaponAngleRelativeSubjectHead(Body, body));
			bool flag2 = num <= 25f;
			bool flag3 = num > 25f && num < 40f;
			bool flag4 = BodyHelpers.TimeSinceSubjectFired(body) <= 3f;
			vector = body.Head.AimPoint;
			vector = ((!flag2 && !flag4) ? body.limbMap[Body.PreferredTargetLimb].AimPointWithExtents(Body.RightUpperArm.transform.position) : body.Head.AimPointWithExtents(Body.RightUpperArm.transform.position));
		}
		float num2 = Mathf.Abs(vector.x - weapon.Muzzle.position.x);
		Vector3 vector2 = Mathf.Min(num2 / 3f, 1f) * targetAimOffset;
		Body.AimPoint = vector + vector2;
		Body.IsUsingPrecision = true;
	}

	private void OperatorHoldAtReady(WeaponBase weapon)
	{
		bool atAttention = Body.AtAttention;
		Vector3 vector = new Vector3(Body.Position + 3f * Body.Direction, (!atAttention) ? 0.7f : 1.4f, Body.transform.position.z);
		float num = Mathf.Abs(vector.x - weapon.Muzzle.position.x);
		float num2 = Mathf.Min(num / 3f, 1f) * Mathf.Abs(Mathf.Sin((Time.time + idleAimPhase) * 0.5f)) * 2f;
		Vector3 vector2 = Vector3.down * idleAimOffset * num2;
		Body.AimPoint = vector + vector2;
		Body.IsUsingPrecision = true;
	}

	private void OccupantAimAtTarget(WeaponBase weapon)
	{
		Target target = blackboard.Target;
		Vector3 vector = ((!(blackboard.Target.Body != null)) ? blackboard.Target.transform.position : target.Body.limbMap[Body.PreferredTargetLimb].AimPointWithExtents(Body.RightUpperArm.transform.position));
		Vector3 vector2 = new Vector3(0f, (1f - Body.Physiology.BloodVolumeNormalized) * -1f, 0f);
		float num = Mathf.Abs(vector.x - weapon.Muzzle.position.x);
		Vector3 vector3 = Mathf.Min(num / 3f, 1f) * targetAimOffset + vector2;
		Vector3 vector4 = new Vector3(0f, Mathf.Sin(Time.time % ((float)Math.PI * 2f) * 4f) * (Body.VisionOverload + Body.Physiology.ShockNormalized), 0f);
		Body.AimPoint = vector + vector3 + vector4;
		Body.IsUsingPrecision = !Body.FreeAimActive;
	}

	private void OccupantHoldAtReady(WeaponBase weapon)
	{
		float value = Body.GetComponent<OccupantMorale>().Value;
		float num = weapon.armConfiguration.retractDistance + blackboard.aggression * blackboard.weaponSkill * 2.5f;
		Vector3 vector = new Vector3(Body.transform.position.x + num * Body.Direction, 0f, Body.transform.position.z);
		float num2 = Mathf.Abs(vector.x - weapon.Muzzle.position.x);
		float num3 = Mathf.Min(num2 / 3f, 1f) * Mathf.Abs(Mathf.Sin((Time.time + idleAimPhase) * 0.5f)) * 2f;
		Vector3 vector2 = new Vector3(0f, (1f - Body.Physiology.BloodVolumeNormalized) * -1f, 0f);
		Vector3 vector3 = new Vector3(0f, (Body.VisionOverload + Body.Physiology.ShockNormalized) * -2f, 0f);
		Body.AimPoint = new Vector3(Body.Position + num * Body.Direction * (1f - Body.VisionOverload), idleAimOffset * num3, Body.transform.position.z) + vector2 + vector3;
		Body.IsUsingPrecision = true;
	}

	private void SetOccupantPreferredTargetLimb()
	{
		float num = blackboard.aggression * blackboard.weaponSkill;
		if (num >= 1.25f)
		{
			Body.PreferredTargetLimb = LimbId.Head;
			return;
		}
		if (num >= 0.85f)
		{
			Body.PreferredTargetLimb = LimbId.Torso;
			return;
		}
		num = UnityEngine.Random.value;
		if (num > 0.66f)
		{
			Body.PreferredTargetLimb = LimbId.Head;
		}
		else if (num > 0.33f)
		{
			Body.PreferredTargetLimb = LimbId.Torso;
		}
		else
		{
			Body.PreferredTargetLimb = LimbId.Abdomen;
		}
	}

	private void SweepingAim()
	{
		float f = Time.time % 1f * (float)Math.PI * 2f;
		Body.AimPoint = new Vector3(Body.Position + 2f * Body.Direction, 1f + Mathf.Sin(f) * sweepSpread, Body.transform.position.z);
		Body.IsUsingPrecision = false;
		useSights = true;
	}

	private void UpdateLineOfFire(WeaponBase weapon)
	{
		CharacterBody characterBody = ((!(Body.Blackboard.Target != null)) ? null : Body.Blackboard.Target.Body);
		Ray ray = new Ray(weapon.FireLineStart(Body.IsUsingPrecision), weapon.Muzzle.forward);
		RaycastHit[] array = Physics.RaycastAll(ray, (!Body.IsUsingPrecision) ? 10f : weapon.PrecisionDistance, Masks.Instance.Limbs);
		RaycastHit[] array2 = array;
		foreach (RaycastHit raycastHit in array2)
		{
			Limb component = raycastHit.collider.GetComponent<Limb>();
			if (component != null && component.Body != characterBody)
			{
				ClearFireSolution = false;
				return;
			}
		}
		ClearFireSolution = true;
	}
}
