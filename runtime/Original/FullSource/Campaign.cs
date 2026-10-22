namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Campaign : MonoBehaviour
{
	[Header("Team")]
	[Tooltip("How many operators are in the team at campaign start?")]
	public int startOperatorCount = 16;

	[Header("Missions")]
	public List<MissionParameters> missions = new List<MissionParameters>();
}

}
