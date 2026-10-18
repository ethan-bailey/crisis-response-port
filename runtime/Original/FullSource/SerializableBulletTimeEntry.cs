namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using FullSerializer;

[fsObject("1", new Type[] { })]
public class SerializableBulletTimeEntry
{
	public bool enabled;

	public float rate = 0.1f;

	public float easeIn = 0.5f;

	public float duration = 0.5f;

	public float easeOut = 1f;

	public SerializableBulletTimeEntry()
	{
	}

	public SerializableBulletTimeEntry(bool enabled, float rate, float easeIn, float duration, float easeOut)
	{
		this.enabled = enabled;
		this.rate = rate;
		this.easeIn = easeIn;
		this.duration = duration;
		this.easeOut = easeOut;
	}

	public bool Different(SerializableBulletTimeEntry other)
	{
		bool flag = false;
		flag |= enabled != other.enabled;
		flag |= rate != other.rate;
		flag |= easeIn != other.easeIn;
		flag |= duration != other.duration;
		return flag | (easeOut != other.easeOut);
	}
}

}
