namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.Events;

public class WeaponHammer : MonoBehaviour
{
	public float restAngle;

	public float readyAngle = 45f;

	public float fallPeriod = 0.01f;

	[Tooltip("The hammer is reset by a moving bolt or slide. Otherwise it will reset itself when it reaches the ready angle.")]
	public bool resetByBolt = true;

	[Tooltip("How long time to reset to ready state when not driven by bolt. In seconds.")]
	public float manualResetPeriod = 0.5f;

	public UnityEvent onStrike = new UnityEvent();

	private bool falling;

	private float releaseTimestamp;

	private float strikeTimestamp;

	public bool Ready { get; private set; }

	private void Start()
	{
		base.transform.localRotation = Quaternion.Euler(0f, 0f, readyAngle);
		Ready = true;
	}

	private void Update()
	{
		if (falling)
		{
			float num = Time.time - releaseTimestamp;
			float num2 = Mathf.LerpAngle(readyAngle, restAngle, num / fallPeriod);
			base.transform.localRotation = Quaternion.Euler(0f, 0f, num2);
			if (num2 == restAngle)
			{
				Strike();
			}
		}
		else if (!resetByBolt && !Ready)
		{
			float num3 = Time.time - strikeTimestamp;
			SetLevel(num3 / manualResetPeriod);
		}
	}

	private void Strike()
	{
		falling = false;
		releaseTimestamp = 0f;
		strikeTimestamp = Time.time;
		onStrike.Invoke();
	}

	public void Release()
	{
		if (Ready)
		{
			Ready = false;
			falling = true;
			releaseTimestamp = Time.time;
		}
	}

	public void SetReady()
	{
		SetLevel(1f);
	}

	public void SetLevel(float level)
	{
		if (!Ready)
		{
			float num = Mathf.LerpAngle(restAngle, readyAngle, level);
			base.transform.localRotation = Quaternion.Euler(0f, 0f, num);
			Ready = num == readyAngle;
		}
	}
}

}
