namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class OccupantMorale : MonoBehaviour
{
	public CharacterPsychologyDriver driver = new CharacterPsychologyDriver();

	private CharacterBody self;

	public float Value
	{
		get
		{
			return (!self.IsRestrained) ? (1f - driver.Calculate(self)) : 0f;
		}
	}

	private void Awake()
	{
		self = GetComponent<CharacterBody>();
	}

	private void Start()
	{
		float morale = self.Blackboard.morale;
		float num = (1f - morale) * 10f;
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
		list.Add(5);
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
				driver.Add(new HealthMeasure(Random.Range(3f, 20f), false, num4));
				break;
			case 1:
				driver.Add(new ShockMeasure(Random.Range(1f, 10f), false, num4));
				break;
			case 2:
				driver.Add(new ConcussionMeasure(Random.Range(1f, 10f), false, num4));
				break;
			case 3:
				driver.Add(new TargetProximityMeasure(Random.Range(1.5f, 3f), 0.1f, false, num4));
				break;
			case 4:
				driver.Add(new TargetWeaponMeasure(false, num4));
				break;
			case 5:
				driver.Add(new WoundedAlliesMeasure(false, num4));
				break;
			}
			list2.RemoveAt(index);
		}
		driver.Add(new GrenadeMeasure(false, Random.Range(3f, 6f)));
	}
}

}
