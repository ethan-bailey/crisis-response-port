namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("1", new Type[] { })]
public class SerializableRangeSlider
{
	public float low;

	public float high = 1f;

	public SerializableRangeSlider()
	{
	}

	public SerializableRangeSlider(float low, float high)
	{
		this.low = low;
		this.high = high;
	}

	public bool Different(SerializableRangeSlider other)
	{
		bool flag = false;
		flag |= low != other.low;
		return flag | (high != other.high);
	}
}

}
