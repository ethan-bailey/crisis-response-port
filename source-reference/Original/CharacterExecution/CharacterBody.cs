using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.Events;

public class CharacterBody : MonoBehaviour
{
	public enum Team
	{
		Operators,
		Occupants
	}

	public enum HeadLookMode
	{
		Weapon,
		AimPoint,
		RightHand,
		LeftHand
	}

	public enum Tempo
	{
		Slow,
		Normal,
		Quick
	}

	public enum CharacterAction
	{
		None,
		SwitchMagazine,
		WorkBolt,
		SwitchWeapon,
		SecureWeapon,
		CheckVitals,
		RemoveObstacle,
		ApplyGauze,
		ExamineDoor,
		DisarmTrap
	}

	private delegate float ViewRangeDelegate();

	public const float DefaultAuditoryNoiseFloor = 0.025f;

	public const float AuditoryPerceptionDelay = 0.16f;

	public const float WeaponAngleTolerance = 10f;

	public const float HeadAlignmentSpeed = 15f;

	public const float TorsoAlignmentSpeed = 10f;

	public const float HoldWeaponBreakForce = 300f;

	public ArmsControl armsControl;

	public Team team;

	public float weight = 80f;

	public float interactionRange = 2f;

	public float colliderVerticalOffset = -0.725f;

	public WeaponMode weaponMode = WeaponMode.OnPoint;

	[Tooltip("Describes how the torso (and perhaps more later) will animate in response to the character being hit. X is time in the range [0,1], starting when the impact occurs. Y is the angle that will be added to the torso.")]
	public AnimationCurve hitRecoveryCurve;

	[Tooltip("To compensate recoil (computer) characters will scale the weapon's recoil climb with randomized values from this range. In normalized percent.")]
	public Vector2 recoilCompensation = new Vector2(0.5f, 1.5f);

	[Header("Locomotion")]
	public Tempo tempo = Tempo.Normal;

	public float walkSpeed = 1.2f;

	public float sprintSpeed = 3.2f;

	[Header("Perception")]
	public AnimationCurve visionOverloadFunction;

	[Tooltip("The character cannot hear sounds of amplitude less than this. Is scaled by current move speed.")]
	public float auditoryNoiseFloor = 0.025f;

	[Header("Prefabs")]
	[Tooltip("This effect is spawned when a head with exposed brain impacts a hard surface.")]
	public GameObject headBrainBounce;

	public Emitter woundEmitterPrefab;

	[Tooltip("This prefab is spawned at body parts with wounds.")]
	public FloorStreakPainter floorStreakPrefab;

	public GameObject gibSkullChunk;

	public GameObject gibFleshChunk;

	[Header("Transforms")]
	public Transform riflePivot;

	public Transform pistolPivot;

	public Transform pistolIKTarget;

	public Transform chestRigIKTarget;

	public Transform holster;

	public Transform stunGrenadeAnchor;

	public Transform fragGrenadeAnchor;

	[Header("Limbs")]
	public Limb Head;

	public Limb Neck;

	public Limb Torso;

	public Limb Abdomen;

	public Limb Pelvis;

	public Limb RightUpperArm;

	public Limb RightLowerArm;

	public Limb RightHand;

	public Limb LeftUpperArm;

	public Limb LeftLowerArm;

	public Limb LeftHand;

	public Limb RightUpperLeg;

	public Limb RightLowerLeg;

	public Limb RightFoot;

	public Limb RightToes;

	public Limb LeftUpperLeg;

	public Limb LeftLowerLeg;

	public Limb LeftFoot;

	public Limb LeftToes;

	public Transform Eye;

	public Transform Mouth;

	public GameObject headGear;

	public IKArmController armController;

	public ArmIKSolver rightArm;

	public ArmIKSolver leftArm;

	public List<Limb> limbs = new List<Limb>();

	public Dictionary<LimbId, Limb> limbMap = new Dictionary<LimbId, Limb>();

	[Header("Events")]
	public UnityEvent EventHit = new UnityEvent();

	public UnityEvent EventRagdoll = new UnityEvent();

	public UnityEvent EventUnconscious = new UnityEvent();

	public UnityEvent EventDeath = new UnityEvent();

	public UnityEvent EventWeaponActivated = new UnityEvent();

	[Header("Utility")]
	public RaycastHit raycastHit;

	private int lastProjectileHitById;

	private float hurtAudioTimestamp = -100f;

	private ViewRangeDelegate getViewRange;

	private Transform liveGrenades;

	private Transform effectsContainer;

	private GameObject helmet;

	private TimeScaler timeScaler;

	private CapsuleCollider bodyCollider;

	private DynamicInteractionMarker restrainMarker;

	private DynamicInteractionMarker stabilizeMarker;

	private Emitter woundEmitter;

	private float visionRange = 4.5f;

	private Animator animator;

	private int hashMoving = Animator.StringToHash("Moving");

	private int hashBackwards = Animator.StringToHash("Backwards");

	private int hashMoveSpeed = Animator.StringToHash("MoveSpeed");

	private int hashKneelRatio = Animator.StringToHash("KneelRatio");

	private int hashCrippledRatio = Animator.StringToHash("CrippledRatio");

	private int roomsMask;

	private int obstaclesMask;

	private bool kinematic = true;

	private float previousPosition;

	private bool triggerPulled;

	private float torsoHitAngleAddition;

	private int hitCount;

	private bool isCompliant;

	private bool deadGripTrigger;

	private float acceleration;

	private float accelerationMagnitude;

	private float headModeTimestamp;

	private HeadLookMode headMode;

	private SpringJoint grabbyGrabLeft;

	private SpringJoint grabbyGrabRight;

	private Settings settings;

	public int SortingIndex { get; private set; }

	public float LastFiredTimestamp { get; set; }

	public float WeaponDropTime { get; private set; }

	public float WeaponAimTolerance => 2f + (1f - Blackboard.weaponSkill) * 30f;

	public float WeaponSkillAlignmentModifier => 1f - (1f - Blackboard.weaponSkill) * 0.66f;

	public Vector2 AutoTriggerReleaseTime => new Vector2(0.5f - Blackboard.weaponSkill * 0.4f, 1f - Blackboard.weaponSkill * 0.8f);

	public float HoldWeaponWorkIntensity { get; private set; }

	public Vector2 SemiTriggerReleaseTime => new Vector2(0.1f + (1f - Blackboard.weaponSkill) * 0.5f, 0.2f + (1f - Blackboard.weaponSkill) * 0.8f);

	public Mission Mission { get; private set; }

	public Communication Communication { get; private set; }

	public WeaponAimer WeaponAimer { get; private set; }

	public Blackboard Blackboard { get; private set; }

	public Equipment Equipment { get; private set; }

	public MuscleControl MuscleControl { get; private set; }

	public CharacterDialog Dialog { get; set; }

	public Grenade ReadyGrenade { get; private set; }

	public CharacterScoreCard ScoreCard { get; private set; }

	public CharacterAction PendingAction { get; set; }

	public float LookAngle { get; private set; }

	public CharacterAudio Audio { get; private set; }

	public AudioInstigator AudioInstigator { get; private set; }

	public AudioSurface AudioSurface { get; private set; }

	public Target Target { get; private set; }

	public float VisionRange => visionRange * (1f - visionOverloadFunction.Evaluate(VisionOverload));

	public float VisibleRange { get; set; }

	public float VisionOverload { get; private set; }

	public float ActionAttentionPeriod { get; set; }

	public bool StunResponseStateActive { get; private set; }

	public Rigidbody Rigidbody { get; private set; }

	public float Position => transform.position.x;

	public float KinematicPosition => Torso.transform.position.x;

	public float CollisionTimestamp { get; private set; }

	public Room Room { get; private set; }

	public Vector3 KinematicWorldPosition => Torso.transform.position;

	public float Direction { get; private set; }

	public bool FacingRight => Direction == 1f;

	public float RotationY { get; private set; }

	public float MovementDelta { get; private set; }

	public float Velocity => Rigidbody.velocity.x;

	public float TargetKneelRatio { get; private set; }

	public float CrippledRatio { get; set; }

	public Physiology Physiology { get; private set; }

	public float LastDirectionAttackedFrom { get; set; }

	public HashSet<int> HitCount { get; private set; }

	public float LastActionTimestamp { get; set; }

	public float LastAttackTimestamp { get; set; }

	public CharacterBody LastAttackTarget { get; set; }

	public float LastHitTimestamp { get; set; }

	public float LastHitEnergy { get; set; }

	public CharacterBody LastHitBy { get; set; }

	public Limb LastHitLimb { get; set; }

	public LimbId PreferredTargetLimb { get; set; }

	public bool HasFreeWeaponSlot => Equipment.primaryWeapon == null || Equipment.secondaryWeapon == null;

	public bool StabilizeVitalsAvailable => IsCompliantForInteraction && Physiology.HemorrhagingScore() < 1f && !Physiology.Exsanguinated;

	public bool RestrainAvailable => !IsRestrained && IsCompliantForInteraction && !IsOperator;

	public WeaponBase ActiveWeapon
	{
		get
		{
			if (Equipment == null)
			{
				return null;
			}
			if (Equipment.primaryWeapon != null && !Equipment.primaryWeapon.IsHolstered)
			{
				return Equipment.primaryWeapon;
			}
			if (Equipment.secondaryWeapon != null && !Equipment.secondaryWeapon.IsHolstered)
			{
				return Equipment.secondaryWeapon;
			}
			return null;
		}
	}

	public WeaponBase InactiveWeapon
	{
		get
		{
			if (IsRagdoll || Equipment == null)
			{
				return null;
			}
			if (Equipment.primaryWeapon != null && Equipment.primaryWeapon.IsHolstered)
			{
				return Equipment.primaryWeapon;
			}
			if (Equipment.secondaryWeapon != null && Equipment.secondaryWeapon.IsHolstered)
			{
				return Equipment.secondaryWeapon;
			}
			return null;
		}
	}

	public Vector3 AimPoint { get; set; }

	public Vector3 WeaponAlignPoint
	{
		get
		{
			WeaponBase activeWeapon = ActiveWeapon;
			if (activeWeapon == null)
			{
				return Vector3.zero;
			}
			if (activeWeapon.IsBlocked && activeWeapon.type != WeaponBrace.Freehand)
			{
				return new Vector3(transform.position.x + Direction * activeWeapon.CollisionClearance, 0f, transform.position.z);
			}
			if (weaponMode == WeaponMode.Lowered || IsLoweringWeapon || (IsSprinting && !IsUsingPrecision))
			{
				return new Vector3(transform.position.x + Direction * activeWeapon.armConfiguration.retractDistance, 0f, transform.position.z);
			}
			return AimPoint;
		}
	}

	public float WeaponTargetAngle { get; set; }

	public float TorsoCurrentAngle { get; set; }

	public float HeadCurrentAngle { get; set; }

	public float AbdomenCurrentAngle { get; set; }

	public float HeadBaseAngle { get; private set; }

	public float TorsoBaseAngle { get; private set; }

	public float KneelRatio { get; private set; }

	public bool IsPlayer => GetComponent<PlayerController>() != null;

	public bool IsOperator { get; private set; }

	public bool IsMoving => Mathf.Abs(MovementDelta) > Time.deltaTime * 0.1f;

	public bool IsMovingBackwards => IsMoving && Mathf.Sign(MovementDelta) != Direction;

	public bool IsSprinting { get; set; }

	public bool IsUsingPrecision { get; set; }

	public bool IsKneeling { get; set; }

	public bool IsOpeningDoor { get; set; }

	public bool IsTurning { get; set; }

	public bool IsLoweringWeapon { get; set; }

	public bool IsHoldingGrenade => ReadyGrenade != null;

	public bool IsManipulatingEquipment { get; set; }

	public bool IsGuarding { get; set; }

	public bool IsFleeing { get; set; }

	public bool IsRagdoll => !Kinematic;

	public bool IsRestrained { get; private set; }

	public bool IsStabilized { get; private set; }

	public bool IsCompliantForInteraction => IsRagdoll || IsOperator || IsCompliant;

	public bool CanBeGauzed => IsCompliantForInteraction && !Physiology.Exsanguinated && Physiology.HemorrhagingScore() < 1f && Time.time - LastHitTimestamp > 3f;

	public bool Invisible { get; set; }

	public bool ShowsSignsOfLife => Physiology.BrainTissueNormalized > 0.5f && (Conscious || MuscleControl.SpasmsActive || MuscleControl.CrampsActive || Physiology.AudibleRespiration);

	public bool IsCompliant
	{
		get
		{
			float num = Time.time - ComplianceStateTimestamp;
			return (isCompliant && num >= 0.5f) || (!isCompliant && num < 1f);
		}
		set
		{
			if (isCompliant != value)
			{
				ComplianceStateTimestamp = Time.time;
			}
			isCompliant = value;
		}
	}

	public bool PendingGauze { get; set; }

	public bool PendingRestrain { get; set; }

	public Grenade.Type PendingGrenadeType { get; set; }

	public bool AtAttention { get; set; }

	public bool FreeAimActive { get; set; }

	public bool HasSpawnedStreaks { get; set; }

	public bool NeedsCover { get; set; }

	public bool Conscious => Physiology.Conscious;

	public bool OnKneesComplying { get; set; }

	public bool Alive => Physiology.Alive;

	public bool Kinematic => kinematic;

	public bool PainMovementsActive { get; set; }

	public bool PinnedBodies { get; private set; }

	public bool NoPlayerLimp { get; private set; }

	public bool FloatingBodies { get; private set; }

	public int ShotsFired { get; set; }

	public int ShotsHit { get; set; }

	public int StunGrenadesUsed { get; set; }

	public int FragGrenadesUsed { get; set; }

	public float ComplianceStateTimestamp { get; set; }

	public Emitter WoundEmitter
	{
		get
		{
			if (woundEmitter == null)
			{
				woundEmitter = UnityEngine.Object.Instantiate(woundEmitterPrefab);
				woundEmitter.transform.SetParent(Mission.EffectsContainer);
			}
			return woundEmitter;
		}
	}

	public HeadLookMode HeadMode
	{
		get
		{
			return headMode;
		}
		set
		{
			if (Time.time - headModeTimestamp >= 1f)
			{
				headMode = value;
				headModeTimestamp = Time.time;
			}
		}
	}

	public bool ReportedDown { get; set; }

	public float LowestDetectableAudio => Mathf.Max(auditoryNoiseFloor * (1f + Mathf.Abs(Velocity)), AuditoryDeficiency * 3f);

	public float AuditoryDeficiency { get; private set; }

	public float FarViewCoord => Eye.transform.position.x + Direction * VisibleRange;

	public SpringJoint LeftRagdollGrab { get; private set; }

	public SpringJoint RightRagdollGrab { get; private set; }

	private void InitializeSettings()
	{
		settings = Global.Context.Settings;
		settings.Extras.onChanged.AddListener(ApplySettings);
		settings.Extras.onShowPhysiologyChanged.AddListener(ShowPhysiology);
		if (settings.Extras.showPhysiologyData)
		{
			ShowPhysiology(show: true);
		}
		ApplySettings(settings.Extras);
	}

	private void ApplySettings(SerializableExtraSettings newSettings)
	{
		FloatingBodies = newSettings.floatingBodies;
		PinnedBodies = newSettings.pinnedBodies;
		NoPlayerLimp = newSettings.playerDoesNotLimp;
		if (NoPlayerLimp && IsPlayer)
		{
			CrippledRatio = 0f;
		}
		SetFloating(newSettings.floatingBodies);
		ApplyPinnedBody(PinnedBodies);
		if (Mission.Started)
		{
			BehaviorTree component = GetComponent<BehaviorTree>();
			if (component != null && !IsOperator)
			{
				component.enabled = !newSettings.disableOccupants;
			}
		}
		foreach (Limb limb in limbs)
		{
			limb.ApplySettings(settings);
		}
	}

	public virtual void Awake()
	{
		Audio = GetComponent<CharacterAudio>();
		AudioInstigator = GetComponent<AudioInstigator>();
		Blackboard = GetComponent<Blackboard>();
		Communication = GetComponent<Communication>();
		WeaponAimer = GetComponent<WeaponAimer>();
		animator = GetComponent<Animator>();
		armController = GetComponent<IKArmController>();
		Rigidbody = GetComponent<Rigidbody>();
		Physiology = GetComponent<Physiology>();
		armsControl = GetComponent<ArmsControl>();
		bodyCollider = GetComponent<CapsuleCollider>();
		MuscleControl = GetComponent<MuscleControl>();
		Target = GetComponent<Target>();
		IsOperator = GetComponent<TeamOperator>() != null;
		Mission = Global.Context.Mission;
		obstaclesMask = Masks.Instance.Obstacles;
		roomsMask = Masks.Instance.Rooms;
		ScoreCard = new CharacterScoreCard(this);
		HitCount = new HashSet<int>();
		GameObject gameObject = GameObject.Find("Live grenades");
		if (gameObject != null)
		{
			liveGrenades = gameObject.transform;
		}
		previousPosition = Position;
		Direction = 1f;
		HeadBaseAngle = -13f;
		TorsoBaseAngle = -15f;
		InitializeLimbs();
		Transform transform = limbs[4].transform.Find("Helmet");
		if (transform != null)
		{
			helmet = limbs[4].transform.Find("Helmet").gameObject;
		}
		IsRestrained = false;
		ReportedDown = false;
		LastHitTimestamp = -100f;
		LastAttackTimestamp = -100f;
		LastFiredTimestamp = -100f;
		ComplianceStateTimestamp = -100f;
		LastActionTimestamp = float.PositiveInfinity;
		CollisionTimestamp = float.PositiveInfinity;
		Mission.onMissionBegin.AddListener(OnMissionBegin);
	}

	public void InitializeDialog(DialogTree tree)
	{
		Dialog = GetComponent<CharacterDialog>();
		Dialog.Initialize(tree);
	}

	public Equipment InitializeEquipment(SerializableEquipment equipment)
	{
		GameObject gameObject = EquipmentHelper.Instance.SpawnEquipment(equipment);
		Transform transform = gameObject.transform;
		gameObject.transform.SetParent(base.transform);
		Equipment = gameObject.GetComponent<Equipment>();
		WeaponBase weaponBase = null;
		WeaponBase weaponBase2 = null;
		if (transform.childCount > 0)
		{
			weaponBase = transform.GetChild(0).GetComponent<WeaponBase>();
			Equipment.primaryWeapon = weaponBase;
			InitializeWeapon(weaponBase);
		}
		if (transform.childCount > 0)
		{
			weaponBase2 = transform.GetChild(0).GetComponent<WeaponBase>();
			Equipment.secondaryWeapon = weaponBase2;
			InitializeWeapon(weaponBase2);
		}
		if (!ActivateWeapon(weaponBase, 0.1f, audio: false))
		{
			ActivateWeapon(weaponBase2, 0.1f, audio: false);
		}
		InitializeArmor(equipment.armor);
		if (Equipment.stunGrenades > 0)
		{
			InstantiateGrenadeOnEquipment(Grenade.Type.Stun);
		}
		if (Equipment.fragGrenades > 0)
		{
			InstantiateGrenadeOnEquipment(Grenade.Type.Frag);
		}
		return Equipment;
	}

	private void InitializeBodyWeight()
	{
		Head.Rigidbody.mass = 0.063f * weight;
		Neck.Rigidbody.mass = 0.01f * weight;
		Torso.Rigidbody.mass = 0.263f * weight;
		Abdomen.Rigidbody.mass = 0.11f * weight;
		Pelvis.Rigidbody.mass = 0.11f * weight;
		RightUpperArm.Rigidbody.mass = 0.0277f * weight;
		LeftUpperArm.Rigidbody.mass = 0.0277f * weight;
		RightLowerArm.Rigidbody.mass = 0.016f * weight;
		LeftLowerArm.Rigidbody.mass = 0.016f * weight;
		RightHand.Rigidbody.mass = 0.0066f * weight;
		LeftHand.Rigidbody.mass = 0.0066f * weight;
		RightUpperLeg.Rigidbody.mass = 0.0988f * weight;
		LeftUpperLeg.Rigidbody.mass = 0.0988f * weight;
		RightLowerLeg.Rigidbody.mass = 0.0465f * weight;
		LeftLowerLeg.Rigidbody.mass = 0.0465f * weight;
		RightFoot.Rigidbody.mass = 0.0145f * weight;
		LeftFoot.Rigidbody.mass = 0.0145f * weight;
	}

	private void InitializeWeapon(WeaponBase weapon)
	{
		if (!(weapon == null))
		{
			Transform parent = ((weapon.type != WeaponBrace.Shoulder) ? transform.Find("Pelvis/Abdomen/Torso/Right upper arm/Right lower arm/Right hand/Pistol pivot") : transform.Find("Pelvis/Abdomen/Torso/Rifle pivot"));
			weapon.Initialize(this);
			weapon.transform.parent = parent;
			weapon.transform.localPosition = Vector3.zero;
			weapon.Turn(Direction);
			if (!weapon.useInternalMagazine)
			{
				Magazine magazine = new Magazine(weapon);
				weapon.InsertMagazine(magazine);
				Equipment.AddMagazineForWeapon(magazine, weapon);
			}
			HolsterWeapon(weapon, audio: false);
		}
	}

	public void SetRandomAmmo()
	{
		WeaponBase primaryWeapon = Equipment.primaryWeapon;
		if (primaryWeapon != null)
		{
			List<AmmunitionBase> ammunition = Global.Context.EquipmentList.GetAmmunition(primaryWeapon.calibre, AmmunitionBase.Type.Any, Team.Occupants);
			if (ammunition.Count > 0)
			{
				primaryWeapon.ammunition = ammunition[UnityEngine.Random.Range(0, ammunition.Count)];
			}
		}
	}

	private void InitializeArmor(ArmorClass.Type type)
	{
		foreach (Limb limb in limbs)
		{
			if (limb.Armor == null)
			{
				continue;
			}
			switch (limb.id)
			{
			case LimbId.Head:
				if (type != ArmorClass.Type.None)
				{
					limb.Armor.type = ArmorClass.Type.TypeIIIA;
				}
				break;
			case LimbId.Abdomen:
			case LimbId.Torso:
			case LimbId.Neck:
				limb.Armor.type = type;
				break;
			}
		}
	}

	private void InitializeLimbs()
	{
		limbs.AddRange(new List<Limb>
		{
			Head, Neck, Torso, Abdomen, Pelvis, RightUpperArm, RightLowerArm, RightHand, LeftUpperArm, LeftLowerArm,
			LeftHand, RightUpperLeg, RightLowerLeg, RightFoot, RightToes, LeftUpperLeg, LeftLowerLeg, LeftFoot, LeftToes
		});
		foreach (Limb limb in limbs)
		{
			limbMap.Add(limb.id, limb);
		}
	}

	private void Start()
	{
		InitializeSettings();
		InitializeBodyWeight();
		timeScaler = Mission.GetComponent<TimeScaler>();
		effectsContainer = MissionHelper.Instance.EffectsContainer;
		if (Mission.InteriorMode != null)
		{
			getViewRange = Mission.InteriorMode.interiorCamera.SubjectViewDistance;
		}
		if (!IsOperator)
		{
			Turn((!(UnityEngine.Random.value > 0.5f)) ? (-1f) : 1f);
		}
	}

	private void OnMissionBegin()
	{
		BehaviorTree component = GetComponent<BehaviorTree>();
		if (component != null && (IsOperator || !Global.Context.Settings.Extras.disableOccupants))
		{
			component.enabled = true;
		}
	}

	private void Update()
	{
		if (RigidbodyDisintegrationDetected())
		{
			Debug.LogErrorFormat("CharacterBody {0} suffered rigidbody disintegration and was destroyed.", name);
			UnityEngine.Object.Destroy(gameObject);
		}
		else if (!Mission.Paused)
		{
			SetVisibleRange();
			UpdateInteractionMarkers();
			WeaponBase activeWeapon = ActiveWeapon;
			if (Physiology.ConsciousAndKinematic)
			{
				UpdateWeapon(activeWeapon);
				UpdateUpperBodyTransforms(activeWeapon);
			}
			UpdateHealth();
			UpdateAudio();
			UpdateAnimation();
			UpdateDeadGripTrigger(activeWeapon);
			visionRange = ((getViewRange != null) ? getViewRange() : 10f);
			Vector3 vector = Eye.transform.position + transform.right * VisibleRange;
		}
	}

	private void LateUpdate()
	{
		if (!Mission.Paused)
		{
			UpdateMovement();
		}
	}

	private bool RigidbodyDisintegrationDetected()
	{
		return (Head.transform.position - Torso.transform.position).sqrMagnitude > 4f;
	}

	private void UpdateAudio()
	{
		if (AuditoryDeficiency > 0f)
		{
			AuditoryDeficiency -= Time.deltaTime * 0.005f;
		}
		if (Physics.Raycast(transform.position, new Vector3(0f, -1f, 0f), out raycastHit, 1.5f, Masks.Instance.Obstacles))
		{
			Collider collider = raycastHit.collider;
			if (collider != null)
			{
				AudioSurface = collider.GetComponent<AudioSurface>();
			}
		}
	}

	private void UpdateHealth()
	{
		VisionOverload = Mathf.Max(VisionOverload - Time.deltaTime * 0.05f, 0f);
	}

	private void UpdateMovement()
	{
		MovementDelta = Position - previousPosition;
		previousPosition = Position;
		if (Mathf.Abs(MovementDelta) > 0f)
		{
			Room = SetCurrentRoom();
		}
	}

	private void ResetAcceleration()
	{
		acceleration = 0f;
	}

	private void UpdateAnimation()
	{
		TargetKneelRatio = ((!IsKneeling) ? 0f : 1f);
		float f = TargetKneelRatio - KneelRatio;
		float num = 5f * Mathf.Clamp(Mathf.Abs(f), 0.025f, 1f);
		KneelRatio += Mathf.Sign(f) * Time.deltaTime * num;
		bodyCollider.height = ((!(KneelRatio < 0.5f)) ? 1.25f : 1f);
		bodyCollider.center = new Vector3((!(KneelRatio < 0.5f)) ? (-0.15f) : 0f, colliderVerticalOffset, 0f);
		float num2 = Mathf.Abs(Velocity);
		animator.SetBool(hashMoving, IsMoving);
		animator.SetBool(hashBackwards, IsMovingBackwards);
		animator.SetFloat(hashMoveSpeed, (!(num2 < walkSpeed)) ? (Mathf.Clamp01((num2 - walkSpeed) / (sprintSpeed - walkSpeed)) * 0.5f) : 0f);
		animator.SetFloat(hashKneelRatio, KneelRatio);
		animator.SetFloat(hashCrippledRatio, (!IsOperator) ? CrippledRatio : 0f);
	}

	private void UpdateDeadGripTrigger(WeaponBase weapon)
	{
		if (deadGripTrigger && !(weapon == null) && weapon.ActiveFireMode == WeaponBase.FireMode.Auto)
		{
			triggerPulled = true;
		}
	}

	public bool IsHoldingWeapon(CharacterBody body)
	{
		return ActiveWeapon != null;
	}

	public bool WeaponIsHeldHigh()
	{
		WeaponBase activeWeapon = ActiveWeapon;
		if (activeWeapon == null)
		{
			return false;
		}
		Vector3 to = activeWeapon.transform.TransformDirection(Vector3.right);
		Vector3 vector = transform.TransformDirection(Vector3.right);
		float num = Vector3.Angle(vector, to);
		return num < 30f;
	}

	public bool WeaponIsFiring()
	{
		WeaponBase activeWeapon = ActiveWeapon;
		if (activeWeapon == null)
		{
			return false;
		}
		return activeWeapon.IsFiring;
	}

	public bool QueryTrigger()
	{
		bool result = triggerPulled;
		triggerPulled = false;
		return result;
	}

	public void PullTrigger()
	{
		if (rightArm.OnTarget)
		{
			triggerPulled = true;
		}
	}

	public bool InsertSingleRound()
	{
		WeaponBase activeWeapon = ActiveWeapon;
		if (activeWeapon == null)
		{
			return false;
		}
		if (activeWeapon.IsPrimary && Equipment.PrimaryRounds.count == 0)
		{
			return false;
		}
		if (!activeWeapon.IsPrimary && Equipment.SecondaryRounds.count == 0)
		{
			return false;
		}
		if (!activeWeapon.fireFromMagazine && activeWeapon.cyclingBolt.Status == CyclingBolt.State.Open && activeWeapon.cyclingBolt.Round == null)
		{
			InsertRoundIntoOpenBolt();
			return true;
		}
		if (activeWeapon.IsPrimary)
		{
			Equipment.PrimaryRounds.count--;
			activeWeapon.InsertRound();
			return true;
		}
		Equipment.SecondaryRounds.count--;
		activeWeapon.InsertRound();
		return true;
	}

	public void InsertRoundIntoOpenBolt()
	{
		WeaponBase activeWeapon = ActiveWeapon;
		if (!(activeWeapon == null))
		{
			if (activeWeapon.IsPrimary)
			{
				Equipment.PrimaryRounds.count--;
			}
			else
			{
				Equipment.SecondaryRounds.count--;
			}
			activeWeapon.InsertRoundIntoOpenBolt();
		}
	}

	public void SetRagdoll()
	{
		if (Time.time - WeaponDropTime > 1f)
		{
			DropWeapons(attachToRagdoll: true);
		}
		rightArm.enabled = false;
		leftArm.enabled = false;
		armsControl.Relax();
		SetKinematic(state: false);
		Target.active = false;
		if (ReadyGrenade != null && ReadyGrenade.PinRemoved)
		{
			ReadyGrenade.StartFuse(this);
		}
		foreach (Limb limb in limbs)
		{
			limb.OnBecomeRagdoll();
		}
		EventRagdoll.Invoke();
	}

	private void GrabbyTest()
	{
		List<Limb> list = new List<Limb>();
		foreach (Limb limb2 in limbs)
		{
			if (limb2.id != LimbId.LeftHand && limb2.id != LimbId.LeftLowerArm && limb2.id != LimbId.LeftUpperArm && limb2.id != LimbId.RightHand && limb2.id != LimbId.RightLowerArm && limb2.id != LimbId.RightUpperArm && limb2.LimbPhysiology.Wounded)
			{
				list.Add(limb2);
			}
		}
		if (list.Count > 0)
		{
			Limb limb = list[UnityEngine.Random.Range(0, list.Count)];
			if (limb.LimbPhysiology != null)
			{
				UnityEngine.Object.Destroy(grabbyGrabLeft);
				UnityEngine.Object.Destroy(grabbyGrabRight);
				grabbyGrabLeft = LeftHand.gameObject.AddComponent<SpringJoint>();
				grabbyGrabRight = RightHand.gameObject.AddComponent<SpringJoint>();
				Wound wound = limb.LimbPhysiology.wounds[UnityEngine.Random.Range(0, limb.LimbPhysiology.wounds.Count)];
				SpringJoint springJoint = grabbyGrabLeft;
				bool autoConfigureConnectedAnchor = false;
				grabbyGrabRight.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
				springJoint.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
				SpringJoint springJoint2 = grabbyGrabLeft;
				Rigidbody rigidbody = limb.Rigidbody;
				grabbyGrabRight.connectedBody = rigidbody;
				springJoint2.connectedBody = rigidbody;
				SpringJoint springJoint3 = grabbyGrabLeft;
				Vector3 localPosition = wound.LocalPosition;
				grabbyGrabRight.connectedAnchor = localPosition;
				springJoint3.connectedAnchor = localPosition;
				SpringJoint springJoint4 = grabbyGrabLeft;
				float spring = 250f;
				grabbyGrabRight.spring = spring;
				springJoint4.spring = spring;
			}
		}
	}

	public void SetUnconscious()
	{
		SetRagdoll();
		PainMovementsActive = false;
		EventUnconscious.Invoke();
		Audio.StopVoice();
		UnityEngine.Object.Destroy(grabbyGrabLeft);
		UnityEngine.Object.Destroy(grabbyGrabRight);
	}

	public void OnDeath()
	{
		UnityEngine.Object.Destroy(grabbyGrabLeft);
		UnityEngine.Object.Destroy(grabbyGrabRight);
		EventDeath.Invoke();
	}

	public void OnWasHit(Projectile projectile)
	{
		Mission.FirstInjuryTimestamp = Time.time;
		InvokeBulletTimeOnHit();
		EventHit.Invoke();
		HitCount.Add(projectile.SwarmId);
		if (Physiology.HemorrhagingScore() < 1f && Mission.AllRoomsChecked && Mission.Occupants != null && Mission.Occupants.AllRenderedSafe && Global.Context.Settings.General.showInteractionMarkers)
		{
			ShowStabilizeMarker(show: true);
		}
		if (!Physiology.invulnerable && Time.time - hurtAudioTimestamp > 10f && UnityEngine.Random.value >= Mathf.Clamp(Physiology.ShockResistance, 0.25f, 0.9f))
		{
			float num = UnityEngine.Random.Range(1.5f, 4f);
			Audio.Hurt(num);
			hurtAudioTimestamp = Time.time + num;
		}
		if (!IsOperator && Conscious)
		{
			if (Physiology.ShouldLoseConsciousness() && Physiology.ShockNormalized * 2f * UnityEngine.Random.value >= Physiology.ShockResistance)
			{
				DropWeapons(attachToRagdoll: false);
			}
			if (UnityEngine.Random.value < 0.25f && !Audio.VoicePlaying)
			{
				Audio.Hurt(0.3f);
			}
		}
	}

	public void OnSuccessfulAttack(CharacterBody victim, int projectileId)
	{
		LastAttackTarget = victim;
		LastAttackTimestamp = Time.time;
		if (projectileId != lastProjectileHitById)
		{
			ShotsHit++;
			lastProjectileHitById = projectileId;
		}
	}

	public void OnWasAttacked(Projectile projectile)
	{
		bool flag = Time.time - LastHitTimestamp < 1f;
		hitCount = ((!flag) ? 1 : (hitCount + 1));
		float burstModifier = ((!flag) ? 1f : (0.2f / (float)hitCount));
		ScoreCard.RegisterPenalties(projectile, Time.time, burstModifier);
		LastHitTimestamp = Time.time;
		LastHitBy = projectile.Owner;
		LastHitEnergy = projectile.InteractionEnergy;
		LastDirectionAttackedFrom = Mathf.Sign(transform.forward.x) * -1f;
	}

	public void SetKinematic(bool state)
	{
		if (kinematic == state)
		{
			return;
		}
		kinematic = state;
		animator.enabled = state;
		foreach (Limb limb in limbs)
		{
			limb.SetKinematic(state, Rigidbody.velocity);
		}
	}

	public void SetFloating(bool enable)
	{
		if (kinematic)
		{
			return;
		}
		foreach (Limb limb in limbs)
		{
			limb.SetFloating(enable);
		}
	}

	private void ApplyPinnedBody(bool enable)
	{
		if (!kinematic)
		{
			Pelvis.SetKinematic(enable);
		}
	}

	public virtual void Turn(float newDirection = 0f)
	{
		if (Direction == newDirection || Time.timeScale == 0f)
		{
			return;
		}
		if (newDirection == 0f)
		{
			newDirection = Direction * -1f;
		}
		Direction = newDirection;
		RotationY = ((Direction != 1f) ? 180f : 0f);
		Vector3 euler = new Vector3(0f, (newDirection != 1f) ? 180f : 0f, 0f);
		transform.rotation = Quaternion.Euler(euler);
		foreach (Limb limb in limbs)
		{
			limb.Flip();
		}
	}

	public void Move(float direction, float magnitude = 1f)
	{
		if (direction != 0f)
		{
			acceleration = direction;
			accelerationMagnitude = magnitude;
		}
	}

	public void LeftFootstep()
	{
		Audio.Footstep();
	}

	public void RightFootstep()
	{
		Audio.Footstep();
	}

	private void FixedUpdate()
	{
		if (acceleration != 0f)
		{
			float num = Mathf.Sign(acceleration);
			bool flag = num != Direction;
			bool flag2 = CrippledRatio > 0.25f && (!IsOperator || !NoPlayerLimp);
			float num2 = walkSpeed * ((!flag2) ? 1f : 0.8f);
			if (IsSprinting && !flag && !flag2)
			{
				num2 = sprintSpeed * ((!IsUsingPrecision) ? 1f : 0.5f);
			}
			if (IsOperator && flag2)
			{
				animator.speed = 0.8f;
			}
			else
			{
				animator.speed = ((!IsSprinting || flag || flag2 || !IsUsingPrecision) ? 1f : 1.35f);
			}
			float num3 = Mathf.Abs(Rigidbody.velocity.x);
			if (!(num3 > num2))
			{
				float num4 = num2 - num3;
				float x = num4 * num * Time.fixedDeltaTime * 750f * accelerationMagnitude;
				Rigidbody.AddForce(x, 0f, 0f, ForceMode.Acceleration);
				ResetAcceleration();
			}
		}
	}

	private Room SetCurrentRoom()
	{
		Vector3 position = Head.transform.position;
		position.y = 5f;
		if (!Physics.Raycast(position, Vector3.down, out raycastHit, 10f, roomsMask))
		{
			return null;
		}
		return raycastHit.collider.GetComponent<Room>();
	}

	public bool ActivateWeapon(WeaponBase weapon, float armTransitionTime = 0.3f, bool audio = true)
	{
		if (weapon == null)
		{
			return false;
		}
		weapon.Activate(this);
		if (audio && weapon.type == WeaponBrace.Freehand)
		{
			Audio.Unholster();
		}
		weapon.transform.SetParent((weapon.type != WeaponBrace.Shoulder) ? pistolPivot : riflePivot);
		weapon.transform.localPosition = Vector3.zero;
		rightArm.SetOnWeapon(weapon, armTransitionTime);
		leftArm.SetOnWeapon(weapon, armTransitionTime, rightArm: false);
		HeadMode = HeadLookMode.Weapon;
		EventWeaponActivated.Invoke();
		IsLoweringWeapon = false;
		PendingAction = CharacterAction.None;
		return true;
	}

	public void HolsterWeapon(WeaponBase weapon, bool audio = true)
	{
		if (weapon == null)
		{
			return;
		}
		if (weapon.type == WeaponBrace.Freehand)
		{
			weapon.transform.SetParent(holster);
			weapon.transform.localPosition = Vector3.zero;
			weapon.transform.localEulerAngles = Vector3.zero;
			if (audio)
			{
				Audio.Holster();
			}
		}
		else
		{
			weapon.transform.SetParent(riflePivot);
			weapon.transform.localPosition = Vector3.zero;
			weapon.transform.localEulerAngles = new Vector3(0f, 0f, 300f);
		}
		armsControl.rightArm.SetHandMode(Hand.Mode.Fist);
		armsControl.leftArm.SetHandMode(Hand.Mode.Fist);
		weapon.Holster();
		QueryTrigger();
	}

	public void PullGrenadePin()
	{
		if (ReadyGrenade != null)
		{
			ReadyGrenade.PullPin(this);
		}
	}

	public void InitiateGrenadeThrow()
	{
		armController.InitiateGrenadeThrow();
	}

	public Grenade ThrowGrenade(Vector3 direction)
	{
		if (ReadyGrenade == null)
		{
			return null;
		}
		DoorController doorController = FindDoor(interactionRange);
		if (doorController != null)
		{
			float num = Mathf.Sign(doorController.Position - Position);
			if (num == Direction && doorController.Angle < 75f && Mathf.Abs(Position - doorController.Knob(Position).position.x) < 0.75f)
			{
				ReadyGrenade.transform.position = doorController.GrenadeThrowPosition(num, 1f);
				direction = doorController.GrenadeThrowDirection(num);
			}
		}
		ReadyGrenade.onDetonate.AddListener(GrenadeDetonationCallback);
		ReadyGrenade.transform.SetParent(Mission.LiveGrenadesContainer);
		ReadyGrenade.Throw(direction);
		ReadyGrenade.StartFuse(this);
		Blackboard.ThrownGrenade = ReadyGrenade;
		Blackboard.ThrowStunTimestamp = Time.time;
		ReadyGrenade = null;
		armController.ResumePreviousWeapon();
		Audio.ThrowGrenade();
		return ReadyGrenade;
	}

	public void ReturnGrenadeToEquipment()
	{
		if (!(ReadyGrenade == null))
		{
			if (((ReadyGrenade.type != Grenade.Type.Frag) ? Equipment.stunGrenades : Equipment.fragGrenades) == 0)
			{
				InstantiateGrenadeOnEquipment(ReadyGrenade.type);
			}
			switch (ReadyGrenade.type)
			{
			case Grenade.Type.Stun:
				Equipment.stunGrenades++;
				break;
			case Grenade.Type.Frag:
				Equipment.fragGrenades++;
				break;
			}
			UnityEngine.Object.Destroy(ReadyGrenade.gameObject);
		}
	}

	private void GrenadeDetonationCallback()
	{
		Blackboard.ThrownGrenade = null;
		Blackboard.StunDetonateTimestamp = Time.time;
	}

	public void ApplyGauzeOnSelf()
	{
		if (Equipment.gauze != 0 && StabilizeVitalsAvailable)
		{
			Blackboard.PendingGauzeTarget = this;
		}
	}

	public DoorController FindDoor(float distance = 1f)
	{
		if (Room == null)
		{
			return null;
		}
		DoorController doorController = Room.NextDoor(Direction);
		DoorController doorController2 = Room.NextDoor(Direction * -1f);
		float num = ((!(doorController != null)) ? float.PositiveInfinity : Mathf.Abs(Position - doorController.Position));
		float num2 = ((!(doorController2 != null)) ? float.PositiveInfinity : Mathf.Abs(Position - doorController2.Position));
		if (Mathf.Min(num, num2) > distance)
		{
			return null;
		}
		return (!(num < num2)) ? doorController2 : doorController;
	}

	public bool ManipulateDoor(float direction, bool updateKnownLockState = false)
	{
		DoorController doorController = FindDoor(interactionRange);
		if (doorController == null || !doorController.InteractorInRange(this))
		{
			return false;
		}
		float num = Mathf.Sign(direction * Direction * doorController.OpeningDirection);
		if (num >= 0f)
		{
			return doorController.Open(this, updateKnownLockState);
		}
		return doorController.Close(this, updateKnownLockState);
	}

	public bool GradualManipulateDoor(float direction, bool updateKnownLockState = false)
	{
		DoorController doorController = FindDoor(interactionRange);
		if (doorController == null || !doorController.InteractorInRange(this))
		{
			return false;
		}
		float num = ((direction != 1f || !(doorController.Angle < 20f)) ? 20f : (20f - doorController.Angle));
		float num2 = Direction * doorController.OpeningDirection;
		return doorController.MovePartial(this, Mathf.Sign(direction) * num2 * num, updateKnownLockState);
	}

	public void DropWeapons(bool attachToRagdoll)
	{
		if (attachToRagdoll)
		{
			bool flag = Physiology.BrainTraumaNormalized > 0f && Physiology.BrainTraumaNormalized < 1f;
			if (flag && UnityEngine.Random.value > 0.5f)
			{
				PullTrigger();
			}
			float num = 0f;
			if (triggerPulled || Time.time - LastFiredTimestamp < 0.05f)
			{
				num = ((!flag || Physiology.ShouldLoseConsciousness()) ? UnityEngine.Random.Range(0.1f, 0.5f) : UnityEngine.Random.Range(0.25f, 2f));
				SetDeadGripTrigger(num);
			}
			AttachRagdollWeapon(ActiveWeapon, UnityEngine.Random.Range(0.5f, 3f));
			DropWeaponAsProxy(InactiveWeapon);
		}
		else if ((bool)Equipment)
		{
			DropWeapon(Equipment.primaryWeapon);
			DropWeapon(Equipment.secondaryWeapon);
		}
		WeaponDropTime = Time.time;
		IsUsingPrecision = false;
		HoldWeaponWorkIntensity = 0f;
		if (Conscious && !IsRagdoll)
		{
			armController.Unarmed();
		}
	}

	private void AttachRagdollWeapon(WeaponBase weapon, float seconds)
	{
		if (weapon != null)
		{
			weapon.SetNonKinematic();
			weapon.AttachForPeriod(this, seconds);
		}
	}

	public WeaponBase DropWeapon(WeaponBase weapon)
	{
		if (weapon == null)
		{
			return null;
		}
		weapon.SetNonKinematic();
		weapon.RigidBody.AddForce(Velocity, 0f, 0f, ForceMode.VelocityChange);
		weapon.ReleaseFromOwner();
		if (weapon.IsPrimary)
		{
			Equipment.primaryWeapon = null;
		}
		else
		{
			Equipment.secondaryWeapon = null;
		}
		return weapon;
	}

	private WeaponBase DropWeaponAsProxy(WeaponBase weapon)
	{
		if (weapon == null)
		{
			return null;
		}
		weapon.ReleaseFromOwner();
		if (weapon.IsPrimary)
		{
			Equipment.primaryWeapon = null;
		}
		else
		{
			Equipment.secondaryWeapon = null;
		}
		GameObject gameObject = new GameObject("Proxy", typeof(DroppedWeaponProxy), typeof(BoxCollider));
		gameObject.GetComponent<DroppedWeaponProxy>().Initialize(weapon, Mission.WeaponsContainer);
		return weapon;
	}

	private void SetDeadGripTrigger(float seconds)
	{
		deadGripTrigger = true;
		Invoke("ReleaseDeadGripTrigger", seconds);
	}

	private void ReleaseDeadGripTrigger()
	{
		deadGripTrigger = false;
	}

	public List<WeaponBase> FindWeapons(float radius)
	{
		List<WeaponBase> list = new List<WeaponBase>();
		foreach (Transform item in Mission.WeaponsContainer)
		{
			WeaponBase weaponBase = item.GetComponent<WeaponBase>();
			if (weaponBase == null)
			{
				DroppedWeaponProxy component = item.GetComponent<DroppedWeaponProxy>();
				weaponBase = ((!(component != null)) ? null : component.weapon);
			}
			if (weaponBase != null && Mathf.Abs(KinematicPosition - weaponBase.rightHandAnchor.position.x) <= radius && !BlockedByObstacle(item.position.x))
			{
				list.Add(weaponBase);
			}
		}
		return list;
	}

	public BallisticObstacle FindObstacle(float radius = 1.5f)
	{
		Ray ray = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), transform.right * Direction);
		Collider[] array = Physics.OverlapSphere(Pelvis.transform.position, radius, Masks.Instance.PhysicalProps);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			BallisticObstacle component = collider.GetComponent<BallisticObstacle>();
			if (component != null && !BlockedByDoor(component.transform.position.x))
			{
				return component;
			}
		}
		return null;
	}

	public WeaponBase FindWeapon(float radius)
	{
		WeaponBase result = null;
		List<WeaponBase> list = FindWeapons(radius);
		float num = float.PositiveInfinity;
		foreach (WeaponBase item in list)
		{
			float num2 = Mathf.Abs(KinematicPosition - item.Position);
			if (num2 <= radius && num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public bool PickupAndEquipWeapon(WeaponBase weapon, bool activate)
	{
		if (weapon == null)
		{
			return false;
		}
		if (Equipment.primaryWeapon != null && Equipment.secondaryWeapon != null)
		{
			return false;
		}
		if (Equipment.primaryWeapon == null)
		{
			Equipment.primaryWeapon = weapon;
			weapon.PickUpInitialize(this, isPrimary: true);
		}
		else if (Equipment.secondaryWeapon == null)
		{
			Equipment.secondaryWeapon = weapon;
			weapon.PickUpInitialize(this, isPrimary: false);
		}
		if (activate)
		{
			return ActivateWeapon(weapon);
		}
		HolsterWeapon(weapon);
		return true;
	}

	public void DropHeadGear()
	{
		if (!(headGear == null))
		{
			Head.Armor.Disable();
			headGear.transform.SetParent(null, worldPositionStays: true);
			headGear.GetComponent<Rigidbody>().isKinematic = false;
			headGear = null;
		}
	}

	public List<CharacterBody> FindBodies(bool notBehind = true, float radius = 2f, bool occupants = true, bool operators = true)
	{
		List<CharacterBody> list = new List<CharacterBody>();
		if (occupants && Mission.Occupants != null)
		{
			foreach (Transform item in Mission.Occupants.Container)
			{
				CharacterBody component = item.GetComponent<CharacterBody>();
				if (!(component == null) && !(component == this))
				{
					float num = Mathf.Abs(component.KinematicPosition - KinematicPosition);
					if ((!notBehind || Mathf.Sign(component.KinematicPosition - KinematicPosition) == Direction) && !(num > radius) && !BlockedByObstacle(component.KinematicPosition))
					{
						list.Add(component);
					}
				}
			}
		}
		if (operators && Mission.Operators != null)
		{
			foreach (CharacterBody body in Mission.Operators.Bodies)
			{
				if (!(body == this))
				{
					float num2 = Mathf.Abs(KinematicPosition - body.KinematicPosition);
					if ((!notBehind || Mathf.Sign(body.KinematicPosition - KinematicPosition) == Direction) && !(num2 > interactionRange) && !BlockedByObstacle(body.KinematicPosition))
					{
						list.Add(body);
					}
				}
			}
		}
		list.Sort(CharacterComparer.RelativePositionDescending(KinematicPosition));
		return list;
	}

	public bool StabilizeBody(CharacterBody target)
	{
		if (target == null || Equipment.gauze == 0 || !target.StabilizeVitalsAvailable)
		{
			return false;
		}
		if (IsOperator && target.Conscious && target != this)
		{
			TeamOperator component = GetComponent<TeamOperator>();
			Vocalize(component.vocalOnGauze.Any());
		}
		target.StabilizeTrauma();
		return true;
	}

	public void RemoveGauzeItem()
	{
		Equipment.gauze--;
	}

	public void SetCensored(bool censored)
	{
		Head.SetCensored(censored);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!(collision.collider.gameObject.tag == "Floor"))
		{
			CollisionTimestamp = Time.time;
			Blackboard.LastCollider = collision.gameObject;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (!(collision.collider.gameObject == null) && !(Blackboard.LastCollider == null) && collision.collider.gameObject == Blackboard.LastCollider)
		{
			Blackboard.LastCollider = null;
		}
	}

	private void UpdateWeapon(WeaponBase weapon)
	{
		if (!(weapon == null))
		{
			weaponMode = (((!weapon.IsBlocked || weapon.type == WeaponBrace.Freehand) && !IsLoweringWeapon) ? WeaponMode.OnPoint : WeaponMode.Lowered);
			if (weapon.IsBlocked)
			{
				AimPoint = new Vector3(transform.position.x + Direction * weapon.CollisionClearance, 0f, transform.position.z);
			}
			else if (weaponMode == WeaponMode.Lowered)
			{
				AimPoint = new Vector3(transform.position.x + Direction * weapon.armConfiguration.retractDistance, 0f, transform.position.z);
			}
			UpdateWeaponTransform(weapon);
		}
	}

	private void UpdateWeaponTransform(WeaponBase weapon)
	{
		Transform muzzle = weapon.Muzzle;
		float num = RightFoot.transform.position.y - LeftFoot.transform.position.y;
		Vector3 aimPoint = AimPoint;
		Vector3 position = RightUpperArm.transform.position;
		Vector3 vector = muzzle.position - position;
		float num2 = 0f;
		if (weapon.type == WeaponBrace.Freehand)
		{
			float min = 0.3f;
			Vector3 vector2 = aimPoint - (aimPoint - position).normalized * 0.35f;
			Vector3 vector3 = vector2 - position;
			float num3 = ((!IsManipulatingEquipment) ? 1f : 0.45f);
			float num4 = Mathf.Clamp(vector3.magnitude, min, weapon.armConfiguration.holdOffDistance * num3);
			float num5 = ((!(RightUpperArm.transform.eulerAngles.z > 180f)) ? RightUpperArm.transform.eulerAngles.z : (RightUpperArm.transform.eulerAngles.z - 360f));
			float num6 = Mathf.Clamp01(1f - Mathf.Abs(num5 - RightLowerArm.transform.eulerAngles.z) / 120f);
			Vector3 position2 = Eye.transform.position;
			Vector3 vector4 = position2 - position;
			Vector3 vector5 = weapon.transform.TransformVector(weapon.rearSightPosition) * num6;
			Vector3 vector6 = position + vector4 * num6;
			Vector3 normalized = (aimPoint - vector6).normalized;
			Vector3 vector7 = weapon.transform.TransformVector(new Vector3((0f - weapon.RecoilKick) * 0.0015f, weapon.RecoilClimb * 0.0035f, 0f));
			if (!FreeAimActive)
			{
				pistolIKTarget.position = vector6 + normalized * num4;
			}
			float num7 = ((!IsOperator) ? Mathf.Min(Physiology.ShockNormalized * 10f, 0.3f) : 0f);
			Vector3 vector8 = weapon.transform.right * num7;
			pistolIKTarget.position += vector7;
			Vector3 eulerAngles = new Vector3(0f, RotationY, weapon.AngleWithClimb + weapon.armConfiguration.rightHandAngle + 90f);
			if (rightArm.OnWeapon)
			{
				RightHand.transform.eulerAngles = eulerAngles;
			}
			if (leftArm.OnWeapon)
			{
				LeftHand.transform.eulerAngles = eulerAngles;
			}
			Vector3 vector9 = aimPoint - vector6;
			Vector2 vector10 = new Vector2(vector9.x, vector9.y);
			float num8 = ((!IsPlayer) ? (weapon.RecoilClimb * UnityEngine.Random.Range(recoilCompensation.x, recoilCompensation.y)) : 0f);
			num2 = Mathf.Atan2(vector10.y, Mathf.Abs(vector10.x)) * 57.29578f - num8;
			weapon.Angle = Mathf.LerpAngle(weapon.Angle, num2, Time.deltaTime * weapon.AlignmentSpeed * WeaponSkillAlignmentModifier);
			weapon.PrecisionCenterCoord = (aimPoint - muzzle.position).magnitude;
		}
		else
		{
			Vector3 vector11 = muzzle.position - weapon.transform.right * vector.magnitude;
			Vector3 vector12 = aimPoint - vector11;
			Vector2 vector13 = new Vector2(vector12.x, vector12.y);
			float num9 = ((!IsPlayer) ? (weapon.RecoilClimb * UnityEngine.Random.Range(recoilCompensation.x, recoilCompensation.y)) : 0f);
			num2 = Mathf.Atan2(vector13.y, Mathf.Abs(vector13.x)) * 57.29578f - num9;
			weapon.Angle = Mathf.LerpAngle(weapon.Angle, num2, Time.deltaTime * weapon.AlignmentSpeed * WeaponSkillAlignmentModifier);
			weapon.PrecisionCenterCoord = (aimPoint - muzzle.position).magnitude;
		}
		float num10 = weapon.TotalWeight / 10f * Mathf.Cos(weapon.Angle * ((float)Math.PI / 180f)) * 0.25f;
		float num11 = Mathf.Abs(weapon.Angle - num2) / 45f;
		HoldWeaponWorkIntensity = num10 * Mathf.Cos(weapon.Angle * ((float)Math.PI / 180f)) + num11 * num10;
	}

	private void UpdateUpperBodyTransforms(WeaponBase weapon)
	{
		float num = 0f;
		if (IsMoving)
		{
			num = ((!IsSprinting) ? 2f : 4f);
		}
		float num2 = Physiology.Respiration * Physiology.RespirationRateFactor * 20f;
		torsoHitAngleAddition = Mathf.Lerp(torsoHitAngleAddition, hitRecoveryCurve.Evaluate(Time.time - LastHitTimestamp) * Mathf.Lerp(0f, 10f, LastHitEnergy / 1500f) * LastDirectionAttackedFrom, Time.deltaTime * 25f);
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
			WeaponArmConfiguration armConfiguration = weapon.armConfiguration;
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

	private void UpdateInteractionMarkers()
	{
		if (stabilizeMarker != null && Physiology.Exsanguinated)
		{
			UnityEngine.Object.Destroy(stabilizeMarker.gameObject);
		}
	}

	public void AddVisionLoad(float load)
	{
		VisionOverload = Mathf.Min(VisionOverload + load, 1f);
	}

	public void SetStunResponseState(bool enable)
	{
		if (!IsRestrained)
		{
			if (enable)
			{
				Vector3 worldSpaceOffset = Eye.TransformVector(new Vector3(0.06f, 0.07f, 0f));
				leftArm.TargetTransition(Eye, worldSpaceOffset, 1f, (!(UnityEngine.Random.value > 0.5f)) ? Hand.Mode.Fist : Hand.Mode.ForwardAssist);
				Blackboard.Target = null;
				StunResponseStateActive = true;
			}
			else
			{
				ReturnLeftHandToWeapon();
				StunResponseStateActive = false;
			}
		}
	}

	public void ReturnLeftHandToWeapon()
	{
		WeaponBase activeWeapon = ActiveWeapon;
		if (activeWeapon != null)
		{
			leftArm.SetOnWeapon(activeWeapon, 1f, rightArm: false);
		}
	}

	private void InvokeBulletTimeOnHit()
	{
		timeScaler.BulletTime((!IsPlayer) ? settings.Time.computerHit : settings.Time.playerHit);
	}

	public void InstantiatePlasticuffOnLeftHand()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Global.Context.EquipmentList.plasticuffs);
		gameObject.transform.SetParent(LeftLowerArm.transform);
		gameObject.transform.localPosition = new Vector3(-0.06f, -0.34f, 0f);
		gameObject.transform.localScale = new Vector3(0.7f, 1f, 0.2f);
		gameObject.transform.localEulerAngles = Vector3.zero;
	}

	public void Restrain()
	{
		if (!IsRestrained)
		{
			IsRestrained = true;
			HingeJoint component = LeftLowerArm.GetComponent<HingeJoint>();
			JointLimits limits = component.limits;
			limits.max = 90f;
			component.limits = limits;
			component = RightLowerArm.GetComponent<HingeJoint>();
			component.limits = limits;
			SpringJoint springJoint = RightHand.gameObject.AddComponent<SpringJoint>();
			springJoint.autoConfigureConnectedAnchor = false;
			springJoint.connectedBody = Torso.Rigidbody;
			SpringJoint springJoint2 = springJoint;
			float minDistance = (springJoint.maxDistance = 0f);
			springJoint2.minDistance = minDistance;
			springJoint.tolerance = 0.01f;
			springJoint.anchor = new Vector3(0f, 0f, -0.09f);
			springJoint.connectedAnchor = new Vector3(-0.1f, -0.025f, -0.1f);
			springJoint.spring = 1500f;
			springJoint = LeftHand.gameObject.AddComponent<SpringJoint>();
			springJoint.autoConfigureConnectedAnchor = false;
			springJoint.connectedBody = Torso.Rigidbody;
			SpringJoint springJoint3 = springJoint;
			minDistance = (springJoint.maxDistance = 0f);
			springJoint3.minDistance = minDistance;
			springJoint.tolerance = 0.01f;
			springJoint.anchor = new Vector3(0f, 0f, 0.09f);
			springJoint.connectedAnchor = new Vector3(-0.1f, -0.025f, 0.1f);
			springJoint.spring = 1500f;
			if (restrainMarker != null)
			{
				UnityEngine.Object.Destroy(restrainMarker.gameObject);
			}
		}
	}

	public void RestrainArm(Transform targetTransform, bool isLeftArm)
	{
		if (isLeftArm)
		{
			leftArm.TargetTransition(targetTransform, 0.5f);
		}
		else
		{
			rightArm.TargetTransition(targetTransform, 0.5f);
		}
	}

	public void StabilizeTrauma()
	{
		Physiology.StopHemorrhage();
		if (stabilizeMarker != null)
		{
			UnityEngine.Object.Destroy(stabilizeMarker.gameObject);
		}
	}

	public void ShowRestrainMarker(bool show)
	{
		if (show)
		{
			if (!(restrainMarker != null) && RestrainAvailable)
			{
				restrainMarker = UnityEngine.Object.Instantiate(Mission.restrainMarker);
				restrainMarker.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
				restrainMarker.entity = RightUpperArm.transform;
			}
		}
		else if (restrainMarker != null)
		{
			UnityEngine.Object.Destroy(restrainMarker.gameObject);
		}
	}

	public void ShowStabilizeMarker(bool show)
	{
		if (show)
		{
			if (!(stabilizeMarker != null) && StabilizeVitalsAvailable)
			{
				stabilizeMarker = UnityEngine.Object.Instantiate(Mission.stabilizeMarker);
				stabilizeMarker.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
				stabilizeMarker.entity = Abdomen.transform;
			}
		}
		else if (stabilizeMarker != null)
		{
			UnityEngine.Object.Destroy(stabilizeMarker.gameObject);
		}
	}

	public void ShowPhysiology(bool show)
	{
		if (!IsPlayer)
		{
			CharacterPhysiologyVisualizer component = GetComponent<CharacterPhysiologyVisualizer>();
			if (component != null)
			{
				component.Show(show);
			}
		}
	}

	public bool TogglePhysiologyVisualizer()
	{
		CharacterPhysiologyVisualizer component = GetComponent<CharacterPhysiologyVisualizer>();
		if (component != null)
		{
			component.Show(!component.enabled);
		}
		return component.enabled;
	}

	public bool MoveMagazineToHand(bool rightHand = true)
	{
		WeaponBase activeWeapon = ActiveWeapon;
		if (activeWeapon == null)
		{
			return false;
		}
		WeaponComponentMagazine componentMagazine = activeWeapon.ComponentMagazine;
		if (componentMagazine == null)
		{
			return false;
		}
		UnityEngine.Object.Destroy(componentMagazine.GetComponent<Rigidbody>());
		Transform transform = ((!rightHand) ? leftArm.hand.grabPoint : rightArm.hand.grabPoint);
		componentMagazine.transform.eulerAngles = new Vector3(0f, RotationY, transform.eulerAngles.z - 90f);
		Vector3 vector = componentMagazine.ikTarget.position - componentMagazine.transform.position;
		componentMagazine.transform.position = transform.position - vector;
		componentMagazine.transform.SetParent(transform, worldPositionStays: true);
		componentMagazine.gameObject.SetActive(value: true);
		return true;
	}

	public float WeaponAimAlignment()
	{
		WeaponBase activeWeapon = ActiveWeapon;
		if (activeWeapon == null)
		{
			return 0f;
		}
		Vector3 position = activeWeapon.Muzzle.position;
		Vector3 right = activeWeapon.transform.right;
		Vector3 vector = WeaponAlignPoint - position;
		return Vector3.Angle(vector, right);
	}

	public float KinematicDistanceToPosition(float position)
	{
		return Mathf.Abs(position - KinematicPosition);
	}

	private void SetVisibleRange()
	{
		Ray ray = new Ray(Eye.transform.position, transform.right);
		float num = VisionRange;
		bool flag = Physics.Raycast(ray, out raycastHit, num, Masks.Instance.BallisticTargetsSansLimbs);
		VisibleRange = ((!flag) ? num : Mathf.Abs(raycastHit.point.x - ray.origin.x));
	}

	public void UpdateVisibleBodies()
	{
		HashSet<CharacterBody> friends = Blackboard.Friends;
		HashSet<Target> enemies = Blackboard.Enemies;
		friends.Clear();
		enemies.Clear();
		float num = 0.5f;
		Ray ray = new Ray(new Vector3(Eye.transform.position.x - num * Direction, 0.5f, transform.position.z), transform.right);
		RaycastHit[] array = Physics.RaycastAll(ray, VisibleRange + num, Masks.Instance.Limbs);
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (raycastHit.collider == null)
			{
				continue;
			}
			Limb component = raycastHit.collider.GetComponent<Limb>();
			if (component == null)
			{
				enemies.Add(raycastHit.collider.GetComponent<Target>());
				continue;
			}
			CharacterBody body = component.Body;
			if (!body.Invisible && !(body == this))
			{
				if (body.team == team)
				{
					friends.Add(body);
				}
				else
				{
					enemies.Add(body.Target);
				}
			}
		}
	}

	public float WorldSpaceFurthestVision()
	{
		return Eye.transform.position.x + Direction * VisibleRange;
	}

	public bool HasActiveTarget()
	{
		Target target = Blackboard.Target;
		return target != null && target.active;
	}

	public Grenade InstantiateGrenadeInHand(Grenade.Type type)
	{
		if (ReadyGrenade != null)
		{
			return ReadyGrenade;
		}
		bool flag = type == Grenade.Type.Frag;
		int num = ((!flag) ? Equipment.stunGrenades : Equipment.fragGrenades);
		Transform transform = ((!flag) ? stunGrenadeAnchor : fragGrenadeAnchor);
		switch (num)
		{
		case 0:
			return null;
		case 1:
			foreach (Transform item in transform)
			{
				if (item.GetComponent<Grenade>() != null)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
			break;
		}
		switch (type)
		{
		case Grenade.Type.Stun:
			Equipment.stunGrenades--;
			break;
		case Grenade.Type.Frag:
			Equipment.fragGrenades--;
			break;
		}
		HeadMode = HeadLookMode.AimPoint;
		Grenade grenade = ((!flag) ? Global.Context.EquipmentList.stunGrenade : Global.Context.EquipmentList.fragGrenade);
		ReadyGrenade = UnityEngine.Object.Instantiate(grenade);
		ReadyGrenade.transform.SetParent(leftArm.hand.grabPoint);
		ReadyGrenade.transform.localPosition = new Vector3(0.03f, 0f, 0f);
		ReadyGrenade.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
		ReadyGrenade.name = grenade.name;
		if (IsPlayer)
		{
			Mission.UI.indicatorLowerLeft.Indicate(ReadyGrenade.name, 2f, audio: false);
		}
		return ReadyGrenade;
	}

	private void InstantiateGrenadeOnEquipment(Grenade.Type type)
	{
		Grenade grenade = UnityEngine.Object.Instantiate((type != Grenade.Type.Frag) ? Global.Context.EquipmentList.stunGrenade : Global.Context.EquipmentList.fragGrenade);
		UnityEngine.Object.Destroy(grenade.GetComponent<Rigidbody>());
		grenade.transform.SetParent((type != Grenade.Type.Frag) ? stunGrenadeAnchor : fragGrenadeAnchor);
		grenade.transform.localPosition = Vector3.zero;
	}

	public Transform ApplyGauzePosition(out Vector3 offset)
	{
		Vector3 zero = Vector3.zero;
		Transform transform = null;
		if (Head.IsHemorrhaging)
		{
			zero = Head.LimbPhysiology.wounds[0].LocalPosition;
			transform = Head.transform;
		}
		else if (Torso.IsHemorrhaging)
		{
			zero = Torso.LimbPhysiology.wounds[0].LocalPosition;
			transform = Torso.transform;
		}
		else if (Abdomen.IsHemorrhaging)
		{
			zero = Abdomen.LimbPhysiology.wounds[0].LocalPosition;
			transform = Abdomen.transform;
		}
		else if (Pelvis.IsHemorrhaging)
		{
			zero = Pelvis.LimbPhysiology.wounds[0].LocalPosition;
			transform = Pelvis.transform;
		}
		else if (RightUpperLeg.IsHemorrhaging)
		{
			zero = RightUpperLeg.LimbPhysiology.wounds[0].LocalPosition;
			transform = RightUpperLeg.transform;
		}
		else if (LeftUpperLeg.IsHemorrhaging)
		{
			zero = LeftUpperLeg.LimbPhysiology.wounds[0].LocalPosition;
			transform = LeftUpperLeg.transform;
		}
		else if (RightLowerLeg.IsHemorrhaging)
		{
			zero = RightLowerLeg.LimbPhysiology.wounds[0].LocalPosition;
			transform = RightLowerLeg.transform;
		}
		else if (LeftLowerLeg.IsHemorrhaging)
		{
			zero = LeftLowerLeg.LimbPhysiology.wounds[0].LocalPosition;
			transform = LeftLowerLeg.transform;
		}
		else
		{
			zero = new Vector3(0.15f, 0.15f, 0f);
			transform = Torso.transform;
		}
		offset = transform.TransformVector(zero);
		return transform;
	}

	public bool CanSeePosition(float position)
	{
		float f = position - Eye.position.x;
		float num = Mathf.Sign(f);
		if (num != Direction || Mathf.Abs(f) > VisibleRange)
		{
			return false;
		}
		return true;
	}

	public bool BlockedByObstacle(float position)
	{
		return VisibleRange < Mathf.Abs(position - Eye.position.x);
	}

	public bool BlockedByDoor(float position)
	{
		if (Room == null)
		{
			return true;
		}
		float f = position - Position;
		float direction = Mathf.Sign(f);
		DoorController doorController = Room.NextDoor(direction);
		if (doorController != null && Mathf.Abs(doorController.Position - Position) < Mathf.Abs(f) && doorController.Angle < 80f)
		{
			return true;
		}
		return false;
	}

	public bool FacingDirection(float direction)
	{
		return Direction == direction;
	}

	public bool Facing(float position)
	{
		return Mathf.Sign(position - Position) == Direction;
	}

	public bool Facing(CharacterBody body)
	{
		if (body == null)
		{
			return false;
		}
		float num = Mathf.Sign(body.Position - Position);
		return Direction == num;
	}

	public bool BodyFacingMe(CharacterBody body)
	{
		if (body == null)
		{
			return false;
		}
		return body.Facing(this);
	}

	public float ThreatValue(CharacterBody evaluator, float reactionTime = 0.33f, bool storeAttackData = false)
	{
		if (ActiveWeapon != null || Time.time - WeaponDropTime < reactionTime)
		{
			return (!evaluator.Facing(this)) ? 0.5f : 1f;
		}
		if (!Conscious && Time.time - Physiology.UnconsciousTimestamp >= reactionTime)
		{
			return 0f;
		}
		float num = Time.time - ComplianceStateTimestamp;
		if ((IsCompliant && num >= 0.5f) || (!IsCompliant && num < 3f))
		{
			return 0.1f;
		}
		return 0.3f;
	}

	public void ClearPendingAction()
	{
		PendingAction = CharacterAction.None;
	}

	public void Vocalize(string text)
	{
		Communication.VocalizeText(text);
	}
}
