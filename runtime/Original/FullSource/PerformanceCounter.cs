namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class PerformanceCounter
{
	private static PerformanceCounter instance = null;

	private static readonly object padlock = new object();

	private const float MinMaxPeriod = 1f;

	private uint measurement;

	private uint measurementSamples;

	private bool measuring;

	private float minimumTimestamp;

	private float maximumTimestamp;

	private int recentMinimum;

	private int recentMaximum;

	public static PerformanceCounter Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new PerformanceCounter();
				}
				return instance;
			}
		}
	}

	public int CurrentFrameRate { get; private set; }

	public int RecentMaximum
	{
		get
		{
			return recentMaximum;
		}
	}

	public int RecentMinimum
	{
		get
		{
			return recentMinimum;
		}
	}

	public int MeasurementMaximum { get; private set; }

	public int MeasurementMinimum { get; private set; }

	public void Update(float deltaTime)
	{
		CurrentFrameRate = ((!(deltaTime <= 0f)) ? Mathf.RoundToInt(1f / deltaTime) : 0);
		if (measuring)
		{
			measurement += (uint)CurrentFrameRate;
			measurementSamples++;
		}
		if (CurrentFrameRate > recentMaximum)
		{
			recentMaximum = CurrentFrameRate;
			maximumTimestamp = Time.realtimeSinceStartup;
			if (CurrentFrameRate > MeasurementMaximum)
			{
				MeasurementMaximum = CurrentFrameRate;
			}
		}
		else if (CurrentFrameRate < recentMinimum)
		{
			recentMinimum = CurrentFrameRate;
			minimumTimestamp = Time.realtimeSinceStartup;
			if (CurrentFrameRate < MeasurementMinimum)
			{
				MeasurementMinimum = CurrentFrameRate;
			}
		}
		if (Time.realtimeSinceStartup - maximumTimestamp >= 1f)
		{
			recentMaximum = CurrentFrameRate;
			maximumTimestamp = Time.realtimeSinceStartup;
		}
		if (Time.realtimeSinceStartup - minimumTimestamp >= 1f)
		{
			recentMinimum = CurrentFrameRate;
			minimumTimestamp = Time.realtimeSinceStartup;
		}
	}

	public void StartMeasurement()
	{
		measuring = true;
		MeasurementMinimum = int.MaxValue;
		MeasurementMaximum = int.MinValue;
		measurement = (measurementSamples = 0u);
	}

	public void StopMeasurement()
	{
		measuring = false;
	}

	public int MeasurementAverage()
	{
		return (int)((measurementSamples != 0) ? (measurement / measurementSamples) : 0);
	}
}

}
