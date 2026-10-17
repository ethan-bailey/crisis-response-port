using UnityEngine;

public class CharacterTemplate : MonoBehaviour
{
	public CharacterBody bodyPrefab;

	public DialogTree dialogTree;

	public float morale = 0.5f;

	public float aggression = 0.5f;

	public float fear = 0.5f;

	public float hitAggression;

	public float hitFear;

	public float fitness = 0.5f;

	public float shockResistance = 0.5f;

	public float blindFireChance = 0.5f;

	public float blindFireDistance = 3f;

	public bool negotiable = true;

	public SerializableEquipment equipment = new SerializableEquipment();

	public float position;
}
