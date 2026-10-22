namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;

public class CyclingBolt : MonoBehaviour
{
	public enum State
	{
		Closed = 0,
		Open = 1,
		Closing = 2,
		Opening = 3
	}

	[Tooltip("Reference to the chamber (on the weapon barrel). This holds casings until they are ejected.")]
	public Transform chamber;

	[Tooltip("How far the bolt translates away from its closed position when cycling. In meters.")]
	public float travelRange = 0.08f;

	[Tooltip("The position of the opening sequence where casings are ejected from the bolt. In normalized range where 1 is fully open.")]
	public float ejectPosition = 1f;

	[Tooltip("The position of the closing sequence where rounds are fed by the bolt. In normalized range where 1 is fully closed.")]
	public float feedPosition;

	[Tooltip("Describes the bolt's offset from its closed position through one cycle. Y is offset in meters and X is normalized position of the cycle.")]
	public AnimationCurve cycle;

	[Tooltip("Should the bolt/slide lock in open position when the magazine is empty?")]
	public bool lockOpen = true;

	[Tooltip("Should the bolt/slide lock in open position when manipulated manually?")]
	public bool manualLockOpen = true;

	[Tooltip("This transform will be moved when the bolt moves manually.")]
	public Transform chargingHandle;

	[Header("Ejection")]
	[Tooltip("Should casing be emitted on bolt cycling?")]
	public bool emitCasings = true;

	[Tooltip("Force applied to ejected casings. In the weapon's local space.")]
	public Vector3 ejectionForce = new Vector3(-1.5f, 0.75f, -1.5f);

	[Tooltip("Random scaling applied to ejected force. Each component is centered around the respective value of the force above.")]
	public Vector3 forceRandomness = new Vector3(1f, 0.5f, 1f);

	[Tooltip("Torque applied on ejected casings. In the weapon's local space.")]
	public Vector3 ejectionTorque = new Vector3(0f, 0.01f, 0.01f);

	[Tooltip("Random scaling applied to ejection torque.")]
	public Vector3 torqueRandomness = new Vector3(0f, 0.01f, 0.01f);

	private bool manualChargingCycle;

	private bool isCarrying;

	private bool hasFed;

	private float feedMalfunctionPosition;

	private float cycleTimestamp = float.NegativeInfinity;

	private float manualCycleTimestamp = float.NegativeInfinity;

	private float calculatedEjectPosition;

	private float calculatedFeedPosition;

	private float openKeyTime;

	private float closedKeyTime;

	private float malfunctionRate;

	private Vector3 closedPosition;

	private Vector3 chargingHandleClosedPosition;

	private WeaponBase weapon;

	private PropagatingAudioSample audioBoltReturn;

	private PropagatingAudioSample audioBoltExtract;

	private UnityEvent onOpened = new UnityEvent();

	private UnityEventBool onClosed = new UnityEventBool();

	public State Status { get; private set; }

	public float CyclePeriod { get; private set; }

	public float BoltOpeningPeriod { get; private set; }

	public float BoltClosingPeriod { get; private set; }

	public bool LockEngaged
	{
		get
		{
			return float.IsPositiveInfinity(cycleTimestamp);
		}
	}

	public bool IsMoving { get; private set; }

	public bool IsOpening
	{
		get
		{
			return Status == State.Opening;
		}
	}

	public bool IsClosing
	{
		get
		{
			return Status == State.Closing;
		}
	}

	public bool IsOpen
	{
		get
		{
			return Status == State.Open;
		}
	}

	public bool IsClosed
	{
		get
		{
			return Status == State.Closed;
		}
	}

	public bool ManualCycle { get; private set; }

	public bool Malfunctioned
	{
		get
		{
			return feedMalfunctionPosition > 0f;
		}
	}

	public bool Live
	{
		get
		{
			return Status == State.Closed && CarryingLiveRound;
		}
	}

	public bool StaticAndNotLive
	{
		get
		{
			return !IsMoving && !Live;
		}
	}

	public bool CarryingLiveRound
	{
		get
		{
			return Round != null && Round.Live;
		}
	}

	public bool UseMalfunctions { get; set; }

	public Casing Round { get; private set; }

	private void Awake()
	{
		IsMoving = false;
		Status = State.Closed;
		closedPosition = base.transform.localPosition;
		chargingHandleClosedPosition = ((!(chargingHandle != null)) ? Vector3.zero : chargingHandle.localPosition);
		openKeyTime = ((cycle.keys.Length < 2) ? 0f : cycle.keys[1].time);
		closedKeyTime = ((cycle.keys.Length < 3) ? 0f : cycle.keys[cycle.keys.Length - 2].time);
		calculatedEjectPosition = openKeyTime * ejectPosition;
		calculatedFeedPosition = (closedKeyTime - openKeyTime) * feedPosition;
	}

	public void Initialize(WeaponBase weapon, UnityAction openedCallback, UnityAction<bool> closedCallback, float feedMalfunctionRate)
	{
		this.weapon = weapon;
		CyclePeriod = 60f / weapon.roundsPerMinute;
		BoltOpeningPeriod = CyclePeriod * openKeyTime;
		BoltClosingPeriod = CyclePeriod * (1f - closedKeyTime);
		malfunctionRate = feedMalfunctionRate;
		onOpened.AddListener(openedCallback);
		onClosed.AddListener(closedCallback);
		audioBoltReturn = weapon.audioBoltReturn;
		audioBoltExtract = weapon.audioBoltExtract;
		FeedRound(false, true);
	}

	private void Update()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (manualChargingCycle)
		{
			float num4 = Time.time - manualCycleTimestamp;
			float num5 = 0.1f;
			float num6 = 0.1f;
			num = num4 / (num5 + num6);
			num2 = (num3 = Mathf.Clamp01((!IsOpening) ? (1f - (num4 - num5) / num6) : (num4 / num5)));
			if (chargingHandle != null)
			{
				float num7 = Mathf.Clamp01((!(num4 < num5)) ? (1f - (num4 - num5) / num6) : (num4 / num5));
				chargingHandle.localPosition = chargingHandleClosedPosition - new Vector3(num7 * travelRange, 0f, 0f);
			}
			if (!IsMoving)
			{
				return;
			}
		}
		else
		{
			if (!IsMoving)
			{
				return;
			}
			float num8 = Time.time - cycleTimestamp;
			num = Mathf.Lerp(0f, 1f, num8 / CyclePeriod);
			num3 = cycle.Evaluate(num);
			num2 = ((!IsOpening) ? (1f - (num8 - BoltOpeningPeriod) / BoltClosingPeriod) : (num8 / BoltOpeningPeriod));
			if (feedMalfunctionPosition > 0f && num >= feedMalfunctionPosition)
			{
				num3 = cycle.Evaluate(feedMalfunctionPosition);
				IsMoving = false;
			}
		}
		base.transform.localPosition = closedPosition - new Vector3(num3 * travelRange, 0f, 0f);
		weapon.barrel.Animate(IsOpening, Mathf.Abs(closedPosition.x - base.transform.localPosition.x), num2, num);
		if (IsOpening)
		{
			if (emitCasings && num >= calculatedEjectPosition)
			{
				EjectCasing((!ManualCycle) ? 1f : 0.25f);
			}
			if (num > openKeyTime)
			{
				OnOpened();
			}
		}
		else if (IsClosing)
		{
			if (!isCarrying && !hasFed && num >= calculatedFeedPosition)
			{
				FeedRound(true, ManualCycle);
				hasFed = true;
			}
			if (num >= 1f)
			{
				OnClosed();
			}
		}
		if (!weapon.fireFromMagazine && IsOpening && weapon.hammer != null)
		{
			weapon.hammer.SetLevel(num3);
		}
	}

	public void Open(bool manual = false)
	{
		if (Status != State.Open && Status != State.Opening)
		{
			IsMoving = true;
			bool flag = (ManualCycle = manual);
			manualChargingCycle = flag;
			Status = State.Opening;
			cycleTimestamp = Time.time;
			if (manual)
			{
				manualCycleTimestamp = Time.time;
			}
			audioBoltExtract.Play(base.transform.position.x, (!(weapon.Owner != null)) ? null : weapon.Owner.AudioInstigator);
		}
	}

	public void Close(bool manual = false)
	{
		if (Status != State.Closed && Status != State.Closing)
		{
			if (!manual && UseMalfunctions && Random.value <= malfunctionRate * weapon.MalfunctionsMultiplier)
			{
				feedMalfunctionPosition = Random.Range(calculatedFeedPosition, 1f);
			}
			IsMoving = true;
			ManualCycle = manual;
			Status = State.Closing;
			cycleTimestamp = Time.time - BoltOpeningPeriod;
			audioBoltReturn.Play(base.transform.position.x, (!(weapon.Owner != null)) ? null : weapon.Owner.AudioInstigator);
		}
	}

	public void LockOpenOnEmptyMagazine()
	{
		if (lockOpen)
		{
			cycleTimestamp = float.PositiveInfinity;
		}
	}

	private void OnOpened()
	{
		if (Status == State.Open)
		{
			return;
		}
		hasFed = false;
		feedMalfunctionPosition = 0f;
		base.transform.localPosition = closedPosition - new Vector3(travelRange, 0f, 0f);
		weapon.barrel.Animate(true, 1f, 1f, openKeyTime / CyclePeriod);
		if (!weapon.fireFromMagazine)
		{
			onOpened.Invoke();
		}
		if ((lockOpen || (ManualCycle && manualLockOpen)) && (LockEngaged || (ManualCycle && (weapon.Magazine == null || weapon.Magazine.Empty))))
		{
			IsMoving = false;
			ManualCycle = false;
			if (chargingHandle == null)
			{
				manualChargingCycle = false;
			}
			Status = State.Open;
		}
		else
		{
			Close(ManualCycle);
		}
	}

	private void OnClosed()
	{
		if (Status != State.Closed)
		{
			IsMoving = false;
			ManualCycle = (manualChargingCycle = false);
			Status = State.Closed;
			cycleTimestamp = float.NegativeInfinity;
			manualCycleTimestamp = float.NegativeInfinity;
			base.transform.localPosition = closedPosition;
			weapon.barrel.SetClosed();
			onClosed.Invoke(isCarrying);
		}
	}

	public void DetonateRound()
	{
		if (!(Round == null))
		{
			Round.Detonate();
			if (weapon.fireFromMagazine)
			{
				isCarrying = false;
			}
		}
	}

	public void EjectCasing(float forceScale = 1f, int index = -1)
	{
		isCarrying = false;
		if (chamber.childCount != 0)
		{
			Vector3 vector = new Vector3(Random.Range(ejectionForce.x - forceRandomness.x * 0.5f, ejectionForce.x + forceRandomness.x), Random.Range(ejectionForce.y - forceRandomness.y * 0.5f, ejectionForce.y + forceRandomness.y), Random.Range(ejectionForce.z - forceRandomness.z * 0.5f, ejectionForce.z + forceRandomness.z));
			Vector3 vector2 = new Vector3(Random.Range(ejectionTorque.x - torqueRandomness.x * 0.5f, ejectionTorque.x + torqueRandomness.x), Random.Range(ejectionTorque.y - torqueRandomness.y * 0.5f, ejectionTorque.y + torqueRandomness.y), Random.Range(ejectionTorque.z - torqueRandomness.z * 0.5f, ejectionTorque.z + torqueRandomness.z));
			Vector3 vector3 = ((!weapon.Owner) ? Vector3.zero : weapon.Owner.GetComponent<Rigidbody>().velocity);
			Casing casing = ((index != -1) ? chamber.GetChild(index).GetComponent<Casing>() : Round);
			casing.Force = vector3 + base.transform.TransformVector(vector) * forceScale;
			casing.Torque = base.transform.TransformVector(vector2) * 0.001f;
			casing.transform.SetParent(weapon.CasingsContainer);
			casing.transform.localScale = Vector3.one;
			casing.Eject();
			if (index == -1)
			{
				Round = null;
			}
		}
	}

	public void FeedRound(bool fromMagazine, bool disableMalfunctions)
	{
		if (!(weapon.ammunition == null))
		{
			Casing casing = ((!fromMagazine) ? weapon.ammunition.casing : weapon.GetNextRoundFromMagazine(disableMalfunctions));
			if (casing != null)
			{
				isCarrying = fromMagazine;
				Round = Object.Instantiate(casing);
				Round.transform.SetParent(chamber, false);
				Round.transform.localPosition = new Vector3(Round.Offset, 0f, 0f);
			}
		}
	}

	public void FeedRoundIntoOpen()
	{
		if (!(weapon.ammunition.casing == null))
		{
			isCarrying = true;
			Round = Object.Instantiate(weapon.ammunition.casing);
			Round.transform.SetParent(chamber, false);
			Round.transform.localPosition = new Vector3(Round.Offset, 0f, 0f);
			chamber.transform.localPosition = new Vector3(0f - travelRange, 0f, 0f);
		}
	}
}

}
