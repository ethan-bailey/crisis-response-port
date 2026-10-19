namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
	public enum Mode
	{
		None = 0,
		Operator = 1,
		Team = 2,
		Marksman = 3
	}

	public Vector3 startPosition;

	public bool marksmanEnabled;

	[Header("Team")]
	public List<TutorialEquipment> equipment = new List<TutorialEquipment>();

	public TutorialTeamBehavior behavior = new TutorialTeamBehavior();

	public List<SerializableEquipment> Equipment()
	{
		List<SerializableEquipment> list = new List<SerializableEquipment>();
		foreach (TutorialEquipment item in equipment)
		{
			list.Add(new SerializableEquipment(item));
		}
		return list;
	}

	public SerializableTeamBehavior Behavior()
	{
		return new SerializableTeamBehavior(behavior);
	}
}

}
