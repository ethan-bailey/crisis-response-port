namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class OccupantSpawner : MonoBehaviour
{
	public static string PrefabPath = "Characters/";

	[Header("Legacy")]
	public bool spawnLegacyGroup;

	public float startCoordinate = 1f;

	public float endCoordinate = 30f;

	[Header("Debug")]
	[Tooltip("Spawn inserted at cursor by pressing Insert.")]
	public SerializableOccupantRandomizedSpawnGroup debugSpawn = new SerializableOccupantRandomizedSpawnGroup();

	[Header("Areas")]
	public List<SpawnArea> areas;

	[Header("Obstacles")]
	[Tooltip("The radius around characters which must be clear of obstacles. In meters.")]
	public float bodyClearRadius = 0.5f;

	[Tooltip("The size of increments to test spawn positions for collision. In meters.")]
	public float collisionIncrement = 0.25f;

	[Tooltip("The max number of tries per spawn to find a valid position.")]
	public int maxTries = 20;

	private Mission mission;

	private Settings settings;

	private SerializableOccupantSettings occupantSettings;

	private List<WeaponBase> availableWeapons;

	private float spawnAreaProbabilitySum;

	private int collisionMask;

	private void Awake()
	{
		mission = Global.Context.Mission;
		mission.AddOccupantsComponent();
	}

	private void Start()
	{
		if (Global.Context.MissionMetaData.tutorialActive)
		{
			Object.Destroy(this);
			return;
		}
		collisionMask = Masks.Instance.BallisticTargets;
		settings = Global.Context.Settings;
		occupantSettings = settings.Occupants;
		availableWeapons = AvailableWeapons();
		InitializeSpawnAreas();
		Spawn();
	}

	public void AddSpawnArea(string name, float xMin, float xMax, float y, float zMin, float zMax, float walkableMin, float walkableMax)
	{
		areas.Add(new SpawnArea
		{
			name = name,
			xMin = xMin,
			xMax = xMax,
			y = y,
			zMin = zMin,
			zMax = zMax,
			walkableMin = walkableMin,
			walkableMax = walkableMax
		});
	}

	private void InitializeSpawnAreas()
	{
		Debug.Log("Initialize spawn areas...");
		float num = 0f;
		foreach (SpawnArea area in areas)
		{
			area.probabilityLow = num;
			num = (area.probabilityHigh = num + area.probability);
		}
		spawnAreaProbabilitySum = num;
	}

	private void Spawn()
	{
		Debug.Log("Spawn occupants...");
		List<SerializableOccupantSpecificSpawnGroup> list = new List<SerializableOccupantSpecificSpawnGroup>();
		bool campaignActive = Global.Context.CampaignManager.CampaignActive;
		if (spawnLegacyGroup && !campaignActive)
		{
			SerializableOccupantRandomizedSpawnGroup serializableOccupantRandomizedSpawnGroup = CreateRandomizedGroup_TEMPORARY();
			SerializableOccupantSpecificSpawnGroup item = CreateSpecificGroup(serializableOccupantRandomizedSpawnGroup);
			list.Add(item);
		}
		if (campaignActive)
		{
			SpawnCampaignMissionCharacters();
			return;
		}
		SerializableOccupantSpawnData data = Global.Context.OccupantSpawnData.data;
		foreach (SerializableOccupantSpecificSpawnGroup specificGroup in data.specificGroups)
		{
			if (specificGroup.enabled)
			{
				list.Add(specificGroup);
			}
		}
		foreach (SerializableOccupantRandomizedSpawnGroup randomGroup in data.randomGroups)
		{
			if (randomGroup.enabled)
			{
				list.Add(CreateSpecificGroup(randomGroup));
			}
		}
		foreach (SerializableOccupantSpecificSpawnGroup item2 in list)
		{
			SpawnSandboxGroup(item2);
		}
	}

	public CharacterBody SpawnOccupantByCommand(float position, float direction = 1f)
	{
		SerializableOccupantRandomizedSpawnGroup parameters = CreateRandomizedGroup_TEMPORARY();
		SerializableOccupantSpawn data = CreateSingleSpawn(position, parameters);
		return SpawnSandboxCharacter(data, false);
	}

	private void SpawnCampaignMissionCharacters()
	{
		foreach (CharacterTemplate occupant in Global.Context.CampaignManager.MissionParameters.occupants)
		{
			SpawnCampaignCharacter(occupant);
		}
	}

	private SerializableOccupantRandomizedSpawnGroup CreateRandomizedGroup_TEMPORARY()
	{
		SerializableOccupantRandomizedSpawnGroup serializableOccupantRandomizedSpawnGroup = new SerializableOccupantRandomizedSpawnGroup();
		serializableOccupantRandomizedSpawnGroup.name = "Occupant";
		serializableOccupantRandomizedSpawnGroup.minCount = occupantSettings.minCount;
		serializableOccupantRandomizedSpawnGroup.maxCount = occupantSettings.maxCount;
		serializableOccupantRandomizedSpawnGroup.minCoord = startCoordinate;
		serializableOccupantRandomizedSpawnGroup.maxCoord = endCoordinate;
		serializableOccupantRandomizedSpawnGroup.minMorale = occupantSettings.morale.low;
		serializableOccupantRandomizedSpawnGroup.maxMorale = occupantSettings.morale.high;
		serializableOccupantRandomizedSpawnGroup.minAggression = occupantSettings.aggression.low;
		serializableOccupantRandomizedSpawnGroup.maxAggression = occupantSettings.aggression.high;
		serializableOccupantRandomizedSpawnGroup.minFear = occupantSettings.fear.low;
		serializableOccupantRandomizedSpawnGroup.maxFear = occupantSettings.fear.high;
		serializableOccupantRandomizedSpawnGroup.minFitness = 0f;
		serializableOccupantRandomizedSpawnGroup.maxFitness = 1f;
		serializableOccupantRandomizedSpawnGroup.minShockResistance = occupantSettings.shockResistance.low;
		serializableOccupantRandomizedSpawnGroup.maxShockResistance = occupantSettings.shockResistance.high;
		serializableOccupantRandomizedSpawnGroup.minBlindFireChance = 0f;
		serializableOccupantRandomizedSpawnGroup.maxBlindFireChance = occupantSettings.aggression.high;
		serializableOccupantRandomizedSpawnGroup.minBlindFireDistance = 0f;
		serializableOccupantRandomizedSpawnGroup.maxBlindFireDistance = 10f * occupantSettings.aggression.high;
		return serializableOccupantRandomizedSpawnGroup;
	}

	private SerializableOccupantSpecificSpawnGroup CreateSpecificGroup(SerializableOccupantRandomizedSpawnGroup group, bool useDebugPosition = false)
	{
		SerializableOccupantSpecificSpawnGroup serializableOccupantSpecificSpawnGroup = new SerializableOccupantSpecificSpawnGroup();
		int num = Random.Range(group.minCount, group.maxCount);
		float num2 = 0f;
		if (useDebugPosition)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo = default(RaycastHit);
			if (Physics.Raycast(ray, out hitInfo, 30f, Masks.Instance.Aim))
			{
				num2 = hitInfo.point.x;
			}
			else
			{
				useDebugPosition = false;
			}
		}
		for (int i = 0; i < num; i++)
		{
			serializableOccupantSpecificSpawnGroup.members.Add(new SerializableOccupantSpawn
			{
				name = ((num != 1) ? string.Format("{0} {1}", group.name, i + 1) : group.name),
				position = ((!useDebugPosition) ? Random.Range(group.minCoord, group.maxCoord) : num2),
				morale = Random.Range(group.minMorale, group.maxMorale),
				aggression = Random.Range(group.minAggression, group.maxAggression),
				fear = Random.Range(group.minFear, group.maxFear),
				negotiable = false,
				fitness = Random.Range(group.minFitness, group.maxFitness),
				shockResistance = Random.Range(group.minShockResistance, group.maxShockResistance),
				blindFireChance = Random.Range(group.minBlindFireChance, group.maxBlindFireChance),
				blindFireDistance = Random.Range(group.minBlindFireDistance, group.maxBlindFireDistance),
				equipment = CreateEquipment_TEMPORARY()
			});
		}
		return serializableOccupantSpecificSpawnGroup;
	}

	private SerializableOccupantSpawn CreateSingleSpawn(float position, SerializableOccupantRandomizedSpawnGroup parameters)
	{
		SerializableOccupantSpawn serializableOccupantSpawn = new SerializableOccupantSpawn();
		serializableOccupantSpawn.name = parameters.name;
		serializableOccupantSpawn.position = position;
		serializableOccupantSpawn.morale = Random.Range(parameters.minMorale, parameters.maxMorale);
		serializableOccupantSpawn.aggression = Random.Range(parameters.minAggression, parameters.maxAggression);
		serializableOccupantSpawn.fear = Random.Range(parameters.minFear, parameters.maxFear);
		serializableOccupantSpawn.negotiable = false;
		serializableOccupantSpawn.fitness = Random.Range(parameters.minFitness, parameters.maxFitness);
		serializableOccupantSpawn.shockResistance = Random.Range(parameters.minShockResistance, parameters.maxShockResistance);
		serializableOccupantSpawn.blindFireChance = Random.Range(parameters.minBlindFireChance, parameters.maxBlindFireChance);
		serializableOccupantSpawn.blindFireDistance = Random.Range(parameters.minBlindFireDistance, parameters.maxBlindFireDistance);
		serializableOccupantSpawn.equipment = CreateEquipment_TEMPORARY();
		return serializableOccupantSpawn;
	}

	private SerializableEquipment CreateEquipment_TEMPORARY()
	{
		SerializableItems serializableItems = new SerializableItems();
		serializableItems.itemPrefabNames = new List<string> { "Primary magazine", "Primary magazine" };
		SerializableItems items = serializableItems;
		object obj;
		if (availableWeapons.Count > 0)
		{
			SerializableWeapon serializableWeapon = new SerializableWeapon();
			serializableWeapon.prefabName = availableWeapons[Random.Range(0, availableWeapons.Count)].name;
			obj = serializableWeapon;
		}
		else
		{
			obj = null;
		}
		SerializableWeapon primary = (SerializableWeapon)obj;
		SerializableEquipment serializableEquipment = new SerializableEquipment();
		serializableEquipment.primary = primary;
		serializableEquipment.armor = ArmorClass.Type.None;
		serializableEquipment.items = items;
		return serializableEquipment;
	}

	private List<CharacterBody> SpawnSandboxGroup(SerializableOccupantSpecificSpawnGroup group)
	{
		List<CharacterBody> list = new List<CharacterBody>();
		foreach (SerializableOccupantSpawn member in group.members)
		{
			if (member.enabled)
			{
				CharacterBody characterBody = SpawnSandboxCharacter(member);
				if (characterBody != null)
				{
					list.Add(characterBody);
				}
			}
		}
		return list;
	}

	private CharacterBody SpawnSandboxCharacter(SerializableOccupantSpawn data, bool randomizePosition = true)
	{
		GameObject gameObject = Resources.Load(PrefabPath + data.bodyPrefab) as GameObject;
		if (gameObject == null)
		{
			return null;
		}
		SpawnArea spawnArea = RandomSpawnArea();
		Vector3 position = ((!randomizePosition) ? new Vector3(data.position, 1.25f, -1.5f) : spawnArea.RandomPosition());
		if (randomizePosition && !AdjustPositionForObstacles(ref position, spawnArea))
		{
			return null;
		}
		CharacterBody characterBody = Object.Instantiate(gameObject.GetComponent<CharacterBody>());
		characterBody.name = data.name;
		characterBody.weight = Random.Range(75f, 85f);
		characterBody.transform.SetParent(mission.Occupants.Container, true);
		characterBody.transform.position = position;
		Occupant occupant = characterBody.gameObject.AddComponent<Occupant>();
		bool flag = Global.Context.SceneTransition.CurrentScene == SceneTransition.Scene.Marksman;
		float num = (spawnArea.xMax - spawnArea.xMin) * ((!flag) ? Random.value : 1f);
		occupant.patrolAreaLeft = Mathf.Max(spawnArea.xMin, position.x - num * 0.5f);
		occupant.patrolAreaRight = Mathf.Min(spawnArea.xMax, position.x + num * 0.5f);
		occupant.walkableAreaLeft = spawnArea.walkableMin;
		occupant.walkableAreaRight = spawnArea.walkableMax;
		characterBody.Blackboard.morale = data.morale;
		characterBody.Blackboard.aggression = data.aggression;
		characterBody.Blackboard.fear = data.fear;
		characterBody.Physiology.fitness = data.fitness;
		characterBody.Physiology.ShockResistance = data.shockResistance;
		characterBody.Physiology.ShockRecovery = Random.Range(0.1f, 1f);
		characterBody.Blackboard.WillBlindFire = Random.value < data.blindFireChance;
		characterBody.Blackboard.BlindFireDistance = data.blindFireDistance;
		characterBody.Blackboard.negotiable = data.negotiable;
		SetWeaponSkill(characterBody);
		characterBody.InitializeEquipment(data.equipment);
		characterBody.SetRandomAmmo();
		Occupants occupants = Global.Context.Mission.Occupants;
		occupants.RegisterBody(characterBody);
		if (Global.Context.CampaignManager.CampaignActive)
		{
			occupants.RegisterOccupantCharacter(characterBody);
		}
		return characterBody;
	}

	private CharacterBody SpawnCampaignCharacter(CharacterTemplate data)
	{
		CharacterBody characterBody = Object.Instantiate(data.bodyPrefab);
		characterBody.name = data.name;
		Vector3 position = AdjustPositionForObstacles_DEPRECATED(data.position);
		position.z -= Random.value * 0.01f;
		characterBody.transform.position = position;
		characterBody.transform.SetParent(mission.Occupants.Container, true);
		characterBody.Blackboard.morale = data.morale;
		characterBody.Blackboard.aggression = data.aggression;
		characterBody.Blackboard.fear = data.fear;
		characterBody.Physiology.fitness = data.fitness;
		characterBody.Physiology.ShockResistance = data.shockResistance;
		characterBody.Physiology.ShockRecovery = Random.Range(0.1f, 1f);
		characterBody.Blackboard.WillBlindFire = Random.value < data.blindFireChance;
		characterBody.Blackboard.BlindFireDistance = data.blindFireDistance;
		characterBody.Blackboard.negotiable = data.negotiable;
		SetWeaponSkill(characterBody);
		characterBody.InitializeEquipment(data.equipment);
		characterBody.SetRandomAmmo();
		characterBody.InitializeDialog(data.dialogTree);
		if (Global.Context.CampaignManager.CampaignActive)
		{
			Global.Context.Mission.Occupants.RegisterOccupantCharacter(characterBody);
		}
		return characterBody;
	}

	private void SetWeaponSkill(CharacterBody body)
	{
		float num = Random.Range(occupantSettings.weaponSkill.low, occupantSettings.weaponSkill.high);
		body.Blackboard.weaponSkill = num;
		float num2 = num + (1f - num) * Random.value;
		float num3 = (1f - Random.Range(occupantSettings.weaponSkill.low, occupantSettings.weaponSkill.high)) * 3f;
		float y = Mathf.Min(num2 + num3, 3f);
		body.recoilCompensation = new Vector2(num2, y);
	}

	private List<WeaponBase> AvailableWeapons()
	{
		List<WeaponBase> list = new List<WeaponBase>();
		List<WeaponBase> occupantWeapons = Global.Context.EquipmentList.OccupantWeapons;
		foreach (WeaponBase item in occupantWeapons)
		{
			if (WeaponTypeEnabled(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private bool WeaponTypeEnabled(WeaponBase weapon)
	{
		switch (weapon.weaponType)
		{
		case WeaponType.Pistol:
			return occupantSettings.handguns;
		case WeaponType.SMG:
			return occupantSettings.smgs;
		case WeaponType.Shotgun:
			return occupantSettings.shotguns;
		case WeaponType.Rifle:
			return occupantSettings.rifles;
		default:
			return false;
		}
	}

	private SpawnArea RandomSpawnArea()
	{
		float num = Random.value * spawnAreaProbabilitySum;
		foreach (SpawnArea area in areas)
		{
			if (num >= area.probabilityLow && num < area.probabilityHigh)
			{
				return area;
			}
		}
		return areas[0];
	}

	private bool AdjustPositionForObstacles(ref Vector3 position, SpawnArea area)
	{
		int num = 0;
		float num2 = 0f;
		float num3 = Mathf.Sign(Random.Range(-1f, 1f));
		Ray ray = new Ray(position + Vector3.right * num2 - Vector3.right * bodyClearRadius, Vector3.right);
		bool flag = false;
		while (Physics.Raycast(ray, bodyClearRadius * 2f, collisionMask) && num < maxTries)
		{
			if (flag)
			{
				num2 += collisionIncrement * Mathf.Sign(num2);
				flag = false;
			}
			else
			{
				flag = true;
				num2 *= -1f;
			}
			ray = new Ray(position + Vector3.right * num2 - Vector3.right * bodyClearRadius, Vector3.right);
			num++;
		}
		position += Vector3.right * num2;
		return num < maxTries && position.x >= area.xMin && position.x <= area.xMax;
	}

	private Vector3 AdjustPositionForObstacles_DEPRECATED(float desiredPosition)
	{
		int num = 0;
		int num2 = 10;
		float num3 = 1f;
		Vector3 vector = Vector3.zero;
		while (num < num2)
		{
			num++;
			vector = new Vector3(Mathf.Clamp(desiredPosition, startCoordinate, endCoordinate), base.transform.position.y, base.transform.position.z);
			Ray ray = new Ray(vector - Vector3.right * num3 * 0.5f, Vector3.right);
			if (!Physics.Raycast(ray, num3, Masks.Instance.BallisticTargetsSansLimbs))
			{
				ray = new Ray(vector + Vector3.right * num3 * 0.5f, Vector3.left);
				if (!Physics.Raycast(ray, num3, Masks.Instance.BallisticTargetsSansLimbs))
				{
					break;
				}
			}
		}
		return vector;
	}
}

}
