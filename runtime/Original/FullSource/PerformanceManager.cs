namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class PerformanceManager
{
	private static PerformanceManager instance = null;

	private static readonly object padlock = new object();

	private MissionHelper helper;

	private List<GameObject> bloodStreams = new List<GameObject>();

	private List<GameObject> floorPools = new List<GameObject>();

	private LinkedList<GameObject> goreHitEffects = new LinkedList<GameObject>();

	private LinkedList<GameObject> environmentHitEffects = new LinkedList<GameObject>();

	private LinkedList<GameObject> environmentHitMeshes = new LinkedList<GameObject>();

	private LinkedList<GameObject> wallStreaks = new LinkedList<GameObject>();

	private Transform gibsContainer;

	private SerializableExtraSettings extraSettings;

	private int maxBloodParticles;

	private float particlesMultiplier;

	private int currentWoundStreakCount;

	private int currentSoakSpotCount;

	private int currentWallStreakCount;

	private int currentFloorStreakCount;

	private int currentGibCount;

	public static PerformanceManager Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new PerformanceManager();
				}
				return instance;
			}
		}
	}

	public int MaxEnvironmentHitEffects { get; private set; }

	public int MaxEnvironmentHitMeshes { get; private set; }

	public int MaxGoreHitEffects { get; private set; }

	public int MaxSoakSpots { get; private set; }

	public int MaxWoundStreaks { get; private set; }

	public int MaxBloodStreams { get; private set; }

	public int MaxWallStreaks { get; private set; }

	public int MaxFloorStreaks { get; private set; }

	public int MaxFloorPools { get; private set; }

	public int MaxBloodParticles
	{
		get
		{
			return Mathf.RoundToInt((maxBloodParticles >= 0) ? ((float)maxBloodParticles * Mathf.Clamp(particlesMultiplier * 0.2f, 1f, 3f)) : 100000f);
		}
	}

	public int MaxGibs { get; private set; }

	public float WoundStreakLifetime { get; private set; }

	public int CurrentEnvironmentHitEffects
	{
		get
		{
			return environmentHitEffects.Count;
		}
	}

	public int CurrentEnvironmentHitMeshes
	{
		get
		{
			return environmentHitMeshes.Count;
		}
	}

	public int CurrentGoreHitEffects
	{
		get
		{
			return goreHitEffects.Count;
		}
	}

	public int CurrentSoakSpots
	{
		get
		{
			return currentSoakSpotCount;
		}
	}

	public int CurrentWoundStreaks
	{
		get
		{
			return currentWoundStreakCount;
		}
	}

	public int CurrentBloodStreams
	{
		get
		{
			return bloodStreams.Count;
		}
	}

	public int CurrentWallStreaks
	{
		get
		{
			return currentWallStreakCount;
		}
	}

	public int CurrentFloorStreaks
	{
		get
		{
			return currentFloorStreakCount;
		}
	}

	public int CurrentFloorPools
	{
		get
		{
			return floorPools.Count;
		}
	}

	public int CurrentBloodParticles
	{
		get
		{
			return -1;
		}
	}

	public int CurrentGibs
	{
		get
		{
			return currentGibCount;
		}
	}

	public bool EnvironmentHitEffectsEnabled
	{
		get
		{
			return MaxEnvironmentHitEffects != 0;
		}
	}

	public bool EnvironmentHitMeshesEnabled
	{
		get
		{
			return MaxEnvironmentHitMeshes != 0;
		}
	}

	public bool GoreHitEffectsEnabled
	{
		get
		{
			return MaxGoreHitEffects != 0;
		}
	}

	public bool SoakSpotsEnabled
	{
		get
		{
			return MaxSoakSpots != 0;
		}
	}

	public bool WoundStreaksEnabled
	{
		get
		{
			return MaxWoundStreaks != 0;
		}
	}

	public bool BloodStreamsEnabled
	{
		get
		{
			return MaxBloodStreams != 0;
		}
	}

	public bool WallStreaksEnabled
	{
		get
		{
			return MaxWallStreaks != 0;
		}
	}

	public bool FloorStreaksEnabled
	{
		get
		{
			return MaxFloorStreaks != 0;
		}
	}

	public bool FloorPoolsEnabled
	{
		get
		{
			return MaxFloorPools != 0;
		}
	}

	public bool BloodParticlesEnabled
	{
		get
		{
			return maxBloodParticles != 0;
		}
	}

	public bool GibsEnabled
	{
		get
		{
			return MaxGibs != 0;
		}
	}

	public bool UnlimitedEnvironmentHitEffects
	{
		get
		{
			return MaxEnvironmentHitEffects < 0;
		}
	}

	public bool UnlimitedEnvironmentHitMeshes
	{
		get
		{
			return MaxEnvironmentHitMeshes < 0;
		}
	}

	public bool UnlimitedGoreHitEffects
	{
		get
		{
			return MaxGoreHitEffects < 0;
		}
	}

	public bool UnlimitedSoakSpots
	{
		get
		{
			return MaxSoakSpots < 0;
		}
	}

	public bool UnlimitedWoundStreaks
	{
		get
		{
			return MaxWoundStreaks < 0;
		}
	}

	public bool UnlimitedBloodStreams
	{
		get
		{
			return MaxBloodStreams < 0;
		}
	}

	public bool UnlimitedWallStreaks
	{
		get
		{
			return MaxWallStreaks < 0;
		}
	}

	public bool UnlimitedFloorStreaks
	{
		get
		{
			return MaxFloorStreaks < 0;
		}
	}

	public bool UnlimitedFloorPools
	{
		get
		{
			return MaxFloorPools < 0;
		}
	}

	public bool UnlimitedBloodParticles
	{
		get
		{
			return maxBloodParticles < 0;
		}
	}

	public bool UnlimitedGibs
	{
		get
		{
			return MaxGibs < 0;
		}
	}

	private PerformanceManager()
	{
		MaxEnvironmentHitEffects = 10;
		MaxEnvironmentHitMeshes = 20;
		MaxGoreHitEffects = 10;
		MaxSoakSpots = 10;
		MaxWoundStreaks = 5;
		MaxBloodStreams = 3;
		MaxWallStreaks = 5;
		MaxFloorStreaks = 5;
		MaxFloorPools = 5;
		maxBloodParticles = 750;
		MaxGibs = 50;
		WoundStreakLifetime = 3f;
		helper = MissionHelper.Instance;
		OnPerformanceSettingsChanged(Global.Context.Settings.Performance);
		Global.Context.Settings.Performance.onChanged.AddListener(OnPerformanceSettingsChanged);
		Global.Context.Settings.Extras.onChanged.AddListener(OnExtraSettingsChanged);
		extraSettings = Global.Context.Settings.Extras;
		particlesMultiplier = extraSettings.emissionsModifier;
	}

	public void OnPerformanceSettingsChanged(SerializablePerformanceSettings settings)
	{
		MaxEnvironmentHitEffects = settings.maxEnvironmentHitEffects;
		MaxEnvironmentHitMeshes = settings.maxEnvironmentHitMeshes;
		MaxGoreHitEffects = settings.maxGoreHitEffects;
		MaxSoakSpots = settings.maxSoakSpots;
		MaxWoundStreaks = settings.maxWoundStreaks;
		MaxBloodStreams = settings.maxBloodStreams;
		MaxWallStreaks = settings.maxWallStreaks;
		MaxFloorStreaks = settings.maxFloorStreaks;
		MaxFloorPools = settings.maxFloorPools;
		maxBloodParticles = settings.maxBloodParticles;
		MaxGibs = settings.maxGibs;
		WoundStreakLifetime = settings.woundStreakLifetime;
		Mission mission = Global.Context.Mission;
		if (!(mission == null))
		{
			ApplyNewMaxCounts();
			mission.UI.resourceCounter.OnPerformanceSettingsChanged(settings);
		}
	}

	private void OnExtraSettingsChanged(SerializableExtraSettings settings)
	{
		particlesMultiplier = settings.emissionsModifier;
	}

	public void OnMissionStart(Transform gibsContainer)
	{
		this.gibsContainer = gibsContainer;
	}

	public void LateUpdate()
	{
		for (int num = floorPools.Count - 1; num >= 0; num--)
		{
			if (floorPools[num] == null)
			{
				floorPools.RemoveAt(num);
			}
		}
		for (int num2 = bloodStreams.Count - 1; num2 >= 0; num2--)
		{
			if (bloodStreams[num2] == null)
			{
				bloodStreams.RemoveAt(num2);
			}
		}
	}

	private void ApplyNewMaxCounts()
	{
		while (environmentHitMeshes.Count > 0 && environmentHitMeshes.Count > MaxEnvironmentHitMeshes)
		{
			if (environmentHitMeshes.First.Value != null)
			{
				Object.Destroy(environmentHitMeshes.First.Value.gameObject);
			}
			environmentHitMeshes.RemoveFirst();
		}
		while (environmentHitEffects.Count > 0 && environmentHitEffects.Count > MaxEnvironmentHitEffects)
		{
			if (environmentHitEffects.First.Value != null)
			{
				Object.Destroy(environmentHitEffects.First.Value.gameObject);
			}
			environmentHitEffects.RemoveFirst();
		}
		while (goreHitEffects.Count > 0 && goreHitEffects.Count > MaxGoreHitEffects)
		{
			if (goreHitEffects.First.Value != null)
			{
				Object.Destroy(goreHitEffects.First.Value.gameObject);
			}
			goreHitEffects.RemoveFirst();
		}
		while (wallStreaks.Count > 0 && wallStreaks.Count > MaxWallStreaks)
		{
			if (wallStreaks.First.Value != null)
			{
				Object.Destroy(wallStreaks.First.Value.gameObject);
			}
			wallStreaks.RemoveFirst();
		}
	}

	public void RemoveAllResources()
	{
		while (environmentHitMeshes.Count > 0)
		{
			if (environmentHitMeshes.First.Value != null)
			{
				Object.Destroy(environmentHitMeshes.First.Value.gameObject);
			}
			environmentHitMeshes.RemoveFirst();
		}
		while (environmentHitEffects.Count > 0)
		{
			if (environmentHitEffects.First.Value != null)
			{
				Object.Destroy(environmentHitEffects.First.Value.gameObject);
			}
			environmentHitEffects.RemoveFirst();
		}
		while (goreHitEffects.Count > 0)
		{
			if (goreHitEffects.First.Value != null)
			{
				Object.Destroy(goreHitEffects.First.Value.gameObject);
			}
			goreHitEffects.RemoveFirst();
		}
		while (wallStreaks.Count > 0)
		{
			if (wallStreaks.First.Value != null)
			{
				Object.Destroy(wallStreaks.First.Value.gameObject);
			}
			wallStreaks.RemoveFirst();
		}
		foreach (GameObject bloodStream in bloodStreams)
		{
			Object.Destroy(bloodStream);
		}
		foreach (GameObject floorPool in floorPools)
		{
			Object.Destroy(floorPool);
		}
		foreach (Transform item in gibsContainer)
		{
			Object.Destroy(item.gameObject);
		}
	}

	public BloodStream CreateBloodStream(Emitter bloodEmitter)
	{
		if (!UnlimitedBloodStreams && bloodStreams.Count >= MaxBloodStreams)
		{
			return null;
		}
		BloodStream bloodStream = Object.Instantiate(helper.BloodStreamPrefab);
		bloodStream.Initialize(helper.EffectsContainer, CreateFloorPool(), bloodEmitter, extraSettings.woundPowerMultiplier, extraSettings.emissionsModifier);
		bloodStreams.Add(bloodStream.gameObject);
		return bloodStream;
	}

	public FloorPoolPainter CreateStreamPool()
	{
		if (!UnlimitedFloorPools && floorPools.Count >= MaxFloorPools)
		{
			return null;
		}
		FloorPoolPainter floorPoolPainter = Object.Instantiate(helper.StreamPoolPrefab);
		floorPoolPainter.name = "Stream pool";
		floorPoolPainter.transform.SetParent(helper.FloorPoolsContainer);
		floorPools.Add(floorPoolPainter.gameObject);
		return floorPoolPainter;
	}

	public FloorPoolPainter CreateFloorPool()
	{
		if (!UnlimitedFloorPools && floorPools.Count >= MaxFloorPools)
		{
			return null;
		}
		FloorPoolPainter floorPoolPainter = Object.Instantiate(helper.FloorPoolPrefab);
		floorPoolPainter.name = "Floor pool";
		floorPoolPainter.transform.SetParent(helper.FloorPoolsContainer);
		floorPools.Add(floorPoolPainter.gameObject);
		return floorPoolPainter;
	}

	public BallisticHitEffect CreateGoreHitEffect(BallisticHitEffect prefab)
	{
		if (prefab == null)
		{
			return null;
		}
		if (!UnlimitedGoreHitEffects && goreHitEffects.Count >= MaxGoreHitEffects && goreHitEffects.Count > 0)
		{
			if (goreHitEffects.First.Value != null)
			{
				Object.Destroy(goreHitEffects.First.Value);
			}
			goreHitEffects.RemoveFirst();
		}
		BallisticHitEffect ballisticHitEffect = Object.Instantiate(prefab);
		goreHitEffects.AddLast(ballisticHitEffect.gameObject);
		return ballisticHitEffect;
	}

	public BallisticHitEffect CreateEnvironmentalHitEffect(BallisticHitEffect prefab)
	{
		if (prefab == null)
		{
			return null;
		}
		if (!UnlimitedEnvironmentHitEffects && environmentHitEffects.Count >= MaxEnvironmentHitEffects && environmentHitEffects.Count > 0)
		{
			if (environmentHitEffects.First.Value != null)
			{
				Object.Destroy(environmentHitEffects.First.Value);
			}
			environmentHitEffects.RemoveFirst();
		}
		BallisticHitEffect ballisticHitEffect = Object.Instantiate(prefab);
		environmentHitEffects.AddLast(ballisticHitEffect.gameObject);
		return ballisticHitEffect;
	}

	public WallStreakPainter CreateWallStreak(Transform parent, Vector3 position)
	{
		if (!UnlimitedWallStreaks && wallStreaks.Count >= MaxWallStreaks && wallStreaks.Count > 0)
		{
			if (wallStreaks.First.Value != null)
			{
				Object.Destroy(wallStreaks.First.Value);
			}
			wallStreaks.RemoveFirst();
		}
		WallStreakPainter wallStreakPainter = Object.Instantiate(helper.WallStreakPrefab);
		wallStreakPainter.transform.SetParent(parent);
		wallStreakPainter.transform.position = position;
		wallStreaks.AddLast(wallStreakPainter.gameObject);
		return wallStreakPainter;
	}

	public BallisticHitMeshEffect SpawnEnvironmentalHitMesh(BallisticHitMeshEffect prefab, Projectile projectile, Transform parent)
	{
		if (prefab == null)
		{
			return null;
		}
		if (!UnlimitedEnvironmentHitMeshes && environmentHitMeshes.Count >= MaxEnvironmentHitMeshes && environmentHitMeshes.Count > 0)
		{
			if (environmentHitMeshes.First.Value != null)
			{
				Object.Destroy(environmentHitMeshes.First.Value);
			}
			environmentHitMeshes.RemoveFirst();
		}
		BallisticHitMeshEffect ballisticHitMeshEffect = Object.Instantiate(prefab);
		ballisticHitMeshEffect.Play(parent, projectile);
		environmentHitMeshes.AddLast(ballisticHitMeshEffect.gameObject);
		return ballisticHitMeshEffect;
	}

	public int RequestWoundStreaks(int count)
	{
		if (UnlimitedWoundStreaks)
		{
			return count;
		}
		int num = Mathf.Min(count, MaxWoundStreaks - currentWoundStreakCount);
		currentWoundStreakCount += num;
		return num;
	}

	public void ReturnWoundStreaks(int count)
	{
		if (!UnlimitedWoundStreaks)
		{
			currentWoundStreakCount = Mathf.Max(currentWoundStreakCount - count, 0);
		}
	}

	public int RequestSoakSpots(int count)
	{
		if (UnlimitedSoakSpots)
		{
			return count;
		}
		int num = Mathf.Min(count, MaxSoakSpots - currentSoakSpotCount);
		currentSoakSpotCount += num;
		return num;
	}

	public void ReturnSoakSpots(int count)
	{
		if (!UnlimitedSoakSpots)
		{
			currentSoakSpotCount = Mathf.Max(currentSoakSpotCount - count, 0);
		}
	}

	public int RequestFloorStreaks(int count)
	{
		if (UnlimitedFloorStreaks)
		{
			return count;
		}
		int num = Mathf.Min(count, MaxFloorStreaks - currentFloorStreakCount);
		currentFloorStreakCount += num;
		return num;
	}

	public void ReturnFloorStreaks(int count)
	{
		if (!UnlimitedFloorStreaks)
		{
			currentFloorStreakCount = Mathf.Max(currentFloorStreakCount - count, 0);
		}
	}

	public void SpawnGibs(int count, GameObject prefab, Vector3 position, Vector3 force, float scale)
	{
		count = Mathf.Min(count, 100);
		if (!UnlimitedGibs)
		{
			int num = MaxGibs - currentGibCount;
			int num2 = ((num < count) ? (count - num) : 0);
			for (int i = 0; i < num2; i++)
			{
				if (gibsContainer.childCount <= 0)
				{
					break;
				}
				Object.Destroy(gibsContainer.GetChild(0).gameObject);
			}
			currentGibCount -= num2;
			currentGibCount += count;
		}
		for (int j = 0; j < count; j++)
		{
			GameObject gameObject = Object.Instantiate(prefab);
			gameObject.transform.SetParent(gibsContainer);
			gameObject.transform.position = position + Random.onUnitSphere * scale * 0.1f;
			gameObject.transform.rotation = Quaternion.LookRotation(Random.onUnitSphere);
			gameObject.transform.localScale = gameObject.transform.localScale * Mathf.Min(scale, 0.75f);
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			force += Random.onUnitSphere * 0.25f;
			component.AddForce(force * Random.Range(0.5f, 1f), ForceMode.Impulse);
		}
	}

	public void Reset()
	{
		floorPools.Clear();
		bloodStreams.Clear();
		goreHitEffects.Clear();
		environmentHitEffects.Clear();
		environmentHitMeshes.Clear();
		currentWoundStreakCount = 0;
		currentSoakSpotCount = 0;
		currentWallStreakCount = 0;
		currentFloorStreakCount = 0;
		currentGibCount = 0;
	}
}

}
