using UnityEngine;
using UnityEngine.Audio;

public class TimeScaler : MonoBehaviour
{
	public enum Mode
	{
		Normal = 0,
		EaseIn = 1,
		Bullet = 2,
		EaseOut = 3
	}

	public AudioMixer mixer;

	public AnimationCurve easeInFunction;

	public AnimationCurve easeOutFunction;

	public bool bulletTimeDisabled;

	public bool slowMotionDisabled;

	private SerializableTimeSettings settings;

	private UIInteriorModeTextIndicator textIndicator;

	private InputHandler inputHandler;

	private float easeIn;

	private float easeOut;

	private float duration;

	private float timeRate = 1f;

	private float bulletTimeRate;

	private float rateBeforePause;

	private float rateTimestamp;

	private float bulletTimestamp;

	private bool startBulletTime;

	public bool Paused { get; private set; }

	public bool InRealTime
	{
		get
		{
			return Time.timeScale == 1f;
		}
	}

	public bool InSlowMotion
	{
		get
		{
			return Time.timeScale != 1f;
		}
	}

	public bool InBulletTime { get; private set; }

	public float TargetRate
	{
		get
		{
			return timeRate;
		}
	}

	public Mode CurrentMode
	{
		get
		{
			if (bulletTimestamp > rateTimestamp)
			{
				float num = Time.realtimeSinceStartup - bulletTimestamp;
				if (num < easeIn)
				{
					return Mode.EaseIn;
				}
				if (num < easeIn + duration)
				{
					return Mode.Bullet;
				}
				if (num < easeIn + duration + easeOut)
				{
					return Mode.EaseOut;
				}
				return Mode.Normal;
			}
			float num2 = Time.realtimeSinceStartup - rateTimestamp;
			if (Time.timeScale > timeRate)
			{
				return Mode.EaseIn;
			}
			if (Time.timeScale < timeRate)
			{
				return Mode.EaseOut;
			}
			return Mode.Normal;
		}
	}

	private void Awake()
	{
		inputHandler = Global.Context.InputHandler;
	}

	private void Start()
	{
		settings = Global.Context.Settings.Time;
		rateTimestamp = Time.realtimeSinceStartup;
		textIndicator = Global.Context.Mission.UI.indicatorUpperLeft;
		bulletTimestamp = -100f;
		Instant(0f);
	}

	public void Pause(bool pause)
	{
		if (pause != Paused)
		{
			Paused = pause;
			if (pause)
			{
				rateBeforePause = Time.timeScale;
				SetTimeScale(0f);
				Global.Context.Mixer.SetPaused(true);
				inputHandler.SuspendInput(true);
			}
			else
			{
				SetTimeScale(rateBeforePause);
				Global.Context.Mixer.SetPaused(false);
				inputHandler.SuspendInput(false);
			}
		}
	}

	public void SetTutorialMode(bool enable)
	{
		slowMotionDisabled = enable;
		bulletTimeDisabled = enable;
		Instant(1f);
	}

	public void Instant(float rate)
	{
		SetTimeScale(rate);
	}

	public void TimeRate(float rate, float easeIn = 1f)
	{
		timeRate = rate;
		this.easeIn = easeIn;
		rateTimestamp = Time.realtimeSinceStartup;
	}

	public void BulletTime(SerializableBulletTimeEntry entry)
	{
		if (entry.enabled)
		{
			BulletTime(entry.rate, entry.duration, entry.easeIn, entry.easeOut);
		}
	}

	private void BulletTime(float rate, float duration, float easeIn = 0.25f, float easeOut = 0.25f)
	{
		if (!bulletTimeDisabled && settings.enableBulletTime)
		{
			startBulletTime = true;
			bulletTimestamp = Time.realtimeSinceStartup;
			bulletTimeRate = rate;
			this.duration = duration;
			this.easeIn = easeIn;
			this.easeOut = easeOut;
			if (easeIn <= 0.017f)
			{
				SetTimeScale(rate);
			}
		}
	}

	public void ToggleBulletTime()
	{
		settings.enableBulletTime = !settings.enableBulletTime;
		settings.Save();
		textIndicator.Indicate(string.Format("Bullet time {0}", (!settings.enableBulletTime) ? "disabled" : "enabled"), 1f, false);
	}

	public void EnableSlowMotion(bool enable, bool indicate = true)
	{
		if (!slowMotionDisabled)
		{
			if (InRealTime && enable)
			{
				TimeRate(settings.slowMotionRate, settings.slowMotionEaseInOut);
			}
			else if (!enable)
			{
				TimeRate(1f, settings.slowMotionEaseInOut);
			}
			if (indicate)
			{
				IndicateTargetRate();
			}
		}
	}

	public void ToggleSlowMotion()
	{
		if (!slowMotionDisabled)
		{
			float num = 1f - settings.slowMotionRate;
			if (Time.timeScale > settings.slowMotionRate + num * 0.5f)
			{
				SetSlowMotion(settings.slowMotionRate, settings.slowMotionEaseInOut);
			}
			else
			{
				SetSlowMotion(1f, settings.slowMotionEaseInOut);
			}
		}
	}

	public void SetSlowMotion(float rate, float easeInOutTime = 1f, bool indicate = true)
	{
		TimeRate(rate, easeInOutTime);
		if (indicate)
		{
			IndicateTargetRate();
		}
	}

	private void Update()
	{
		if (Paused)
		{
			return;
		}
		UserInput();
		if (Time.timeScale == timeRate && !startBulletTime)
		{
			return;
		}
		startBulletTime = false;
		float num = NormalizedTimeInMode();
		if (num < 0f)
		{
			Instant(timeRate);
			return;
		}
		float num2 = 1f;
		switch (CurrentMode)
		{
		case Mode.EaseIn:
			num2 = Mathf.Lerp(Time.timeScale, bulletTimeRate, easeInFunction.Evaluate(num));
			break;
		case Mode.Bullet:
			num2 = bulletTimeRate;
			break;
		case Mode.EaseOut:
			num2 = Mathf.Lerp(Time.timeScale, timeRate, easeOutFunction.Evaluate(num));
			break;
		}
		if (num2 < 0f && num2 > 1f)
		{
			Debug.LogWarning("Invalid time scale.");
			SetTimeScale(timeRate);
		}
		else if (num2 != Time.timeScale)
		{
			SetTimeScale(num2);
		}
	}

	private void SetTimeScale(float rate)
	{
		Time.timeScale = rate;
		mixer.SetFloat("RealtimePitch", rate);
	}

	private void UserInput()
	{
		if (inputHandler.Keys.bulletTime.Down)
		{
			ToggleBulletTime();
		}
		else if (inputHandler.Keys.slowMotion.Down)
		{
			ToggleSlowMotion();
		}
		else if (inputHandler.Keys.decreaseSlowMotion.Down)
		{
			settings.slowMotionRate = Mathf.Clamp(timeRate * 0.66666f, 0.0001f, 1f);
			TimeRate(settings.slowMotionRate);
			IndicateTargetRate();
		}
		else if (inputHandler.Keys.increaseSlowMotion.Down)
		{
			settings.slowMotionRate = Mathf.Clamp(timeRate * 1.5f, 0.0001f, 1f);
			TimeRate(settings.slowMotionRate);
			IndicateTargetRate();
		}
	}

	private float NormalizedTimeInMode()
	{
		float num = Mathf.Min(Time.realtimeSinceStartup - bulletTimestamp, Time.realtimeSinceStartup - rateTimestamp);
		if (num < easeIn)
		{
			return num / easeIn;
		}
		if (num < easeIn + duration)
		{
			return (num - easeIn) / duration;
		}
		if (num < easeIn + duration + easeOut)
		{
			return (num - easeIn - duration) / easeOut;
		}
		return -1f;
	}

	private void IndicateTargetRate()
	{
		textIndicator.Indicate(string.Format("Time rate: {0:F2}%", timeRate * 100f), 1f, false);
	}
}
