namespace Crisis.Runtime.FullSource
{
using System;

[Serializable]
public class SerializableOccupantSpawn
{
	public string name = "Occupant";

	public bool enabled = true;

	public string bodyPrefab = "Occupant";

	public float position;

	public bool negotiable = true;

	public float morale = 0.5f;

	public float aggression = 0.5f;

	public float fear = 0.5f;

	public float hitAggression;

	public float hitFear;

	public float fitness = 0.5f;

	public float shockResistance = 0.5f;

	public float blindFireChance = 0.5f;

	public float blindFireDistance = 3f;

	public SerializableEquipment equipment;

	public SerializableOccupantSpawn()
	{
	}

	public SerializableOccupantSpawn(CharacterTemplate template)
	{
		name = template.name;
		position = template.position;
		morale = template.morale;
		aggression = template.aggression;
		fear = template.fear;
		negotiable = template.negotiable;
		fitness = template.fitness;
		shockResistance = template.shockResistance;
		blindFireChance = template.blindFireChance;
		blindFireDistance = template.blindFireDistance;
		equipment = template.equipment.Copy();
	}
}

}
