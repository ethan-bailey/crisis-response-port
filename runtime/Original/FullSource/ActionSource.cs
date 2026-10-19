namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class ActionSource
{
	public enum Type
	{
		Character = 0,
		Audio = 1,
		Position = 2,
		Grenade = 3,
		Other = 4,
		Marksman = 5
	}

	public Type type = Type.Other;

	public CharacterBody body;

	public Transform transform;

	public float position;
}

}
