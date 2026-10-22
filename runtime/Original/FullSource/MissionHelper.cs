namespace Crisis.Runtime.FullSource
{
using Object = UnityEngine.Object;
using UnityEngine;

public class MissionHelper
{
	private static MissionHelper instance = null;

	private static readonly object padlock = new object();

	public static MissionHelper Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new MissionHelper();
				}
				return instance;
			}
		}
	}

	public Mission Mission { get; private set; }

	public Transform EffectsContainer
	{
		get
		{
			return Mission.EffectsContainer;
		}
	}

	public Transform CensorBoxContainer
	{
		get
		{
			return Mission.CensorBoxContainer;
		}
	}

	public Transform GibsContainer
	{
		get
		{
			return Mission.GibsContainer;
		}
	}

	public Transform FloorPoolsContainer
	{
		get
		{
			return Mission.FloorPoolsContainer;
		}
	}

	public BloodStream BloodStreamPrefab
	{
		get
		{
			return Mission.bloodStreamPrefab;
		}
	}

	public FloorPoolPainter StreamPoolPrefab
	{
		get
		{
			return Mission.streamPoolPrefab;
		}
	}

	public FloorPoolPainter FloorPoolPrefab
	{
		get
		{
			return Mission.floorPoolPrefab;
		}
	}

	public WallStreakPainter WallStreakPrefab
	{
		get
		{
			return Mission.wallStreakPrefab;
		}
	}

	public GameObject HeadCensorPrefab
	{
		get
		{
			return Mission.headCensorPrefab;
		}
	}

	public ParticleSystem WeaponSmokeEmitter
	{
		get
		{
			return Mission.WeaponSmokeEmitter;
		}
	}

	public Material RenderMaterialInstance { get; private set; }

	public void Initialize(Mission mission)
	{
		Mission = mission;
		RenderMaterialInstance = new Material(Shader.Find("GUI/Text Shader"))
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		RenderMaterialInstance.shader.hideFlags = HideFlags.HideAndDontSave;
	}

	public void Release()
	{
		if (RenderMaterialInstance != null)
		{
			Object.Destroy(RenderMaterialInstance);
		}
	}
}

}
