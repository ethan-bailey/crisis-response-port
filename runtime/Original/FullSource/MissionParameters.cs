namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class MissionParameters : MonoBehaviour
{
	public string description;

	public int day;

	public int scene;

	public List<CharacterTemplate> occupants;

	public Mission.MissionView startView;

	public bool teamStandDownEnabled;

	public float teamStandDownLeftPosition;

	public float teamStandDownRightPosition;

	public bool phoneDropOffEnabled;

	public float phoneDropOffPosition;

	public float phoneDropOffArea = 1f;

	public CharacterTemplate phoneAnswerer;

	public float phoneAnswerDelay = 15f;
}

}
