using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class ExteriorCamera : MonoBehaviour
{
	public Vector2 interval = new Vector2(0.1f, 0.3f);

	public float extent;

	public float smoothing = 2f;

	public float offset = 4f;

	private Transform team;

	private Vector3 target;

	private Vector3 shake;

	private NoiseAndGrain grainEffect;

	private float last;

	private float next = 0.2f;

	private void Awake()
	{
		grainEffect = GetComponent<NoiseAndGrain>();
	}

	private void Start()
	{
		Global.Context.Settings.Performance.onCameraGrainChanged.AddListener(EnableGrainEffect);
		EnableGrainEffect(Global.Context.Settings.Performance.cameraGrain);
		team = Global.Context.Mission.Occupants.Container;
		target = TargetPosition();
	}

	private void EnableGrainEffect(bool enable)
	{
		grainEffect.enabled = enable;
	}

	private Vector3 TargetPosition()
	{
		CharacterBody characterBody = FrontBody();
		float num = ((!(characterBody != null)) ? base.transform.position.x : characterBody.transform.position.x);
		return new Vector3(num + offset, base.transform.position.y, base.transform.position.z);
	}

	private void Update()
	{
		Vector3 vector = TargetPosition();
		base.transform.position = Vector3.Lerp(target, vector + shake, smoothing * Time.deltaTime);
		target = vector;
		if (!(Time.time - last < next))
		{
			shake = Random.insideUnitSphere * extent;
			next = Random.Range(interval.x, interval.y);
			last = Time.time;
		}
	}

	private CharacterBody FrontBody()
	{
		float num = float.PositiveInfinity;
		Transform transform = null;
		foreach (Transform item in team)
		{
			if (!(item.position.x >= num))
			{
				num = item.position.x;
				transform = item;
			}
		}
		if (transform != null)
		{
			return transform.GetComponent<CharacterBody>();
		}
		return null;
	}
}
