namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UITitleSpriteFlasher : MonoBehaviour
{
	public float flashDuration = 0.05f;

	public AnimationCurve flash;

	private Image image;

	private Material material;

	private float timestamp;

	private bool active;

	private void Awake()
	{
		image = GetComponent<Image>();
		material = Object.Instantiate(image.material);
		image.material = material;
	}

	private void Update()
	{
		if (active)
		{
			float num = (Time.time - timestamp) / flashDuration;
			SetWeight(flash.Evaluate(num));
			if (num >= 1f)
			{
				SetWeight(0f);
				active = false;
			}
		}
	}

	public void Flash()
	{
		active = true;
		timestamp = Time.time;
	}

	private void SetWeight(float value)
	{
		material.SetFloat("_Weight", value);
	}
}

}
