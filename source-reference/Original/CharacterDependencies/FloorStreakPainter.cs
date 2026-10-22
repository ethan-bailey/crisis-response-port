using System;
using System.Collections.Generic;
using UnityEngine;

public class FloorStreakPainter : MonoBehaviour
{
	[Serializable]
	public class FloorStreakSegment
	{
		public Vector3 endPosition = default(Vector3);

		public float endWidth = 0.05f;

		public float lerpPortion = 0.1f;
	}

	public float startWidth = 0.05f;

	public float endWidth = 0.01f;

	public AnimationCurve randomTurn;

	public List<FloorStreakSegment> segments = new List<FloorStreakSegment>();

	private Vector3 segmentStart;

	private Vector3 segmentEnd;

	private Vector3 segmentDirection;

	private Vector3 previousDirection;

	private int segment;

	private float segmentLength;

	private float segmentLerp;

	private float width;

	private float friction = 1f;

	public PaintableSurface Surface { get; set; }

	public void Initialize(PaintableSurface surface = null)
	{
		Surface = surface;
		if (Surface == null && !FindSurface())
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		friction = 1f / Surface.friction;
		RandomInitialize();
		StartFirstSegment();
	}

	private void RandomInitialize()
	{
		Vector3 position = base.transform.position;
		startWidth = UnityEngine.Random.Range(0.02f, 0.05f) * Surface.friction;
		float num = UnityEngine.Random.value * (float)Math.PI * 2f;
		int num2 = 0;
		float num3 = startWidth;
		while (num3 > 0.01f)
		{
			Vector3 vector = new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num));
			position += vector * UnityEngine.Random.Range(0.075f, 0.2f) * friction;
			segments.Add(new FloorStreakSegment
			{
				lerpPortion = UnityEngine.Random.Range(0.05f, 0.25f),
				endPosition = position,
				endWidth = num3
			});
			num += randomTurn.Evaluate(UnityEngine.Random.value) * friction;
			num3 = num3 * UnityEngine.Random.Range(0.75f, 1f) * (1f - (float)num2 * 0.05f);
			num2++;
		}
	}

	private bool FindSurface()
	{
		RaycastHit hitInfo = default(RaycastHit);
		Ray ray = new Ray(base.transform.position + Vector3.up * 0.5f, Vector3.down);
		if (!Physics.Raycast(ray, out hitInfo, 1f, Masks.Instance.Obstacles))
		{
			return false;
		}
		Surface = hitInfo.collider.GetComponent<PaintableSurface>();
		return Surface != null;
	}

	private void Update()
	{
		float num = Vector3.Distance(segmentStart, base.transform.position) / segmentLength;
		base.transform.position += base.transform.forward * Mathf.Min(0.1f, width) * Time.deltaTime;
		Vector3 vector = Vector3.Lerp(previousDirection, segmentDirection, num);
		base.transform.LookAt(base.transform.position + vector);
		width = startWidth - (startWidth - endWidth) * num;
		PaintSegment(base.transform.right * (0f - width));
		PaintSegment(base.transform.right * width);
		if (num >= 1f)
		{
			StartNextSegment();
		}
		if (width < 0.01f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void StartFirstSegment()
	{
		segment = 0;
		segmentLength = (segments[segment].endPosition - base.transform.position).magnitude;
		segmentStart = base.transform.position;
		segmentEnd = segments[segment].endPosition;
		endWidth = segments[segment].endWidth;
		segmentLerp = segments[segment].lerpPortion;
		segmentDirection = (previousDirection = (segmentEnd - base.transform.position).normalized);
		base.transform.LookAt(segmentEnd);
	}

	private bool StartNextSegment()
	{
		segment++;
		if (segment >= segments.Count)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return false;
		}
		startWidth = width;
		segmentLength = (segments[segment].endPosition - base.transform.position).magnitude;
		segmentStart = base.transform.position;
		segmentEnd = segments[segment].endPosition;
		previousDirection = segmentDirection;
		segmentDirection = (segmentEnd - base.transform.position).normalized;
		endWidth = segments[segment].endWidth;
		segmentLerp = segments[segment].lerpPortion;
		return true;
	}

	private void PaintSegment(Vector3 segment)
	{
		float num = segment.magnitude * Surface.PixelsPerMeter * 1.5f;
		for (int i = 0; (float)i < num; i++)
		{
			Surface.PaintPixelNoRetry(base.transform.position + segment * ((float)i / num));
		}
	}
}
