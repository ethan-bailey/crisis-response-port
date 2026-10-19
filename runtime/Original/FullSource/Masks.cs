namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public sealed class Masks
{
	private static Masks instance = null;

	private static readonly object padlock = new object();

	public int AffectedByExplosions { get; private set; }

	public int AffectedByMuzzleBlasts { get; private set; }

	public int Aim { get; private set; }

	public int Casings { get; private set; }

	public int Characters { get; private set; }

	public int Doors { get; private set; }

	public int FogOfWar { get; private set; }

	public int Weapons { get; private set; }

	public int Interactables { get; private set; }

	public int Limbs { get; private set; }

	public int Obstacles { get; private set; }

	public int ObstaclesWalls { get; private set; }

	public int ObstaclesDoors { get; private set; }

	public int ObstaclesProps { get; private set; }

	public int Walls { get; private set; }

	public int BallisticTargetsSansLimbs { get; private set; }

	public int ExplosionShockwave { get; private set; }

	public int MarksmanObstacles { get; private set; }

	public int BallisticTargets { get; private set; }

	public int PhysicalProps { get; private set; }

	public int Rooms { get; private set; }

	public int Windows { get; private set; }

	public int ExteriorGeometry { get; private set; }

	public int Smoke { get; private set; }

	public int ReflectionProbesCullingMask { get; private set; }

	public static Masks Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new Masks();
				}
				return instance;
			}
		}
	}

	private Masks()
	{
		Aim = LayerMask.GetMask("AimQuad");
		Casings = LayerMask.GetMask("Casings");
		Characters = LayerMask.GetMask("Characters");
		Doors = LayerMask.GetMask("Doors");
		FogOfWar = LayerMask.GetMask("Fog of war");
		Weapons = LayerMask.GetMask("Weapons");
		Interactables = LayerMask.GetMask("Interactables");
		Limbs = LayerMask.GetMask("Limbs");
		Obstacles = LayerMask.GetMask("Obstacle geometry");
		Walls = LayerMask.GetMask("Wall geometry");
		PhysicalProps = LayerMask.GetMask("Physical props");
		Rooms = LayerMask.GetMask("Rooms");
		Windows = LayerMask.GetMask("Windows");
		Smoke = LayerMask.GetMask("Smoke");
		ExteriorGeometry = LayerMask.GetMask("Exterior geometry") | Windows;
		AffectedByExplosions = PhysicalProps | Doors | Limbs;
		AffectedByMuzzleBlasts = PhysicalProps;
		BallisticTargetsSansLimbs = Obstacles | Walls | ExteriorGeometry | Doors | PhysicalProps;
		ExplosionShockwave = Walls | ExteriorGeometry | Doors;
		BallisticTargets = Limbs | Obstacles | Walls | ExteriorGeometry | Doors | PhysicalProps | Windows;
		ObstaclesWalls = Obstacles | Walls;
		ObstaclesDoors = Doors | Obstacles;
		ObstaclesProps = Obstacles | PhysicalProps;
		MarksmanObstacles = Obstacles | Walls | LayerMask.GetMask("Exterior geometry") | Doors | PhysicalProps | Limbs;
		ReflectionProbesCullingMask = LayerMask.GetMask("Default") | Doors | Obstacles | ExteriorGeometry | PhysicalProps | Walls;
	}
}

}
