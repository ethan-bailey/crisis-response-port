namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class RecoilManager : MonoBehaviour
{
	private class Recoil
	{
		public float startTime;

		public float endTime;

		public float power;
	}

	private List<Recoil> recoils;

	public void Add(float _startTime, float _duration, float _power)
	{
		recoils.Add(new Recoil
		{
			startTime = _startTime,
			endTime = _duration,
			power = _power
		});
	}

	public float Value()
	{
		float time = Time.time;
		float num = 0f;
		for (int num2 = recoils.Count - 1; num2 >= 0; num2--)
		{
			Recoil recoil = recoils[num2];
			if (time >= recoil.endTime)
			{
				recoils.RemoveAt(num2);
			}
			else
			{
				num += recoil.power;
			}
		}
		return num;
	}
}

}
