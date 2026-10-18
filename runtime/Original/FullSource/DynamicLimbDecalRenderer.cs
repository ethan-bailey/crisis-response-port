namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine;

public class DynamicLimbDecalRenderer : MonoBehaviour
{
	private List<LimbStreak> streaks = new List<LimbStreak>();

	private LimbRenderer limbRenderer;

	private Limb limb;

	private PerformanceManager perfMgr;

	public bool RenderingActive { get; private set; }

	private void Awake()
	{
		limb = GetComponent<Limb>();
		limbRenderer = limb.limbRenderer;
		perfMgr = PerformanceManager.Instance;
	}

	private void Update()
	{
		bool flag = false;
		Vector3 localDownDirection = transform.InverseTransformDirection(Vector3.down);
		int num = 0;
		for (int num2 = streaks.Count - 1; num2 >= 0; num2--)
		{
			LimbStreak limbStreak = streaks[num2];
			if (Time.time - limbStreak.timestamp > limbStreak.lifetime)
			{
				streaks.RemoveAt(num2);
				num++;
			}
			else
			{
				flag = true;
				if (Time.time - limbStreak.resetTimestamp > limbStreak.period)
				{
					limbStreak.Randomize();
				}
				else
				{
					limbStreak.Update(limbRenderer, localDownDirection, transform);
				}
			}
		}
		perfMgr.ReturnWoundStreaks(num);
		bool flag2 = false;
		RenderingActive = flag2 || flag;
		if (RenderingActive)
		{
			Render();
		}
		else
		{
			limbRenderer.SetStatic();
		}
	}

	public void SpawnSoakSpot(Vector3 localPosition, float radiusMeters, float period, float updatePeriod)
	{
	}

	public void SpawnStreaks(int count, Vector3 localPosition, float offsetRadiusMeters, float velocityScale, float lifetime)
	{
		int num = perfMgr.RequestWoundStreaks(count);
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				streaks.Add(new LimbStreak(localPosition, offsetRadiusMeters, velocityScale, lifetime));
			}
			RenderingActive = true;
		}
	}

	private void Render()
	{
		if (!limbRenderer.IsDynamic)
		{
			limbRenderer.SetDynamic();
		}
		limbRenderer.BeginBloodPass();
		foreach (LimbStreak streak in streaks)
		{
			streak.Render(limbRenderer, transform);
		}
		limbRenderer.EndPass();
	}
}

}
