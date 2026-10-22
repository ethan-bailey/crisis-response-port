namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public class WallStreakPainter : MonoBehaviour
{
	private const float massLossPerAlphaUnit = 0.00035f;

	private const float massMinimum = 0.1f;

	private const float massMaximum = 3f;

	private Vector3 velocity;

	private Vector3 gravity;

	private Vector3 previousPosition;

	private Vector3 staticVelocity;

	private float pointInterval;

	private float mass;

	private float startMass;

	private float distance;

	private float fill;

	private Vector3 line;

	private Vector3 dir;

	public PaintableSurface Surface { get; set; }

	public void Initialize(PaintableSurface surface, Vector3 velocity)
	{
		Surface = surface;
		pointInterval = 0.5f / Surface.PixelsPerMeter;
		startMass = (mass = Random.Range(0.1f, 3f));
		previousPosition = transform.position;
		gravity = new Vector3(0f, Random.Range(1f, 1.5f) / Surface.friction, 0f);
		float x = Mathf.Max(Mathf.Abs(velocity.x) * 0.125f, 0.5f) * Mathf.Sign(velocity.x);
		float y = Mathf.Max(Mathf.Abs(velocity.y) * 0.0575f, 0.25f) * Mathf.Sign(velocity.y);
		this.velocity = new Vector3(x, y, 0f);
		staticVelocity = new Vector3(0f, -0.1f / Surface.friction, 0f);
	}

	private void Update()
	{
		velocity.x *= Mathf.Clamp01(60f * Time.deltaTime * 0.7f);
		velocity.y *= Mathf.Clamp01(60f * Time.deltaTime * 0.8f);
		Vector3 vector = (velocity - gravity) * (Mathf.Max(0f, mass - 1f) / startMass);
		transform.position += (vector + staticVelocity) * Time.deltaTime;
		line = transform.position - previousPosition;
		distance = line.magnitude;
		if (!(distance < pointInterval))
		{
			fill = 0f;
			dir = line.normalized;
			float num = 0f;
			while (fill < distance)
			{
				float alpha = Mathf.Clamp(mass, 0.33f, 0.25f);
				num += Surface.PaintPixelAddAlpha(previousPosition + dir * fill, alpha);
				fill += pointInterval;
			}
			mass -= num * 150f * 0.00035f * Surface.friction;
			previousPosition = transform.position;
			if (mass <= 0f || transform.position.y < 0f)
			{
				Object.Destroy(gameObject);
			}
		}
	}
}

}
