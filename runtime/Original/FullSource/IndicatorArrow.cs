namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class IndicatorArrow : MonoBehaviour
{
	public float distance = 0.1f;

	public float period = 0.5f;

	public AnimationCurve curve;

	public Transform arrow;

	private Vector3 offset;

	private float timestamp;

	private void Start()
	{
		offset = arrow.localPosition;
		timestamp = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		float num = Time.realtimeSinceStartup - timestamp;
		if (num > period)
		{
			timestamp = Time.realtimeSinceStartup;
		}
		arrow.localPosition = offset + Vector3.up * curve.Evaluate(num / period) * distance;
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}
}

}
