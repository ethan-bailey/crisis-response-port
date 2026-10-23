namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class OperatorSpawner : MonoBehaviour
{
	public bool active = true;

	[Tooltip("The name of the prefab (under Resources/Characters) to use.")]
	public string prefabName = "Operator";

	public Vector3 position = new Vector3(-3f, 1.5f, -1.5f);

	[Tooltip("The distance characters will be spawned from eachother. The sign sets the direction relative from the spawn position.")]
	public float spacing = -1f;

	[Header("Scene override")]
	public bool forcePlayerControl;

	private Mission mission;

	private Settings settings;

	private void Awake()
	{
		settings = Global.Context.Settings;
		mission = Global.Context.Mission;
		mission.AddOperatorsComponent();
		if (active)
		{
			SpawnTeam();
		}
	}

	private CharacterBody GetOperatorPrefab()
	{
		GameObject gameObject = Resources.Load("Characters/" + prefabName) as GameObject;
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<CharacterBody>();
	}

	private void SpawnTeam()
	{
		Transform container = mission.Operators.Container;
		OperatorSpawnData operatorSpawnData = Global.Context.OperatorSpawnData;
		List<SerializableEquipment> list = ((!mission.TutorialMode || !mission.TutorialMode.Active) ? operatorSpawnData.Team.entryMembers : mission.TutorialMode.Tutorial.Equipment());
		int num = 0;
		CharacterBody operatorPrefab = GetOperatorPrefab();
		if (operatorPrefab == null)
		{
			Debug.LogError("Failed to spawn operators. Operator prefab load failed.");
			return;
		}
		foreach (SerializableEquipment item in list)
		{
			if (item.enabled)
			{
				CharacterBody characterBody = Object.Instantiate(operatorPrefab);
				characterBody.name = string.Format("Operator {0}", num + 1);
				characterBody.transform.SetParent(container, false);
				characterBody.transform.position = new Vector3(position.x + spacing * (float)num, position.y, -1.5f - Random.value * 0.01f);
				characterBody.Physiology.ShockResistance = 0.75f;
				characterBody.Physiology.ShockRecovery = 0.5f;
				characterBody.InitializeEquipment(item);
				characterBody.Blackboard.preferHeadTarget = false;
				characterBody.Blackboard.weaponSkill = 0.9f;
				TeamOperator component = characterBody.GetComponent<TeamOperator>();
				component.restrainOccupants = item.restrainOccupants;
				component.secureWeapons = item.secureWeapons;
				component.administerFirstAid = item.administerFirstAid;
				mission.Operators.RegisterBody(characterBody);
				num++;
			}
		}
	}
}

}
