namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class ImageColorFlasher : MonoBehaviour
{
	public Image image;

	public Color flashColor;

	public float minInterval = 5f;

	public float maxInterval = 15f;

	public float lightUpTime = 0.1f;

	public float followUpPeriod = 0.1f;

	public int minFollowUpFlashes;

	public int maxFollowUpFlashes = 2;

	private float startTime;

	private float flashTime;

	private int followUpFlashes;

	private int followUpCount;

	private Color original;

	private void Start()
	{
		original = image.color;
	}

	public void FlashIn(float seconds, int followUpFlashes = 1)
	{
		startTime = Time.realtimeSinceStartup;
		flashTime = seconds;
		this.followUpFlashes = followUpFlashes;
		followUpCount = 0;
	}

	private void SetNextFlash()
	{
		startTime = Time.realtimeSinceStartup;
		flashTime = Random.Range(minInterval, maxInterval);
		followUpFlashes = Random.Range(minFollowUpFlashes, maxFollowUpFlashes);
		followUpCount = 0;
	}

	private void Update()
	{
		float num = Time.realtimeSinceStartup - startTime;
		if (num >= flashTime && num - flashTime < lightUpTime)
		{
			image.color = flashColor;
		}
		else if (num >= flashTime + lightUpTime)
		{
			image.color = original;
			if (followUpCount < followUpFlashes)
			{
				flashTime += followUpPeriod;
				followUpCount++;
			}
			else
			{
				SetNextFlash();
			}
		}
	}
}

}
