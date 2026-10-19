namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class OccupantFear : MonoBehaviour
{
	public CharacterPsychologyDriver driver = new CharacterPsychologyDriver();

	private CharacterBody self;

	public float Value
	{
		get
		{
			return driver.Calculate(self);
		}
	}

	private void Start()
	{
		self = GetComponent<CharacterBody>();
		float fear = self.Blackboard.fear;
		float num = fear * 5f;
		if (num == 0f)
		{
			return;
		}
		float num2 = num;
		List<int> list = new List<int>();
		list.Add(0);
		list.Add(1);
		list.Add(2);
		list.Add(3);
		list.Add(4);
		List<int> list2 = list;
		while (list2.Count > 0)
		{
			int index = Random.Range(0, list2.Count);
			int num3 = list2[index];
			float num4 = ((list2.Count <= 1) ? num : Random.Range(num * 0.2f, num));
			num -= num4;
			switch (num3)
			{
			case 0:
				driver.Add(new TargetProximityMeasure(2f, 0.1f, false, num4));
				break;
			case 1:
				driver.Add(new TargetWeaponMeasure(false, num4));
				break;
			case 2:
				driver.Add(new ShockMeasure(Random.Range(0.5f, 3f), false, num4));
				break;
			case 3:
				driver.Add(new ConcussionMeasure(1f, false, num4));
				break;
			case 4:
				driver.Add(new AudioMeasure(Random.Range(1f, 10f), 1f, false, num4));
				break;
			}
			list2.RemoveAt(index);
		}
		driver.Add(new GrenadeMeasure(false, Random.Range(3f, 6f)));
		driver.Add(new ComplianceMeasure(5f, 5f));
		if (Global.Context.Mission.Marksman != null)
		{
			driver.Add(new MarksmanMeasure(Global.Context.Mission.Marksman, false, 1f));
		}
	}
}

}
