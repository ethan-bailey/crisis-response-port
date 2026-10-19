namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableOccupantRandomizedSpawnGroup
{
	public string name = "Occupant";

	public bool enabled = true;

	public List<string> bodyPrefabs = new List<string> { "Occupant" };

	public int minCount = 3;

	public int maxCount = 6;

	public float minCoord = 1f;

	public float maxCoord = 31f;

	public float minMorale;

	public float maxMorale = 1f;

	public float minAggression;

	public float maxAggression = 1f;

	public float minFear;

	public float maxFear = 1f;

	public float minHitAggression = -0.25f;

	public float maxHitAggression = 0.25f;

	public float minHitFear = -0.25f;

	public float maxHitFear = 0.25f;

	public float minFitness;

	public float maxFitness = 1f;

	public float minShockResistance;

	public float maxShockResistance = 0.5f;

	public float minBlindFireChance;

	public float maxBlindFireChance = 0.5f;

	public float minBlindFireDistance = 2f;

	public float maxBlindFireDistance = 3f;
}

}
