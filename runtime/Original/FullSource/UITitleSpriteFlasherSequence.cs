namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class UITitleSpriteFlasherSequence : MonoBehaviour
{
	public float interval = 0.2f;

	public List<UITitleSpriteFlasher> elements = new List<UITitleSpriteFlasher>();

	private int current;

	private float timestamp;

	private bool active;

	private void Update()
	{
		if (!active)
		{
			return;
		}
		float num = Time.time - timestamp;
		if (num >= (float)current * interval)
		{
			elements[current++].Flash();
			if (current == elements.Count)
			{
				active = false;
			}
		}
	}

	public void Flash()
	{
		current = 0;
		active = true;
		timestamp = Time.time;
	}

	public void FlashIn(float seconds)
	{
		current = 0;
		active = true;
		timestamp = Time.time + seconds;
	}

	public void FlashAll()
	{
		foreach (UITitleSpriteFlasher element in elements)
		{
			element.Flash();
		}
	}
}

}
