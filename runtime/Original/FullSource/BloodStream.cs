namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BloodStream : MonoBehaviour
{
	private class BloodstreamParticle
	{
		public bool animate = true;

		public float radius;

		public float disperseTime;

		public float landTimestamp;

		public Vector3 position;

		public Vector3 velocity;

		public Vector3 landPosition;

		public float positionLerp;

		public BloodstreamParticle prev;

		public PaintableSurface surface;
	}

	[Tooltip("Controls segmentation density.")]
	[Range(0f, 1f)]
	public float fidelity = 1f;

	[Tooltip("Number of vertices per segment (circle).")]
	[Range(3f, 16f)]
	public int edgeCount = 6;

	public float dampingFactor = 1f;

	[Range(0f, 1f)]
	public float particleBounce = 0.25f;

	[Range(0f, 45f)]
	public float particleBounceAngle = 30f;

	[Range(0f, 500f)]
	public float particleMultiplier = 50f;

	public float disperseTime = 0.25f;

	public float disperseTimeRandom = 0.1f;

	[Range(0f, 25f)]
	public float disperseAngle = 5f;

	private Mesh mesh;

	private MeshFilter meshFilter;

	private List<BloodstreamParticle> particles = new List<BloodstreamParticle>();

	private BloodstreamParticle latestParticle;

	private Emitter particleEmitter;

	private RaycastHit raycastHit;

	private Transform helperTransform;

	private float emissionsMultiplier;

	private float disperseTimeMultiplier;

	private float segmentInterval;

	private float lastEmitTimestamp;

	private float lastBuiltTimestamp;

	private float previousRadius;

	public FloorPoolPainter PoolPainter { get; private set; }

	public void Initialize(Transform parent, FloorPoolPainter poolPainter, Emitter emitter, float velocityMultiplier, float emissionsModifier)
	{
		transform.SetParent(parent);
		transform.position = Vector3.zero;
		meshFilter = GetComponent<MeshFilter>();
		particleEmitter = emitter;
		emissionsMultiplier = 1f + (emissionsModifier - 1f) * 0.05f;
		disperseTimeMultiplier = 1f / velocityMultiplier;
		mesh = new Mesh
		{
			name = "Blood stream"
		};
		meshFilter.mesh = mesh;
		segmentInterval = 0.1f * (1f - fidelity);
		raycastHit = default;
		helperTransform = new GameObject("Helper").transform;
		helperTransform.SetParent(transform);
		if (poolPainter != null)
		{
			PoolPainter = poolPainter;
			poolPainter.Initialize(transform.position, shortLived: true, isStatic: false);
		}
		lastEmitTimestamp = Time.realtimeSinceStartup;
	}

	public void Emit(float pressure, Vector3 position, Vector3 velocity)
	{
		if (!(Time.realtimeSinceStartup - lastEmitTimestamp < segmentInterval))
		{
			float radius = Mathf.Clamp(pressure * 0.02f, 0.005f, 0.0175f);
			latestParticle = AppendNewParticle(radius, position, velocity);
		}
	}

	public void Terminate(Vector3 position, Vector3 velocity)
	{
		latestParticle = AppendNewParticle(previousRadius * 0.5f, position, velocity);
	}

	private BloodstreamParticle AppendNewParticle(float radius, Vector3 position, Vector3 velocity)
	{
		float num = Time.time + (disperseTime + disperseTimeRandom * UnityEngine.Random.value) * disperseTimeMultiplier;
		BloodstreamParticle bloodstreamParticle = new BloodstreamParticle();
		bloodstreamParticle.radius = radius * emissionsMultiplier;
		bloodstreamParticle.position = position;
		bloodstreamParticle.velocity = velocity;
		bloodstreamParticle.disperseTime = num;
		bloodstreamParticle.prev = null;
		BloodstreamParticle bloodstreamParticle2 = bloodstreamParticle;
		if (latestParticle != null)
		{
			latestParticle.prev = bloodstreamParticle2;
		}
		latestParticle = bloodstreamParticle2;
		particles.Add(bloodstreamParticle2);
		previousRadius = radius;
		lastEmitTimestamp = Time.time;
		return bloodstreamParticle2;
	}

	private void OnDestroy()
	{
		UnityEngine.Object.DestroyImmediate(mesh);
	}

	private void Update()
	{
		if (Time.deltaTime != 0f)
		{
			BuildMesh();
			UpdateParticles(Time.deltaTime);
			if (Time.time - lastEmitTimestamp >= 1f)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	private void UpdateParticles(float delta)
	{
		for (int num = particles.Count - 1; num >= 0; num--)
		{
			BloodstreamParticle bloodstreamParticle = particles[num];
			if (float.IsNaN(bloodstreamParticle.position.x) || float.IsNaN(bloodstreamParticle.position.z))
			{
				particles.RemoveAt(num);
			}
			else if (bloodstreamParticle.animate)
			{
				bloodstreamParticle.velocity = new Vector3(bloodstreamParticle.velocity.x, bloodstreamParticle.velocity.y - 9.82f * delta, bloodstreamParticle.velocity.z);
				if (bloodstreamParticle.prev != null && (Time.time > bloodstreamParticle.disperseTime || bloodstreamParticle.position.z > -1f || bloodstreamParticle.position.z < -2.5f || bloodstreamParticle.position.y > 2.25f))
				{
					CreateParticleColumn(bloodstreamParticle, bloodstreamParticle.prev);
					particles.RemoveAt(num);
				}
				else if (bloodstreamParticle.position.y <= 0f)
				{
					FindSurface(bloodstreamParticle);
				}
				else
				{
					bloodstreamParticle.position += bloodstreamParticle.velocity * delta;
				}
			}
			else if (bloodstreamParticle.surface != null && bloodstreamParticle.prev != null)
			{
				float num2 = Mathf.Abs(bloodstreamParticle.positionLerp - bloodstreamParticle.prev.position.y) / bloodstreamParticle.positionLerp;
				Vector3 vector = new Vector3(Mathf.Lerp(bloodstreamParticle.landPosition.x, bloodstreamParticle.prev.position.x, num2), 0f, Mathf.Lerp(bloodstreamParticle.landPosition.z, bloodstreamParticle.prev.position.z, num2));
				bloodstreamParticle.surface.PaintWedge(bloodstreamParticle.position, vector, bloodstreamParticle.radius * 2f, bloodstreamParticle.radius * 2f);
				bloodstreamParticle.position = vector;
				if ((bool)particleEmitter)
				{
					particleEmitter.BloodStreamBounceEmit(vector, bloodstreamParticle.prev.velocity * particleBounce, bloodstreamParticle.radius, particleBounceAngle, Mathf.CeilToInt(bloodstreamParticle.radius * 0.33f * Time.timeScale * particleMultiplier));
				}
				if (num2 >= 1f || Time.time - bloodstreamParticle.landTimestamp > 0.1f)
				{
					particles.RemoveAt(num);
				}
			}
			else
			{
				particles.RemoveAt(num);
			}
		}
	}

	private bool FindSurface(BloodstreamParticle particle)
	{
		if (Physics.Raycast(new Vector3(particle.position.x, 0.25f, particle.position.z), Vector3.down, out raycastHit, 0.5f, Masks.Instance.ObstaclesWalls))
		{
			particle.animate = false;
			particle.landTimestamp = Time.time;
			particle.surface = raycastHit.collider.GetComponent<PaintableSurface>();
			particle.landPosition = raycastHit.point;
			particle.positionLerp = ((particle.prev == null) ? particle.position.y : particle.prev.position.y);
			return true;
		}
		return false;
	}

	private void CreateParticleColumn(BloodstreamParticle leader, BloodstreamParticle trailer)
	{
		particleEmitter.EmitColumn(trailer.position, leader.position, trailer.radius, leader.radius, trailer.velocity, leader.velocity, disperseAngle);
	}

	private void BuildMesh()
	{
		mesh.Clear();
		if (particles.Count < 2)
		{
			return;
		}
		int num = edgeCount - 1;
		float num2 = (float)Math.PI * 2f / (float)num;
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		List<int> list3 = new List<int>();
		int num3 = 0;
		foreach (BloodstreamParticle particle in particles)
		{
			if (float.IsNaN(particle.position.x) || float.IsNaN(particle.position.z))
			{
				continue;
			}
			int num4 = (num3 - 1) * edgeCount;
			int i = 0;
			helperTransform.position = particle.position;
			helperTransform.rotation = Quaternion.LookRotation(particle.velocity.normalized, Vector3.up);
			float num5 = 0f;
			for (; i < edgeCount; i++)
			{
				num5 += num2;
				float num6 = particle.radius * ((num3 != 0) ? 1f : 0.2f);
				Vector3 item = helperTransform.TransformPoint(new Vector3(num6 * Mathf.Cos(num5), num6 * Mathf.Sin(num5), 0f));
				list.Add(item);
				int num7 = num3 * edgeCount;
				if (num3 > 0 && i > 0)
				{
					list3.AddRange(new int[6]
					{
						num4 + i - 1,
						num7 + i - 1,
						num4 + i,
						num7 + i - 1,
						num7 + i,
						num4 + i
					});
				}
				list2.Add(new Vector2((float)i / (float)num, (num3 % 2 != 0) ? 1f : 0f));
			}
			num3++;
		}
		mesh.vertices = list.ToArray();
		mesh.triangles = list3.ToArray();
		mesh.uv = list2.ToArray();
		mesh.RecalculateNormals();
		lastBuiltTimestamp = Time.time;
	}
}

}
