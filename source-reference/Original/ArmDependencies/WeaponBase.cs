using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Utilities;

public class WeaponBase : MonoBehaviour
{
	public enum ChamberState
	{
		Live = 0,
		Spent = 1,
		Empty = 2
	}

	public enum BoltState
	{
		Closed = 0,
		Opening = 1,
		OpeningManual = 2,
		Open = 3,
		Closing = 4,
		ClosingManual = 5
	}

	public enum FireMode
	{
		Safe = 0,
		Semi = 1,
		Burst = 2,
		Auto = 3
	}

	public enum BoltReleaseAnimation
	{
		NoAnimation = 0,
		ControlPosition = 1,
		BoltHandle = 2
	}

	public static string WeaponsPath = "Weapons/";

	private static string ComponentsPath = "Weapons/Components/";

	private const float ShoulderedMinAlignmentSpeed = 4f;

	private const float ShoulderedMaxAlignmentSpeed = 12f;

	private const float FreehandMinAlignmentSpeed = 7.5f;

	private const float FreehandMaxAlignmentSpeed = 12f;

	private const float ShoulderedMinWeight = 2.5f;

	private const float ShoulderedMaxWeight = 10f;

	private const float FreehandMinWeight = 0.6f;

	private const float FreehandMaxWeight = 2f;

	public Availability availability;

	[Header("Main characteristics")]
	public string designation;

	public WeaponType weaponType;

	public WeaponBrace type;

	public Calibre calibre;

	[Tooltip("Sets the movement speed of the weapon's mechanism.")]
	public float roundsPerMinute = 600f;

	public bool rifledBarrel = true;

	[Tooltip("Diameter of the inside of the barrel. In millimeters.")]
	public float boreDiameter = 9f;

	[Tooltip("Determines the spin speed of fired projectiles. In inches, how long distance of the barrel it takes a projectile to make one revolution.")]
	public float rifleTwistRate = 9f;

	[Tooltip("The ammunition that will be selected for this weapon by default in UI.")]
	public AmmunitionBase defaultAmmunition;

	[Tooltip("The ammunition that has been selected for this weapon. (Assigned from UI.)")]
	public AmmunitionBase ammunition;

	public List<WeaponComponentSlot> componentSlots;

	[Tooltip("Weight in kilograms.")]
	public float weight;

	[Tooltip("This factor scales the muzzle velocity of the ammunition being fired. It is meant to model the effect of different barrel lengths. In normalized percent")]
	[Range(0.5f, 1.5f)]
	public float velocityModifier = 1f;

	[Tooltip("This factor scales the weapon's total weight in the alignment calculation: less weight gives quicker alignment. In normalized percent.")]
	[Range(0.5f, 2f)]
	public float alignmentModifier = 1f;

	[Tooltip("The time it takes to replace a magazine or insert a single round. In seconds.")]
	public float reloadTime = 1f;

	[Tooltip("When set the action must be cycled manually.")]
	public bool manualCycling;

	[Range(0f, 1f)]
	public float boltFeedMalfunctionRate;

	[Header("Emissions")]
	public GameObject muzzleFlash;

	public float muzzleFlashScale = 1f;

	public bool emitSmoke = true;

	public bool emitFlash = true;

	[Header("Recoil")]
	[Tooltip("Describes the bolt's offset from its closed position through one travel cycle. Y is offset and X is completion of the cycle.")]
	public AnimationCurve boltCycle;

	[Tooltip("Describes the weapon's kick from firing one round. This is transfered to the firing character's torso angle. X is time and Y is recoil. In normalized form.")]
	public AnimationCurve recoilCycle;

	[Tooltip("Describes the weapon's muzzle climb from firing one round. X is time and Y climb. In normalized form.")]
	public AnimationCurve recoilClimbCycle;

	[Tooltip("Scales the recoil kick.")]
	public float recoilKickModifier = 1f;

	[Tooltip("Scales the recoil climb.")]
	public float recoilClimbModifier = 1f;

	[Header("Pistol recoil")]
	[Tooltip("This scales how much of the recoil climb is transfered to the character's lower arms.")]
	public float lowerArmsRecoilFactor = 0.2f;

	[Tooltip("This scales how much of the recoil climb is transfered to the character's upper arms.")]
	public float upperArmsRecoilFactor = 0.1f;

	[Header("Accuracy")]
	[Tooltip("Upper bound for how much projectiles may divert from the weapon's bore. In degrees. Note that this is added to the ammunition's inaccuracy that works the same way.")]
	public float inaccuracy = 1f;

	[Tooltip("How distance translates to lengthwise precision. X: Distance, Y: Precision distance (whole, not half). Both in meters.")]
	public AnimationCurve rangePenalty;

	[Tooltip("Penalty to lengthwise precision while moving. This value is multiplied by the character's velocity to form the penalty in meters.")]
	public float movementPenalty = 0.33f;

	[Tooltip("Penalty to lengthwise accuracy from recoil kick. Is applied to precision distance as a scaler, calculated as:\n(1 + recoil kick) * Kick penalty.\n(Recoil kick is in degrees.)")]
	public float kickPenalty = 1f;

	[Header("Fire modes")]
	public List<FireMode> fireModes;

	public int burstCount = 3;

	[Header("Mechanics")]
	public WeaponHammer hammer;

	public CyclingBolt cyclingBolt;

	public WeaponBarrel barrel;

	[Tooltip("When this is set rounds are not moved away from the magazine before being fired.")]
	public bool fireFromMagazine;

	[Tooltip("Enable this for weapons that load loose ammunition, i.e. shotguns and revolvers.")]
	public bool useInternalMagazine;

	[Header("Malfunctions")]
	public float feedMalfunction;

	private float recoilPeriod;

	[Header("Detonation audio")]
	public AudioSource detonationSource;

	[Tooltip("This sample is played when the weapon is fired without suppressor or with supersonic ammunition.")]
	public PropagatingAudioSample audioFire = new PropagatingAudioSample(0f, AudioStimuliType.Firearm);

	[Tooltip("This sample is played when the weapon is firing supersonic ammunition and has a suppressor attached.")]
	public PropagatingAudioSample audioSuppressedSupersonic = new PropagatingAudioSample(0f, AudioStimuliType.Firearm);

	[Tooltip("This sample is played when the weapon is firing subsonic ammunition and has a suppressor attached.")]
	public PropagatingAudioSample audioSuppressedSubsonic = new PropagatingAudioSample(0f, AudioStimuliType.Firearm);

	[Tooltip("This sample is played on top of the other samples when the weapon is firing supersonic ammunition.")]
	public PropagatingAudioSample audioSupersonic = new PropagatingAudioSample(2500f, AudioStimuliType.Firearm);

	[Header("Mechanics audio")]
	public AudioSource mechanicsSource;

	public PropagatingAudioSample audioBoltExtract = new PropagatingAudioSample(0.1f, AudioStimuliType.Firearm);

	public PropagatingAudioSample audioBoltReturn = new PropagatingAudioSample(0.1f, AudioStimuliType.Firearm);

	public PropagatingAudioSample audioFiringPin = new PropagatingAudioSample(0.1f, AudioStimuliType.Firearm);

	public PropagatingAudioSample audioFireSelect = new PropagatingAudioSample(0.1f, AudioStimuliType.Firearm);

	public PropagatingAudioSample audioExtractMagazine = new PropagatingAudioSample(0.1f, AudioStimuliType.Firearm);

	public PropagatingAudioSample audioInsertMagazine = new PropagatingAudioSample(0.1f, AudioStimuliType.Firearm);

	public PropagatingAudioSample audioInsertRound = new PropagatingAudioSample(0.1f, AudioStimuliType.Firearm);

	[Tooltip("This value scales the audio intensity of the ammunition being fired.")]
	public float audioPowerMultiplier = 1f;

	[Tooltip("The mixer group to use when firing unsuppressed.")]
	public AudioMixerGroup unsuppressedMixerGroup;

	[Tooltip("The mixer group to use when firing suppressed.")]
	public AudioMixerGroup suppressedMixerGroup;

	[Header("Animation")]
	public WeaponArmConfiguration armConfiguration;

	[Tooltip("Offset from the magazine slot transform used for inserting magazines from.")]
	public Vector3 magazineInsertTrack;

	[Tooltip("This vector is used to align the weapon on its targets. It is an offset from the weapon's origo.")]
	public Vector3 rearSightPosition;

	[Tooltip("Time in seconds it takes to move the hand to the charging handle for manipulation.")]
	public float boltManipulationTransitionTime = 0.2f;

	[Tooltip("Which arm animation to use for activating the bolt release control.")]
	public BoltReleaseAnimation boltReleaseAnimation;

	[Tooltip("When true characters will use their left hand to open the bolt. Otherwise the right.")]
	public bool leftHandBoltManipulation;

	[Tooltip("When true characters will use their left hand to close the bolt. Otherwise the right.")]
	public bool leftHandBoltClose;

	[Tooltip("The position to activate the weapon's bolt release at, when different from the bolt handle.")]
	public Vector3 boltReleasePosition;

	[Header("Transforms")]
	[Tooltip("The position on the weapon that the character will strive to place its right hand.")]
	public Transform rightHandAnchor;

	[Tooltip("The position on the weapon that the character will strive to place its left hand.")]
	public Transform leftHandAnchor;

	public Transform muzzleTransform;

	public Transform magazineTransform;

	[Tooltip("Position where hand will be placed to manipulate the weapon's bolt.")]
	public Transform boltHandleTransform;

	public Transform slotsTransform;

	[Tooltip("The parent transform of the weapon's colliders. Used to compute the weapon's bounds for rendering UI icons.")]
	public Transform colliderTransform;

	[Header("Description")]
	[TextArea(5, 10)]
	public string description;

	public string origin;

	public ProjectileEvent onSpawnProjectile = new ProjectileEvent();

	private Settings settings;

	private float projectileForceMultiplier = 1f;

	private float recoilSuppressorModifier = 1f;

	private List<Recoil> recoilList = new List<Recoil>();

	private ParticleSystem muzzleSmokeEmitter;

	private Transform projectilesContainer;

	private RaycastHit raycastHit;

	private TimeScaler timeScaler;

	private Mission mission;

	private DynamicInteractionMarker interactionMarker;

	private FixedJoint handJoint;

	private int fireModeIndex;

	private int burstRoundsRemaining;

	private int consecutiveRounds;

	private float kickValue;

	private float climbValue;

	private float staticWeight;

	private float direction = 1f;

	private float recoilToWeightFactor = 200f;

	private bool initialized;

	private bool hammerReady = true;

	private int affectedByMuzzleBlastsMask;

	private float muzzleBlastRange;

	private float muzzleBlastRangeSquared;

	public float BoltCyclePeriod
	{
		get
		{
			return 60f / roundsPerMinute;
		}
	}

	public float BoltOpeningPeriod
	{
		get
		{
			return BoltCyclePeriod * boltCycle.keys[1].time;
		}
	}

	public float BoltClosingPeriod
	{
		get
		{
			return BoltCyclePeriod * (1f - boltCycle.keys[boltCycle.keys.Length - 2].time);
		}
	}

	public Transform CasingsContainer { get; private set; }

	public float TotalWeight
	{
		get
		{
			return staticWeight + MagazineWeight();
		}
	}

	public float AlignmentSpeed
	{
		get
		{
			if (type == WeaponBrace.Shoulder)
			{
				return Mathf.Lerp(12f, 4f, Mathf.Clamp(TotalWeight * alignmentModifier, 2.5f, 10f) / 10f);
			}
			return Mathf.Lerp(12f, 7.5f, Mathf.Clamp(TotalWeight * alignmentModifier, 0.6f, 2f) / 2f);
		}
	}

	public float AlignmentSpeedNormalized
	{
		get
		{
			return AlignmentSpeed / 12f;
		}
	}

	public CharacterBody Owner { get; private set; }

	public Transform Muzzle { get; private set; }

	public Vector3 Pivot
	{
		get
		{
			return (!(base.transform.parent == null)) ? base.transform.parent.position : Vector3.zero;
		}
	}

	public Vector3 PivotToMuzzle
	{
		get
		{
			return Muzzle.position - Pivot;
		}
	}

	public CyclingBolt.State BoltStatus
	{
		get
		{
			return cyclingBolt.Status;
		}
	}

	public Magazine Magazine { get; private set; }

	public FireMode ActiveFireMode
	{
		get
		{
			return fireModes[fireModeIndex];
		}
	}

	public ChamberState ChamberStatus { get; set; }

	public float Position
	{
		get
		{
			return base.transform.position.x;
		}
	}

	public float Angle { get; set; }

	public float RecoilKick
	{
		get
		{
			return kickValue;
		}
	}

	public float RecoilClimb
	{
		get
		{
			return climbValue;
		}
	}

	public float AngleWithClimb
	{
		get
		{
			return Angle + RecoilClimb;
		}
	}

	public bool IsBlocked { get; private set; }

	public bool IsFiring
	{
		get
		{
			return Owner != null && Time.time - Owner.LastFiredTimestamp < 0.5f;
		}
	}

	public bool IsPrimary { get; set; }

	public bool HammerReady
	{
		get
		{
			return (!(hammer != null)) ? hammerReady : hammer.Ready;
		}
		set
		{
			if (hammer != null)
			{
				if (value)
				{
					hammer.SetReady();
				}
			}
			else
			{
				hammerReady = value;
			}
		}
	}

	public Rigidbody RigidBody { get; private set; }

	public bool IsHeld { get; private set; }

	public bool IsHolstered { get; private set; }

	public bool IsRagdoll
	{
		get
		{
			return !RigidBody.isKinematic;
		}
	}

	public bool PendingPickup { get; set; }

	public bool ChamberPermanentlyEmpty
	{
		get
		{
			return cyclingBolt.StaticAndNotLive;
		}
	}

	public bool IsCycling
	{
		get
		{
			return cyclingBolt.IsMoving;
		}
	}

	public bool ReadyToFire
	{
		get
		{
			return cyclingBolt.Live;
		}
	}

	public float MalfunctionsMultiplier { get; set; }

	public int ReadynessValue
	{
		get
		{
			int num = ((!ChamberPermanentlyEmpty) ? 10 : 0);
			int num2 = ((Magazine != null) ? ((int)(Magazine.Ratio * 5f)) : 0);
			return num + num2;
		}
	}

	public Vector3 BorePoint
	{
		get
		{
			return Muzzle.position + Muzzle.forward * PrecisionCenterCoord;
		}
	}

	public float PrecisionCenterCoord { get; set; }

	public float PrecisionDistance
	{
		get
		{
			return rangePenalty.Evaluate(PrecisionCenterCoord) * MovementFactor() * (1f + kickValue * kickPenalty);
		}
	}

	public float PrecisionHalfDistance
	{
		get
		{
			return (!(Owner != null)) ? (PrecisionDistance * 0.5f) : (PrecisionDistance * 0.5f * (1f + Owner.Physiology.Respiration * Owner.Physiology.RespirationRateFactor));
		}
	}

	public float PrecisionBegin
	{
		get
		{
			return Mathf.Max(PrecisionCenterCoord - PrecisionHalfDistance, 0f);
		}
	}

	public float PrecisionEnd
	{
		get
		{
			return PrecisionCenterCoord + PrecisionHalfDistance;
		}
	}

	public float CombinedInaccuracy
	{
		get
		{
			return inaccuracy + ((!(ammunition != null)) ? 0f : ammunition.inaccuracy);
		}
	}

	public float CollisionClearance { get; private set; }

	public WeaponComponentMagazine ComponentMagazine { get; private set; }

	public WeaponComponentFlashlight ComponentFlashlight { get; private set; }

	public WeaponComponentLaser ComponentLaser { get; private set; }

	public WeaponComponentSuppressor ComponentSuppressor { get; set; }

	public UnityEvent onNewMagazine { get; set; }

	public static string WeaponTypeToString(WeaponType type)
	{
		switch (type)
		{
		case WeaponType.Pistol:
			return "pistol";
		case WeaponType.SMG:
			return "sub machinegun";
		case WeaponType.Shotgun:
			return "shotgun";
		case WeaponType.Rifle:
			return "rifle";
		default:
			return string.Empty;
		}
	}

	private void InitializeSettings()
	{
		settings = Global.Context.Settings;
		settings.Extras.onChanged.AddListener(ApplyExtraSettings);
		ApplyExtraSettings(settings.Extras);
	}

	private void ApplyExtraSettings(SerializableExtraSettings newSettings)
	{
		bool weaponMalfunctions = newSettings.weaponMalfunctions;
		if (cyclingBolt != null)
		{
			cyclingBolt.UseMalfunctions = weaponMalfunctions;
		}
		if (Magazine != null)
		{
			Magazine.UseMalfunctions = weaponMalfunctions;
		}
		projectileForceMultiplier = newSettings.forceMultiplier * 0.015f;
		MalfunctionsMultiplier = newSettings.malfunctionsMultiplier;
	}

	private void Awake()
	{
		affectedByMuzzleBlastsMask = Masks.Instance.AffectedByMuzzleBlasts;
		RigidBody = GetComponent<Rigidbody>();
		if (hammer != null)
		{
			hammer.onStrike.AddListener(StrikeFiringPin);
		}
	}

	private void Start()
	{
		mission = Global.Context.Mission;
		if (mission != null)
		{
			projectilesContainer = mission.ProjectilesContainer;
			muzzleSmokeEmitter = mission.WeaponSmokeEmitter;
			cyclingBolt.Initialize(this, OnBoltOpened, OnBoltClosed, boltFeedMalfunctionRate);
		}
		InitializeAudio();
		InitializeSettings();
	}

	private void OnDisable()
	{
		RemoveInteractionMarker();
	}

	private void RemoveInteractionMarker()
	{
		if (interactionMarker != null)
		{
			UnityEngine.Object.Destroy(interactionMarker.gameObject);
		}
	}

	public void PopulateRequiredComponentsForUI()
	{
		for (int i = 0; i < componentSlots.Count; i++)
		{
			WeaponComponentSlot weaponComponentSlot = componentSlots[i];
			if (weaponComponentSlot.required && !(weaponComponentSlot.component != null) && !(weaponComponentSlot.defaultComponent == null))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(weaponComponentSlot.defaultComponent.gameObject);
				gameObject.name = weaponComponentSlot.defaultComponent.name;
				gameObject.transform.SetParent(weaponComponentSlot.transform, false);
				AssignComponentFunctions(this, gameObject.GetComponent<WeaponComponent>());
			}
		}
	}

	public void RefreshComponentFunctionsForUI()
	{
		for (int i = 0; i < componentSlots.Count; i++)
		{
			AssignComponentFunctions(this, componentSlots[i].component);
		}
	}

	private void InitializeAudio()
	{
		if (detonationSource != null)
		{
			detonationSource.priority = 20;
			audioFire.Initialize(detonationSource);
			audioSuppressedSupersonic.Initialize(detonationSource);
			audioSuppressedSubsonic.Initialize(detonationSource);
			audioSupersonic.Initialize(detonationSource);
		}
		if (mechanicsSource != null)
		{
			mechanicsSource.priority = 80;
			audioBoltExtract.Initialize(mechanicsSource);
			audioBoltReturn.Initialize(mechanicsSource);
			audioFiringPin.Initialize(mechanicsSource);
			audioFireSelect.Initialize(mechanicsSource);
			audioExtractMagazine.Initialize(mechanicsSource);
			audioInsertMagazine.Initialize(mechanicsSource);
			audioInsertRound.Initialize(mechanicsSource);
		}
	}

	private void InitializeAmmunition()
	{
		if (ammunition == null)
		{
			return;
		}
		if (ammunition.calibre.name != calibre.name)
		{
			Debug.LogWarningFormat("Weapon {0} instantiated with incompatible caliber: {1}", base.name, ammunition.calibre.name);
			ammunition = null;
			return;
		}
		muzzleBlastRange = ammunition.MuzzleBlastRange;
		muzzleBlastRangeSquared = Mathf.Pow(muzzleBlastRange, 2f);
		if (useInternalMagazine)
		{
			Magazine = new Magazine(this);
		}
	}

	public void CalculateStaticWeight()
	{
		float num = weight;
		foreach (WeaponComponentSlot componentSlot in componentSlots)
		{
			if (!(componentSlot.component == null))
			{
				num += componentSlot.component.weight;
			}
		}
		staticWeight = num;
	}

	public void Initialize(CharacterBody owner)
	{
		Owner = owner;
		mission = Global.Context.Mission;
		CasingsContainer = mission.CasingsContainer;
		timeScaler = mission.TimeScaler;
		onNewMagazine = new UnityEvent();
		leftHandAnchor.localPosition = armConfiguration.leftHandOffset;
		rightHandAnchor.localPosition = armConfiguration.rightHandOffset;
		Muzzle = ((!(ComponentSuppressor != null)) ? muzzleTransform : ComponentSuppressor.muzzle);
		detonationSource.pitch = ((!(ComponentSuppressor != null)) ? 1f : ammunition.suppressedAudioPitch);
		recoilSuppressorModifier = ((!(ComponentSuppressor != null)) ? 1f : ComponentSuppressor.RecoilModifier);
		recoilPeriod = Mathf.Max(recoilCycle.keys[recoilCycle.keys.Length - 1].time, recoilClimbCycle.keys[recoilClimbCycle.keys.Length - 1].time);
		InitializeAmmunition();
		foreach (WeaponComponentSlot componentSlot in componentSlots)
		{
			if (!(componentSlot.component == null))
			{
				componentSlot.StartRuntime();
			}
		}
		initialized = true;
	}

	public void InitializeForUI()
	{
		PopulateRequiredComponentsForUI();
		RefreshComponentFunctionsForUI();
		recoilSuppressorModifier = ((!(ComponentSuppressor != null)) ? 1f : ComponentSuppressor.RecoilModifier);
		InitializeAmmunition();
		CalculateStaticWeight();
	}

	public SerializableWeapon Serialize()
	{
		SerializableWeapon serializableWeapon = new SerializableWeapon();
		serializableWeapon.prefabName = base.name;
		serializableWeapon.ammunitionPrefabName = ammunition.name;
		serializableWeapon.componentSlots = new List<SerializableWeaponComponentSlot>();
		SerializableWeapon serializableWeapon2 = serializableWeapon;
		foreach (WeaponComponentSlot componentSlot in componentSlots)
		{
			serializableWeapon2.componentSlots.Add(componentSlot.Serialize());
		}
		return serializableWeapon2;
	}

	public static WeaponBase Deserialize(SerializableWeapon data, bool isPrimary)
	{
		if (data == null)
		{
			return null;
		}
		GameObject gameObject = Resources.Load(WeaponsPath + data.prefabName) as GameObject;
		if (gameObject == null)
		{
			return null;
		}
		if (!gameObject.GetComponent<WeaponBase>().availability.IsAvailable(Version.Instance.Staging))
		{
			return null;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
		gameObject2.name = gameObject.name;
		WeaponBase component = gameObject2.GetComponent<WeaponBase>();
		component.IsPrimary = isPrimary;
		string text = data.ammunitionPrefabName;
		EquipmentList equipmentList = Global.Context.EquipmentList;
		if (data.ammunitionPrefabName == equipmentList.randomAmmunition.name)
		{
			AmmunitionBase ammunitionBase = equipmentList.RandomAmmunition(component.calibre);
			text = ((!(ammunitionBase != null)) ? string.Empty : ammunitionBase.name);
		}
		component.ammunition = AmmunitionBase.GetAmmunition(text, component.defaultAmmunition);
		for (int i = 0; i < component.componentSlots.Count; i++)
		{
			WeaponComponentSlot weaponComponentSlot = component.componentSlots[i];
			if (i >= 0 && i < data.componentSlots.Count && data.componentSlots[i] != null && !string.IsNullOrEmpty(data.componentSlots[i].component))
			{
				SerializableWeaponComponentSlot serializableWeaponComponentSlot = data.componentSlots[i];
				string text2 = ((!string.IsNullOrEmpty(serializableWeaponComponentSlot.component)) ? serializableWeaponComponentSlot.component : serializableWeaponComponentSlot.defaultComponent);
				if (!string.IsNullOrEmpty(text2))
				{
					GameObject componentPrefab = Resources.Load(ComponentsPath + text2) as GameObject;
					InstantiateComponent(component, componentPrefab, weaponComponentSlot);
				}
			}
			else if (weaponComponentSlot.required && !(weaponComponentSlot.defaultComponent == null) && weaponComponentSlot.defaultComponent.Compatible(weaponComponentSlot))
			{
				InstantiateComponent(component, weaponComponentSlot.defaultComponent.gameObject, weaponComponentSlot);
				weaponComponentSlot.component = weaponComponentSlot.defaultComponent;
			}
		}
		component.CalculateStaticWeight();
		return component;
	}

	public static void InstantiateComponent(WeaponBase weapon, GameObject componentPrefab, WeaponComponentSlot slot)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(componentPrefab.gameObject);
		gameObject.name = componentPrefab.name;
		gameObject.transform.SetParent(slot.transform, false);
		WeaponComponent component = gameObject.GetComponent<WeaponComponent>();
		component.SetOrientation(slot.orientation);
		AssignComponentFunctions(weapon, component);
		slot.component = component;
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

	public void PickUpInitialize(CharacterBody newOwner, bool isPrimary)
	{
		Owner = newOwner;
		RemoveInteractionMarker();
		IsPrimary = isPrimary;
		SetKinematic();
	}

	public void Activate(CharacterBody owner)
	{
		Owner = owner;
		SuspendComponents(false);
		IsHeld = true;
		IsHolstered = false;
		ResetBurstCount();
	}

	public void Holster()
	{
		SuspendComponents(true);
		IsHolstered = true;
		IsHeld = false;
		ResetBurstCount();
	}

	public void SuspendComponents(bool suspend)
	{
		if (ComponentLaser != null)
		{
			ComponentLaser.Suspend(suspend);
		}
		if (ComponentFlashlight != null)
		{
			ComponentFlashlight.Suspend(suspend);
		}
	}

	private void Update()
	{
		if (initialized && !(Owner == null) && !IsHolstered)
		{
			UpdateBlockedStatus();
			UpdateTriggerMechanism();
			CalculateRecoil();
			if (IsHeld && !IsRagdoll)
			{
				UpdateTransform();
			}
			if (IsRagdoll && handJoint == null)
			{
				IsHeld = false;
			}
		}
	}

	public static void AssignComponentFunctions(WeaponBase weapon, WeaponComponent component)
	{
		if (component == null)
		{
			return;
		}
		component.Weapon = weapon;
		foreach (WeaponComponent.Function function in component.functions)
		{
			switch (function)
			{
			case WeaponComponent.Function.Laser:
				weapon.ComponentLaser = component.GetComponent<WeaponComponentLaser>();
				break;
			case WeaponComponent.Function.Light:
				weapon.ComponentFlashlight = component.GetComponent<WeaponComponentFlashlight>();
				break;
			case WeaponComponent.Function.Magazine:
				weapon.ComponentMagazine = component.GetComponent<WeaponComponentMagazine>();
				break;
			case WeaponComponent.Function.Suppressor:
				weapon.ComponentSuppressor = component.GetComponent<WeaponComponentSuppressor>();
				break;
			}
		}
	}

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

	private void UpdateBlockedStatus()
	{
		if (!(Owner == null))
		{
			Vector3 position = Owner.RightUpperArm.transform.position;
			Vector3 right = Owner.transform.right;
			float magnitude = (Muzzle.position - position).magnitude;
			IsBlocked = Physics.Raycast(position, right, out raycastHit, magnitude, Masks.Instance.BallisticTargetsSansLimbs);
			CollisionClearance = ((!IsBlocked) ? 100f : (raycastHit.point - position).x);
		}
	}

	private void UpdateTransform()
	{
		float y = ((!(Owner != null)) ? base.transform.eulerAngles.y : Owner.transform.eulerAngles.y);
		base.transform.eulerAngles = new Vector3(0f, y, AngleWithClimb);
	}

	public bool InsertRound()
	{
		if (fireFromMagazine && (cyclingBolt.Round == null || cyclingBolt.Round.Spent))
		{
			cyclingBolt.FeedRound(false, cyclingBolt.manualLockOpen);
		}
		else
		{
			if (Magazine == null)
			{
				return false;
			}
			Magazine.AddOne();
		}
		audioInsertRound.Play(Owner.Position, Owner.AudioInstigator);
		return true;
	}

	public void InsertRoundIntoOpenBolt()
	{
		cyclingBolt.FeedRoundIntoOpen();
		audioInsertRound.Play(Owner.Position, Owner.AudioInstigator);
	}

	public void ReturnMagazine()
	{
		audioInsertMagazine.Play(Owner.Position, Owner.AudioInstigator);
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

	public void Turn(float newDirection)
	{
		direction = newDirection;
	}

	public float GetDirection()
	{
		return direction;
	}

	private float MovementFactor()
	{
		if (Owner == null)
		{
			return 1f;
		}
		if (!Owner.IsMoving)
		{
			return (!Owner.IsKneeling) ? 1f : 0.8f;
		}
		return 1f + Mathf.Abs(Owner.Velocity) * movementPenalty;
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
		AudioInstigator instigator = ((!(Owner != null)) ? null : Owner.AudioInstigator);
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
		InvokeBulletTime();
		SpawnProjectiles();
		AddSingleRecoil();
		EmitMuzzleSmoke();
		EmitMuzzleFlash();
		ApplyMuzzleBlastForces();
		if (!manualCycling && ammunition.cyclesAction)
		{
			cyclingBolt.Open();
		}
		if (Owner != null)
		{
			Owner.LastFiredTimestamp = Time.time;
			Owner.ShotsFired++;
		}
		Global.Context.Mixer.PlayActionMusic(5f);
	}

	public void ResetBurstCount()
	{
		consecutiveRounds = 0;
		burstRoundsRemaining = 0;
	}

	private void ApplyMuzzleBlastForces()
	{
		if (!ammunition.applyMuzzleBlastForces)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(Muzzle.position, muzzleBlastRange, affectedByMuzzleBlastsMask);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			if (!(attachedRigidbody == null))
			{
				Vector3 vector = attachedRigidbody.transform.position - base.transform.position;
				float time = vector.sqrMagnitude / muzzleBlastRangeSquared;
				float num = ammunition.muzzleBlastForce.Evaluate(time) * Time.deltaTime * 1500f;
				attachedRigidbody.AddForceAtPosition(vector.normalized * num, Muzzle.position, ForceMode.Force);
			}
		}
		if (Owner != null && Owner.Room != null)
		{
			float probability = Mathf.Clamp01(muzzleBlastRange * 0.33f);
			float num2 = UnityEngine.Random.Range(3.5f, 5f) - Mathf.Max(muzzleBlastRange, 3f);
			Owner.Room.TriggerFireAlarm(num2, num2 * 2f, probability);
		}
	}

	private void InvokeBulletTime()
	{
		if (settings.Time.enableBulletTime && !(Owner == null))
		{
			if (Owner.IsPlayer)
			{
				timeScaler.BulletTime(settings.Time.playerFire);
			}
			else
			{
				timeScaler.BulletTime(settings.Time.computerFire);
			}
		}
	}

	private void EmitMuzzleSmoke()
	{
		if (emitSmoke)
		{
			muzzleSmokeEmitter.transform.position = Muzzle.position;
			muzzleSmokeEmitter.transform.rotation = Muzzle.rotation;
			muzzleSmokeEmitter.Emit(1);
			if (ComponentSuppressor != null)
			{
				muzzleSmokeEmitter.transform.position = cyclingBolt.transform.position;
				muzzleSmokeEmitter.transform.LookAt(cyclingBolt.transform.position + Vector3.back);
				muzzleSmokeEmitter.Emit(1);
			}
		}
	}

	public float DetonationAudioPower(AmmunitionBase ammunition, WeaponComponentSuppressor suppressor, bool superSonic)
	{
		float num = ((!superSonic) ? 0.6f : 1f);
		float num2 = ((!(suppressor != null)) ? 1f : suppressor.powerFactor);
		return ammunition.DetonationAudioPower * audioPowerMultiplier * num * num2;
	}

	private void EmitMuzzleAudio(bool superSonic = true)
	{
		PropagatingAudioSample propagatingAudioSample = ((!(ComponentSuppressor != null)) ? audioFire : ((!superSonic) ? audioSuppressedSubsonic : audioSuppressedSupersonic));
		if (propagatingAudioSample.sample == null)
		{
			propagatingAudioSample = audioFire;
		}
		float customIntensity = DetonationAudioPower(ammunition, ComponentSuppressor, superSonic);
		AudioInstigator instigator = ((!(Owner != null)) ? null : Owner.AudioInstigator);
		propagatingAudioSample.PlayCustom(Muzzle.position.x, instigator, customIntensity, audioPowerMultiplier);
		if (superSonic)
		{
			audioSupersonic.PlayCustom(Muzzle.position.x, instigator, audioSupersonic.intensity, 1f);
		}
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

	private void EmitMuzzleFlash()
	{
		if (emitFlash && !(ComponentSuppressor != null) && !(muzzleFlash == null))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(muzzleFlash);
			Vector3 localScale = gameObject.transform.localScale * muzzleFlashScale;
			gameObject.transform.position = Muzzle.position;
			gameObject.transform.rotation = Muzzle.rotation;
			gameObject.transform.localScale = localScale;
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

	public void ToggleLaser()
	{
		if (ComponentLaser != null)
		{
			ComponentLaser.Toggle();
		}
	}

	public void EnableLaser(bool enable = true)
	{
		if (ComponentLaser != null)
		{
			ComponentLaser.SetState(enable);
		}
	}

	public void ToggleLight()
	{
		if (ComponentFlashlight != null)
		{
			ComponentFlashlight.Toggle();
		}
	}

	public void EnableLight(bool enable = true)
	{
		if (ComponentFlashlight != null)
		{
			ComponentFlashlight.SetState(enable);
		}
	}

	public void CycleFireModes()
	{
		FireMode activeFireMode = ActiveFireMode;
		fireModeIndex = (fireModeIndex + 1) % fireModes.Count;
		if (ActiveFireMode == FireMode.Safe)
		{
			fireModeIndex = (fireModeIndex + 1) % fireModes.Count;
		}
		if (ActiveFireMode != activeFireMode)
		{
			audioFireSelect.Play(base.transform.position.x, Owner.AudioInstigator);
		}
	}

	public void SetFireMode(FireMode mode, bool audio = true)
	{
		FireMode activeFireMode = ActiveFireMode;
		for (int i = 0; i < fireModes.Count; i++)
		{
			if (fireModes[i] == mode)
			{
				fireModeIndex = i;
				break;
			}
		}
		if (ActiveFireMode != activeFireMode && audio)
		{
			audioFireSelect.Play(base.transform.position.x, Owner.AudioInstigator);
		}
	}

	public void SetKinematic()
	{
		RigidBody.interpolation = RigidbodyInterpolation.None;
		RigidBody.isKinematic = true;
	}

	public void SetNonKinematic()
	{
		SuspendComponents(true);
		base.transform.SetParent(Global.Context.Mission.WeaponsContainer);
		RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
		RigidBody.isKinematic = false;
		if (mission != null && mission.AllClear && Global.Context.Settings.General.showInteractionMarkers)
		{
			ShowInteractionMarker(true);
		}
		if (!(Owner == null) && IsHolstered && type == WeaponBrace.Freehand)
		{
			AddFixedJoint(Owner.RightUpperLeg.transform, float.PositiveInfinity);
		}
	}

	public void ReleaseFromOwner()
	{
		SuspendComponents(true);
		IsHolstered = false;
		IsHeld = false;
		Owner = null;
		GlobalContext context = Global.Context;
		if (context.Mission.Occupants != null && context.Mission.Occupants.AllRenderedSafe && context.Settings.General.showInteractionMarkers)
		{
			ShowInteractionMarker(true);
		}
	}

	public void AttachForPeriod(CharacterBody body, float seconds)
	{
		AddFixedJoint(body.RightHand.transform, UnityEngine.Random.Range(10f, 300f));
		Invoke("DestroyJoint", seconds);
	}

	private void AddFixedJoint(Transform transformWithRigidbody, float breakForce)
	{
		handJoint = base.gameObject.AddComponent<FixedJoint>();
		handJoint.connectedBody = transformWithRigidbody.GetComponent<Rigidbody>();
		handJoint.breakForce = breakForce;
	}

	private void DestroyJoint()
	{
		if (handJoint != null)
		{
			UnityEngine.Object.Destroy(handJoint);
		}
		ReleaseFromOwner();
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
			RigidBody.AddForce(base.transform.right * num * -2f, ForceMode.Impulse);
		}
	}

	public void ShowInteractionMarker(bool show)
	{
		if (show)
		{
			if (interactionMarker == null)
			{
				interactionMarker = UnityEngine.Object.Instantiate(mission.secureWeaponMarker);
				interactionMarker.entity = rightHandAnchor;
			}
		}
		else
		{
			RemoveInteractionMarker();
		}
	}

	public Rect RenderBounds()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		bool flag = true;
		Collider component = colliderTransform.GetComponent<Collider>();
		if (component != null)
		{
			Bounds bounds = component.bounds;
			num = bounds.min.x;
			num3 = bounds.min.y;
			num2 = bounds.max.x;
			num4 = bounds.max.y;
			flag = false;
		}
		foreach (Transform item in colliderTransform)
		{
			Collider component2 = item.GetComponent<Collider>();
			if (!(component2 == null))
			{
				Bounds bounds2 = component2.bounds;
				if (flag)
				{
					num = bounds2.min.x;
					num3 = bounds2.min.y;
					num2 = bounds2.max.x;
					num4 = bounds2.max.y;
					flag = false;
				}
				else
				{
					num = Mathf.Min(num, bounds2.min.x);
					num3 = Mathf.Min(num3, bounds2.min.y);
					num2 = Mathf.Max(num2, bounds2.max.x);
					num4 = Mathf.Max(num4, bounds2.max.y);
				}
			}
		}
		foreach (WeaponComponentSlot componentSlot in componentSlots)
		{
			WeaponComponent component3 = componentSlot.component;
			if (!(component3 == null))
			{
				Rect rect = component3.RenderBounds();
				num = Mathf.Min(num, rect.min.x);
				num3 = Mathf.Min(num3, rect.min.y);
				num2 = Mathf.Max(num2, rect.max.x);
				num4 = Mathf.Max(num4, rect.max.y);
			}
		}
		Vector2 vector = new Vector2(num2 - num, num4 - num3);
		return new Rect(num - base.transform.position.x, num3 - base.transform.position.y, vector.x, vector.y);
	}

	public List<WeaponComponent> FindCompatibleComponents()
	{
		List<WeaponComponent> result = new List<WeaponComponent>();
		foreach (WeaponComponentSlot componentSlot in componentSlots)
		{
		}
		return result;
	}

	public float Length()
	{
		return base.transform.InverseTransformPoint(Muzzle.position).x;
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

	public float RequiredCleareance(float angle)
	{
		if (type == WeaponBrace.Shoulder)
		{
			return Mathf.Abs(Mathf.Sin(angle * ((float)Math.PI / 180f))) * Length();
		}
		return Length();
	}

	public Vector3 FireLineStart(bool precision)
	{
		return Muzzle.position + ((!precision) ? Vector3.zero : (Muzzle.forward * PrecisionBegin));
	}

	public static AmmunitionBase DefaultAmmunition(string weaponPrefabName)
	{
		UnityEngine.Object obj = Resources.Load(WeaponsPath + weaponPrefabName);
		return (!(obj != null)) ? null : (obj as GameObject).GetComponent<WeaponBase>().defaultAmmunition;
	}
}
