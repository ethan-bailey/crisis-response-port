namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableTeamMember
{
	public string firstName = "Name";

	public string lastName = "Surname";

	public bool male;

	public int age = 35;

	public float height = 1.75f;

	public float weight = 85f;

	public SerializableOperatorAttributes attributes = new SerializableOperatorAttributes();

	public List<SerializableOperatorSkill> skills = new List<SerializableOperatorSkill>();

	public List<SerializableActivity> schedule = new List<SerializableActivity>();

	public int shift;

	public int indexInShift;

	public SerializableTeamMember()
	{
	}

	public SerializableTeamMember(SerializableTeamMember data)
	{
		Overwrite(data);
	}

	public void Overwrite(SerializableTeamMember data)
	{
		firstName = data.firstName;
		lastName = data.lastName;
		male = data.male;
		age = data.age;
		height = data.height;
		weight = data.weight;
		attributes.Overwrite(data.attributes);
		skills.Clear();
		skills.InsertRange(0, data.skills);
		schedule.InsertRange(0, data.schedule);
		shift = data.shift;
		indexInShift = data.indexInShift;
	}

	public void SetShift(int shiftIndex, int indexInShift)
	{
		shift = shiftIndex;
		this.indexInShift = indexInShift;
	}
}

}
