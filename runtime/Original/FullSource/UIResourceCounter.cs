namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIResourceCounter : MonoBehaviour
{
	public UIResourceCounterEntry entryPrefab;

	public int frameInterval = 10;

	private PerformanceManager perfMgr;

	private int frameCount;

	private UIResourceCounterEntry environmentHitEffects;

	private UIResourceCounterEntry environmentHitMeshes;

	private UIResourceCounterEntry goreHitEffects;

	private UIResourceCounterEntry soakSpots;

	private UIResourceCounterEntry woundStreaks;

	private UIResourceCounterEntry bloodStreams;

	private UIResourceCounterEntry wallStreaks;

	private UIResourceCounterEntry floorStreaks;

	private UIResourceCounterEntry floorPools;

	private UIResourceCounterEntry bloodParticles;

	private UIResourceCounterEntry gibs;

	private void Start()
	{
		if (!Version.Instance.Staging)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		perfMgr = PerformanceManager.Instance;
		GetComponent<Image>().enabled = true;
		OnPerformanceSettingsChanged(Global.Context.Settings.Performance);
		Global.Context.Settings.Performance.onChanged.AddListener(OnPerformanceSettingsChanged);
	}

	public void OnPerformanceSettingsChanged(SerializablePerformanceSettings settings)
	{
		foreach (Transform item in base.transform)
		{
			Object.Destroy(item.gameObject);
		}
		goreHitEffects = CreateEntry("Gore hit fx", settings.maxGoreHitEffects, false);
		environmentHitEffects = CreateEntry("Env hit fx", settings.maxEnvironmentHitEffects, false);
		environmentHitMeshes = CreateEntry("Env hit holes", settings.maxEnvironmentHitMeshes, false);
		soakSpots = CreateEntry("Soak spots", settings.maxSoakSpots, true);
		bloodStreams = CreateEntry("Streams", settings.maxBloodStreams, true);
		woundStreaks = CreateEntry("Wound streaks", settings.maxWoundStreaks, true);
		wallStreaks = CreateEntry("Wall streaks", settings.maxWallStreaks, true);
		floorStreaks = CreateEntry("Floor streaks", settings.maxFloorStreaks, true);
		floorPools = CreateEntry("Pools", settings.maxFloorPools, true);
		bloodParticles = CreateEntry("Particles", settings.maxBloodParticles, true);
		gibs = CreateEntry("Gibs", settings.maxGibs, false);
	}

	private void Update()
	{
		if (frameCount < frameInterval)
		{
			frameCount++;
			return;
		}
		frameCount = 0;
		goreHitEffects.SetValue(perfMgr.CurrentGoreHitEffects);
		environmentHitEffects.SetValue(perfMgr.CurrentEnvironmentHitEffects);
		environmentHitMeshes.SetValue(perfMgr.CurrentEnvironmentHitMeshes);
		soakSpots.SetValue(perfMgr.CurrentSoakSpots);
		bloodStreams.SetValue(perfMgr.CurrentBloodStreams);
		woundStreaks.SetValue(perfMgr.CurrentWoundStreaks);
		wallStreaks.SetValue(perfMgr.CurrentWallStreaks);
		floorStreaks.SetValue(perfMgr.CurrentFloorStreaks);
		floorPools.SetValue(perfMgr.CurrentFloorPools);
		bloodParticles.SetValue(perfMgr.CurrentBloodParticles);
		gibs.SetValue(perfMgr.CurrentGibs);
	}

	private UIResourceCounterEntry CreateEntry(string label, int max, bool colorize)
	{
		UIResourceCounterEntry uIResourceCounterEntry = Object.Instantiate(entryPrefab);
		uIResourceCounterEntry.Initialize(label, max, base.transform, colorize);
		return uIResourceCounterEntry;
	}
}

}
