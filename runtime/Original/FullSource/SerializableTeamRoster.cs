namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableTeamRoster
{
	public List<SerializableTeamMember> members = new List<SerializableTeamMember>();

	public void Overwrite(SerializableTeamRoster other)
	{
		members.Clear();
		foreach (SerializableTeamMember member in other.members)
		{
			members.Add(new SerializableTeamMember(member));
		}
	}

	public void PerformScheduledActivities(int day)
	{
		int num = day % 5;
		foreach (SerializableTeamMember member in members)
		{
			for (int i = num * 4; i < (num + 1) * 4; i++)
			{
				if (member.schedule[i].enabled)
				{
					int activity = member.schedule[i].activity;
					member.skills[activity].Drill(1);
				}
			}
		}
	}

	public void UpdateSkills()
	{
		foreach (SerializableTeamMember member in members)
		{
			foreach (SerializableOperatorSkill skill in member.skills)
			{
				skill.Decline(1);
			}
		}
	}

	public void BuildShifts()
	{
		int num = 0;
		int num2 = 0;
		int num3 = members.Count / 4;
		foreach (SerializableTeamMember member in members)
		{
			member.SetShift(num, num2++);
			if (num2 == num3)
			{
				num++;
				num2 = 0;
			}
		}
	}
}

}
