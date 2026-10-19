namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableOccupantSpawnData
{
	public List<SerializableOccupantRandomizedSpawnGroup> randomGroups = new List<SerializableOccupantRandomizedSpawnGroup>();

	public List<SerializableOccupantSpecificSpawnGroup> specificGroups = new List<SerializableOccupantSpecificSpawnGroup>();
}

}
