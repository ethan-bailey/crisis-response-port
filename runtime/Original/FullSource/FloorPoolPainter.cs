namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FloorPoolPainter : MonoBehaviour
{
	private class PoolPoint
	{
		public Vector3 position;

		public Vector3 direction;

		public float distance = 1f;
	}

	[Tooltip("The number of vertices the pool polygon will have.")]
	public int vertexCount = 10;

	public Vector2 randomAngle = new Vector2(-20f, 20f);

	public GameObject censorBoxPrefab;

	[Header("Animation")]
	[Tooltip("This function takes the pool's volume in liters and gives the resulting radius.")]
	public AnimationCurve volumeToRadius;

	public AnimationCurve randomOffset;

	[Header("Debug")]
	public float flow;

	public float buffer;

	public bool disableInactivityTimeout;

	private float volume;

	private float previousVolume;

	private float lastUpdate;

	private float shapeUpdateTime;

	private float friction = 1f;

	private const float Tau = (float)Math.PI * 2f;

	private const float InactivityPeriod = 0.5f;

	private const float MoveSensitivity = 0.0025f;

	private Vector3 previousPosition;

	private Vector3 previousLimbPosition;

	private Vector3 findSurfacePosition;

	private Material renderMaterialInstance;

	private Vector3 censorMin;

	private Vector3 censorMax;

	private List<PoolPoint> points = new List<PoolPoint>();

	private bool initialized;

	private bool isShortLived;

	private bool isStatic;

	public PaintableSurface Surface { get; set; }

	public GameObject CensorBox { get; private set; }

	private void Start()
	{
		if (!initialized)
		{
			Initialize(transform.position, shortLived: true, isStatic: false);
		}
	}

	public void Initialize(Vector3 worldPosition, bool shortLived, bool isStatic, PaintableSurface surface = null)
	{
		initialized = true;
		this.isStatic = isStatic;
		isShortLived = shortLived;
		MissionHelper instance = MissionHelper.Instance;
		transform.position = (previousPosition = (previousLimbPosition = (findSurfacePosition = worldPosition)));
		renderMaterialInstance = instance.RenderMaterialInstance;
		if (surface != null)
		{
			Surface = surface;
			friction = 1f / surface.friction;
		}
		Settings settings = Global.Context.Settings;
		vertexCount = settings.Performance.paintTexturePixelsPerMeter;
		if (vertexCount < 3)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		if (Surface == null)
		{
			Surface = FindSurface();
			if (Surface == null)
			{
				UnityEngine.Object.Destroy(gameObject);
				return;
			}
		}
		if (!isShortLived && settings.Graphics.applyCensoring && censorBoxPrefab != null)
		{
			SpawnCensorBox(updateBounds: false);
		}
		GenerateShape();
		lastUpdate = Time.time;
	}

	private void Update()
	{
		if (Time.deltaTime <= 0f)
		{
			return;
		}
		if (buffer > 0f)
		{
			float sqrMagnitude = (transform.position - previousPosition).sqrMagnitude;
			if (!isStatic && sqrMagnitude > float.Epsilon)
			{
				volume *= Mathf.Clamp01(1f - sqrMagnitude / 0.0025f);
				if ((transform.position - findSurfacePosition).sqrMagnitude >= 0.0625f)
				{
					Surface = FindSurface();
					if (Surface == null)
					{
						UnityEngine.Object.Destroy(gameObject);
						return;
					}
					friction = 1f / Surface.friction;
					findSurfacePosition = transform.position;
					if (Time.time - shapeUpdateTime >= 1f)
					{
						GenerateShape();
					}
				}
			}
			float num = Mathf.Min(Mathf.Min(flow, 0.5f) * Time.deltaTime, buffer);
			buffer -= num;
			volume += num;
			if (Surface != null)
			{
				float num2 = VolumeToRadius(volume);
				float num3 = VolumeToRadius(previousVolume);
				int num4 = Mathf.Max(1, Mathf.CeilToInt((num3 - num2) * Surface.PixelsPerMeter * 1.5f));
				for (int i = 0; i < num4; i++)
				{
					float radius = Mathf.Lerp(num3, num2, (float)i / (float)num4);
					for (int j = 0; j < points.Count; j++)
					{
						UpdatePoint(points[j], radius);
						PaintEdge(points[(j != 0) ? (j - 1) : (points.Count - 1)], points[j]);
					}
				}
			}
			if (CensorBox != null)
			{
				UpdateCensorBox();
			}
			previousVolume = volume;
			previousPosition = transform.position;
			lastUpdate = Time.time;
		}
		else if (!disableInactivityTimeout && Time.time - lastUpdate >= 0.5f)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public void UpdateLimbPool(Vector3 worldPosition, float flow)
	{
		if (Mathf.Abs(worldPosition.x - previousLimbPosition.x) > 0.25f)
		{
			transform.position = (previousLimbPosition = worldPosition);
			volume = (previousVolume = 0f);
			GenerateShape();
		}
		if (flow > 0f)
		{
			lastUpdate = Time.time;
		}
		this.flow = (buffer = flow);
	}

	public void MoveAndUpdate(Vector3 worldPosition, PaintableSurface surface, float flow)
	{
		isShortLived = true;
		transform.position = worldPosition;
		Surface = surface;
		this.flow = flow;
		buffer = flow;
		lastUpdate = Time.time;
	}

	public void SetCensored()
	{
		if (CensorBox == null)
		{
			SpawnCensorBox(updateBounds: true);
		}
	}

	private void SpawnCensorBox(bool updateBounds)
	{
		if (CensorBox == null)
		{
			CensorBox = UnityEngine.Object.Instantiate(censorBoxPrefab);
			CensorBox.transform.SetParent(MissionHelper.Instance.CensorBoxContainer);
		}
		if (!updateBounds)
		{
			return;
		}
		foreach (PoolPoint point in points)
		{
			UpdateCensorBoxWithPoint(point.position);
		}
		Vector3 vector = censorMax - censorMin;
		Vector3 vector2 = transform.position + censorMin + vector * 0.5f;
		CensorBox.transform.position = new Vector3(vector2.x, transform.position.y + 0.005f, vector2.z);
		CensorBox.transform.localScale = new Vector3(vector.x, 1f, vector.z);
	}

	private void UpdateCensorBox()
	{
		Vector3 vector = censorMax - censorMin;
		Vector3 vector2 = transform.position + censorMin + vector * 0.5f;
		CensorBox.transform.position = new Vector3(vector2.x, transform.position.y + 0.005f, vector2.z);
		CensorBox.transform.localScale = new Vector3(vector.x, 1f, vector.z);
	}

	private void GenerateShape()
	{
		points.Clear();
		float num = 1f;
		float num2 = 1f;
		for (int i = 0; i <= vertexCount; i++)
		{
			float num3 = (float)i / (float)vertexCount;
			float num4 = Mathf.Max((float)(i - vertexCount + 4) / 4f, 0f);
			float num5 = (float)Math.PI * 2f + UnityEngine.Random.Range(0f - randomAngle.x, randomAngle.y) * ((float)Math.PI / 180f);
			Vector3 normalized = new Vector3(Mathf.Sin(num3 * num5), 0f, Mathf.Cos(num3 * num5)).normalized;
			float num6 = ((i != 0) ? ((1f - num4) * num2 + num4 * num) : num);
			num2 = num6 + randomOffset.Evaluate(UnityEngine.Random.value);
			points.Add(new PoolPoint
			{
				direction = normalized,
				distance = num6
			});
		}
		shapeUpdateTime = Time.time;
	}

	public void SetFlow(float flow, float volume)
	{
		this.flow = flow;
		buffer += volume;
	}

	public bool TestUpdatePosition(float worldPosition)
	{
		return Mathf.Abs(worldPosition - transform.position.x) > 0.1f;
	}

	private PaintableSurface FindSurface()
	{
		RaycastHit hitInfo = default;
		Ray ray = new Ray(new Vector3(transform.position.x, 0.5f, -1.5f), Vector3.down);
		if (!Physics.Raycast(ray, out hitInfo, 1f, Masks.Instance.Obstacles))
		{
			return null;
		}
		return hitInfo.collider.GetComponent<PaintableSurface>();
	}

	private void PaintEdge(PoolPoint p1, PoolPoint p2)
	{
		Vector3 vector = transform.position + p1.position;
		Vector3 vector2 = p2.position - p1.position;
		float magnitude = vector2.magnitude;
		float num = magnitude * Surface.PixelsPerMeter * 1.5f;
		for (int i = 0; (float)i < num; i++)
		{
			Vector3 worldPoint = vector + vector2 * ((float)i / num);
			Surface.PaintPixelNoRetry(worldPoint);
		}
	}

	private void UpdatePoint(PoolPoint point, float radius)
	{
		point.position = point.direction * point.distance * radius * friction;
		if (CensorBox != null)
		{
			UpdateCensorBoxWithPoint(point.position);
		}
	}

	private float VolumeToRadius(float volume)
	{
		return Mathf.Sqrt(volume * 0.01f / (float)Math.PI);
	}

	private void UpdateCensorBoxWithPoint(Vector3 point)
	{
		if (point.x > censorMax.x)
		{
			censorMax.x = point.x;
		}
		else if (point.x < censorMin.x)
		{
			censorMin.x = point.x;
		}
		if (point.z > censorMax.z)
		{
			censorMax.z = point.z;
		}
		else if (point.z < censorMin.z)
		{
			censorMin.z = point.z;
		}
	}

	private void DestroyCensorBox()
	{
		censorMin = (censorMax = Vector3.zero);
		UnityEngine.Object.Destroy(CensorBox);
	}
}

}
