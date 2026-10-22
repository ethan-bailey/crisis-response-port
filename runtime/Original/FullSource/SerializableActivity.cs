namespace Crisis.Runtime.FullSource
{
using System;

[Serializable]
public class SerializableActivity
{
	public int day;

	public int slot;

	public int activity;

	public bool enabled;

	public SerializableActivity()
	{
	}

	public SerializableActivity(SerializableActivity data)
	{
		Overwrite(data);
	}

	public void Overwrite(SerializableActivity data)
	{
		day = data.day;
		slot = data.slot;
		activity = data.activity;
		enabled = data.enabled;
	}
}

}
