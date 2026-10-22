namespace Crisis.Runtime.FullSource
{
using System;
using FullSerializer;

[fsObject("1", new Type[] { })]
public class SerializableMarksmanRifle
{
	public string prefabName = "AR-30";

	public string ammunitionPrefab = "308 Winchester BSP";

	public SerializableMarksmanRifle Overwrite(SerializableMarksmanRifle data)
	{
		prefabName = data.prefabName;
		ammunitionPrefab = data.ammunitionPrefab;
		return this;
	}
}

}
