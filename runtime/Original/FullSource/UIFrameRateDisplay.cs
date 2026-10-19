namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIFrameRateDisplay : MonoBehaviour
{
	public Text text;

	private PerformanceCounter counter;

	private const float MeasurementPeriod = 0.1f;

	private int measurement;

	private int samples;

	private float timestamp;

	private void Start()
	{
		counter = PerformanceCounter.Instance;
	}

	private void Update()
	{
		measurement += counter.CurrentFrameRate;
		samples++;
		if (Time.realtimeSinceStartup - timestamp >= 0.1f)
		{
			timestamp = Time.realtimeSinceStartup;
			int num = ((samples != 0) ? (measurement / samples) : 0);
			measurement = (samples = 0);
			text.text = string.Format("{0}", num);
			text.color = ((num >= 55) ? Color.green : ((num < 50) ? Color.red : Color.yellow));
		}
	}
}

}
