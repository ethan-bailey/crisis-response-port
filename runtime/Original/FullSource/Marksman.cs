namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Marksman : MonoBehaviour
{
	private class BoltManipulationInput
	{
		public bool operatingBolt;

		public float xDelta;

		public float yDelta;

		public float yMovementNormalized;

		public float xMovementNormalized;

		public float xBoltPenalty;

		public float yBoltPenalty;
	}

	public enum ComputerMode
	{
		Hold = 0,
		Engage = 1,
		Free = 2
	}

	private enum RespirationAction
	{
		Inhale = 0,
		Hold = 1,
		Exhale = 2,
		Pause = 3
	}

	private enum ComputerReload
	{
		Ready = 0,
		Scheduled = 1,
		RotateOpen = 2,
		TranslateBack = 3,
		CheckMagazine = 4,
		PendingNewMagazine = 5,
		TranslateForward = 6,
		RotateClosed = 7
	}

	public AnimationCurve respirationCycle;

	[Header("Rifle")]
	public int magazineCapacity = 5;

	public float riflingTwistRate = 10f;

	[Range(0f, 1f)]
	[Tooltip("How quick to align on a target.")]
	public float slerpFactor = 0.2f;

	public MarksmanScope scope = new MarksmanScope();

	[Header("Respiration")]
	[Tooltip("Scales the time to complete one respiration cycle at normal respiration.")]
	public float respirationRestPeriodScaler = 1f;

	[Tooltip("Scales the time to complete one respiration cycle at heavy respiration.")]
	public float respirationHeavyPeriodScaler = 0.65f;

	[Tooltip("How much normal respiration offsets the aimpoint 10 meters out.")]
	public float respirationRestOffset = 0.0065f;

	[Tooltip("How much the heaviest breath offsets the aimpoint 10 meters out.")]
	public float respirationHeavyOffset = 0.008f;

	[Tooltip("How long breath can be held before it is resumed. In seconds.")]
	public float respirationMaxHoldPeriod = 15f;

	public AudioSource breathingAudioSource;

	[Tooltip("Curve for tweening the volume of inhale and exhale audio so they don't noticably interrupt eachother.")]
	public AnimationCurve breathingVolume;

	[Header("Recoil")]
	public AnimationCurve recoilTranslationCurve;

	public AnimationCurve recoilPitchCurve;

	public AnimationCurve recoilYawCurve;

	[Tooltip("How far the rifle translates backward on firing. In meters.")]
	public float recoilTranslation;

	[Tooltip("How much the rifle rotates on pitch after firing. Measured as the distance the aimpoint rises 10 meters in front of rifle.")]
	public float recoilPitch;

	[Tooltip("How much the rifle rotates on yaw after firing. Measured as the distance the aimpoint rises 10 meters in front of rifle.")]
	public float recoilYaw;

	[Tooltip("Time in seconds the recoil yaw rotation will take to complete. The peak is at half the time.")]
	public float recoilPeriod;

	[Header("Player control")]
	[Tooltip("How precis mouse movements are required to operate the bolt with optimum efficiency.")]
	[Range(0f, 1f)]
	public float gestureStrictness;

	[Header("AI control")]
	public bool computerControl;

	private const float skill = 0.25f;

	public ComputerMode computerMode;

	[Tooltip("Delay between bolt operation steps. In seconds.")]
	[Range(0f, 0.2f)]
	public float stepDelay = 0.2f;

	[Tooltip("How long it takes to change magazines of the rifle. In seconds.")]
	public float reloadTime = 1f;

	[Range(0f, 1f)]
	public float boltRotationTime = 1f;

	[Range(0f, 1f)]
	public float boltTranslationTime = 1f;

	[Header("Audio")]
	public AudioClip audioFire;

	public AudioClip audioFiringPin;

	public AudioClip audioRotateOpenBegin;

	public AudioClip audioRotateOpenStop;

	public AudioClip audioRotateClosed;

	public AudioClip audioTranslateOpen;

	public AudioClip audioTranslateOpenEmpty;

	public AudioClip audioTranslateClosedBegin;

	public AudioClip audioTranslateClosedBeginEmpty;

	public AudioClip audioTranslateClosedStop;

	public AudioClip audioExtractCasing;

	public AudioClip audioRemoveMagazine;

	public AudioClip audioInsertMagazine;

	public AudioClip audioScopeClick;

	public List<AudioClip> inhales = new List<AudioClip>();

	public List<AudioClip> exhales = new List<AudioClip>();

	[Header("Radio audio")]
	public AudioClip radioAffermative;

	public AudioClip radioNegative;

	public AudioClip radioTaskComplete;

	[Header("Interior audio")]
	public List<AudioClip> interiorDetonate = new List<AudioClip>();

	public List<AudioClip> exteriorDetonate = new List<AudioClip>();

	[Header("Tutorial mode")]
	public bool disableBoltRotation;

	public bool disableBoltTranslation;

	[Header("References")]
	public MarksmanTargetIndicator targetIndicator;

	public Transform targetRotation;

	public Transform targetPosition;

	public float aiLerpOffsetFactor = 0.075f;

	private Mission mission;

	private Target target;

	private InputHandler inputHandler;

	private InteriorMode interiorMode;

	private Operators operators;

	private Marksmen marksmen;

	private MarksmanMode marksmanMode;

	private MarksmanCamera marksmanCamera;

	private BoltManipulationInput boltInput = new BoltManipulationInput();

	private SerializableGraphicsSettings graphicsSettings;

	private MarksmanBoltActionVisualization boltActionVisualization;

	private RespirationAction respiration;

	private ComputerReload computerReload;

	private Vector3 muzzleOffset = new Vector3(0f, -0.03f, 0.5f);

	private Vector3 homePosition;

	private Transform projectilesContainer;

	private float aimSensitivity;

	private float boltManipulationSensitivity;

	private const float NormalTargetProximity = 0.01f;

	private const float UrgentTargetProximity = 0.03f;

	private const float MinimumTimeAligned = 0.2f;

	private bool missionHasStarted;

	private bool chamberEmpty;

	private bool rotationOpening;

	private bool translationOpening;

	private float boltRotationNormalized;

	private float boltTranslationNormalized;

	private bool boltCarrying;

	private bool boltCarryingLive;

	private float fireTimestamp;

	private bool enableBreathingOffset = true;

	private float projectileForceMultiplier = 1f;

	private float recoilScale = 1f;

	private bool holdBreathHasBeenReleased = true;

	private bool kindMode = true;

	private bool disableBoltBackwardTranslation;

	private bool showTargetIndication;

	private bool threatOverridesHoldFire = true;

	private bool playerControl = true;

	private float computerTimeAligned;

	private float computerTargetTimestamp;

	private float resolutionBasedMouseSensitivity = 1f;

	private float respirationPeriodScale;

	private float respirationOffset;

	private float respirationTime;

	private float respirationPartCycle;

	private float respirationCycleTime;

	private float respirationHeldTime;

	private float respirationInhalePeriod = 2f;

	private float respirationHoldPeriod = 0.75f;

	private float respirationExhalePeriod = 2.5f;

	private float respirationPausePeriod = 0.75f;

	private bool requestHoldBreath;

	private bool targetIsThreateningOperator;

	private bool tutorialMode;

	public MarksmanRifle Rifle { get; private set; }

	public AmmunitionBase Ammunition { get; private set; }

	public AudioSource AudioSource { get; private set; }

	public AudioInstigator AudioInstigator { get; private set; }

	public Vector3 AimPoint { get; private set; }

	public float ScopeZeroRange { get; set; }

	public bool MagazineEmpty
	{
		get
		{
			return RoundsInMagazine <= 0;
		}
	}

	public bool ChamberLive { get; private set; }

	public bool BoltSeated
	{
		get
		{
			return boltRotationNormalized < 0.05f && boltTranslationNormalized < 0.05f;
		}
	}

	public bool BoltSeatedAndLive
	{
		get
		{
			return BoltSeated && ChamberLive;
		}
	}

	public bool BoltRotationFullyOpen { get; private set; }

	public bool BoltTranslationFullyOpen { get; private set; }

	public bool BoltTranslationFullyClosed { get; private set; }

	private float MaxMovementOnOtherAxis
	{
		get
		{
			return Mathf.Lerp(0.001f, 1f, 1f - gestureStrictness);
		}
	}

	public Projectile CurrentProjectile { get; private set; }

	public int CurrentMagnificationLevel { get; private set; }

	public int RoundsInMagazine { get; private set; }

	public bool FiringPinReady { get; private set; }

	public bool OutOfBreath { get; private set; }

	public bool AutomaticCheat { get; set; }

	public int ShotsFired { get; private set; }

	public int ShotsHit { get; set; }

	public CharacterBody LastVictim { get; set; }

	public float FireTimestamp
	{
		get
		{
			return fireTimestamp;
		}
	}

	public Target Target
	{
		get
		{
			return MarkedTarget ?? target;
		}
	}

	public Target MarkedTarget { get; set; }

	private void Awake()
	{
		GlobalContext context = Global.Context;
		mission = context.Mission;
		tutorialMode = mission.TutorialMode != null && mission.TutorialMode.Active;
		AudioSource = GetComponent<AudioSource>();
		AudioInstigator = GetComponent<AudioInstigator>();
		AudioSource.priority = 15;
		breathingAudioSource.priority = 120;
		inputHandler = context.InputHandler;
		AimPoint = new Vector3(10f, 1f, 0f);
		graphicsSettings = context.Settings.Graphics;
		enableBreathingOffset = context.Settings.General.enableMarksmanBobbing;
		showTargetIndication = context.Settings.General.showMarksmanTargetIndication || tutorialMode;
		mission.onMissionBegin.AddListener(OnMissionBegin);
		projectilesContainer = mission.ProjectilesContainer;
		FiringPinReady = true;
		aimSensitivity = context.Settings.General.marksmanAimSensitivity * 5f;
		boltManipulationSensitivity = context.Settings.General.marksmanReloadSensitivity * 20f;
		GameObject gameObject = Resources.Load(WeaponBase.WeaponsPath + context.OperatorSpawnData.Team.marksman.rifle.prefabName) as GameObject;
		if (gameObject != null)
		{
			Rifle = gameObject.GetComponent<MarksmanRifle>();
		}
	}

	public void Initialize(Marksmen marksmen)
	{
		this.marksmen = marksmen;
	}

	public void InitializePosition(Vector3 position)
	{
		base.transform.position = (homePosition = position);
	}

	private void Start()
	{
		GlobalContext context = Global.Context;
		targetRotation.position = base.transform.position;
		ScopeZeroRange = Mathf.Abs(base.transform.position.z);
		threatOverridesHoldFire = context.OperatorSpawnData.Team.behavior.marksmanThreatOverride;
		interiorMode = context.Mission.InteriorMode;
		marksmanMode = context.Mission.MarksmanMode;
		operators = context.Mission.Operators;
		marksmanCamera = marksmanMode.marksmanCamera;
		boltActionVisualization = marksmanMode.boltActionVisualization;
		boltActionVisualization.MagazineSize = magazineCapacity;
		boltActionVisualization.transform.position = base.transform.position + new Vector3(0f, muzzleOffset.y, 0f);
		InitializeAmmunition(context.OperatorSpawnData.Team.marksman.rifle.ammunitionPrefab, context.EquipmentList.defaultMarksmanAmmunition);
		SetScopeZoomLevel(CurrentMagnificationLevel, false);
		boltActionVisualization.Initialize(Ammunition != null);
		SerializableExtraSettings extras = context.Settings.Extras;
		projectileForceMultiplier = extras.forceMultiplier;
		recoilScale = extras.marksmanRecoil;
		playerControl = extras.playerMarksmanControl;
		context.Settings.General.onChanged.AddListener(OnGeneralSettingsChanged);
		context.Settings.Extras.onChanged.AddListener(OnExtraSettingsChanged);
		context.Settings.General.onMarksmanIndicationChanged.AddListener(OnShowMarksmanIndicationChanged);
		OnGeneralSettingsChanged(context.Settings.General);
		OnExtraSettingsChanged(context.Settings.Extras);
		if (context.Mission.TutorialMode != null && !context.Mission.TutorialMode.Active)
		{
			SetModeImmediate(context.OperatorSpawnData.Team.behavior.marksmanMode);
		}
	}

	private void OnMissionBegin()
	{
		missionHasStarted = true;
	}

	public void InitializeAmmunition(string ammunitionPrefabName, AmmunitionBase fallback)
	{
		if (Rifle == null)
		{
			Debug.LogError("Missing rifle when initializing marksman!");
			return;
		}
		EquipmentList equipmentList = Global.Context.EquipmentList;
		if (ammunitionPrefabName == equipmentList.randomAmmunition.name)
		{
			AmmunitionBase ammunitionBase = equipmentList.RandomAmmunition(Rifle.caliber);
			ammunitionPrefabName = ((!(ammunitionBase != null)) ? string.Empty : ammunitionBase.name);
		}
		Ammunition = AmmunitionBase.GetAmmunition(ammunitionPrefabName, fallback);
		if (Ammunition != null && Ammunition == equipmentList.noAmmunition && !Global.Context.Mission.TutorialMode.Active)
		{
			Ammunition = null;
			boltActionVisualization.roundPrefab = null;
			chamberEmpty = true;
		}
		else
		{
			boltActionVisualization.roundPrefab = Ammunition.marksmanRound;
			RoundsInMagazine = magazineCapacity;
			ChamberLive = true;
		}
	}

	private void OnGeneralSettingsChanged(SerializableGeneralSettings newSettings)
	{
		enableBreathingOffset = newSettings.enableMarksmanBobbing;
		aimSensitivity = newSettings.marksmanAimSensitivity * resolutionBasedMouseSensitivity * 5f;
		boltManipulationSensitivity = newSettings.marksmanReloadSensitivity * resolutionBasedMouseSensitivity * 20f;
	}

	private void OnShowMarksmanIndicationChanged(bool show)
	{
		showTargetIndication = show || tutorialMode;
	}

	private void OnExtraSettingsChanged(SerializableExtraSettings newSettings)
	{
		projectileForceMultiplier = newSettings.forceMultiplier;
		recoilScale = newSettings.marksmanRecoil;
		playerControl = newSettings.playerMarksmanControl;
	}

	private void Update()
	{
		UpdateRespiration();
		Vector3 vector = Vector3.zero;
		if (!AutomaticCheat)
		{
			float time = (Time.time - fireTimestamp) / recoilPeriod;
			base.transform.position = homePosition - base.transform.forward * recoilTranslationCurve.Evaluate(time) * recoilTranslation * recoilScale;
			vector = new Vector3((0f - recoilYawCurve.Evaluate(time)) * recoilYaw, recoilPitchCurve.Evaluate(time) * recoilPitch, 0f);
		}
		Vector3 vector2 = ((enableBreathingOffset && !computerControl) ? new Vector3(0f, respirationCycle.Evaluate(respirationCycleTime) * respirationOffset, 0f) : Vector3.zero);
		targetRotation.LookAt(AimPoint);
		base.transform.LookAt(base.transform.position + base.transform.forward * 10f + vector2 + vector * recoilScale);
		float num = 2.525f - Mathf.Pow(Mathf.Clamp01(Quaternion.Angle(base.transform.rotation, targetRotation.rotation)), 2f) * 2.5f;
		float t = ((!computerControl) ? slerpFactor : num);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, targetRotation.rotation, t);
		bool flag = mission.View == Mission.MissionView.Marksman;
		computerControl = !playerControl || !flag;
		Ray ray = new Ray(base.transform.position, base.transform.forward);
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(ray, out hitInfo, 100f, Masks.Instance.Aim))
		{
			targetPosition.position = hitInfo.point;
		}
		if (!mission.UI.missionEndMenu.Visible)
		{
			UpdateControls();
		}
		if (computerControl && missionHasStarted)
		{
			UpdateComputer();
		}
		if (flag)
		{
			targetIndicator.SetVisible(false);
		}
	}

	private void UpdateComputer()
	{
		if (computerReload == ComputerReload.Ready && !ChamberLive)
		{
			computerReload = ComputerReload.RotateOpen;
		}
		boltInput.operatingBolt = computerReload != ComputerReload.Ready;
		float num = 0.4375f;
		switch (computerReload)
		{
		case ComputerReload.RotateOpen:
			boltInput.xDelta = 0f - num;
			boltInput.yDelta = 0f;
			if (boltRotationNormalized >= 1f)
			{
				computerReload = ComputerReload.TranslateBack;
			}
			break;
		case ComputerReload.TranslateBack:
			boltInput.xDelta = 0f;
			boltInput.yDelta = 0f - num;
			if (boltTranslationNormalized >= 1f)
			{
				computerReload = ComputerReload.CheckMagazine;
			}
			break;
		case ComputerReload.CheckMagazine:
			if (MagazineEmpty)
			{
				StartChangeMagazine();
				computerReload = ComputerReload.PendingNewMagazine;
			}
			else
			{
				computerReload = ComputerReload.TranslateForward;
			}
			break;
		case ComputerReload.PendingNewMagazine:
			if (!MagazineEmpty)
			{
				computerReload = ComputerReload.TranslateForward;
			}
			break;
		case ComputerReload.TranslateForward:
			boltInput.xDelta = 0f;
			boltInput.yDelta = num;
			if (boltTranslationNormalized <= 0f)
			{
				computerReload = ComputerReload.RotateClosed;
			}
			break;
		case ComputerReload.RotateClosed:
			boltInput.xDelta = num;
			boltInput.yDelta = 0f;
			if (boltRotationNormalized <= 0f)
			{
				computerReload = ComputerReload.Ready;
			}
			break;
		}
		if (computerReload != ComputerReload.Ready)
		{
			return;
		}
		Target target = Target;
		if (target != null && !target.active && computerMode == ComputerMode.Engage)
		{
			SetModeImmediate(ComputerMode.Hold);
		}
		targetIndicator.SetVisible(showTargetIndication && target != null && target.active);
		if (computerReload != ComputerReload.Ready && showTargetIndication && target != null)
		{
			float position = ((!(target.Body != null)) ? target.transform.position.x : target.Body.Head.AimPoint.x);
			targetIndicator.SetStatus(MarksmanTargetIndicator.Status.NotReady, position);
		}
		ComputerUpdateTarget();
		if (target == null)
		{
			ComputerResetControls();
			return;
		}
		float magnitude = (target.transform.position - base.transform.position).magnitude;
		Vector3 vector = base.transform.position + base.transform.forward * magnitude;
		float num2 = Mathf.Abs(AimPoint.x - vector.x) * aiLerpOffsetFactor;
		float num3 = magnitude / Ammunition.velocity;
		Vector3 vector2 = ((!(target.Body != null)) ? Vector3.zero : (Vector3.right * target.Body.Velocity * (num3 + num2)));
		Vector3 vector3 = new Vector3(0f, 4.91f * Mathf.Pow(num3, 2f), 0f);
		Vector3 vector4 = ((!(target.Body != null)) ? target.transform.position : target.Body.Head.AimPoint);
		AimPoint = vector4 + vector2 + vector3;
		float magnitude2 = (AimPoint - base.transform.position).magnitude;
		Ray ray = new Ray(base.transform.position, (vector4 - base.transform.position).normalized);
		RaycastHit hitInfo = default(RaycastHit);
		bool flag = false;
		if (target.Body == null && tutorialMode)
		{
			if (Physics.Raycast(ray, out hitInfo, magnitude2, Masks.Instance.MarksmanObstacles))
			{
				TeamTutorialMarksmanTarget component = hitInfo.collider.GetComponent<TeamTutorialMarksmanTarget>();
				flag = component != null && component.target == target;
			}
		}
		else
		{
			Limb limb = null;
			if (Physics.Raycast(ray, out hitInfo, magnitude2, Masks.Instance.MarksmanObstacles))
			{
				limb = hitInfo.transform.GetComponent<Limb>();
				flag = limb != null && limb.Body.Target == target;
			}
		}
		bool flag2 = requestHoldBreath;
		requestHoldBreath = flag && (target.Body == null || !target.Body.IsMoving);
		if (flag2 && !requestHoldBreath)
		{
			holdBreathHasBeenReleased = true;
		}
		float num4 = ((!targetIsThreateningOperator) ? 0.01f : 0.03f);
		bool flag3 = Mathf.Abs(vector.x - AimPoint.x) / magnitude2 < num4 && Mathf.Abs(vector.y - AimPoint.y) / magnitude2 < num4 * 2f;
		if (flag)
		{
			if (flag3)
			{
				computerTimeAligned += Time.deltaTime;
				if (computerTimeAligned < 0.2f)
				{
					if (showTargetIndication)
					{
						targetIndicator.SetStatus(MarksmanTargetIndicator.Status.NotReady, vector4.x);
					}
				}
				else
				{
					if (target.Body != null && target.Body.IsMoving && target.Body.IsSprinting)
					{
						return;
					}
					if (showTargetIndication)
					{
						targetIndicator.SetStatus(MarksmanTargetIndicator.Status.Ready, vector4.x);
					}
					if (BoltSeated && ChamberLive && FiringPinReady && (computerMode != ComputerMode.Hold || targetIsThreateningOperator))
					{
						Fire();
						computerTimeAligned = 0f;
						if (computerMode == ComputerMode.Engage)
						{
							SetModeImmediate(ComputerMode.Hold);
						}
					}
				}
			}
			else
			{
				computerTimeAligned = 0f;
				if (showTargetIndication)
				{
					targetIndicator.SetStatus(MarksmanTargetIndicator.Status.NotReady, vector4.x);
				}
			}
		}
		else
		{
			computerTimeAligned = 0f;
			if (showTargetIndication)
			{
				targetIndicator.SetStatus(MarksmanTargetIndicator.Status.Occluded, vector4.x);
			}
		}
	}

	public void ComputerCommand(ComputerMode mode, float delay = 0.5f)
	{
		if (computerMode == mode && mode != ComputerMode.Hold)
		{
			mode = ComputerMode.Hold;
		}
		UIInteriorModeTextIndicator indicatorUpperRight = Global.Context.Mission.UI.indicatorUpperRight;
		if (mode != ComputerMode.Engage)
		{
			if (mode != ComputerMode.Free)
			{
				if (mode == ComputerMode.Hold)
				{
					indicatorUpperRight.Indicate("Marksman hold", radioAffermative);
				}
			}
			else
			{
				indicatorUpperRight.Indicate("Marksman free", radioAffermative);
			}
		}
		else
		{
			indicatorUpperRight.Indicate("Marksman engage", radioAffermative);
		}
		Coroutine.DelayedAction(delay, () =>
		{
			SetModeImmediate(mode);
		}, this);
	}

	public void SetModeImmediate(ComputerMode mode)
	{
		computerMode = mode;
		if (!(interiorMode == null))
		{
			interiorMode.UI.marksmanIndicator.SetMode(mode);
		}
	}

	public void ComputerSetTarget(Target target, float delay = 0.5f)
	{
		Global.Context.Mission.UI.indicatorUpperRight.Indicate(string.Format("Marksman watching {0}", (!(target != null)) ? "all" : target.name), radioAffermative, 2.5f);
		Coroutine.DelayedAction(delay, () =>
		{
			SetTargetImmediate(target);
		}, this);
	}

	private void SetTargetImmediate(Target target)
	{
		MarkedTarget = target;
	}

	private void ComputerUpdateTarget()
	{
		if (mission.UI.endOverlay.Visible)
		{
			CharacterBody subject = mission.InteriorMode.interiorCamera.Subject;
			if (subject != null && !subject.IsOperator && !subject.IsRagdoll)
			{
				this.target = subject.Target;
				return;
			}
		}
		TeamOperator leader = operators.MainGroup.Leader;
		TeamOperator leader2 = operators.SupportGroup.Leader;
		float num = ((!(leader != null)) ? float.NegativeInfinity : leader.Body.Position);
		float num2 = ((!(leader2 != null)) ? float.NegativeInfinity : leader2.Body.Position);
		TeamOperator teamOperator = ((!(num > num2)) ? leader2 : leader);
		CharacterBody characterBody = ((!(teamOperator != null)) ? null : teamOperator.Body);
		CharacterBody characterBody2 = ((!(this.target != null)) ? null : this.target.Body);
		targetIsThreateningOperator = characterBody2 != null && characterBody != null && threatOverridesHoldFire && characterBody2 != null && !characterBody.Invisible && !characterBody2.IsRagdoll && (characterBody2.WeaponIsFiring() || (characterBody2.WeaponIsHeldHigh() && characterBody2.CanSeePosition(characterBody.Position)));
		if (MarkedTarget != null)
		{
			if (MarkedTarget.active)
			{
				return;
			}
			MarkedTarget = null;
			Global.Context.Mission.UI.indicatorUpperRight.Indicate("Target down", radioTaskComplete, 3f);
		}
		if (this.target != null && this.target.active && Time.time - computerTargetTimestamp < 1f)
		{
			return;
		}
		Target target = null;
		float num3 = float.PositiveInfinity;
		if (tutorialMode)
		{
			List<Target> marksmanTargets = (mission.TutorialMode.Tutorial as TeamTutorial).marksmanTargets;
			foreach (Target item in marksmanTargets)
			{
				if (item.active)
				{
					float num4 = 0f;
					float x = item.transform.position.x;
					if (characterBody != null)
					{
						bool flag = (characterBody.Direction > 0f && 0f - characterBody.Position < 0f) || (characterBody.Direction < 0f && x - characterBody.Position > 0f);
						float num5 = ((!(interiorMode != null)) ? 10f : interiorMode.interiorCamera.SubjectViewDistance());
						num4 = Mathf.Abs(characterBody.Position - x) + ((!flag) ? 0f : num5);
					}
					else
					{
						num4 = x;
					}
					if ((!(characterBody != null) || !(num4 < 0.75f)) && num4 < num3)
					{
						target = item;
						num3 = num4;
					}
				}
			}
		}
		else
		{
			List<CharacterBody> bodies = mission.Occupants.Bodies;
			foreach (CharacterBody item2 in bodies)
			{
				if (!item2.IsRagdoll)
				{
					float num6 = 0f;
					if (characterBody != null)
					{
						bool flag2 = (characterBody.Direction > 0f && item2.Position - characterBody.Position < 0f) || (characterBody.Direction < 0f && item2.Position - characterBody.Position > 0f);
						float num7 = ((!(interiorMode != null)) ? 10f : interiorMode.interiorCamera.SubjectViewDistance());
						num6 = Mathf.Abs(characterBody.Position - item2.Position) + ((!flag2) ? 0f : num7);
					}
					else
					{
						num6 = item2.Position;
					}
					if ((!(characterBody != null) || !(num6 < 0.75f)) && num6 < num3)
					{
						target = item2.Target;
						num3 = num6;
					}
				}
			}
		}
		if (target != this.target)
		{
			computerTargetTimestamp = Time.time;
		}
		this.target = target;
	}

	private void ComputerResetControls()
	{
		if (!boltInput.operatingBolt)
		{
			boltInput.xDelta = (boltInput.yDelta = 0f);
			computerTimeAligned = 0f;
		}
	}

	private void UpdateRespiration()
	{
		if (!computerControl)
		{
			if (inputHandler.Keys.interact.Pressed && holdBreathHasBeenReleased)
			{
				requestHoldBreath = true;
				holdBreathHasBeenReleased = false;
			}
			else if (!inputHandler.Keys.interact.Pressed)
			{
				requestHoldBreath = false;
				holdBreathHasBeenReleased = true;
			}
		}
		OutOfBreath = respirationHeldTime >= respirationMaxHoldPeriod;
		if (OutOfBreath)
		{
			requestHoldBreath = false;
		}
		bool flag = requestHoldBreath && !OutOfBreath;
		float num = respirationHeldTime / respirationMaxHoldPeriod;
		respirationPeriodScale = Mathf.Lerp(respirationRestPeriodScaler, respirationHeavyPeriodScaler, num);
		respirationOffset = Mathf.Lerp(respirationRestOffset, respirationHeavyOffset, num);
		if (flag)
		{
			respirationHeldTime += Time.deltaTime;
		}
		else
		{
			respirationHeldTime = Mathf.Max(respirationHeldTime - Time.deltaTime * 0.75f, 0f);
		}
		switch (respiration)
		{
		case RespirationAction.Inhale:
			respirationTime += Time.deltaTime;
			respirationPartCycle = respirationTime / (respirationInhalePeriod * ((!flag) ? respirationPeriodScale : 1f));
			respirationCycleTime = respirationPartCycle * 0.35f;
			breathingAudioSource.volume = breathingVolume.Evaluate(respirationPartCycle);
			break;
		case RespirationAction.Hold:
			respirationTime += ((!flag) ? Time.deltaTime : 0f);
			respirationPartCycle = ((!flag) ? (respirationTime / (respirationHoldPeriod * Mathf.Clamp01(1f - num * 2f))) : 0.99f);
			break;
		case RespirationAction.Exhale:
			respirationTime += Time.deltaTime;
			respirationPartCycle = respirationTime / (respirationExhalePeriod * ((!flag) ? respirationPeriodScale : 1f));
			respirationCycleTime = 0.35f + respirationPartCycle * 0.65f;
			breathingAudioSource.volume = breathingVolume.Evaluate(respirationPartCycle);
			break;
		case RespirationAction.Pause:
			respirationTime += ((!flag) ? Time.deltaTime : 0f);
			respirationPartCycle = ((!flag) ? (respirationTime / (respirationPausePeriod * Mathf.Clamp01(1f - num * 2f))) : 0.99f);
			break;
		}
		if (!(respirationPartCycle >= 1f))
		{
			return;
		}
		respirationTime = 0f;
		if (respiration == RespirationAction.Pause)
		{
			respiration = RespirationAction.Inhale;
			PlayInhale();
			return;
		}
		respiration++;
		if (respiration == RespirationAction.Exhale)
		{
			PlayExhale();
		}
	}

	private void UpdateControls()
	{
		if (!computerControl && Global.Context.Mission.View == Mission.MissionView.Marksman)
		{
			boltInput.operatingBolt = inputHandler.Keys.precision.Pressed;
			boltInput.xDelta = Input.GetAxis("Mouse X");
			boltInput.yDelta = Mathf.Max((!boltInput.operatingBolt || !disableBoltBackwardTranslation) ? (-1f) : 0f, Input.GetAxis("Mouse Y"));
			if (Version.Instance.Staging && Input.GetKeyDown(KeyCode.Tab))
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
		boltInput.yMovementNormalized = Mathf.Clamp01(Mathf.Abs(boltInput.yDelta) / MaxMovementOnOtherAxis);
		boltInput.xMovementNormalized = Mathf.Clamp01(Mathf.Abs(boltInput.xDelta) / MaxMovementOnOtherAxis);
		boltInput.xBoltPenalty = 1f - boltInput.yMovementNormalized;
		boltInput.yBoltPenalty = 1f - boltInput.xMovementNormalized;
		if (boltInput.operatingBolt)
		{
			float num = ((!disableBoltRotation) ? (boltInput.xDelta * boltManipulationSensitivity * boltInput.xBoltPenalty * -1f * Time.deltaTime) : 0f);
			float num2 = ((!disableBoltTranslation) ? (boltInput.yDelta * boltManipulationSensitivity * boltInput.yBoltPenalty * -1f * Time.deltaTime) : 0f);
			if (boltTranslationNormalized <= 0.005f)
			{
				boltTranslationNormalized = 0f;
				boltRotationNormalized = Mathf.Clamp01(boltRotationNormalized + num);
			}
			if (boltRotationNormalized >= 0.995f)
			{
				boltRotationNormalized = 1f;
				boltTranslationNormalized = Mathf.Clamp01(boltTranslationNormalized + num2);
			}
			UpdateBolt();
		}
		else
		{
			Vector3 vector = new Vector3(boltInput.xDelta, boltInput.yDelta, 0f) * aimSensitivity * Time.deltaTime;
			AimPoint = new Vector3(Mathf.Clamp(AimPoint.x + vector.x, marksmen.upperLeft.x, marksmen.lowerRight.x), Mathf.Clamp(AimPoint.y + vector.y, marksmen.lowerRight.y, marksmen.upperLeft.y), 0f);
			boltActionVisualization.Display(!BoltSeated);
		}
		boltActionVisualization.SetTransforms(boltRotationNormalized, boltTranslationNormalized);
		if (boltInput.operatingBolt || !BoltSeated)
		{
			boltActionVisualization.Render();
		}
		if (!computerControl && BoltSeated && inputHandler.Keys.fire.Pressed)
		{
			Fire();
		}
		if (inputHandler.Keys.magazine.Down)
		{
			StartChangeMagazine();
		}
		UpdateScope();
	}

	private void SetScopeZoomLevel(int level, bool audio = true)
	{
		CurrentMagnificationLevel = level;
		marksmanCamera.SetFieldOfView(scope.fieldsOfView[CurrentMagnificationLevel]);
		if (audio)
		{
			AudioSource.PlayOneShot(audioScopeClick, 0.5f);
		}
	}

	private void UpdateScope()
	{
		int num = CurrentMagnificationLevel;
		if (inputHandler.Keys.doorPush.Down)
		{
			num++;
		}
		else if (inputHandler.Keys.doorPull.Down)
		{
			num--;
		}
		num = Mathf.Clamp(num, 0, scope.fieldsOfView.Count - 1);
		if (num != CurrentMagnificationLevel)
		{
			SetScopeZoomLevel(num);
		}
	}

	private void UpdateBolt()
	{
		UpdateBoltRotation();
		UpdateBoltTranslation();
		float num = Mathf.Clamp01(boltInput.yDelta / 50f) * -1f;
		Vector3 force = new Vector3(num, num * 5f, Mathf.Clamp01(boltInput.xDelta / 50f) * -1f);
		boltActionVisualization.ApplyCasingsForce(force);
	}

	private void UpdateBoltRotation()
	{
		bool boltRotationFullyOpen = BoltRotationFullyOpen;
		bool flag = rotationOpening;
		BoltRotationFullyOpen = boltRotationNormalized >= 1f;
		rotationOpening = boltRotationNormalized >= 0.1f;
		if (!flag && rotationOpening)
		{
			AudioSource.PlayOneShot(audioRotateOpenBegin);
		}
		else if (flag && !rotationOpening)
		{
			AudioSource.PlayOneShot(audioRotateClosed);
		}
		if (!boltRotationFullyOpen && BoltRotationFullyOpen)
		{
			AudioSource.PlayOneShot(audioRotateOpenStop);
		}
		if (!kindMode)
		{
			float num = ((!BoltRotationFullyOpen && rotationOpening) ? boltInput.yMovementNormalized : 1f);
			AimPoint += new Vector3(boltInput.xDelta * num, boltInput.yDelta, 0f) * aimSensitivity * Time.deltaTime;
		}
	}

	private void UpdateBoltTranslation()
	{
		bool boltTranslationFullyOpen = BoltTranslationFullyOpen;
		bool flag = translationOpening;
		BoltTranslationFullyOpen = boltTranslationNormalized >= 1f;
		BoltTranslationFullyClosed = boltTranslationNormalized <= 0f;
		translationOpening = boltTranslationNormalized >= 0.1f;
		FiringPinReady = FiringPinReady || boltTranslationNormalized >= 0.5f;
		if (!flag && translationOpening)
		{
			BoltExtractFromChamber();
			AudioSource.PlayOneShot((!MagazineEmpty) ? audioTranslateOpen : audioTranslateOpenEmpty);
		}
		else if (flag && !translationOpening)
		{
			BoltSeatRoundInChamber();
			AudioSource.PlayOneShot(audioTranslateClosedStop);
			if (tutorialMode)
			{
				disableBoltBackwardTranslation = false;
			}
		}
		if (!boltTranslationFullyOpen && BoltTranslationFullyOpen)
		{
			ExtractCasing(boltInput.yMovementNormalized);
			AudioSource.PlayOneShot(audioRotateOpenBegin);
			if (tutorialMode)
			{
				disableBoltBackwardTranslation = true;
			}
		}
		if (boltTranslationFullyOpen && !BoltTranslationFullyOpen)
		{
			if (MagazineEmpty)
			{
				AudioSource.PlayOneShot(audioTranslateClosedBeginEmpty);
			}
			else
			{
				BoltPickupNewRoundFromMagazine();
				AudioSource.PlayOneShot(audioTranslateClosedBegin);
			}
		}
		if (!kindMode)
		{
			float num = ((!BoltTranslationFullyOpen && translationOpening) ? boltInput.xMovementNormalized : 1f);
			AimPoint += new Vector3(boltInput.xDelta, boltInput.yDelta * num, 0f) * aimSensitivity * Time.deltaTime;
		}
	}

	private void BoltExtractFromChamber()
	{
		boltCarrying = !chamberEmpty;
		boltCarryingLive = ChamberLive;
		chamberEmpty = true;
		ChamberLive = false;
	}

	private void BoltPickupNewRoundFromMagazine()
	{
		boltActionVisualization.BoltPickupRound();
		RoundsInMagazine--;
		boltCarrying = (boltCarryingLive = true);
	}

	private void BoltSeatRoundInChamber()
	{
		chamberEmpty = !boltCarrying;
		ChamberLive = boltCarryingLive;
		boltCarrying = false;
		boltCarryingLive = false;
	}

	private void ExtractCasing(float force)
	{
		if (boltCarrying)
		{
			boltActionVisualization.BoltDisposeRound(force);
			boltCarrying = (boltCarryingLive = false);
			AudioSource.PlayOneShot(audioExtractCasing);
		}
	}

	public void Fire()
	{
		if (!FiringPinReady)
		{
			return;
		}
		FiringPinReady = false;
		AudioSource.PlayOneShot(audioFiringPin);
		if (ChamberLive)
		{
			DetonateRound();
			if (AutomaticCheat)
			{
				mission.ActionScheduler.Schedule(AutoMarksmanCheatResetMechanism, 0.025f);
			}
		}
	}

	private void DetonateRound()
	{
		fireTimestamp = Time.time;
		ChamberLive = false;
		SpawnProjectile();
		if (!AutomaticCheat)
		{
			boltActionVisualization.DetonateRound();
		}
		PlayDetonationAudio();
		Global.Context.Mixer.PlayActionMusic(7.5f);
		ShotsFired++;
	}

	private void PlayDetonationAudio()
	{
		AudioSource.PlayOneShot(audioFire);
		if (interiorMode != null)
		{
			interiorMode.ScheduleMarksmanDetonation(this);
		}
	}

	private void SpawnProjectile()
	{
		Projectile projectile = Object.Instantiate(Ammunition.projectile);
		int id = (projectile.SwarmId = Time.frameCount);
		projectile.Id = id;
		projectile.name = Ammunition.projectile.name;
		projectile.transform.SetParent(projectilesContainer, false);
		projectile.transform.position = boltActionVisualization.ProjectilePosition();
		projectile.transform.LookAt(base.transform.position + base.transform.forward * ScopeZeroRange);
		projectile.InitializeMarksman(Ammunition, riflingTwistRate, AudioInstigator);
		projectile.ForceMultiplier = projectileForceMultiplier * 0.015f;
		projectile.onInteraction.AddListener(OnProjectileInteraction);
		projectile.MarksmanOwner = this;
		CurrentProjectile = projectile;
		Global.Context.Mission.TimeScaler.BulletTime(Global.Context.Settings.Time.marksmanFire);
	}

	private void AutoMarksmanCheatResetMechanism()
	{
		bool firingPinReady = (ChamberLive = true);
		FiringPinReady = firingPinReady;
	}

	private void OnProjectileInteraction(Projectile projectile)
	{
		CurrentProjectile = null;
	}

	public void OnSuccessfulAttack(CharacterBody victim)
	{
		LastVictim = victim;
		ShotsHit++;
	}

	public void StartBoltCycle()
	{
		AudioSource.PlayOneShot(audioRotateOpenBegin);
		Coroutine.DelayedAction(boltRotationTime, BoltRotatedOpen, this);
	}

	private void BoltRotatedOpen()
	{
		AudioSource.PlayOneShot(audioRotateOpenStop);
		Coroutine.DelayedAction(0.2f, StartExtractBolt, this);
	}

	public void StartExtractBolt()
	{
		AudioSource.PlayOneShot((!MagazineEmpty) ? audioTranslateOpen : audioTranslateOpenEmpty);
		Coroutine.DelayedAction(boltTranslationTime, BoltTranslateOpened, this);
	}

	private void BoltTranslateOpened()
	{
		AudioSource.PlayOneShot(audioExtractCasing);
		Coroutine.DelayedAction(0.25f, StartReturnBolt, this);
	}

	public void StartReturnBolt()
	{
		AudioSource.PlayOneShot((!MagazineEmpty) ? audioTranslateClosedBegin : audioTranslateClosedBeginEmpty);
		Coroutine.DelayedAction(boltTranslationTime, BoltTranslateClosed, this);
	}

	private void BoltTranslateClosed()
	{
		AudioSource.PlayOneShot(audioTranslateClosedStop);
		Coroutine.DelayedAction(0.2f, StartRotateBoltClosed, this);
	}

	public void StartRotateBoltClosed()
	{
		Coroutine.DelayedAction(boltRotationTime, BoltRotationClosed, this);
	}

	private void BoltRotationClosed()
	{
		AudioSource.PlayOneShot(audioRotateClosed);
		if (!MagazineEmpty)
		{
			ChamberLive = true;
			RoundsInMagazine--;
		}
	}

	public void StartChangeMagazine()
	{
		RoundsInMagazine = 0;
		boltActionVisualization.ClearMagazine();
		AudioSource.PlayOneShot(audioRemoveMagazine);
		Coroutine.DelayedAction(reloadTime, ChangeMagazine, this);
	}

	public void TutorialReplaceAmmunition(AmmunitionBase ammunition, bool replaceMagazine)
	{
		Ammunition = ammunition;
		boltActionVisualization.roundPrefab = ammunition.marksmanRound;
		if (replaceMagazine)
		{
			TutorialFillMagazine();
		}
	}

	public void TutorialFillMagazine()
	{
		boltActionVisualization.ClearAndReplaceMagazine(Ammunition.marksmanRound);
		RoundsInMagazine = magazineCapacity;
	}

	private void FeedRoundIntoChamber()
	{
		ChamberLive = true;
		RoundsInMagazine--;
	}

	private void ChangeMagazine()
	{
		AudioSource.PlayOneShot(audioInsertMagazine);
		if (Ammunition != null)
		{
			RoundsInMagazine = magazineCapacity;
			boltActionVisualization.ReplaceMagazine();
			boltActionVisualization.Render();
		}
		computerReload = ComputerReload.Ready;
	}

	private void PlayInhale()
	{
		breathingAudioSource.Stop();
		breathingAudioSource.PlayOneShot(inhales[Random.Range(0, inhales.Count)]);
	}

	private void PlayExhale()
	{
		breathingAudioSource.Stop();
		breathingAudioSource.PlayOneShot(exhales[Random.Range(0, exhales.Count)]);
	}
}

}
