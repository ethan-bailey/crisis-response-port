using System;
using System.Collections.Generic;
using UnityEngine;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;

public static class StaticUtilities
{
	public static bool RectIntersect(Rect a, Rect b, out Rect intersect)
	{
		Vector2 min = a.min;
		Vector2 max = a.max;
		Vector2 min2 = b.min;
		Vector2 max2 = b.max;
		Vector2 vector = new Vector2(Mathf.Max(min.x, min2.x), Mathf.Max(min.y, min2.y));
		Vector2 vector2 = new Vector2(Mathf.Min(max.x, max2.x), Mathf.Min(max.y, max2.y));
		if (vector.x >= vector2.x || vector.y >= vector2.y)
		{
			intersect = default(Rect);
			return false;
		}
		intersect = new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
		return true;
	}
}
