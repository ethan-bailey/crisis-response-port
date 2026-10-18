using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Limb : BallisticMaterial
{
	[Header("Limb properties")]
	[Tooltip("The id of this limb. Used for lookup access. Must be unique among limbs in the body.")]
	public LimbId id;

	[Tooltip("The limb will be placed as a child to this transform when it enters kinematic state.")]
	public Transform kinematicParent;

	[Tooltip("The AI uses this radius to produce a more precise aimpoint on the surface of the limb. Offset from the aim point in meters.")]
	public float radius = 0.15f;

	[Tooltip("This is the limb renderer transform except for the hands.")]
	public Transform visual;

	public List<LimbAttachment> attachments = new List<LimbAttachment>();

	[Header("Aim target")]
	[Tooltip("Local position on the limb used by AI for aiming.")]
	public Vector2 aimpoint;

	[Tooltip("This scales how much exit wound sizes increase in size when they accumulate. Zero: No ammumulation.")]
	public float exitAccumulationFactor = 0.25f;

	[Header("Rendering")]
	public LimbRenderer limbRenderer;

	[Tooltip("The local z position when the body faces left.")]
	public float zLeft;

	public int renderIndexLeft;

	public int renderIndexRight;

	[Header("Bleed points")]
	[Tooltip("This is only used for the head. These points bleed when concussed.")]
	public List<Vector2> bleedPoints = new List<Vector2>();

	private Physiology physiology;

	private Vector3 potentialForce;

	private Vector3 potentialPosition;

	private Vector3 previousPosition;

	private ParticleSystem.ShapeModule emitterShapeModule;

	private ParticleSystem.MainModule emitterMainModule;

	private List<FloorStreakPainter> floorStreaks;

	private float lastExitTimestamp;

	private float accumulatedExitVolume;

	private float zRight;

	private float potentialTime;

	private float woundStreakShortLifetime = 2f;

	private float woundStreakMediumLifetime = 4f;

	private float woundStreakLongLifetime = 6f;

	private bool hasSpawnedStreaks;

	private bool useSurfaceDecals;

	private bool useWoundDecals;

	private bool useCensoring;

	private bool useImpactEmissions;

	private float rigidBodyDefaultDrag;

	public GameObject CensorBox { get; private set; }

	public LimbArmor Armor { get; private set; }

	public Vector3 AimPoint => transform.TransformPoint(aimpoint.x, aimpoint.y, 0f);

	public Vector3 Velocity { get; private set; }

	public CharacterBody Body { get; private set; }

	public LimbPhysiology LimbPhysiology { get; private set; }

	public LimbMuscle LimbMuscle { get; private set; }

	public Rigidbody Rigidbody { get; private set; }

	public HingeJoint Joint { get; private set; }

	public DynamicLimbDecalRenderer DynamicDecalRenderer { get; private set; }

	public FloorPoolPainter FloorPoolPainter { get; private set; }

	public Emitter WoundEmitter { get; private set; }

	public bool Stationary { get; private set; }

	public bool Wounded => LimbPhysiology.Wounded;

	public bool IsHemorrhaging => LimbPhysiology.IsHemorrhaging;

	public bool HasArmor => Armor != null && Armor.type != ArmorClass.Type.None;

	public bool Censored => CensorBox != null;

	public int HitCount { get; private set; }

	public override void Awake()
	{
		base.Awake();
		Rigidbody = GetComponent<Rigidbody>();
		Joint = GetComponent<HingeJoint>();
		Armor = GetComponent<LimbArmor>();
		Body = transform.GetComponentInParent<CharacterBody>();
		physiology = Body.GetComponent<Physiology>();
		LimbPhysiology = GetComponent<LimbPhysiology>();
		LimbMuscle = GetComponent<LimbMuscle>();
		zRight = ((!(visual != null)) ? 0f : visual.localPosition.z);
	}

	public override void Start()
	{
		base.Start();
		floorStreaks = new List<FloorStreakPainter>();
		WoundEmitter = Body.WoundEmitter;
		emitterMainModule = WoundEmitter.system.main;
		emitterMainModule.maxParticles = perfMgr.MaxBloodParticles;
		emitterShapeModule = WoundEmitter.system.shape;
	}

	public void ApplySettings(Settings settings)
	{
		useSurfaceDecals = settings.Performance.SurfaceDecalsEnabled;
		useWoundDecals = settings.Performance.woundDecals;
		useImpactEmissions = ((id != LimbId.Head) ? settings.Performance.BodyEmissionsEnabled : settings.Performance.HeadEmissionsEnabled);
		woundStreakShortLifetime = 0.5f * perfMgr.WoundStreakLifetime;
		woundStreakMediumLifetime = 0.75f * perfMgr.WoundStreakLifetime;
		woundStreakLongLifetime = perfMgr.WoundStreakLifetime;
		useCensoring = settings.Graphics.applyCensoring;
		if (LimbPhysiology != null)
		{
			bool flag = settings.Performance.DynamicLimbDecalMode == SerializablePerformanceSettings.DynamicLimbDecalsType.All || (id == LimbId.Head && settings.Performance.DynamicLimbDecalMode == SerializablePerformanceSettings.DynamicLimbDecalsType.OnlyHead);
			LimbPhysiology.ApplySettings(perfMgr.BloodParticlesEnabled, perfMgr.BloodStreamsEnabled, flag, settings.Extras.woundPowerMultiplier);
			if (flag && limbRenderer != null && DynamicDecalRenderer == null)
			{
				DynamicDecalRenderer = gameObject.AddComponent<DynamicLimbDecalRenderer>();
			}
			else if (!flag && DynamicDecalRenderer != null)
			{
				Object.Destroy(DynamicDecalRenderer);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (!(Time.deltaTime <= 0f))
		{
			if (Time.time >= potentialTime + 0.09f)
			{
				potentialForce = Vector3.zero;
			}
			Velocity = (transform.position - previousPosition) * (1f / Time.deltaTime);
			Stationary = Mathf.Abs(Velocity.x) < 0.01f;
			previousPosition = transform.position;
			UpdateFloorStreaks();
			UpdateFloorPool();
		}
	}

	public void WoundEmission(int count, Vector3 worldPosition, Vector3 worldDirection, float startVelocity, float emissionRadiusMeters)
	{
		WoundEmitter.SetTransform(worldPosition, Quaternion.LookRotation(worldDirection), Velocity);
		emitterMainModule.startSpeed = startVelocity;
		emitterShapeModule.radius = emissionRadiusMeters;
		emitterShapeModule.angle = 1f;
		WoundEmitter.Emit(count);
	}

	public void OnBecomeRagdoll()
	{
		if (LimbMuscle != null)
		{
			LimbMuscle.Relax();
		}
	}

	protected override Projectile HandleDeflection(Projectile projectile, Vector3 normal, bool simulation = false)
	{
		float energy = projectile.Energy;
		base.HandleDeflection(projectile, normal, simulation);
		float num = energy - projectile.Energy;
		if (num > 0f)
		{
			AddTrauma(0f, projectile.transform.forward * num * projectile.ForceMultiplier, projectile.transform.position);
			LimbPhysiology.AddShock(num * projectile.Ammunition.shockModifier);
		}
		return projectile;
	}

	private Projectile ArmorEntryTest(Projectile projectile)
	{
		float energy = projectile.Energy;
		Armor.Test(projectile, entry: true);
		float num = energy - projectile.Energy;
		if (num > 0f)
		{
			AddTrauma(0f, projectile.transform.forward * num * projectile.ForceMultiplier, projectile.transform.position);
			LimbPhysiology.AddShock(num);
		}
		return projectile;
	}

	protected override Projectile HandleEntry(Projectile projectile, Vector3 normal, bool isEditor = false)
	{
		if (Armor != null && Armor.type != ArmorClass.Type.None)
		{
			ArmorEntryTest(projectile);
		}
		PlayImpactAudio();
		float energy = projectile.Energy;
		base.HandleEntry(projectile, normal);
		float num = energy - projectile.Energy;
		AddTrauma(projectile.DisplacedMaterialVolume, projectile.transform.forward * num * projectile.ForceMultiplier, projectile.transform.position);
		LimbPhysiology.AddShock(num);
		return projectile;
	}

	protected override void FindInternalDistanceAndExitPosition(Projectile projectile)
	{
		if (limbRenderer == null)
		{
			Debug.LogErrorFormat("Limb {0} of {1} has no LimbRenderer!", name, Body.name);
			return;
		}
		availableInternalDistance = 0f;
		if (projectile.FiredByMarksman)
		{
			Vector3 position = projectile.transform.position;
			position.z = transform.position.z;
			Vector3 vector = position - projectile.transform.position;
			Vector3 vector2 = Vector3.Project(vector, projectile.transform.forward);
			projectile.transform.position += vector2;
		}
		LimbRenderer.TraceResult traceResult = FindEntryExitPoints(projectile, LimbLayer.SkinLayer, ref worldEntryPosition, ref worldExitPosition);
		if (traceResult == LimbRenderer.TraceResult.EntryExit)
		{
			availableInternalDistance = ((!projectile.FiredByMarksman) ? (worldEntryPosition - worldExitPosition).magnitude : depth);
		}
	}

	protected override Projectile HandleInterior(Projectile projectile, Vector3 normal, bool isEditor = false)
	{
		// Reject cross-actor limb/physiology identity before original tissue damage.
		OriginalActorIdentity.RequireProjectileHitContext(Body, this, projectile);
		float energy = projectile.Energy;
		float diameter = projectile.Diameter;
		float temporalCavityFactor = projectile.TemporalCavityFactor;
		base.HandleInterior(projectile, normal);
		float num = energy - projectile.Energy;
		float shock = num * temporalCavityFactor;
		physiology.AnnouncePosition = projectile.transform.position;
		AddTrauma(projectile.DisplacedMaterialVolume, projectile.transform.forward * num * projectile.ForceMultiplier, projectile.transform.position);
		LimbPhysiology.VitalsTest(projectile, worldEntryPosition, worldExitPosition, shock);
		LimbPhysiology.AddShock(shock);
		float num2 = projectile.Ammunition.diameter * 0.5f;
		float entryHoleModifier = projectile.Ammunition.entryHoleModifier;
		float num3 = num2 * 3f * entryHoleModifier;
		LimbPhysiology.CreateWound(projectile.Diameter, projectile.DisplacedMaterialVolume, num3 * 0.0005f, worldEntryPosition, normal);
		if (limbRenderer == null)
		{
			return projectile;
		}
		if (projectile.FiredByMarksman && useWoundDecals)
		{
			limbRenderer.SetDynamic();
			limbRenderer.BeginDamagePass();
			limbRenderer.DrawDamage(worldEntryPosition, num2 * entryHoleModifier, 0.8f);
			limbRenderer.DrawDamage(worldEntryPosition, num3, Random.Range(0.5f, 0.6f));
			limbRenderer.DrawDamage(worldEntryPosition, num2 * 2.5f * entryHoleModifier, Random.Range(0.2f, 0.4f));
			limbRenderer.EndDamagePass();
		}
		else if (useWoundDecals)
		{
			float num4 = 1.05f - Mathf.Clamp01((worldEntryPosition - AimPoint).magnitude / 0.15f);
			limbRenderer.SetDynamic();
			limbRenderer.BeginDamagePass();
			limbRenderer.DrawDamage(worldEntryPosition, num2 * 2f * entryHoleModifier, num4);
			limbRenderer.DrawDamage(worldEntryPosition, num3, Random.Range(0.5f, 0.6f));
			limbRenderer.DrawDamage(worldEntryPosition, num2 * 2.5f * entryHoleModifier, Random.Range(0.2f, 0.4f));
			limbRenderer.EndDamagePass();
			if (projectile.Distance < 0.6f && projectile.FirstHitCharacter == null)
			{
				float radiusMillimeters = Mathf.Max(50f, projectile.Distance * 250f);
				float num5 = (1f - Mathf.Min(projectile.Distance, 0.6f) / 0.6f) * 0.3f;
				float num6 = Mathf.Clamp01(projectile.Energy * 0.0005f) * 0.3f;
				float opacity = (num5 + num6) * projectile.Ammunition.burnModifier;
				limbRenderer.DrawBurn(worldEntryPosition, radiusMillimeters, opacity);
			}
		}
		if (useCensoring)
		{
			SetCensored(censored: true);
		}
		return projectile;
	}

	protected override void HandleExit(Projectile projectile, bool simulated = false)
	{
		base.HandleExit(projectile);
		if (projectile.Status != BallisticResult.Continue)
		{
			return;
		}
		AccumulateExitingProjectiles(projectile);
		float num = projectile.Diameter * 0.5f;
		float num2 = Mathf.Min(projectile.DisplacedMaterialVolume * 0.05f, num * 5f);
		float num3 = projectile.Energy * 0.01f;
		float num4 = num + num2 + num3;
		float magnitude = (projectile.transform.position - AimPoint).magnitude;
		float num5 = Mathf.Clamp01(magnitude / 0.15f);
		float num6 = Mathf.Clamp01(magnitude / 0.1f);
		float num7 = ((id != LimbId.Head) ? projectile.Ammunition.exitHoleModifier : projectile.Ammunition.headExitHoleModifier);
		float num8 = 1f + accumulatedExitVolume * 0.002f * exitAccumulationFactor;
		float num9 = num4 * num5 * num7 * num8;
		float num10 = num9 * 0.5f;
		if (perfMgr.GibsEnabled)
		{
			SpawnGibs(projectile, num9);
		}
		if (useWoundDecals)
		{
			if (projectile.FiredByMarksman)
			{
				Vector3 vector = worldEntryPosition - AimPoint;
				vector.z = 0f;
				vector.Normalize();
				float num11 = Random.Range(0.04f, 0.07f);
				float num12 = num11 * 1000f;
				float num13 = num11 * Random.Range(500f, 800f);
				Vector3 vector2 = worldEntryPosition + vector * (0.15f - num11);
				Vector3 worldPosition = worldEntryPosition + vector * 0.15f;
				limbRenderer.SetDynamic();
				limbRenderer.BeginDamagePass();
				limbRenderer.DrawDamage(vector2, num12, Random.Range(0.3f, 0.6f), 0.45f);
				limbRenderer.DrawDamage(vector2, num12 * Random.Range(0.5f, 0.85f), 1f, 0.6f);
				limbRenderer.DrawDamage(worldPosition, num13, 1f);
				limbRenderer.EndDamagePass();
				LimbPhysiology.CreateWound(num13 * 0.75f, projectile.DisplacedMaterialVolume, num11 * 0.75f, vector2, projectile.transform.forward);
			}
			else
			{
				Vector3 position = projectile.transform.position;
				Vector3 vector3 = position + projectile.transform.forward * num4 * 0.001f;
				limbRenderer.SetDynamic();
				limbRenderer.BeginDamagePass();
				for (int i = 0; i < 3; i++)
				{
					Vector2 vector4 = Random.insideUnitCircle * num10 * 0.001f;
					Vector3 worldPosition2 = projectile.transform.position + new Vector3(vector4.x, vector4.y, 0f);
					float num14 = Random.Range(0.1f, 0.5f) * num6 * projectile.Ammunition.depthModifier;
					limbRenderer.DrawDamage(worldPosition2, num10 * Random.Range(0.5f, 1f), num14);
				}
				limbRenderer.EndDamagePass();
				limbRenderer.ClearBlood(projectile.transform.position, num9, 1f);
				limbRenderer.ClearBurn(projectile.transform.position, num9);
				LimbPhysiology.CreateWound(projectile.Diameter, projectile.DisplacedMaterialVolume, num9 * 0.0005f, projectile.transform.position, projectile.transform.forward);
			}
		}
		if (Armor != null && Armor.type != ArmorClass.Type.None)
		{
			ArmorExitTest(projectile);
		}
	}

	private Projectile ArmorExitTest(Projectile projectile)
	{
		float energy = projectile.Energy;
		Projectile projectile2 = Armor.Test(projectile, entry: false);
		float num = energy - projectile.Energy;
		return projectile;
	}

	private void AccumulateExitingProjectiles(Projectile projectile)
	{
		if (Time.time - lastExitTimestamp < 0.0167f)
		{
			accumulatedExitVolume = Mathf.Min(accumulatedExitVolume + projectile.DisplacedMaterialVolume, 1000f);
		}
		else
		{
			accumulatedExitVolume = 0f;
		}
		lastExitTimestamp = Time.time;
	}

	private void SpawnGibs(Projectile projectile, float woundRadiusMillimeters)
	{
		if (!(woundRadiusMillimeters < 30f))
		{
			int num = Mathf.CeilToInt(woundRadiusMillimeters * 0.1f);
			if (num != 0)
			{
				GameObject prefab = ((id != LimbId.Head) ? Body.gibFleshChunk : Body.gibSkullChunk);
				Vector3 force = projectile.transform.forward * projectile.Velocity * 0.00075f;
				float scale = Mathf.Clamp01(woundRadiusMillimeters / 200f);
				perfMgr.SpawnGibs(num, prefab, projectile.Position, force, scale);
			}
		}
	}

	protected override void SpawnEntryEffect(Projectile projectile, Vector3 normal)
	{
		if (!perfMgr.GoreHitEffectsEnabled)
		{
			return;
		}
		BallisticHitEffect ballisticHitEffect = perfMgr.CreateGoreHitEffect(properties.entryEffect);
		if (ballisticHitEffect != null)
		{
			if ((bool)projectile.Trace)
			{
				projectile.Trace.SpawnHitEffect(projectile.transform.position, transform, normal);
			}
			float num = Vector3.Dot(normal, projectile.transform.forward);
			Vector3 worldDirection = ((!(num < -0.75f)) ? Vector3.Reflect(normal, projectile.transform.forward * -1f) : (projectile.transform.forward * -1f));
			ballisticHitEffect.InitializeTransform(container, projectile.Position - projectile.transform.forward * ballisticHitEffect.offset, worldDirection);
			ballisticHitEffect.PlayEntry(projectile);
		}
	}

	protected override void SpawnExitEffect(Projectile projectile)
	{
		if (perfMgr.GoreHitEffectsEnabled)
		{
			BallisticHitEffect ballisticHitEffect = perfMgr.CreateGoreHitEffect(properties.exitEffect);
			if (ballisticHitEffect != null)
			{
				ballisticHitEffect.InitializeTransform(container, projectile.transform.position, projectile.transform.forward);
				ballisticHitEffect.PlayExit(projectile, Rigidbody.velocity, Body.AudioInstigator);
			}
		}
	}

	private void PlayImpactAudio()
	{
		if (HasArmor)
		{
			Body.Audio.ArmorImpact();
		}
		if ((id == LimbId.Torso || id == LimbId.Abdomen) && Body.Alive && !(Body.Physiology.LungsOxygen < 0.25f))
		{
			Body.Audio.ImpactExhale();
		}
	}

	public void SetKinematic(bool kinematic, Vector3 worldSpaceVelocity = default(Vector3))
	{
		if (id != LimbId.Pelvis || !Body.PinnedBodies)
		{
			Rigidbody.isKinematic = kinematic;
			if (!kinematic)
			{
				transform.SetParent(Body.transform, worldPositionStays: true);
				Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				SetFloating(Body.FloatingBodies);
				Rigidbody.AddForce(worldSpaceVelocity, ForceMode.VelocityChange);
				Rigidbody.angularDrag = 10f;
				Rigidbody.AddForceAtPosition(potentialForce, potentialPosition, ForceMode.Impulse);
				potentialForce = Vector3.zero;
			}
		}
	}

	public void Flip()
	{
		Vector3 localPosition = visual.localPosition;
		localPosition.z = ((!Body.FacingRight) ? zLeft : zRight);
		visual.localPosition = localPosition;
		foreach (LimbAttachment attachment in attachments)
		{
			Vector3 localPosition2 = attachment.transform.localPosition;
			localPosition2.z = ((!Body.FacingRight) ? attachment.zLeft : attachment.zRight);
			attachment.transform.localPosition = localPosition2;
		}
	}

	public void SetFloating(bool enable)
	{
		if (Rigidbody.useGravity != !enable)
		{
			Rigidbody.useGravity = !enable;
			if (enable)
			{
				rigidBodyDefaultDrag = Rigidbody.drag;
			}
			Rigidbody.drag = ((!enable) ? rigidBodyDefaultDrag : 0f);
		}
	}

	private LimbRenderer.TraceResult FindEntryExitPoints(Projectile projectile, LimbLayer layer, ref Vector3 worldEntry, ref Vector3 worldExit, bool findExit = true)
	{
		LimbRenderer.TraceResult traceResult = LimbRenderer.TraceResult.Failure;
		traceResult = (projectile.FiredByMarksman ? limbRenderer.FindMarksmanEntryAndExit(projectile.transform.position, projectile.transform.forward, ref worldEntry, ref worldExit) : limbRenderer.FindEntryExit(projectile.transform.position, projectile.transform.forward, 0.45f, ref worldEntry, ref worldExit));
		if (traceResult == LimbRenderer.TraceResult.EntryExit)
		{
			foundPotentialExit = true;
		}
		return traceResult;
	}

	public void AddTrauma(float volume, Vector3 impulse, Vector3 impulsePosition)
	{
		LimbPhysiology.AddTrauma(volume);
		ApplyForceAtPosition(impulse, impulsePosition);
		Body.EventHit.Invoke();
	}

	public void CreateConcussiveWounds(float magnitude)
	{
		if (!(DynamicDecalRenderer == null) && perfMgr.WoundStreaksEnabled)
		{
			for (int i = 0; i < bleedPoints.Count; i++)
			{
				DynamicDecalRenderer.SpawnStreaks(Mathf.CeilToInt(magnitude * 3f), bleedPoints[i], 0.02f, magnitude * 0.1f, woundStreakLongLifetime);
			}
		}
	}

	public void StopHemorrhage()
	{
		if (LimbPhysiology != null)
		{
			LimbPhysiology.StopHemorrhage();
		}
	}

	public void SpawnDynamicDecals(Wound wound)
	{
		if (!(DynamicDecalRenderer == null))
		{
			float num = Mathf.Max(physiology.BloodVolumeNormalized * ((id != LimbId.Head) ? 0.075f : 0.025f), wound.Diameter * 0.001f);
			float period = ((id != LimbId.Head) ? (3f + (1f - LimbPhysiology.bloodPressureModifier) * 15f + wound.Diameter / 40f * 5f) : 0.5f);
			if (num > 0.005f)
			{
				DynamicDecalRenderer.SpawnSoakSpot(wound.LocalPosition, num, period, 0.1f);
			}
			if (perfMgr.WoundStreaksEnabled)
			{
				DynamicDecalRenderer.SpawnStreaks(5, wound.LocalPosition, wound.EmissionRadiusMeters, 1f, woundStreakShortLifetime);
				DynamicDecalRenderer.SpawnStreaks(3, wound.LocalPosition, wound.EmissionRadiusMeters, 1f, woundStreakMediumLifetime);
				DynamicDecalRenderer.SpawnStreaks(1, wound.LocalPosition, wound.EmissionRadiusMeters, 1f, woundStreakLongLifetime);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (id != LimbId.Head || !useImpactEmissions || collision.contacts.Length == 0)
		{
			return;
		}
		ContactPoint contactPoint = collision.contacts[0];
		GameObject gameObject = contactPoint.otherCollider.gameObject;
		if (!(gameObject.tag != "Obstacle") || !(gameObject.tag != "Floor"))
		{
			float num = 10f * Rigidbody.mass * collision.relativeVelocity.sqrMagnitude;
			if (num >= 1f)
			{
				LimbPhysiology.AddShock(num);
			}
			if (id == LimbId.Head && LimbPhysiology.wounds.Count > 0)
			{
				BallisticHitEffect ballisticHitEffect = perfMgr.CreateGoreHitEffect(Body.headBrainBounce.GetComponent<BallisticHitEffect>());
				Wound wound = LimbPhysiology.wounds[0];
				int count = Mathf.RoundToInt(collision.relativeVelocity.magnitude * physiology.BrainTissueNormalized * wound.Diameter);
				ballisticHitEffect.transform.SetParent(MissionHelper.Instance.EffectsContainer, worldPositionStays: false);
				ballisticHitEffect.transform.position = transform.TransformPoint(wound.LocalPosition);
				ballisticHitEffect.PlayBounce(count, Rigidbody.velocity.magnitude);
			}
		}
	}

	public void ApplyForceAtPosition(Vector3 force, Vector3 position)
	{
		if (force.sqrMagnitude != 0f)
		{
			float value = Random.value;
			float num = Mathf.Abs(force.x) + Mathf.Abs(force.y);
			force.x += Mathf.Abs(force.z) * (Mathf.Abs(force.x) / num) * Mathf.Sign(force.x) * value;
			force.y += Mathf.Abs(force.z) * (Mathf.Abs(force.y) / num) * Mathf.Sign(force.y) * value;
			force.z = 0f;
			if (Rigidbody.isKinematic)
			{
				potentialForce *= 0.1f;
				potentialForce += force;
				potentialPosition = position;
				potentialTime = Time.time;
			}
			else
			{
				Rigidbody.AddForceAtPosition(force, position, ForceMode.Impulse);
			}
		}
	}

	private void UpdateFloorStreaks()
	{
		if (!perfMgr.FloorStreaksEnabled || id != LimbId.Head || !LimbPhysiology.HasHemorrhagingWounds)
		{
			return;
		}
		if (!hasSpawnedStreaks)
		{
			if (Stationary)
			{
				int num = perfMgr.RequestFloorStreaks(Random.Range(0, 3));
				Wound wound = LimbPhysiology.wounds[0];
				Vector3 localPosition = wound.LocalPosition;
				Vector3 vector = transform.TransformPoint(localPosition);
				Vector3 position = new Vector3(vector.x, 0f, vector.z);
				for (int i = 0; i < num; i++)
				{
					FloorStreakPainter floorStreakPainter = Object.Instantiate(Body.floorStreakPrefab);
					floorStreakPainter.transform.SetParent(MissionHelper.Instance.EffectsContainer);
					floorStreakPainter.transform.position = position;
					floorStreakPainter.Initialize();
					floorStreaks.Add(floorStreakPainter);
				}
				hasSpawnedStreaks = true;
			}
		}
		else
		{
			if (Stationary)
			{
				return;
			}
			perfMgr.ReturnFloorStreaks(floorStreaks.Count);
			foreach (FloorStreakPainter floorStreak in floorStreaks)
			{
				if (floorStreak != null)
				{
					Object.Destroy(floorStreak.gameObject);
				}
			}
			floorStreaks.Clear();
			hasSpawnedStreaks = false;
		}
	}

	private void UpdateFloorPool()
	{
		if (LimbPhysiology == null || !perfMgr.FloorPoolsEnabled || !LimbPhysiology.IsHemorrhaging)
		{
			return;
		}
		if (FloorPoolPainter == null)
		{
			if (Stationary)
			{
				FloorPoolPainter floorPoolPainter = perfMgr.CreateFloorPool();
				if (floorPoolPainter != null)
				{
					floorPoolPainter.name = $"{name} pool";
					floorPoolPainter.Initialize(new Vector3(transform.position.x, 0f, transform.position.z), shortLived: false, isStatic: true);
				}
				FloorPoolPainter = floorPoolPainter;
			}
		}
		else if (LimbPhysiology.wounds.Count > 0)
		{
			Wound wound = LimbPhysiology.wounds[0];
			Vector3 localPosition = wound.LocalPosition;
			Vector3 vector = transform.TransformPoint(localPosition);
			FloorPoolPainter.UpdateLimbPool(new Vector3(vector.x, 0f, vector.z), LimbPhysiology.Hemorrhaging);
		}
	}

	public Vector3 AimPointWithExtents(Vector3 origin)
	{
		Vector3 vector = transform.TransformPoint(aimpoint.x, aimpoint.y, 0f);
		return vector + (origin - vector).normalized * radius;
	}

	public void SetCensored(bool censored)
	{
		if (id != LimbId.Head)
		{
			return;
		}
		if (censored)
		{
			if (CensorBox == null && Wounded)
			{
				CensorBox = Object.Instantiate(MissionHelper.Instance.HeadCensorPrefab);
				CensorBox.name = $"{Body.name} {name} censor";
				CensorBox.transform.SetParent(transform);
				CensorBox.transform.position = AimPoint + Vector3.back * 0.05f;
			}
		}
		else if (CensorBox != null)
		{
			Object.Destroy(CensorBox);
		}
	}

	public void Detach()
	{
		if (Body.Physiology.invulnerable)
		{
			return;
		}
		if (useWoundDecals && (bool)this.limbRenderer)
		{
			this.limbRenderer.SetDynamic();
			this.limbRenderer.BeginDamagePass();
			this.limbRenderer.DrawDamage(transform.position, 100f, Random.Range(0.5f, 0.7f), 0.1f);
			this.limbRenderer.DrawDamage(transform.position, 75f, 1f, 0.1f);
			this.limbRenderer.EndDamagePass();
		}
		if ((bool)LimbPhysiology)
		{
			LimbPhysiology.CreateWound(50f, 400f, 0.04f, transform.position, transform.TransformDirection(Vector3.up));
		}
		Limb limb = ((!(transform.parent != null)) ? null : transform.parent.GetComponent<Limb>());
		if ((bool)limb)
		{
			if ((bool)limb.limbRenderer && useWoundDecals)
			{
				LimbRenderer limbRenderer = limb.limbRenderer;
				limbRenderer.SetDynamic();
				limbRenderer.BeginDamagePass();
				limbRenderer.DrawDamage(transform.position, Random.Range(75f, 100f), Random.Range(0.5f, 0.7f), 0.1f);
				limbRenderer.EndDamagePass();
			}
			if ((bool)limb.LimbPhysiology)
			{
				limb.LimbPhysiology.CreateWound(50f, 400f, 0.04f, transform.position, limb.transform.TransformDirection(Vector3.down));
			}
		}
		if ((bool)Joint)
		{
			Object.Destroy(Joint);
			Rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
			transform.SetParent(Body.Mission.BodyPartsContainer);
		}
	}
}

}
