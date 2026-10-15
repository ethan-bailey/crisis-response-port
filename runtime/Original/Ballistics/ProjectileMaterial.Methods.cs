using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;
using UnityEngine.Events;
public partial class Projectile {
private BallisticResult HandleGenericHit(RaycastHit hit)
	{
		if (hit.collider == null)
		{
			return BallisticResult.Error;
		}
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (attachedRigidbody != null)
		{
			attachedRigidbody.AddForceAtPosition(base.transform.forward * Energy * ForceMultiplier * 0.5f, hit.point);
		}
		BallisticMaterial component = hit.collider.GetComponent<BallisticMaterial>();
		if (component == null)
		{
			return BallisticResult.Error;
		}
		RecalculateVelocity();
		if (component.PassThroughWithoutContact(hit.point, this))
		{
			return BallisticResult.Continue;
		}
		base.transform.position = nonDiscriminatingResult.point;
		component.HandleProjectile(this, hit.normal);
		onInteraction.Invoke(this);
		if (Trace != null)
		{
			Trace.SpawnHitEffect(base.transform.position, hit.transform, hit.normal);
		}
		return Status;
	}
public void BeginNewInteraction()
	{
		InteractionEnergy = 0f;
		DisplacedMaterialVolume = 0f;
		InteractionPosition = base.transform.position;
		UpdateRotation();
		NewTracePosition();
	}
public void NewTracePosition()
	{
		if (Trace != null)
		{
			Trace.AddPosition(base.transform.position);
		}
	}
public void RestartAtPosition(Vector3 worldPosition)
	{
		Status = BallisticResult.Continue;
		freeFallTimestamp = Time.time;
		base.transform.position = worldPosition;
		meshTransform.localScale = new Vector3(DeformationRatio, DeformationRatio, Mathf.Max(1f / DeformationRatio, 0.25f));
		base.gameObject.SetActive(true);
	}
public void Deflect(float maximum)
	{
		float num = 5f / Mass;
		float num2 = Random.value * maximum * DeformationRatio * num * (1f - Ammunition.elasticity);
		base.transform.Rotate(Vector3.forward, Random.value * 360f);
		base.transform.Rotate(Vector3.right, num2);
		Tumble = Random.onUnitSphere * FrictionEnergyNormalized * num2;
		AssignNewSpin();
	}
public void AssignNewSpinAndTumble()
	{
		AssignNewSpin();
		Tumble = Random.onUnitSphere * FrictionEnergyNormalized * (5f / Mass);
	}
public void OnFragmentation()
	{
		FragmentationTime = Time.time;
		if (Ammunition.explosionMode == AmmunitionBase.ExplosionMode.OnFragmentation)
		{
			Explode(base.transform.position);
		}
	}
public void InitializeProjectileFragment(Projectile projectile, int uniqueId, Vector3 worldDisperseOrigin, float mass, float diameter, float velocity, float alpha, float beta, float disperseOffset)
	{
		Id = uniqueId;
		SwarmId = projectile.Id;
		base.transform.SetParent(projectile.transform.parent);
		base.transform.rotation = Quaternion.LookRotation(projectile.transform.forward);
		base.transform.Rotate(Vector3.forward, beta);
		base.transform.Rotate(Vector3.right, alpha);
		base.transform.position = worldDisperseOrigin + base.transform.forward * disperseOffset * Random.Range(0.7f, 1f);
		AudioInstigator = projectile.AudioInstigator;
		HasFragmented = true;
		HasExploded = true;
		Weapon = projectile.Weapon;
		Owner = projectile.Owner;
		Ammunition = projectile.Ammunition;
		Mass = mass;
		this.diameter = diameter;
		float baseVelocity = (Velocity = velocity);
		BaseVelocity = baseVelocity;
		StartEnergy = VelocityToEnergy(BaseVelocity);
		CommonInitialize();
		Spin = projectile.Spin;
		Tumble = Random.onUnitSphere;
		RiflingTwistRate = projectile.RiflingTwistRate;
		ChangeToFragmentedMesh();
		if (projectile.segmentLength > 0f)
		{
			SetActiveSegment(projectile.Distance, projectile.segmentLength);
		}
	}
public void SpawnSpecificFragments(Vector3 worldOrigin, UnityAction<Projectile, bool> exitCallback = null)
	{
		bool fragmentEvenPattern = Ammunition.fragmentEvenPattern;
		float num = 360f / (float)(Ammunition.specificFragmentation.Count - 1);
		int frameCount = Time.frameCount;
		for (int i = 0; i < Ammunition.specificFragmentation.Count - 1; i++)
		{
			Projectile projectile = Object.Instantiate(this);
			AmmunitionBase.FragmentationSpecification fragmentationSpecification = Ammunition.specificFragmentation[i];
			projectile.InitializeProjectileFragment(this, frameCount + i, worldOrigin, fragmentationSpecification.mass, fragmentationSpecification.diameter, Velocity * Random.Range(0.75f, 0.85f), fragmentationSpecification.deflection, (!fragmentEvenPattern) ? Random.Range(0f, 360f) : (num * (float)i), PenetrationDistance);
			if (exitCallback != null)
			{
				exitCallback(projectile, false);
			}
		}
	}
public void SpawnRandomFragments(Vector3 worldOrigin, UnityAction<Projectile, bool> exitCallback = null)
	{
		float num = Mass - Ammunition.fragmentMainBodyMass;
		int frameCount = Time.frameCount;
		while (num > 0f)
		{
			Projectile projectile = Object.Instantiate(this);
			float alpha = Random.Range(Ammunition.fragmentDeflectionMin, Ammunition.fragmentDeflectionMax);
			float num2 = Random.Range(Ammunition.fragmentMassMin, Ammunition.fragmentMassMax);
			if (num2 > num)
			{
				num2 = num;
			}
			float num3 = Random.Range(Ammunition.fragmentDiameterMin, Ammunition.fragmentDiameterMax);
			projectile.InitializeProjectileFragment(this, frameCount++, worldOrigin, num2, num3, Velocity * Random.Range(0.5f, 0.75f), alpha, Random.Range(0f, 360f), PenetrationDistance);
			num -= num2;
			if (exitCallback != null)
			{
				exitCallback(projectile, false);
			}
		}
	}
}
