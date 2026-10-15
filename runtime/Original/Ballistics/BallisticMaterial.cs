using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;

public class BallisticMaterial : MonoBehaviour
{
	public enum Type
	{
		Default = 0,
		Wood = 1,
		Concrete = 2,
		Glass = 3,
		Plastic = 4
	}

	public BallisticMaterialProperties properties;


	public bool impenetrable;

	public bool surfaceTestOnExit;


	public float width = 1f;

	public float depth;

	public ProjectileEvent onInteractionBegin = new ProjectileEvent();

	public ProjectileEvent onInteractionEnd = new ProjectileEvent();

	protected bool foundPotentialExit;

	protected float availableInternalDistance;

	protected float randomDensity;

	protected Vector3 worldEntryPosition;

	protected Vector3 worldExitPosition;

	protected Transform container;

	protected PerformanceManager perfMgr;

	private List<DelayedBallisticExit> delayedExits = new List<DelayedBallisticExit>();

	public float SurfaceThickness { get; set; }

	public float SurfaceDensity { get; set; }

	public float MinimumDeflection { get; set; }

	public float MaximumDeflection { get; set; }

	public float Density { get; set; }

	public Vector2 DensityRandomness { get; set; }

	public virtual void Awake()
	{
		Initialize(properties);
		perfMgr = PerformanceManager.Instance;
	}

	private void Initialize(BallisticMaterialProperties properties)
	{
		if (!(properties == null))
		{
			SurfaceThickness = properties.surfaceThickness;
			SurfaceDensity = properties.surfaceDensity;
			MaximumDeflection = properties.maximumDeflection * 10f;
			Density = properties.density;
			DensityRandomness = properties.densityRandomness;
		}
	}

	public virtual void Start()
	{
		container = MissionHelper.Instance.EffectsContainer;
		if (properties == null)
		{
			Debug.LogWarningFormat("Ballistic material without properties: {0}", base.name);
		}
	}

	public virtual void Update()
	{
		for (int num = delayedExits.Count - 1; num >= 0; num--)
		{
			DelayedBallisticExit delayedBallisticExit = delayedExits[num];
			if (!(Time.time < delayedBallisticExit.activationTime))
			{
				HandleDelayedExit(delayedBallisticExit);
				delayedExits.RemoveAt(num);
			}
		}
	}

	public Projectile HandleProjectile(Projectile projectile, Vector3 normal)
	{
		projectile.BeginNewInteraction();
		onInteractionBegin.Invoke(projectile);
		randomDensity = Density * UnityEngine.Random.Range(DensityRandomness.x, DensityRandomness.y);
		worldEntryPosition = projectile.transform.position;
		OnProjectileHit(projectile, normal);
		SpawnEntryEffect(projectile, normal);
		HandleDeflection(projectile, normal);
		if (projectile.Status == BallisticResult.Deflected)
		{
			onInteractionEnd.Invoke(projectile);
			projectile.Status = BallisticResult.Continue;
			return projectile;
		}
		if (projectile.Status != BallisticResult.Continue)
		{
			onInteractionEnd.Invoke(projectile);
			return projectile;
		}
		HandleEntry(projectile, normal);
		if (projectile.Status != BallisticResult.Continue)
		{
			onInteractionEnd.Invoke(projectile);
			return projectile;
		}
		FindInternalDistanceAndExitPosition(projectile);
		HandleInterior(projectile, normal);
		if (projectile.Status == BallisticResult.Continue || projectile.Status == BallisticResult.Stop)
		{
			onInteractionEnd.Invoke(projectile);
		}
		return projectile;
	}

	protected virtual void OnProjectileHit(Projectile projectile, Vector3 normal)
	{
	}

	public virtual bool PassThroughWithoutContact(Vector3 worldPosition, Projectile projectile)
	{
		return false;
	}

	protected virtual void FindInternalDistanceAndExitPosition(Projectile projectile)
	{
		worldExitPosition = projectile.transform.position + projectile.transform.forward * width;
		if (projectile.FiredByMarksman)
		{
			availableInternalDistance = depth;
		}
		else
		{
			availableInternalDistance = width;
		}
		foundPotentialExit = true;
	}

	public static float ChannelVolume(float length, float startDiameterMillimeters, float endDiameterMillimeters, float cavityFactor = 1f)
	{
		float f = (startDiameterMillimeters + endDiameterMillimeters * cavityFactor) * 0.5f;
		return (float)Math.PI * Mathf.Pow(f, 2f) * length;
	}

	protected virtual Projectile HandleDeflection(Projectile projectile, Vector3 normal, bool simulation = false)
	{
		if (!DeflectProjectile(projectile, normal, simulation) && !simulation)
		{
			SpawnExitEffect(projectile);
		}
		return projectile;
	}

	public float EnergyToPenetrateSurface(Projectile projectile)
	{
		float distance = ((SurfaceThickness != 0f) ? Mathf.Min(SurfaceThickness, projectile.Ammunition.MetricLength * 0.5f) : (projectile.Ammunition.MetricLength * 0.5f));
		return projectile.EnergyToPenetrate(distance, SurfaceDensity);
	}

	protected bool DeflectProjectile(Projectile projectile, Vector3 normal, bool simulation = false)
	{
		float num = Mathf.Abs(Vector3.Dot(projectile.transform.forward, normal));
		float num2 = EnergyToPenetrateSurface(projectile);
		float num3 = num * projectile.Energy;
		bool flag = num3 >= num2;
		if (flag)
		{
			projectile.ConvertEnergyToFriction(num2);
			Vector3 forward = projectile.transform.forward;
			Vector3 normalized = Vector3.Cross(projectile.transform.forward, normal).normalized;
			float a = Vector3.Angle(normal * -1f, projectile.transform.forward) * UnityEngine.Random.Range(DensityRandomness.x, DensityRandomness.y) * MarksmanMode.DeflectionModifier;
			Vector3 to = Vector3.Cross(normal, normalized);
			float b = Vector3.Angle(projectile.transform.forward, to) * 0.9f;
			projectile.transform.Rotate(projectile.transform.InverseTransformDirection(normalized), Mathf.Min(a, b));
			Vector3 forward2 = Vector3.Lerp(projectile.transform.forward, forward, projectile.Ammunition.elasticity);
			projectile.transform.rotation = Quaternion.LookRotation(forward2);
		}
		else if (projectile.EnergyNormalized > projectile.Ammunition.bounceThreshold)
		{
			projectile.ConvertEnergyToFriction(projectile.Energy * num);
			Vector3 a2 = Vector3.Reflect(projectile.transform.forward, normal);
			Vector3 vector = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
			Vector3 b2 = Vector3.one - vector * projectile.Ammunition.elasticity;
			Vector3 forward3 = Vector3.Scale(a2, b2);
			projectile.transform.rotation = Quaternion.LookRotation(forward3);
			projectile.StartPosition = projectile.transform.position;
			projectile.Status = BallisticResult.Deflected;
		}
		else
		{
			projectile.ConvertRemainingEnergyToFriction();
		}
		projectile.AssignNewSpinAndTumble();
		if (!simulation)
		{
			SpawnFragments(projectile);
		}
		return flag;
	}

	protected virtual Projectile HandleEntry(Projectile projectile, Vector3 normal, bool simulation = false)
	{
		if (impenetrable)
		{
			if (!simulation)
			{
				SpawnEntryEffect(projectile, normal);
			}
			projectile.ConvertRemainingEnergyToFriction();
			projectile.Status = BallisticResult.Stop;
			return projectile;
		}
		if (SurfaceThickness == 0f)
		{
			return projectile;
		}
		float diameter = projectile.Diameter;
		float energy = projectile.EnergyToPenetrate(SurfaceThickness, SurfaceDensity);
		projectile.ConvertEnergyToFriction(energy);
		projectile.DisplacedMaterialVolume += ChannelVolume(SurfaceThickness, diameter, projectile.Diameter);
		if (projectile.Energy <= 1f)
		{
			projectile.Status = BallisticResult.Stop;
			return projectile;
		}
		return projectile;
	}

	protected virtual Projectile HandleInterior(Projectile projectile, Vector3 normal, bool simulation = false)
	{
		float b = projectile.PotentialPenetrationDistance(randomDensity);
		projectile.PenetrationDistance = Mathf.Min(availableInternalDistance, b);
		float diameter = projectile.Diameter;
		float permanentCavityFactor = projectile.PermanentCavityFactor;
		if (projectile.PenetrationDistance < availableInternalDistance)
		{
			projectile.ConvertRemainingEnergyToFriction();
			projectile.DisplacedMaterialVolume += ChannelVolume(projectile.PenetrationDistance, diameter, projectile.Diameter, permanentCavityFactor);
			projectile.Status = BallisticResult.Stop;
		}
		else if (foundPotentialExit)
		{
			float energy = projectile.EnergyToPenetrate(projectile.PenetrationDistance, Density);
			projectile.ConvertEnergyToFriction(energy);
			projectile.DisplacedMaterialVolume += ChannelVolume(projectile.PenetrationDistance, diameter, projectile.Diameter, permanentCavityFactor);
			projectile.Deflect(projectile.PenetrationDistance * MaximumDeflection);
			bool flag = projectile.TestFragmentation();
			float num = projectile.PenetrationDistance / ((projectile.Ammunition.velocity + projectile.Velocity) * 0.5f);
			if (projectile.Velocity > 1f)
			{
				if (num < Time.deltaTime)
				{
					ImmediateProjectileExit(projectile);
					SpawnFragments(projectile);
				}
				else
				{
					if (flag)
					{
						EnqueFragments(projectile, num);
						if (projectile.Status == BallisticResult.Stop)
						{
							return projectile;
						}
					}
					EnqueueProjectileExit(projectile, num);
				}
			}
			else
			{
				projectile.Status = BallisticResult.Stop;
			}
		}
		else
		{
			projectile.Status = BallisticResult.Error;
		}
		return projectile;
	}

	protected virtual void HandleExit(Projectile projectile, bool simulation = false)
	{
		if (surfaceTestOnExit)
		{
			float energy = projectile.Energy;
			float diameter = projectile.Diameter;
			float energy2 = projectile.EnergyToPenetrate(SurfaceThickness, SurfaceDensity);
			projectile.ConvertEnergyToFriction(energy2);
			projectile.DisplacedMaterialVolume += ChannelVolume(SurfaceThickness, diameter, projectile.Diameter, projectile.PermanentCavityFactor);
			if (projectile.Energy <= 1f)
			{
				projectile.Status = BallisticResult.Stop;
				return;
			}
		}
		SpawnExitMesh(projectile);
		SpawnExitEffect(projectile);
	}

	private void HandleDelayedExit(DelayedBallisticExit exit)
	{
		exit.projectile.RestartAtPosition(base.transform.TransformPoint(exit.localResumePosition));
		HandleExit(exit.projectile);
	}

	protected void EnqueueProjectileExit(Projectile projectile, float delay)
	{
		projectile.Status = BallisticResult.Delayed;
		projectile.gameObject.SetActive(false);
		delayedExits.Add(new DelayedBallisticExit
		{
			activationTime = Time.time + delay,
			projectile = projectile,
			localResumePosition = base.transform.InverseTransformPoint(worldExitPosition)
		});
	}

	protected void ImmediateProjectileExit(Projectile projectile, bool simulation = false)
	{
		projectile.NextPosition = worldExitPosition;
		projectile.RestartAtPosition(worldExitPosition);
		HandleExit(projectile, simulation);
	}

	protected virtual void SpawnEntryEffect(Projectile projectile, Vector3 normal)
	{
		if (!perfMgr.EnvironmentHitEffectsEnabled)
		{
			return;
		}
		BallisticHitEffect ballisticHitEffect = perfMgr.CreateEnvironmentalHitEffect(properties.entryEffect);
		if (ballisticHitEffect != null)
		{
			if ((bool)projectile.Trace)
			{
				projectile.Trace.SpawnHitEffect(projectile.transform.position, base.transform, normal);
			}
			float num = Vector3.Dot(normal, projectile.transform.forward);
			Vector3 worldDirection = ((!(num < -0.75f)) ? Vector3.Reflect(normal, projectile.transform.forward * -1f) : (projectile.transform.forward * -1f));
			ballisticHitEffect.InitializeTransform(container, projectile.transform.position - projectile.transform.forward * ballisticHitEffect.offset, worldDirection);
			ballisticHitEffect.PlayEntry(projectile);
		}
	}

	protected virtual void SpawnExitEffect(Projectile projectile)
	{
		if (perfMgr.EnvironmentHitEffectsEnabled)
		{
			BallisticHitEffect ballisticHitEffect = perfMgr.CreateEnvironmentalHitEffect(properties.exitEffect);
			if (ballisticHitEffect != null)
			{
				ballisticHitEffect.InitializeTransform(container, projectile.transform.position + projectile.transform.forward * ballisticHitEffect.offset, projectile.transform.forward);
				ballisticHitEffect.PlayExit(projectile, Vector3.zero, projectile.AudioInstigator);
			}
		}
	}

	protected void SpawnEntryMesh(Projectile projectile, Vector3 worldSpaceNormal)
	{
	}

	protected void SpawnExitMesh(Projectile projectile)
	{
		if (properties.exitHole != null)
		{
			perfMgr.SpawnEnvironmentalHitMesh(properties.exitHole, projectile, base.transform);
		}
	}

	private void EnqueFragments(Projectile projectile, float delay)
	{
		switch (projectile.Ammunition.fragmentationMode)
		{
		case AmmunitionBase.FragmentationMode.Randomized:
			EnqueueRandomFragments(projectile, delay);
			break;
		case AmmunitionBase.FragmentationMode.Specified:
			EnqueueSpecificFragments(projectile, delay);
			break;
		}
		FinalizeFragmentation(projectile);
	}

	private void EnqueueRandomFragments(Projectile projectile, float delay)
	{
		AmmunitionBase ammunition = projectile.Ammunition;
		float num = projectile.Mass - ammunition.fragmentMainBodyMass;
		int frameCount = Time.frameCount;
		while (num > 0f)
		{
			float num2 = ((!(num < ammunition.fragmentMassMin)) ? UnityEngine.Random.Range(ammunition.fragmentMassMin, ammunition.fragmentMassMax) : num);
			Projectile projectile2 = UnityEngine.Object.Instantiate(projectile);
			float diameter = UnityEngine.Random.Range(ammunition.fragmentDiameterMin, ammunition.fragmentDiameterMax);
			float alpha = UnityEngine.Random.Range(ammunition.fragmentDeflectionMin, ammunition.fragmentDeflectionMax);
			projectile2.InitializeProjectileFragment(projectile, frameCount++, worldEntryPosition, num2, diameter, projectile.Velocity, alpha, UnityEngine.Random.Range(0f, 360f), projectile.PenetrationDistance);
			projectile2.gameObject.SetActive(false);
			delayedExits.Add(new DelayedBallisticExit
			{
				activationTime = Time.time + delay * UnityEngine.Random.Range(0.8f, 1.2f),
				projectile = projectile2,
				localResumePosition = base.transform.InverseTransformPoint(projectile2.transform.position)
			});
			num -= num2;
		}
	}

	private void EnqueueSpecificFragments(Projectile projectile, float delay)
	{
		bool fragmentEvenPattern = projectile.Ammunition.fragmentEvenPattern;
		float num = 360f / (float)(projectile.Ammunition.specificFragmentation.Count - 1);
		int frameCount = Time.frameCount;
		for (int i = 0; i < projectile.Ammunition.specificFragmentation.Count - 1; i++)
		{
			AmmunitionBase.FragmentationSpecification fragmentationSpecification = projectile.Ammunition.specificFragmentation[i];
			Projectile projectile2 = UnityEngine.Object.Instantiate(projectile);
			projectile2.InitializeProjectileFragment(projectile, frameCount++, worldEntryPosition, fragmentationSpecification.mass, fragmentationSpecification.diameter, projectile.Velocity, fragmentationSpecification.deflection, (!fragmentEvenPattern) ? UnityEngine.Random.Range(0f, 360f) : (num * (float)i), projectile.PenetrationDistance);
			projectile2.gameObject.SetActive(false);
			delayedExits.Add(new DelayedBallisticExit
			{
				activationTime = Time.time + delay * UnityEngine.Random.Range(0.8f, 1.2f),
				projectile = projectile2,
				localResumePosition = base.transform.InverseTransformPoint(projectile2.transform.position)
			});
		}
	}

	private void SpawnFragments(Projectile projectile)
	{
		if (projectile.TestFragmentation())
		{
			switch (projectile.Ammunition.fragmentationMode)
			{
			case AmmunitionBase.FragmentationMode.Randomized:
				projectile.SpawnRandomFragments(worldEntryPosition, HandleExit);
				break;
			case AmmunitionBase.FragmentationMode.Specified:
				projectile.SpawnSpecificFragments(worldEntryPosition, HandleExit);
				break;
			}
			FinalizeFragmentation(projectile);
		}
	}

	private void FinalizeFragmentation(Projectile projectile)
	{
		projectile.OnFragmentation();
		projectile.HasFragmented = true;
		if (projectile.Ammunition.fragmentMainBodyMass == 0f)
		{
			projectile.Status = BallisticResult.Stop;
			return;
		}
		projectile.Mass = projectile.Ammunition.fragmentMainBodyMass;
		projectile.ChangeToFragmentedMesh(true);
	}
}
