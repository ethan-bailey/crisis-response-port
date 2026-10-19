namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections;

public static class EnumHelper
{
	public static int GetEnumIndex(Type enumClass, int value, int fallback = 0)
	{
		int num = 0;
		Array values = Enum.GetValues(enumClass);
		IEnumerator enumerator = values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if ((int)enumerator.Current == value)
			{
				return num;
			}
			num++;
		}
		return fallback;
	}

	public static int GetEnumValue(Type enumClass, int index, int fallback = 0)
	{
		int num = 0;
		Array values = Enum.GetValues(enumClass);
		IEnumerator enumerator = values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (num == index)
			{
				return (int)enumerator.Current;
			}
			num++;
		}
		return fallback;
	}
}

}
