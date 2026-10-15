using UnityEngine;
using System;
using System.Collections.Generic;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
namespace Crisis.Runtime.Weapons;
public partial class WeaponBase {
private void UpdateTriggerMechanism()
	{
		if (Owner == null || ActiveFireMode == FireMode.Safe)
		{
			return;
		}
		if ((ActiveFireMode == FireMode.Semi && consecutiveRounds > 0) || (ActiveFireMode == FireMode.Burst && consecutiveRounds >= burstCount))
		{
			ResetBurstCount();
		}
		else if (HammerReady)
		{
			if (IsHeld && Owner.QueryTrigger())
			{
				ReleaseFiringPin();
			}
			else if (ActiveFireMode == FireMode.Burst && burstRoundsRemaining > 0)
			{
				ReleaseFiringPin();
			}
		}
	}
private void ReleaseFiringPin()
	{
		if (fireFromMagazine || cyclingBolt.Status == CyclingBolt.State.Closed)
		{
			if (hammer != null)
			{
				hammer.Release();
			}
			else
			{
				StrikeFiringPin();
			}
		}
	}
public void StrikeFiringPin()
	{
		HammerReady = false;
		object instigator = ((!(Owner != null)) ? null : Owner.AudioInstigator);
		audioFiringPin.Play(base.transform.position.x, instigator);
		if (cyclingBolt.Live && ammunition != null)
		{
			DetonateRound();
			return;
		}
		ResetBurstCount();
		if (fireFromMagazine)
		{
			cyclingBolt.Open();
		}
	}
private void DetonateRound()
	{
		if (ActiveFireMode == FireMode.Burst && burstRoundsRemaining == 0)
		{
			burstRoundsRemaining = burstCount;
		}
		cyclingBolt.DetonateRound();
		consecutiveRounds++;
		ChamberStatus = ChamberState.Spent;
		Effects.Emit("InvokeBulletTime",null);
		SpawnProjectiles();
		AddSingleRecoil();
		Effects.Emit("EmitMuzzleSmoke",null);
		Effects.Emit("EmitMuzzleFlash",null);
		Effects.Emit("ApplyMuzzleBlastForces",null);
		if (!manualCycling && ammunition.cyclesAction)
		{
			cyclingBolt.Open();
		}
		if (Owner != null)
		{
			Owner.LastFiredTimestamp = Time.time;
			Owner.ShotsFired++;
		}
		Effects.Emit("actionMusic",5f);
	}
public void ResetBurstCount()
	{
		consecutiveRounds = 0;
		burstRoundsRemaining = 0;
	}
public void OnBoltOpened()
	{
		HammerReady = true;
		if (Magazine == null || Magazine.Empty)
		{
			cyclingBolt.LockOpenOnEmptyMagazine();
			ResetBurstCount();
		}
		if (burstRoundsRemaining > 0)
		{
			burstRoundsRemaining--;
		}
	}
public void OnBoltClosed(bool feedRound)
	{
		if (feedRound)
		{
			ChamberStatus = ChamberState.Live;
		}
	}
public Casing GetNextRoundFromMagazine(bool disableMalfunctions)
	{
		if (Magazine == null || Magazine.Empty)
		{
			return null;
		}
		if (!Magazine.RemoveOne(disableMalfunctions))
		{
			return null;
		}
		return ammunition.casing;
	}
private void CalculateRecoil()
	{
		float num = 0f;
		float num2 = 0f;
		for (int num3 = recoilList.Count - 1; num3 >= 0; num3--)
		{
			Recoil recoil = recoilList[num3];
			if (Time.time >= recoil.timestamp + recoilPeriod)
			{
				recoilList.RemoveAt(num3);
			}
			else
			{
				float time = Time.time - recoil.timestamp;
				num += recoilCycle.Evaluate(time) * recoil.kick;
				num2 += recoilClimbCycle.Evaluate(time) * recoil.climb;
			}
		}
		if (ammunition != null)
		{
			kickValue = num * ammunition.recoil;
			climbValue = num2 * ammunition.recoil;
		}
	}
public float RecoilImpulse()
	{
		return ammunition.MuzzleEnergy * (float)ammunition.projectileCount * recoilSuppressorModifier / (TotalWeight * recoilToWeightFactor);
	}
private void AddSingleRecoil()
	{
		float num = RecoilImpulse();
		recoilList.Add(new Recoil(Time.time, num * recoilKickModifier, num * recoilClimbModifier));
		if (IsRagdoll)
		{
			throw new NotSupportedException("Ragdoll firing is not ported");
		}
	}
private float MagazineWeight()
	{
		if (Magazine == null)
		{
			return 0f;
		}
		if (ammunition == null)
		{
			return ComponentMagazine.weight;
		}
		float num = ((Magazine == null) ? 0f : ((float)Magazine.roundCount * ammunition.roundWeight * 0.001f));
		return ComponentMagazine.weight + num;
	}
public bool InsertMagazine(Magazine magazine)
	{
		if (magazine == null)
		{
			return false;
		}
		Magazine = magazine;
		onNewMagazine.Invoke();
		ComponentMagazine.transform.SetParent(magazineTransform);
		ComponentMagazine.transform.localPosition = Vector3.zero;
		ComponentMagazine.transform.localEulerAngles = Vector3.zero;
		if (Owner != null)
		{
			audioInsertMagazine.Play(Owner.Position, Owner.AudioInstigator);
		}
		return true;
	}
public void RemoveMagazine(bool drop = false, bool audio = true)
	{
		if (fireFromMagazine)
		{
			audioExtractMagazine.Play(Owner.Position, Owner.AudioInstigator);
			if (cyclingBolt.chamber.childCount <= 0)
			{
				return;
			}
			for (int num = cyclingBolt.chamber.childCount - 1; num >= 0; num--)
			{
				Transform child = cyclingBolt.chamber.GetChild(num);
				if (child.childCount == 0)
				{
					cyclingBolt.EjectCasing(1f, num);
				}
			}
		}
		else
		{
			if (Magazine != null)
			{
				audioExtractMagazine.Play(Owner.Position, Owner.AudioInstigator);
			}
			if (!useInternalMagazine)
			{
				ComponentMagazine.Drop();
				Magazine = null;
			}
		}
	}
private void SpawnProjectiles()
	{
		CalculateRecoil();
		bool flag = false;
		int frameCount = Time.frameCount;
		for (int i = 0; i < ammunition.projectileCount; i++)
		{
			float x = UnityEngine.Random.value * ammunition.spatialDispersion;
			Vector2 vector = UnityEngine.Random.insideUnitCircle * (boreDiameter * 0.0005f - ammunition.DiameterInMeters * 0.5f);
			Vector3 forward = Muzzle.forward;
			if (direction == -1f)
			{
				forward.x *= -1f;
			}
			Projectile projectile = UnityEngine.Object.Instantiate(ammunition.projectile);
			projectile.Id = frameCount + i;
			projectile.SwarmId = frameCount;
			projectile.name = ammunition.projectile.name;
			projectile.transform.SetParent(projectilesContainer, false);
			projectile.transform.position = Muzzle.position + new Vector3(x, vector.x, vector.y);
			projectile.transform.rotation = Quaternion.LookRotation(forward, Muzzle.up);
			float angle = UnityEngine.Random.Range(0f, CombinedInaccuracy);
			projectile.transform.Rotate(UnityEngine.Random.onUnitSphere, angle);
			bool usePrecision = Owner != null && Owner.IsUsingPrecision;
			projectile.InitializeXY(this, usePrecision, PrecisionBegin, PrecisionDistance, velocityModifier);
			projectile.ForceMultiplier = projectileForceMultiplier;
			flag = flag || projectile.SuperSonic;
			onSpawnProjectile.Invoke(projectile);
		}
		EmitMuzzleAudio(flag);
		mission.LastWeaponFireTimestamp = Time.time;
	}
}