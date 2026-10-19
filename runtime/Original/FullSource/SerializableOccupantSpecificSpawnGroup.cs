namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableOccupantSpecificSpawnGroup
{
	public string name = "Occupant group";

	public bool enabled = true;

	public List<SerializableOccupantSpawn> members = new List<SerializableOccupantSpawn>();
}

}
