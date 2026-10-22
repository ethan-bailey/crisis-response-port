namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("NEXT-AFTER-0.0.8.005", new Type[] { })]
public class SerializableLocationSettings_NEXT_AFTER_0_0_8_005
{
	public bool generateLevel;

	public int minRoomCount = 5;

	public int maxRoomCount = 6;

	public float minRoomLength = 2f;

	public float maxRoomLength = 8f;

	public float openDoorsProbability = 0.2f;

	public float lockedDoorsProbability = 0.5f;

	public float riggedDoorsProbability = 0.025f;

	public float obstacleFrequency;
}

}
