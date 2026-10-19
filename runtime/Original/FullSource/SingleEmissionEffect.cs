namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public class SingleEmissionEffect : MonoBehaviour
{
	private static float MinDiameter = 4f;

	private static float DiameterDividend = 26f;

	[Tooltip("If this is set the graphics setting for particles lifetime will not be used.")]
	public bool dontUseGlobalLifetime;

	[Tooltip("The lifetime of the particles emitted and the object this script is attached.")]
	public float lifeTime = 1f;

	[Tooltip("The projectile's velocity is divided by this number to form a factor that scales the number of particles. The result is clamped to [0, 1].")]
	public float volumeVelocity = 800f;

	[Header("Entry")]
	[Tooltip("How many particles should be emitted on entry in relation to the projectile's energy and diameter.")]
	public float entryRatio = 1f;

	[Tooltip("How fast the particles from the entry should travel as a ratio of the projectile's velocity.")]
	public float entryVelocityRatio = 1f;

	[Tooltip("Randomize the rotation of this emitter.")]
	public bool randomRotation;

	[Tooltip("How many degrees the rotation should be offset from the projectile's trajectory.")]
	public Vector2 randomRotationMagnitude = new Vector2(0f, 90f);

	[Header("Exit")]
	[Tooltip("How many particles should be emitted on exit as a ratio to the volume of displaced material.")]
	public float volumeRatio = 1f;

	[Tooltip("How fast the particles should exit as a ratio of the projectile's exit velocity.")]
	public float exitVelocityRatio = 1f;

	[Tooltip("Enable this for emissions using physical meshes. These will then be scaled by the size of displaced volume.")]
	public bool chunkEmission;

	private ParticleSystem emitter;

	private bool activated;

	private float timestamp;

	public void EntryEmission(Projectile projectile, Vector3 parentVelocity = default(Vector3))
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.velocity = parentVelocity;
		}
		if (randomRotation)
		{
			base.transform.Rotate(Random.onUnitSphere, Random.Range(randomRotationMagnitude.x, randomRotationMagnitude.y));
		}
		float velocity = projectile.Velocity;
		Settings settings = Global.Context.Settings;
		float emissionsModifier = settings.Extras.emissionsModifier;
		int num = Mathf.FloorToInt(projectile.Energy * projectile.MetricDiameter * entryRatio * Mathf.Clamp(velocity / volumeVelocity, 0f, 1f) * emissionsModifier * projectile.particleCountScaler);
		if (num == 0)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		emitter = GetComponent<ParticleSystem>();
		uint randomSeed = (uint)(int.MaxValue + Random.Range(int.MinValue, int.MaxValue));
		ParticleSystem.MainModule main = emitter.main;
		emitter.randomSeed = randomSeed;
		ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
		startSpeed.constantMin = velocity * entryVelocityRatio * 0.25f;
		startSpeed.constantMax = velocity * entryVelocityRatio;
		main.startSpeed = startSpeed;
		main.startLifetime = ((!dontUseGlobalLifetime) ? ((float)settings.Performance.particlesLifetime) : lifeTime);
		emitter.Simulate(Time.deltaTime, true, true, false);
		emitter.Emit(num);
		emitter.Play();
		Activate();
	}

	public void ExitEmission(Projectile projectile, Vector3 parentVelocity = default(Vector3))
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.velocity = parentVelocity;
		}
		Settings settings = Global.Context.Settings;
		float emissionsModifier = settings.Extras.emissionsModifier;
		int num = Mathf.FloorToInt(projectile.DisplacedMaterialVolume * volumeRatio * Mathf.Clamp(projectile.Velocity / volumeVelocity, 0f, 1f) * emissionsModifier * projectile.particleCountScaler);
		if (num == 0)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		emitter = GetComponent<ParticleSystem>();
		uint randomSeed = (uint)(int.MaxValue + Random.Range(int.MinValue, int.MaxValue));
		ParticleSystem.MainModule main = emitter.main;
		emitter.randomSeed = randomSeed;
		ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
		startSpeed.constantMin = 0f;
		startSpeed.constantMax = projectile.Velocity * exitVelocityRatio;
		main.startSpeed = startSpeed;
		if (chunkEmission)
		{
			float t = (projectile.Diameter - MinDiameter) / DiameterDividend;
			float num2 = Mathf.Lerp(0.75f, 1.75f, t) * settings.Extras.chunkSizeMultiplier;
			main.startSizeMultiplier *= num2;
			num = Mathf.CeilToInt((float)num * (1f / num2));
		}
		main.startLifetime = ((!dontUseGlobalLifetime) ? ((float)settings.Performance.particlesLifetime) : lifeTime);
		emitter.Simulate(Time.deltaTime, true, true, false);
		emitter.Emit(Mathf.Min(num, 500));
		emitter.Play();
		Activate();
	}

	public void BounceEmission(int count, float velocity, Vector3 parentVelocity = default(Vector3))
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.velocity = parentVelocity;
		}
		Settings settings = Global.Context.Settings;
		float emissionsModifier = settings.Extras.emissionsModifier;
		if (count == 0)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		emitter = GetComponent<ParticleSystem>();
		uint randomSeed = (uint)(int.MaxValue + Random.Range(int.MinValue, int.MaxValue));
		ParticleSystem.MainModule main = emitter.main;
		emitter.randomSeed = randomSeed;
		ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
		startSpeed.constantMin = 0f;
		startSpeed.constantMax = velocity * exitVelocityRatio;
		main.startSpeed = startSpeed;
		if (chunkEmission)
		{
			main.startSizeMultiplier *= settings.Extras.chunkSizeMultiplier;
		}
		main.startLifetime = ((!dontUseGlobalLifetime) ? ((float)settings.Performance.particlesLifetime) : lifeTime);
		emitter.Simulate(Time.deltaTime, true, true, false);
		emitter.Emit(count);
		emitter.Play();
		Activate();
	}

	private void Activate()
	{
		timestamp = Time.time;
		activated = true;
	}

	private void Update()
	{
		if (activated && Time.time - timestamp >= lifeTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}

}
