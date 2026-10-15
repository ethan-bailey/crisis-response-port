using UnityEngine;

public class InverseKinematics2D
{
	private static float epsilon = 0.0001f;

	public static bool SolveTwoBoneAnalytic(out float angle1, out float angle2, bool solvePosAngle2, float length1, float length2, float targetX, float targetY)
	{
		bool result = true;
		float num = targetX * targetX + targetY * targetY;
		float num2 = 2f * length1 * length2;
		float num3;
		float num4;
		if (num2 > epsilon)
		{
			num3 = (num - length1 * length1 - length2 * length2) / num2;
			if ((double)num3 < -1.0 || (double)num3 > 1.0)
			{
				result = false;
			}
			num3 = Mathf.Max(-1f, Mathf.Min(1f, num3));
			angle2 = Mathf.Acos(num3);
			if (!solvePosAngle2)
			{
				angle2 = 0f - angle2;
			}
			num4 = Mathf.Sin(angle2);
		}
		else
		{
			float num5 = (length1 + length2) * (length1 + length2);
			if (num < num5 - epsilon || num > num5 + epsilon)
			{
				result = false;
			}
			angle2 = 0f;
			num3 = 1f;
			num4 = 0f;
		}
		float num6 = length1 + length2 * num3;
		float num7 = length2 * num4;
		float y = targetY * num6 - targetX * num7;
		float x = targetX * num6 + targetY * num7;
		angle1 = Mathf.Atan2(y, x);
		return result;
	}
}
