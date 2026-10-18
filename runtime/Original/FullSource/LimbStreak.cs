namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public class LimbStreak
{
	public Vector3 localSourcePosition;

	public Vector3 localPosition;

	public float offsetRadiusMeters;

	public float velocity;

	public float velocityScale;

	public float width;

	public float lifetime;

	public float timestamp;

	public float resetTimestamp;

	public float period;

	public LimbStreak(Vector3 localWoundPosition, float offsetRadiusMeters, float velocityScale, float lifetime)
	{
		timestamp = Time.time;
		this.lifetime = lifetime;
		this.velocityScale = velocityScale;
		this.offsetRadiusMeters = offsetRadiusMeters;
		localSourcePosition = localWoundPosition;
		Randomize();
	}

	public void Randomize()
	{
		Vector3 onUnitSphere = Random.onUnitSphere;
		onUnitSphere.z = 0f;
		onUnitSphere.Normalize();
		localPosition = localSourcePosition + onUnitSphere * offsetRadiusMeters;
		resetTimestamp = Time.time;
		width = Random.Range(0.5f, 2f);
		velocity = Random.Range(0.05f, 0.4f) * velocityScale;
		period = 0.3f / velocity;
	}

	public void Update(LimbRenderer renderer, Vector3 localDownDirection, Transform transform)
	{
		localPosition += localDownDirection * velocity * Time.deltaTime;
	}

	public void Render(LimbRenderer renderer, Transform transform)
	{
		renderer.DrawBlood(transform.TransformPoint(localPosition), width, 1f);
	}
}

}
