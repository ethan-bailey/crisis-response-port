namespace Crisis.Runtime.FullSource
{
using Object = UnityEngine.Object;
using UnityEngine;

public class ProjectileTrace : MonoBehaviour
{
	[Tooltip("Time in seconds for the line renderer trail to fade out.")]
	[Range(0f, 0.25f)]
	public float traceFadeTime = 0.1f;

	public GameObject hitEffectPrefab;

	private Projectile projectile;

	private ParticleSystem particleSystem;

	private ParticleSystem.MainModule mainModule;

	private LineRenderer lineRenderer;

	private int lineRendererIndex;

	private float timestamp;

	private void Update()
	{
		SetAlpha();
		SetScale(Time.timeScale);
		if (projectile != null && lineRenderer != null)
		{
			float sqrMagnitude = (projectile.transform.position - lineRenderer.GetPosition(lineRendererIndex - 1)).sqrMagnitude;
			if (sqrMagnitude > 1f)
			{
				AddPosition(projectile.transform.position);
			}
			else
			{
				lineRenderer.SetPosition(lineRendererIndex - 1, projectile.transform.position);
			}
		}
	}

	public void Initialize(Projectile projectile)
	{
		timestamp = Time.time;
		this.projectile = projectile;
		particleSystem = GetComponent<ParticleSystem>();
		mainModule = particleSystem.main;
		lineRenderer = GetComponent<LineRenderer>();
		LineRenderer obj = lineRenderer;
		float metricDiameter = projectile.MetricDiameter;
		lineRenderer.endWidth = metricDiameter;
		obj.startWidth = metricDiameter;
		SetScale(Time.timeScale);
		base.transform.SetParent(projectile.transform);
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
		AddPosition(projectile.transform.position);
		AddPosition(projectile.transform.position);
	}

	private void SetScale(float scale)
	{
		ParticleSystem.MinMaxCurve startSpeed = mainModule.startSpeed;
		startSpeed.constantMin = 0f;
		startSpeed.constantMax = scale * 15f;
		mainModule.startSpeed = startSpeed;
	}

	private void SetAlpha()
	{
		if (lineRenderer != null)
		{
			float num = Mathf.Clamp01((Time.time - timestamp) / traceFadeTime);
			Color startColor = lineRenderer.startColor;
			startColor.a = Mathf.Lerp(1f * Time.timeScale, 0f, num);
			LineRenderer obj = lineRenderer;
			Color color = startColor;
			lineRenderer.endColor = color;
			obj.startColor = color;
			if (num >= 1f)
			{
				Object.Destroy(lineRenderer);
			}
		}
	}

	public void AddPosition(Vector3 position)
	{
		if ((bool)lineRenderer)
		{
			lineRenderer.positionCount++;
			lineRenderer.SetPosition(lineRendererIndex, position);
			lineRendererIndex++;
		}
	}

	public void SpawnHitEffect(Vector3 position, Transform parent, Vector3 hitNormal)
	{
		GameObject gameObject = Object.Instantiate(hitEffectPrefab);
		gameObject.transform.SetParent(parent);
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.position = position;
		gameObject.transform.LookAt(position + hitNormal);
	}
}

}
