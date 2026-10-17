using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
	public const int MaxIterations = 3;

	public const float FragmentOffset = 0.025f;

	public const float LeastBounceEnergy = 100f;

	public const float ForceScale = 0.015f;

	[Tooltip("This transform is the parent of the projectile's visual representation. It is scaled to visualize deformation.")]
	public Transform meshTransform;

	[Tooltip("Mesh used to visualize fragments of this projectile.")]
	public List<MeshFilter> fragmentMeshes = new List<MeshFilter>();

	[Tooltip("Material used for the fragment mesh.")]
	public Material fragmentMaterial;

	public ProjectileTrace tracePrefab;

	[Tooltip("Scales the overall count of particles spawned in interactions with this projectile. Added to restrict particles spawned by frag grenades.")]
	[Range(0f, 1f)]
	public float particleCountScaler = 1f;

	public ProjectileEvent onInteraction = new ProjectileEvent();

	public ProjectileLimbEvent onLimbInteraction = new ProjectileLimbEvent();

	private const float MaxPositionX = 100f;

	private const float MaxPositionZ = 400f;

	private RaycastHit limbRaycastResult;

	private RaycastHit nonDiscriminatingResult;

	private Ray raycastRay = default(Ray);

	private int ballisticMask;

	private int limbMask;

	private int obstacleMask;

	private float diameter;

	private float freeFallTimestamp;

	private float segmentBeginMagnitude;

	private float segmentEndMagnitude;

	private float segmentLength;

	public int Id { get; set; }

	public int SwarmId { get; set; }

	public float SpawnTime { get; private set; }

	public float FragmentationTime { get; private set; }

	public float FrictionEnergy { get; private set; }

	public float Spin { get; set; }

	private float RiflingTwistRate { get; set; }

	public WeaponBase Weapon { get; private set; }

	public CharacterBody Owner { get; set; }

	public Marksman MarksmanOwner { get; set; }

	public AudioInstigator AudioInstigator { get; private set; }

	public AmmunitionBase Ammunition { get; private set; }

	public CharacterBody FirstHitCharacter { get; private set; }

	public Vector3 StartPosition { get; set; }

	public Vector3 InteractionPosition { get; set; }

	public bool FiredWithPrecision { get; private set; }

	public Vector3 SegmentStart { get; private set; }

	public Vector3 Tumble { get; set; }

	public bool IsStray { get; private set; }

	public bool HasFragmented { get; set; }

	public bool HasExploded { get; set; }

	public bool FiredByMarksman
	{
		get
		{
			return MarksmanOwner != null;
		}
	}

	public ProjectileTrace Trace { get; private set; }

	public float ForceMultiplier { get; set; }

	public BallisticResult Status { get; set; }

	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
	}

	public Vector3 NextPosition { get; set; }

	public float Mass { get; set; }

	public float MetricMass
	{
		get
		{
			return Mass * 0.001f;
		}
	}

	public float BaseVelocity { get; set; }

	public float StartEnergy { get; private set; }

	public float Diameter
	{
		get
		{
			return DiameterAtFrictionEnergy(FrictionEnergy);
		}
	}

	public float MetricDiameter
	{
		get
		{
			return Diameter * 0.001f;
		}
	}

	public float FrictionEnergyNormalized
	{
		get
		{
			return Mathf.Clamp01(FrictionEnergy / StartEnergy);
		}
	}

	public float DeformationRatio
	{
		get
		{
			return Ammunition.expansionFunction.Evaluate(FrictionEnergyNormalized);
		}
	}

	public float BallisticCoefficient
	{
		get
		{
			return (1f - FrictionEnergyNormalized) * Ammunition.ballisticCoefficient;
		}
	}

	public bool SuperSonic
	{
		get
		{
			return BaseVelocity >= 343f;
		}
	}

	public float TemporalCavityFactor
	{
		get
		{
			return Ammunition.TemporalCavityFactor(Velocity);
		}
	}

	public float PermanentCavityFactor
	{
		get
		{
			return Ammunition.PermanentCavityFactor(Velocity);
		}
	}

	public float PenetrationDistance { get; set; }

	public float DisplacedMaterialVolume { get; set; }

	public float InteractionEnergy { get; set; }

	public float Distance
	{
		get
		{
			return DistanceAtPosition(base.transform.position);
		}
	}

	public float Velocity { get; private set; }

	public float Drag
	{
		get
		{
			return (1f - BallisticCoefficient) * Ammunition.flightDrag;
		}
	}

	public float PotentialFlightRangeLeft
	{
		get
		{
			return VelocityAtPosition(base.transform.position) / Drag;
		}
	}

	public float Energy
	{
		get
		{
			return KineticEnergy(MetricMass, Velocity);
		}
	}

	public float EnergyNormalized
	{
		get
		{
			return Energy / Ammunition.MuzzleEnergy;
		}
	}

	public static float KineticEnergy(float massInKilograms, float velocityInMetersPerSecond)
	{
		return 0.5f * massInKilograms * Mathf.Pow(velocityInMetersPerSecond, 2f);
	}

	public Limb RaycastLimbs(Ray ray, float distance)
	{
		if (Physics.Raycast(ray, out limbRaycastResult, distance, limbMask))
		{
			BallisticMaterial component = limbRaycastResult.transform.GetComponent<BallisticMaterial>();
		}
		return null;
	}

	public void RecalculateVelocity()
	{
		Velocity = Mathf.Max(VelocityAtPosition(base.transform.position), 0f);
	}

	public float DistanceAtPosition(Vector3 worldPosition)
	{
		return (worldPosition - StartPosition).magnitude;
	}

	public float DragVelocityAtPosition(Vector3 worldPosition)
	{
		return DistanceAtPosition(worldPosition) * Drag;
	}

	public float VelocityAtPosition(Vector3 worldPosition)
	{
		return Mathf.Max(BaseVelocity - DragVelocityAtPosition(worldPosition), 0f);
	}

	public Vector3 GravityVelocityAtTime(float time)
	{
		return new Vector3(0f, -9.82f * (time - freeFallTimestamp), 0f);
	}

	public float DiameterAtFrictionEnergy(float frictionEnergy, bool skipRotation = false)
	{
		float originalDiameterInMillimeters = diameter;
		if (!skipRotation)
		{
			float t = Vector3.Dot(base.transform.forward, meshTransform.forward);
			originalDiameterInMillimeters = Mathf.Lerp(Ammunition.length, diameter, t);
		}
		return Ammunition.DiameterAtEnergy(frictionEnergy, originalDiameterInMillimeters);
	}

	private void CommonInitialize()
	{
		SpawnTime = Time.time;
		freeFallTimestamp = Time.time;
		StartPosition = base.transform.position;
		limbMask = Masks.Instance.Limbs;
		ballisticMask = Masks.Instance.BallisticTargets;
		obstacleMask = Masks.Instance.BallisticTargetsSansLimbs;
		InteractionPosition = base.transform.position;
		if (tracePrefab != null)
		{
			Trace = Object.Instantiate(tracePrefab);
			Trace.Initialize(this);
		}
	}

	public void InitializeXY(WeaponBase weapon, bool usePrecision, float segmentStart = 0f, float segmentLength = 0f, float velocityModifier = 1f)
	{
		Weapon = Weapon;
		Owner = weapon.Owner;
		AudioInstigator = ((!(Owner != null)) ? null : Owner.AudioInstigator);
		Ammunition = weapon.ammunition;
		Mass = Ammunition.mass;
		diameter = Ammunition.diameter;
		InitializeVelocity(velocityModifier);
		CommonInitialize();
		if (usePrecision)
		{
			SetActiveSegment(segmentStart, segmentLength);
		}
		if (weapon.rifledBarrel)
		{
			RiflingTwistRate = weapon.rifleTwistRate;
			AssignNewSpin();
		}
	}

	public void InitializeBenchmark(AmmunitionBase ammunition)
	{
		Weapon = null;
		Owner = null;
		AudioInstigator = ((!(Owner != null)) ? null : Owner.AudioInstigator);
		Ammunition = ammunition;
		Mass = Ammunition.mass;
		diameter = Ammunition.diameter;
		InitializeVelocity(1f);
		CommonInitialize();
		RiflingTwistRate = 9f;
		AssignNewSpin();
	}

	public void InitializeMarksman(AmmunitionBase ammunition, float riflingTwistRate = 10f, AudioInstigator instigator = null)
	{
		Ammunition = ammunition;
		Mass = Ammunition.mass;
		AudioInstigator = instigator;
		diameter = Ammunition.diameter;
		InitializeVelocity(1f);
		RiflingTwistRate = riflingTwistRate;
		CommonInitialize();
		AssignNewSpin();
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

	public void InitializeExplosionFragment(AmmunitionBase ammunition, int swarmId, Transform parent, Vector3 worldOrigin, float mass, float diameter, float velocity, float disperseOffset, float alpha)
	{
		SwarmId = swarmId;
		base.transform.SetParent(parent);
		base.transform.Rotate(Vector3.up, 90f);
		base.transform.Rotate(Vector3.up, alpha);
		base.transform.Rotate(Vector3.right, Random.Range(0f, 360f));
		base.transform.position = worldOrigin + base.transform.forward * disperseOffset;
		HasFragmented = true;
		HasExploded = true;
		Ammunition = ammunition;
		Mass = mass;
		this.diameter = diameter;
		float baseVelocity = (Velocity = velocity);
		BaseVelocity = baseVelocity;
		StartEnergy = VelocityToEnergy(BaseVelocity);
		CommonInitialize();
		Spin = Random.value * 1000f;
		Tumble = Random.onUnitSphere;
		RiflingTwistRate = Random.value * 20f;
		ChangeToFragmentedMesh();
	}

	private void InitializeVelocity(float velocityModifier)
	{
		float num = 1f + Random.Range(Ammunition.velocityModifier * -0.5f, Ammunition.velocityModifier * 0.5f);
		float baseVelocity = (Velocity = Ammunition.velocity * num * velocityModifier);
		BaseVelocity = baseVelocity;
		StartEnergy = VelocityToEnergy(BaseVelocity);
	}

	public void ChangeToFragmentedMesh(bool isMainBody = false)
	{
		if (meshTransform.childCount > 0)
		{
			Object.Destroy(meshTransform.GetChild(0).gameObject);
		}
		MeshFilter meshFilter = Object.Instantiate(fragmentMeshes[Random.Range(0, fragmentMeshes.Count)]);
		meshFilter.transform.SetParent(meshTransform, false);
		meshFilter.transform.localScale = Vector3.one * Mass / Ammunition.mass;
		meshFilter.GetComponent<MeshRenderer>().material = fragmentMaterial;
	}

	public void RestartAtPosition(Vector3 worldPosition)
	{
		Status = BallisticResult.Continue;
		freeFallTimestamp = Time.time;
		base.transform.position = worldPosition;
		meshTransform.localScale = new Vector3(DeformationRatio, DeformationRatio, Mathf.Max(1f / DeformationRatio, 0.25f));
		base.gameObject.SetActive(true);
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

	public void SetActiveSegment(float startDistance, float length)
	{
		FiredWithPrecision = true;
		SegmentStart = base.transform.position + base.transform.forward * startDistance;
		segmentBeginMagnitude = startDistance;
		segmentEndMagnitude = startDistance + length;
		segmentLength = length;
		Debug.DrawLine(SegmentStart, SegmentStart + base.transform.forward * segmentLength, Color.green, 1f);
	}

	private void Update()
	{
		float num = 0f;
		float deltaTime = Time.deltaTime;
		int num2 = 3;
		bool flag = TestTermination();
		if (!flag && base.gameObject.activeInHierarchy && num < deltaTime && num2 > 0)
		{
			num2--;
			Vector3 position = base.transform.position;
			float num3 = deltaTime - num;
			float num4 = Velocity * num3;
			NextPosition = base.transform.position + base.transform.forward * num4 + GravityVelocityAtTime(Time.time + num3) * num3;
			Vector3 normalized = (NextPosition - base.transform.position).normalized;
			bool flag2 = Physics.Raycast(base.transform.position, normalized, out nonDiscriminatingResult, num4, ballisticMask);
			BallisticResult ballisticResult = BallisticResult.Continue;
			if (flag2)
			{
				base.transform.position = nonDiscriminatingResult.point;
				Limb component = nonDiscriminatingResult.collider.GetComponent<Limb>();
				TutorialSoftTarget component2 = nonDiscriminatingResult.collider.GetComponent<TutorialSoftTarget>();
				if (component == null && component2 == null)
				{
					ballisticResult = HandleGenericHit(nonDiscriminatingResult);
				}
				else if (!FiredWithPrecision)
				{
					ballisticResult = HandleLimbHit(nonDiscriminatingResult);
				}
				else
				{
					float num5 = DistanceAtPosition(NextPosition);
					if (num5 >= segmentBeginMagnitude && Distance < segmentEndMagnitude)
					{
						float num6 = Mathf.Max(Distance, segmentBeginMagnitude);
						float num7 = Mathf.Min(num5, segmentEndMagnitude);
						float maxDistance = num7 - num6;
						raycastRay.origin = StartPosition + normalized * num6;
						raycastRay.direction = normalized;
						if (Physics.Raycast(raycastRay, out limbRaycastResult, maxDistance, limbMask))
						{
							ballisticResult = HandleLimbHit(limbRaycastResult);
						}
					}
				}
				IsStray = true;
			}
			else
			{
				base.transform.position = NextPosition;
			}
			num += (position - base.transform.position).magnitude / Velocity;
			TestExplosion(num);
			if (ballisticResult == BallisticResult.Stop || ballisticResult == BallisticResult.Error)
			{
				flag = true;
			}
		}
		UpdateRotation();
		if (flag)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public bool TestTermination()
	{
		return Time.time - SpawnTime > 1f || Energy < 2f || Mathf.Abs(base.transform.position.x) > 100f || base.transform.position.y <= -0.1f || Mathf.Abs(base.transform.position.y) > 20f || Mathf.Abs(base.transform.position.z) > 400f;
	}

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

	private BallisticResult HandleLimbHit(RaycastHit hit)
	{
		base.transform.position = hit.point;
		Limb component = hit.collider.GetComponent<Limb>();
		RecalculateVelocity();
		if (component == null)
		{
			BallisticMaterial component2 = hit.collider.GetComponent<BallisticMaterial>();
			return component2.HandleProjectile(this, hit.normal).Status;
		}
		Projectile projectile = component.HandleProjectile(this, hit.normal);
		RecordAttackInfo(component.Body);
		component.Body.LastHitLimb = component;
		component.Body.OnWasHit(this);
		onInteraction.Invoke(this);
		onLimbInteraction.Invoke(this, component);
		return projectile.Status;
	}

	private void RecordAttackInfo(CharacterBody victim)
	{
		if (FiredByMarksman)
		{
			victim.OnWasAttacked(this);
			MarksmanOwner.OnSuccessfulAttack(victim);
		}
		else
		{
			if (FirstHitCharacter == null && Owner != null)
			{
				Owner.OnSuccessfulAttack(victim, SwarmId);
			}
			victim.OnWasAttacked(this);
		}
		FirstHitCharacter = victim;
	}

	public float VelocityToEnergy(float velocity)
	{
		return 0.5f * MetricMass * Mathf.Pow(velocity, 2f);
	}

	public void RemoveEnergy(float energy)
	{
		BaseVelocity = Mathf.Sqrt(Mathf.Max(Mathf.Pow(BaseVelocity, 2f) - energy / (0.5f * MetricMass), 0f));
	}

	public float ConvertEnergyToFriction(float energy)
	{
		FrictionEnergy += energy;
		InteractionEnergy += energy;
		RemoveEnergy(energy);
		RecalculateVelocity();
		return 0f;
	}

	public void ConvertRemainingEnergyToFriction()
	{
		FrictionEnergy += Energy;
		InteractionEnergy += Energy;
		BaseVelocity = 0f;
		Velocity = 0f;
	}

	public float PotentialExpansion(float energy, float density, float diameterInMillimeters, float frictionFactor)
	{
		return 1f;
	}

	public float PotentialPenetrationDistance(float energy, float density, float diameterInMillimeters, float frictionFactor)
	{
		return Mathf.Atan(energy / (Mathf.Pow(density, 2f) * frictionFactor * 0.001f * diameterInMillimeters));
	}

	public float PotentialPenetrationDistance(float density)
	{
		return PotentialPenetrationDistance(Energy, density, Diameter, Ammunition.frictionFactor);
	}

	public float EnergyToPenetrate(float distance, float density)
	{
		return EnergyToPenetrate(distance, density, Ammunition.frictionFactor, Diameter);
	}

	public float EnergyToPenetrate(float distance, float density, float frictionFactor, float diameterInMillimeters)
	{
		return Mathf.Tan(distance) * Mathf.Pow(density, 2f) * frictionFactor * 0.001f * diameterInMillimeters;
	}

	public bool TestFragmentation()
	{
		if (HasFragmented)
		{
			return false;
		}
		return FrictionEnergy >= Ammunition.FragmentationEnergy(Mass);
	}

	public static float MaximumRange(float muzzleVelocity, float ballisticCoefficient, float flightDrag)
	{
		return muzzleVelocity / (1f - 1f * ballisticCoefficient * flightDrag);
	}

	public void OnFragmentation()
	{
		FragmentationTime = Time.time;
		if (Ammunition.explosionMode == AmmunitionBase.ExplosionMode.OnFragmentation)
		{
			Explode(base.transform.position);
		}
	}

	private void TestExplosion(float time)
	{
		if (Ammunition.explosionMode == AmmunitionBase.ExplosionMode.None || Ammunition.explosionMode == AmmunitionBase.ExplosionMode.OnFragmentation)
		{
			return;
		}
		float num = 0f;
		if (Ammunition.explosionMode == AmmunitionBase.ExplosionMode.Timed)
		{
			num = Ammunition.explosionTime - (time - SpawnTime);
		}
		else
		{
			if (!HasFragmented)
			{
				return;
			}
			num = Ammunition.explosionTime - (time - FragmentationTime);
		}
		if (num <= Time.deltaTime)
		{
			Explode(base.transform.position + base.transform.forward * (Time.deltaTime - num) * Velocity);
		}
	}

	public void Explode(Vector3 position)
	{
		if (HasExploded)
		{
			return;
		}
		HasExploded = true;
		if (!(Ammunition.explosionPrefab == null))
		{
			Explosion explosion = Object.Instantiate(Ammunition.explosionPrefab);
			explosion.transform.SetParent(base.transform.parent);
			explosion.transform.position = position;
			explosion.transform.rotation = base.transform.rotation;
			switch (Ammunition.fragmentationMode)
			{
			case AmmunitionBase.FragmentationMode.Randomized:
				SpawnRandomFragments(position);
				break;
			case AmmunitionBase.FragmentationMode.Specified:
				SpawnSpecificFragments(position);
				break;
			}
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

	public void AssignNewSpin()
	{
		Spin = -360f * (Velocity / (RiflingTwistRate * 0.0254f));
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

	public void UpdateRotation()
	{
		float magnitude = (InteractionPosition - base.transform.position).magnitude;
		Vector3 localEulerAngles = magnitude * -360f * new Vector3(Tumble.x, Tumble.y, Tumble.z);
		meshTransform.localEulerAngles = localEulerAngles;
	}
}
