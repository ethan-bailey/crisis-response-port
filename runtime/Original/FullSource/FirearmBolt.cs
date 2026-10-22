namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class FirearmBolt : FirearmFunction
{
	public delegate bool LockOpenDelegate();

	public enum State
	{
		Closed = 0,
		Opening = 1,
		Open = 2,
		Closing = 3
	}

	public float openTime = 0.5f;

	public float closeTime = 1f;

	public float range = 0.05f;

	public UnityEventVoid onOpened = new UnityEventVoid();

	public UnityEventVoid onClosed = new UnityEventVoid();

	private LockOpenDelegate lockOpen;

	private Vector3 homePosition;

	private float lockedPosition;

	private float openTimestamp;

	private float closeTimestamp;

	public State CurrentState { get; private set; }

	public bool ManualOperation { get; private set; }

	private void Start()
	{
		homePosition = base.transform.localPosition;
		lockedPosition = base.transform.localPosition.x;
	}

	private void Update()
	{
		if (CurrentState == State.Opening)
		{
			float num = (Time.time - openTimestamp) / openTime;
			if (num >= 1f)
			{
				SetOffset(0f - range);
				OnOpened(ManualOperation);
			}
			else
			{
				SetOffset(Mathf.Lerp(0f, 0f - range, num));
			}
		}
		else if (CurrentState == State.Closing)
		{
			float num2 = (Time.time - closeTimestamp) / closeTime;
			if (num2 >= 1f)
			{
				SetOffset(0f);
				OnClosed();
			}
			else
			{
				SetOffset(Mathf.Lerp(0f - range, 0f, num2));
			}
		}
	}

	public void Initialize(LockOpenDelegate lockOpenDelegate)
	{
		lockOpen = lockOpenDelegate;
	}

	public void Open(bool manual)
	{
		if (CurrentState != State.Open && CurrentState != State.Opening)
		{
			ManualOperation = manual;
			openTimestamp = Time.time;
			CurrentState = State.Opening;
		}
	}

	public void Close(bool manual)
	{
		if (CurrentState != State.Closed && CurrentState != State.Closing)
		{
			ManualOperation = manual;
			closeTimestamp = Time.time;
			CurrentState = State.Closing;
		}
	}

	private void OnOpened(bool manual)
	{
		CurrentState = State.Open;
		onOpened.Invoke();
		if (lockOpen == null || !lockOpen())
		{
			Close(manual);
		}
	}

	private void OnClosed()
	{
		CurrentState = State.Closed;
		onClosed.Invoke();
	}

	private void SetOffset(float offset)
	{
		base.transform.localPosition = homePosition + new Vector3(offset, 0f, 0f);
	}
}

}
