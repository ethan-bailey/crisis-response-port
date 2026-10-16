using UnityEngine;

public class Emitter : MonoBehaviour
{
	public ParticleSystem system;

	private PerformanceManager perfMgr;

	private ParticleSystem.MainModule mainModule;

	private ParticleSystem.ShapeModule shapeModule;

	private Rigidbody rigidBody;

	private float particleCountMultiplier = 1f;

	private void Start()
	{
		perfMgr = PerformanceManager.Instance;
		shapeModule = system.shape;
		mainModule = system.main;
		mainModule.maxParticles = perfMgr.MaxBloodParticles;
		rigidBody = GetComponent<Rigidbody>();
		Global.Context.Settings.Performance.onChanged.AddListener(OnPerformanceSettingsChanged);
		Global.Context.Settings.Extras.onChanged.AddListener(OnExtraSettingsChanged);
	}

	public void OnPerformanceSettingsChanged(SerializablePerformanceSettings newSettings)
	{
		mainModule.maxParticles = perfMgr.MaxBloodParticles;
	}

	public void OnExtraSettingsChanged(SerializableExtraSettings newSettings)
	{
		particleCountMultiplier = newSettings.emissionsModifier;
		mainModule.maxParticles = perfMgr.MaxBloodParticles;
	}

	public void SetTransform(Vector3 position, Quaternion rotation, Vector3 velocity)
	{
		base.transform.position = position;
		base.transform.rotation = rotation;
		rigidBody.velocity = velocity;
	}

	public void Emit(int count)
	{
		int count2 = Mathf.CeilToInt((float)count * particleCountMultiplier);
		system.Emit(count2);
	}

	public void BloodStreamBounceEmit(Vector3 position, Vector3 velocity, float radius, float angle, int count)
	{
		if (!float.IsNaN(position.x))
		{
			int count2 = Mathf.CeilToInt((float)count * particleCountMultiplier);
			base.transform.position = position + Vector3.up * 0.01f;
			base.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(velocity, Vector3.up));
			shapeModule.radius = radius;
			shapeModule.angle = angle;
			mainModule.startSpeed = new ParticleSystem.MinMaxCurve(0f, velocity.magnitude);
			system.Emit(count2);
		}
	}

	public void EmitColumn(Vector3 start, Vector3 end, float startRadius, float endRadius, Vector3 startVelocity, Vector3 endVelocity, float angle)
	{
		if (!float.IsNaN(start.x))
		{
			base.transform.position = start;
			Vector3 normalized = startVelocity.normalized;
			Quaternion quaternion = Quaternion.LookRotation(normalized);
			Quaternion b = Quaternion.LookRotation(endVelocity.normalized);
			float num = startVelocity.magnitude;
			float magnitude = endVelocity.magnitude;
			base.transform.rotation = quaternion;
			shapeModule.angle = angle;
			shapeModule.radius = startRadius;
			mainModule.startSpeed = num;
			float magnitude2 = (end - start).magnitude;
			float num2 = magnitude2 * 100f;
			int num3 = Mathf.CeilToInt(num2);
			float num4 = magnitude2 / num2;
			float num5 = (magnitude - num) / num2;
			float num6 = (endVelocity - startVelocity).magnitude / num2;
			float num7 = (endRadius - startRadius) / num2;
			int count = Mathf.CeilToInt(2f * particleCountMultiplier * Time.timeScale);
			for (int i = 0; i < num3; i++)
			{
				system.Emit(count);
				num += num5;
				mainModule.startSpeed = (num += num5);
				shapeModule.radius = (startRadius += num7);
				base.transform.position += normalized * num4;
				base.transform.rotation = Quaternion.Lerp(quaternion, b, (float)i / num2);
			}
		}
	}
}
