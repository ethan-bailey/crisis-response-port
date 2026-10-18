namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;
using UnityEngine.UI;

public class UIHitAnnouncement : MonoBehaviour
{
	public Text fill;

	public Text outline;

	public AnimationCurve scale;

	public AnimationCurve alpha;

	private float timestamp;

	private float duration;

	private Vector3 startPosition;

	private Vector3 travel;

	private Color fillColor;

	private Color outlineColor;

	public string Text
	{
		set
		{
			fill.text = value;
			outline.text = value;
		}
	}

	public void Initialize(Vector3 screenPosition)
	{
		startPosition = screenPosition;
		timestamp = Time.time;
		duration = ((scale.keys.Length <= 0) ? 0f : scale.keys[scale.keys.Length - 1].time);
		fillColor = new Color(Random.Range(0.3f, 1f), 0f, 0f);
		outlineColor = Color.white;
		travel = Random.insideUnitCircle * Random.Range(0f, 500f);
	}

	private void Update()
	{
		float num = Time.time - timestamp;
		if (num >= duration)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		float num2 = ((!(duration > 0f)) ? 0f : (num / duration));
		float num3 = scale.Evaluate(num);
		base.transform.localScale = new Vector3(num3, num3, 1f);
		base.transform.position = startPosition + num2 * travel;
		fillColor.a = (outlineColor.a = alpha.Evaluate(num2));
		fill.color = fillColor;
		outline.color = outlineColor;
	}
}

}
