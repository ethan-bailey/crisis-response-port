namespace Crisis.Runtime.FullSource
{
using System;

public static class CrMath
{
	public static float RoundTruncate(float f, int digits)
	{
		if (f == 0f)
		{
			return 0f;
		}
		double num = f;
		double num2 = Math.Pow(10.0, Math.Floor(Math.Log10(Math.Abs(num))) + 1.0 - (double)digits);
		return (float)(num2 * Math.Truncate(num / num2));
	}
}

}
