using Random = UnityEngine.Random;
using UnityEngine;

public static class ArmorClass
{
	public enum Type
	{
		None = 0,
		TypeI = 1,
		TypeIIA = 2,
		TypeII = 3,
		TypeIIIA = 4,
		TypeIII = 5,
		TypeIV = 6
	}

	public static bool PenetrationTest(Type type, float damage, Projectile projectile)
	{
		float distance = 0f;
		float num = 0f;
		float num2 = 1f;
		switch (type)
		{
		case Type.TypeI:
			distance = 0.005f;
			num = 1500f;
			num2 = 1f;
			break;
		case Type.TypeIIA:
			distance = 0.005f;
			num = 2500f;
			num2 = 1f;
			break;
		case Type.TypeII:
			distance = 0.005f;
			num = 4000f;
			num2 = 2f;
			break;
		case Type.TypeIIIA:
			distance = 0.005f;
			num = 6000f;
			num2 = 3f;
			break;
		case Type.TypeIII:
			distance = 0.01f;
			num = 6000f;
			num2 = 4f;
			break;
		case Type.TypeIV:
			distance = 0.015f;
			num = 8000f;
			num2 = 0.5f;
			break;
		}
		float num3 = damage / (num * num2);
		float num4 = Random.Range(1f - num3, 1f - num3 * 0.25f);
		float num5 = projectile.EnergyToPenetrate(distance, num) * num4;
		bool result = num5 < projectile.Energy;
		projectile.ConvertEnergyToFriction(num5);
		return result;
	}
}
