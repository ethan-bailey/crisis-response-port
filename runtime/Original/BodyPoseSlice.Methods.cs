using UnityEngine;
namespace Crisis.Runtime.Presentation;
public sealed partial class BodyPoseSlice {
private void UpdateUpperBodyTransforms(WeaponPoseSnapshot weapon)
	{
		float num = 0f;
		if (IsMoving)
		{
			num = ((!IsSprinting) ? 2f : 4f);
		}
		float num2 = Physiology.Respiration * Physiology.RespirationRateFactor * 20f;
		torsoHitAngleAddition = Mathf.Lerp(torsoHitAngleAddition, 0f /* no-hit scope: original multiplier LastHitEnergy is zero */, Time.deltaTime * 25f);
		float num3 = 0f;
		float num4 = 0f;
		float num5 = torsoHitAngleAddition;
		if (weapon == null)
		{
			if (HeadMode == HeadLookMode.AimPoint)
			{
				Vector3 vector = AimPoint - Eye.transform.position;
				LookAngle = Mathf.Atan2(vector.y, Mathf.Abs(vector.x)) * 57.29578f * 0.8f;
			}
			else if (HeadMode == HeadLookMode.RightHand)
			{
				Vector3 vector2 = rightArm.ikTarget.position - Eye.transform.position;
				LookAngle = Mathf.Atan2(vector2.y, Mathf.Abs(vector2.x)) * 57.29578f * 0.8f;
			}
			else if (HeadMode == HeadLookMode.LeftHand)
			{
				Vector3 vector3 = leftArm.ikTarget.position - Eye.transform.position;
				LookAngle = Mathf.Atan2(vector3.y, Mathf.Abs(vector3.x)) * 57.29578f * 0.8f;
			}
			num3 = LookAngle + HeadBaseAngle;
			num4 = num2 - 17f;
		}
		else
		{
			float num6 = ((!IsUsingPrecision) ? 0f : 3f);
			float num7 = ((!IsKneeling) ? 0f : 3f);
			float num8 = ((!FreeAimActive) ? 1f : 0f);
			float num9 = ((!WeaponAimer.IsUsingSights) ? 0f : (weapon.Angle * 0.3f - num - num7 - num6)) * num8;
			ArmConfigurationSnapshot armConfiguration = weapon.armConfiguration;
			num4 = armConfiguration.torsoAngle + num9 + num2 - weapon.RecoilKick;
			float num10 = ((weapon.type != WeaponBrace.Shoulder) ? 0.6f : 1f);
			num5 += weapon.RecoilKick * num10;
			num3 = ((!WeaponAimer.IsUsingSights) ? HeadBaseAngle : (weapon.Angle + armConfiguration.headAngle));
			if (StunResponseStateActive)
			{
				num3 += VisionOverload * -100f;
			}
		}
		float num11 = 0f;
		float num12 = 0f;
		if (!Physiology.invulnerable)
		{
			num11 = Mathf.Clamp01(1f - Physiology.BloodVolumeNormalized) * -45f;
			num12 = Mathf.Clamp01(1f - Physiology.BloodVolumeNormalized) * -25f;
		}
		num3 += num11;
		num4 += num12;
		HeadCurrentAngle = Mathf.LerpAngle(HeadCurrentAngle, num3, Time.deltaTime * 15f);
		TorsoCurrentAngle = Mathf.Lerp(TorsoCurrentAngle, num4, Time.deltaTime * 10f);
		Torso.transform.eulerAngles = new Vector3(0f, RotationY, TorsoCurrentAngle + num5);
		Abdomen.transform.eulerAngles = new Vector3(0f, RotationY, num11);
		if (!FreeAimActive)
		{
			Head.transform.eulerAngles = new Vector3(0f, RotationY, HeadCurrentAngle);
			Neck.transform.eulerAngles = new Vector3(0f, RotationY, HeadCurrentAngle);
		}
	}
}
public sealed partial class WeaponAimerSlice {
private void UpdateHeadState()
	{
		WeaponPoseSnapshot activeWeapon = Body.ActiveWeapon;
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
}
