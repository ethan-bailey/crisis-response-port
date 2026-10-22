using UnityEngine;

public class DoorController : Interactable
{
	public enum Lockable
	{
		NotLockable = 0,
		FromLeft = 1,
		FromRight = 2
	}

	public enum DoorTrapState
	{
		Unknown = 0,
		NoTrap = 1,
		HasTrap = 2
	}

	public const float Width = 0.9f;

	public const float AudioReduction = 12f;

	public Transform leftKnob;

	public Transform rightKnob;

	public Transform hinge;

	[Tooltip("If set the door can't be interacted with.")]
	public bool inoperable;

	public DoorOpenState openState;

	public DoorLockState lockState;

	public DoorMechanismState mechanismState;

	public DoorLockState KnownLockState;

	[Tooltip("Reference to the object holding the door mesh and the door obstacle collider. Used in collision events.")]
	public GameObject physicalDoor;

	[Tooltip("Angle at which the door is fully open.")]
	[Range(0f, 200f)]
	public float openAngle = 180f;

	[Tooltip("Rotation speed in degrees per second.")]
	public float rotationSpeed = 120f;

	[Header("Locking")]
	public Lockable lockingMode;

	[Header("Randomization")]
	[Tooltip("In percent how likely the door is to be open on mission start.")]
	[Range(0f, 100f)]
	public float openProbability;

	[Tooltip("In percent how likely the door is to be locked on mission start.")]
	[Range(0f, 100f)]
	public float lockedProbability;

	[Header("Booby trap")]
	public DoorTrap trapPrefab;

	[Range(0f, 1f)]
	public float trapProbability;

	[Header("Audio")]
	public PropagatingAudioSample audioOpen = new PropagatingAudioSample(12f, AudioStimuliType.DoorInteraction);

	public PropagatingAudioSample audioOpenWhenLocked = new PropagatingAudioSample(24f, AudioStimuliType.DoorInteraction);

	public PropagatingAudioSample audioClose = new PropagatingAudioSample(30f, AudioStimuliType.DoorInteraction);

	public PropagatingAudioSample audioGradualOpen = new PropagatingAudioSample(3f, AudioStimuliType.DoorInteraction);

	public PropagatingAudioSample audioGradualMove = new PropagatingAudioSample(2f, AudioStimuliType.DoorInteraction);

	public PropagatingAudioSample audioGradualClose = new PropagatingAudioSample(3f, AudioStimuliType.DoorInteraction);

	public PropagatingAudioSample audioLock = new PropagatingAudioSample(18f, AudioStimuliType.DoorInteraction);

	public PropagatingAudioSample audioUnlock = new PropagatingAudioSample(18f, AudioStimuliType.DoorInteraction);

	[Header("Fog of war")]
	public Transform leftFog;

	public Transform rightFog;

	private float leftFogHome;

	private float rightFogHome;

	private float lockPeriod = 0.6f;

	private float lockTime;

	private float partialAngle;

	private bool triggeredBulletTime;

	private bool locksDisabledBySetting;

	private bool forcedManipulation;

	private const float gradualManipulationAudioIntensity = 0.1f;

	private TimeScaler timeScaler;

	private AudioPropagator audioPropagator;

	private SerializableTimeSettings timeSettings;

	public AudioSource AudioSource { get; private set; }

	public float Angle
	{
		get
		{
			return hinge.transform.localRotation.eulerAngles.y;
		}
	}

	public bool IsMoving
	{
		get
		{
			return openState == DoorOpenState.Opening || openState == DoorOpenState.Closing || openState == DoorOpenState.PartialMoving;
		}
	}

	public float OpeningDirection
	{
		get
		{
			return (base.transform.localRotation.eulerAngles.y != 0f) ? 1f : (-1f);
		}
	}

	public float Position
	{
		get
		{
			return base.transform.position.x;
		}
	}

	public Vector3 HingePosition
	{
		get
		{
			return hinge.position;
		}
	}

	public float OpenClearanceRight
	{
		get
		{
			return (OpeningDirection != 1f) ? 0f : 0.9f;
		}
	}

	public float OpenClearanceLeft
	{
		get
		{
			return (OpeningDirection != 1f) ? 0.9f : 0f;
		}
	}

	public float LastInteractionTimestamp { get; private set; }

	public Room LeftRoom { get; private set; }

	public Room RightRoom { get; private set; }

	public Collider Collider { get; private set; }

	public bool IsRigged
	{
		get
		{
			return Trap != null && TrapState == DoorTrapState.HasTrap;
		}
	}

	public DoorTrap Trap { get; private set; }

	public DoorTrapState TrapState { get; set; }

	private void Awake()
	{
		audioPropagator = AudioPropagator.Instance;
		timeSettings = Global.Context.Settings.Time;
		AudioSource = GetComponent<AudioSource>();
		KnownLockState = DoorLockState.Unknown;
		Collider = physicalDoor.GetComponent<Collider>();
	}

	private void Start()
	{
		InitializeAudio();
		SetStartingState();
		GlobalContext context = Global.Context;
		context.Mission.Doors.Register(this);
		timeScaler = context.Mission.TimeScaler;
		if (openState == DoorOpenState.Closed && !Global.Context.Mission.TutorialMode.Active)
		{
			InitializeTrap();
		}
		leftFogHome = ((!(leftFog != null)) ? 0f : leftFog.position.x);
		rightFogHome = ((!(rightFog != null)) ? 0f : rightFog.position.x);
	}

	private void SetStartingState()
	{
		if (!inoperable && !Global.Context.Mission.TutorialMode.Active)
		{
			if (Random.value * 100f < openProbability)
			{
				SetOpen();
			}
			else if (lockingMode != Lockable.NotLockable && !Global.Context.Settings.Extras.disableLocks && Random.value * 100f < lockedProbability)
			{
				lockState = DoorLockState.Locked;
			}
		}
	}

	public void SetLockDisabledState(bool disable)
	{
		locksDisabledBySetting = disable;
		if (locksDisabledBySetting)
		{
			lockState = DoorLockState.Unlocked;
		}
	}

	public void SetLeftRoom(Room room)
	{
		LeftRoom = room;
	}

	public void SetRightRoom(Room room)
	{
		RightRoom = room;
	}

	private void InitializeAudio()
	{
		audioOpen.Initialize(AudioSource);
		audioOpenWhenLocked.Initialize(AudioSource);
		audioClose.Initialize(AudioSource);
		audioGradualOpen.Initialize(AudioSource);
		audioGradualMove.Initialize(AudioSource);
		audioGradualClose.Initialize(AudioSource);
		audioLock.Initialize(AudioSource);
		audioUnlock.Initialize(AudioSource);
	}

	private void InitializeTrap()
	{
		if (!(trapPrefab == null) && !(Random.value > Global.Context.Settings.Location.riggedDoorsProbability))
		{
			Trap = Object.Instantiate(trapPrefab);
			Trap.Initialize(this);
		}
	}

	private void Update()
	{
		if (inoperable)
		{
			return;
		}
		if (interactor != null && (interactor.IsRagdoll || !InteractorInRange(interactor)))
		{
			interactor = null;
		}
		UpdateInteractorPresence();
		if (openState == DoorOpenState.Opening)
		{
			hinge.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
			partialAngle = Angle;
			if (!triggeredBulletTime && hinge.localEulerAngles.y >= SerializableTimeSettings.DoorTriggerAngle)
			{
				TriggerBulletTime();
			}
			if (hinge.localEulerAngles.y >= openAngle)
			{
				OnOpened();
			}
		}
		else if (openState == DoorOpenState.Closing)
		{
			hinge.Rotate(0f, rotationSpeed * (0f - Time.deltaTime), 0f);
			partialAngle = Angle;
			if (triggeredBulletTime && hinge.localEulerAngles.y < SerializableTimeSettings.DoorTriggerAngle)
			{
				triggeredBulletTime = false;
			}
			if (hinge.localEulerAngles.y >= 300f)
			{
				AudioInstigator instigator = ((!(interactor != null)) ? null : interactor.AudioInstigator);
				audioClose.Play(base.transform.position.x, instigator);
				OnClosed();
			}
		}
		else if (openState == DoorOpenState.PartialMoving)
		{
			float num = partialAngle - Angle;
			float num2 = Mathf.Sign(num) * rotationSpeed * Time.deltaTime;
			if (interactor != null)
			{
				interactor.Move(OpeningDirection * Mathf.Sign(num));
			}
			if (Mathf.Abs(num) < num2)
			{
				num2 = num;
				openState = DoorOpenState.Partial;
			}
			float num3 = Mathf.Clamp(Angle + num2, 0f, openAngle);
			if (num3 != Angle)
			{
				hinge.localRotation = Quaternion.Euler(0f, num3, 0f);
				if (num3 >= openAngle)
				{
					OnOpened();
				}
				else if (num3 <= 0f)
				{
					audioGradualClose.Play(base.transform.position.x, interactor.AudioInstigator);
					OnClosed();
				}
			}
			if (!triggeredBulletTime && hinge.localEulerAngles.y >= SerializableTimeSettings.DoorTriggerAngle)
			{
				TriggerBulletTime();
			}
			else if (triggeredBulletTime && hinge.localEulerAngles.y < SerializableTimeSettings.DoorTriggerAngle)
			{
				triggeredBulletTime = false;
			}
		}
		if (lockState == DoorLockState.Unlocking)
		{
			lockTime += Time.deltaTime;
			if (lockTime >= lockPeriod)
			{
				if (lockState == KnownLockState)
				{
					KnownLockState = DoorLockState.Unlocked;
				}
				lockState = DoorLockState.Unlocked;
			}
		}
		else
		{
			if (lockState != DoorLockState.Locking)
			{
				return;
			}
			lockTime += Time.deltaTime;
			if (lockTime >= lockPeriod)
			{
				if (lockState == KnownLockState)
				{
					KnownLockState = DoorLockState.Locked;
				}
				lockState = DoorLockState.Locked;
			}
		}
	}

	private void UpdateInteractorPresence()
	{
		if (!(interactor == null))
		{
			float num = Time.time - LastInteractionTimestamp;
			if (num >= 3f || interactor.IsRagdoll || !InteractorInRange(interactor))
			{
				interactor = null;
			}
		}
	}

	private void SetInteractor(CharacterBody body)
	{
		interactor = body;
		LastInteractionTimestamp = Time.time;
	}

	public bool Toggle(CharacterBody interactor)
	{
		if (inoperable)
		{
			return false;
		}
		SetInteractor(interactor);
		if (openState == DoorOpenState.Closed || openState == DoorOpenState.Closing)
		{
			return Open(interactor);
		}
		if (openState == DoorOpenState.Open || openState == DoorOpenState.Opening)
		{
			return Close(interactor);
		}
		return false;
	}

	public void SetOpen()
	{
		hinge.localEulerAngles = new Vector3(0f, openAngle, 0f);
		openState = DoorOpenState.Open;
		KnownLockState = DoorLockState.Unlocked;
		TrapState = DoorTrapState.NoTrap;
	}

	public void SetClosed()
	{
		hinge.localEulerAngles = new Vector3(0f, 0f, 0f);
		openState = DoorOpenState.Closed;
	}

	public bool Open(CharacterBody interactor, bool updateKnownLockState = false)
	{
		if (inoperable)
		{
			return false;
		}
		SetInteractor(interactor);
		if (lockState != DoorLockState.Unlocked)
		{
			audioOpenWhenLocked.Play(base.transform.position.x, interactor.AudioInstigator);
			if (updateKnownLockState)
			{
				KnownLockState = lockState;
			}
			return false;
		}
		if (openState == DoorOpenState.Open || openState == DoorOpenState.Opening)
		{
			return true;
		}
		if (openState == DoorOpenState.Closed)
		{
			audioOpen.Play(base.transform.position.x, interactor.AudioInstigator);
		}
		if (updateKnownLockState)
		{
			KnownLockState = lockState;
		}
		openState = DoorOpenState.Opening;
		return true;
	}

	public bool Close(CharacterBody interactor, bool updateKnownLockState = false)
	{
		if (inoperable)
		{
			return false;
		}
		SetInteractor(interactor);
		if (openState == DoorOpenState.Closed || openState == DoorOpenState.Closing)
		{
			return true;
		}
		lockState = DoorLockState.Unlocked;
		if (updateKnownLockState)
		{
			KnownLockState = lockState;
		}
		openState = DoorOpenState.Closing;
		return true;
	}

	public void ForceOpen()
	{
		if (openState != DoorOpenState.Open)
		{
			forcedManipulation = true;
			openState = DoorOpenState.Opening;
		}
	}

	public void ForceClose()
	{
		if (openState != DoorOpenState.Closed)
		{
			forcedManipulation = true;
			openState = DoorOpenState.Closing;
		}
	}

	public bool MovePartial(CharacterBody interactor, float degrees, bool updateKnownLockState = false)
	{
		if (inoperable)
		{
			return false;
		}
		SetInteractor(interactor);
		if (updateKnownLockState)
		{
			KnownLockState = lockState;
		}
		if (lockState == DoorLockState.Locked || lockState == DoorLockState.Locking)
		{
			audioOpenWhenLocked.PlayCustom(base.transform.position.x, interactor.AudioInstigator, 0.1f, 0.1f);
			return false;
		}
		float num = Mathf.Clamp(partialAngle + degrees, 0f, openAngle);
		if (num == Angle)
		{
			return true;
		}
		if (openState == DoorOpenState.Closed)
		{
			audioGradualOpen.Play(base.transform.position.x, interactor.AudioInstigator);
		}
		else
		{
			audioGradualMove.Play(base.transform.position.x, interactor.AudioInstigator);
		}
		partialAngle = num;
		openState = DoorOpenState.PartialMoving;
		return true;
	}

	public bool CanOperateLock(CharacterBody body)
	{
		float fromWorldSide = Mathf.Sign(body.Position - Position);
		return CanOperateLock(fromWorldSide, body.Blackboard.HasMasterKey);
	}

	public bool IsBusy()
	{
		return interactor != null;
	}

	public bool CanOperateLock(float fromWorldSide, bool hasKey)
	{
		if (locksDisabledBySetting || lockingMode == Lockable.NotLockable)
		{
			return false;
		}
		if (hasKey)
		{
			return true;
		}
		if ((fromWorldSide == -1f && lockingMode == Lockable.FromLeft) || (fromWorldSide == 1f && lockingMode == Lockable.FromRight))
		{
			return true;
		}
		return false;
	}

	public bool ToggleLock(CharacterBody interactor, bool updateKnownLockState = false)
	{
		if (inoperable)
		{
			return false;
		}
		if (lockState == DoorLockState.Unlocked || lockState == DoorLockState.Unlocking)
		{
			return Lock(interactor);
		}
		return Unlock(interactor);
	}

	public bool Unlock(CharacterBody interactor, bool updateKnownLockState = false)
	{
		if (inoperable)
		{
			return false;
		}
		if (lockState == DoorLockState.Locking)
		{
			return false;
		}
		if (lockState == DoorLockState.Unlocked || lockState == DoorLockState.Unlocking)
		{
			return true;
		}
		lockTime = 0f;
		SetInteractor(interactor);
		audioUnlock.Play(base.transform.position.x, interactor.AudioInstigator);
		lockState = DoorLockState.Unlocking;
		if (updateKnownLockState)
		{
			KnownLockState = lockState;
		}
		return true;
	}

	public bool Lock(CharacterBody interactor, bool updateKnownLockState = false)
	{
		if (inoperable)
		{
			return false;
		}
		if (openState != DoorOpenState.Closed)
		{
			return false;
		}
		if (lockState == DoorLockState.Unlocking)
		{
			return false;
		}
		if (lockState == DoorLockState.Locked || lockState == DoorLockState.Locking)
		{
			return true;
		}
		lockTime = 0f;
		SetInteractor(interactor);
		audioLock.Play(base.transform.position.x, interactor.AudioInstigator);
		lockState = DoorLockState.Locking;
		if (updateKnownLockState)
		{
			KnownLockState = lockState;
		}
		return true;
	}

	public void Examine()
	{
		KnownLockState = lockState;
		TrapState = ((!(Trap != null) || !Trap.Armed) ? DoorTrapState.NoTrap : DoorTrapState.HasTrap);
	}

	public string ExamineAndReturnStatusString()
	{
		Examine();
		string result = string.Empty;
		if (inoperable)
		{
			result = "inoperable";
		}
		else if (Trap != null && Trap.Armed)
		{
			result = ((openState != DoorOpenState.Closed || KnownLockState != DoorLockState.Locked) ? "rigged" : "rigged and locked");
		}
		else if (openState == DoorOpenState.Closed && KnownLockState != DoorLockState.Unknown)
		{
			result = ((KnownLockState != DoorLockState.Locked) ? "unlocked" : "locked");
		}
		else
		{
			switch (openState)
			{
			case DoorOpenState.Closed:
				result = "closed";
				break;
			case DoorOpenState.Closing:
				result = "closing";
				break;
			case DoorOpenState.Open:
				result = "open";
				break;
			case DoorOpenState.Opening:
				result = "opening";
				break;
			case DoorOpenState.Partial:
				result = "ajar";
				break;
			case DoorOpenState.PartialMoving:
				result = "moving";
				break;
			}
		}
		return result;
	}

	private void OnClosed()
	{
		hinge.localEulerAngles = new Vector3(0f, 0f, 0f);
		partialAngle = Angle;
		openState = DoorOpenState.Closed;
		lockState = DoorLockState.Unlocked;
		forcedManipulation = false;
	}

	private void OnOpened()
	{
		hinge.localEulerAngles = new Vector3(0f, openAngle, 0f);
		partialAngle = Angle;
		openState = DoorOpenState.Open;
		forcedManipulation = false;
	}

	public bool InteractorInRange(CharacterBody body)
	{
		float sqrMagnitude = (body.Head.transform.position - leftKnob.position).sqrMagnitude;
		float sqrMagnitude2 = (body.Head.transform.position - rightKnob.position).sqrMagnitude;
		Transform transform = ((!(sqrMagnitude < sqrMagnitude2)) ? rightKnob : leftKnob);
		float num = Mathf.Abs(transform.position.x - body.Head.transform.position.x);
		return num < 1f;
	}

	private void TriggerBulletTime()
	{
		if (interactor != null && interactor.IsPlayer)
		{
			timeScaler.BulletTime(timeSettings.playerOpenDoor);
		}
		triggeredBulletTime = true;
	}

	public Room RoomOppositeFrom(float position)
	{
		float num = Mathf.Sign(base.transform.position.x - position);
		return (!(num > 0f)) ? LeftRoom : RightRoom;
	}

	public Transform Knob(float interactorPosition)
	{
		return (!(Mathf.Sign(interactorPosition - base.transform.position.x) > 0f)) ? leftKnob : rightKnob;
	}

	public Vector3 GrenadeThrowPosition(float directionToDoor, float height)
	{
		float openingDirection = OpeningDirection;
		if (directionToDoor == -1f)
		{
			return physicalDoor.transform.TransformPoint(new Vector3(directionToDoor * -0.175f, height, -0.99f));
		}
		Vector3 position = physicalDoor.transform.position;
		return new Vector3(position.x + directionToDoor * openingDirection * 0.15f, height, position.z + 0.9f * openingDirection * 0.85f);
	}

	public Vector3 GrenadeThrowDirection(float directionToDoor)
	{
		if (directionToDoor == 1f)
		{
			if (OpeningDirection == 1f)
			{
				return hinge.TransformVector(Vector3.forward * -1f);
			}
			return hinge.TransformVector(Vector3.forward);
		}
		if (OpeningDirection == 1f)
		{
			return hinge.TransformVector(Vector3.forward);
		}
		return hinge.TransformVector(Vector3.forward * directionToDoor);
	}

	public void TriggerTrapCue(AudioClip cue)
	{
		AudioSource.PlayOneShot(cue);
		TrapState = DoorTrapState.HasTrap;
	}

	public void OnTrapDisarmed(AudioClip cue)
	{
		TrapState = DoorTrapState.NoTrap;
		AudioSource.loop = false;
		AudioSource.PlayOneShot(cue);
	}
}
