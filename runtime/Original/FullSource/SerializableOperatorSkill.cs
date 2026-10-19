namespace Crisis.Runtime.FullSource
{
using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SerializableOperatorSkill
{
	public enum Enum
	{
		Pistols = 0,
		RiflesAndSMGs = 1,
		Shotguns = 2,
		Marksmanship = 3,
		Explosives = 4,
		Breaching = 5,
		Lockpicking = 6,
		ThreatIdentification = 7,
		FirstAid = 8
	}

	public string name = "Skill";

	public string description = "Skill description.";

	public float current = 0.5f;

	public float maximum = 0.5f;

	public int daysUnmaintained;

	[NonSerialized]
	public UnityEvent onChanged = new UnityEvent();

	public int UpkeepPeriod
	{
		get
		{
			if (current >= 0.9f)
			{
				return 1;
			}
			if (current >= 0.75f)
			{
				return 2;
			}
			if (current >= 0.6f)
			{
				return 3;
			}
			if (current >= 0.45f)
			{
				return 4;
			}
			if (current >= 0.3f)
			{
				return 5;
			}
			if (current >= 0.15f)
			{
				return 6;
			}
			return 7;
		}
	}

	public SerializableOperatorSkill(SerializableOperatorSkillTemplate template, float maximum, float currentAsRatioOfMaximum)
	{
		name = template.name;
		description = template.description;
		this.maximum = maximum;
		current = maximum * currentAsRatioOfMaximum;
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext context)
	{
		onChanged = new UnityEvent();
	}

	public void Decline(int days)
	{
		float num = current;
		current = Mathf.Clamp01(current - CampaignManager.DailySkillCurrentDecline * (float)days);
		daysUnmaintained += days;
		if (daysUnmaintained > UpkeepPeriod)
		{
			maximum = Mathf.Clamp01(maximum - CampaignManager.DailySkillMaximumDecline * (float)days);
		}
		if (num != current)
		{
			onChanged.Invoke();
		}
	}

	public void Drill(int hours)
	{
		float num = current;
		daysUnmaintained = 0;
		current = Mathf.Clamp(current + (float)hours * CampaignManager.DrillingHourIncrease, 0f, maximum);
		if (num != current)
		{
			onChanged.Invoke();
		}
	}
}

}
