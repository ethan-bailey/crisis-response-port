using System;
using UnityEngine;

[Serializable]
public class SpawnArea
{
	public string name = "Spawn area";

	public float xMin;

	public float xMax;

	public float y = 1.25f;

	public float zMin = -1.5f;

	public float zMax = -1.5f;

	public float walkableMin;

	public float walkableMax;

	public float probability = 1f;

	[NonSerialized]
	public float probabilityLow;

	[NonSerialized]
	public float probabilityHigh = 1f;

	public Vector3 RandomPosition()
	{
		return new Vector3(UnityEngine.Random.Range(xMin, xMax), y, UnityEngine.Random.Range(zMin, zMax));
	}
}
