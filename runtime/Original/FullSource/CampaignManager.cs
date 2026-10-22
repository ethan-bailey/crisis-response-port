namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using System.IO;
using Helpers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CampaignManager : MonoBehaviour
{
	public static float DrillingHourIncrease = 0.01f;

	public static float DailySkillCurrentDecline = 0.01f;

	public static float DailySkillMaximumDecline = 0.0025f;

	public static float SkillDegradationDelay = 7f;

	public Campaign campaign;

	public SerializableCampaign data;

	public CharacterNameGenerator nameGenerator;

	[Tooltip("These are the skills that operators will start with.")]
	public List<SerializableOperatorSkillTemplate> operatorSkills = new List<SerializableOperatorSkillTemplate>();

	[Header("Starting properties template")]
	[Tooltip("Operator ages are picked from this curve. X: Input in range [0, 1]. Y: Age in years.")]
	public AnimationCurve ageDistribution;

	[Tooltip("Male operator heights are picked from this curve. X: Input in range [0, 1]. Y: Height in meters.")]
	public AnimationCurve maleHeightDistribution;

	[Tooltip("Male operator weight per height unit is picked from this curve. X: Input in range [0, 1]. Y: Weight in kilograms per meter height.")]
	public AnimationCurve maleWeightDistribution;

	[Tooltip("Female operator heights are picked from this curve. X: Input in range [0, 1]. Y: Height in meters.")]
	public AnimationCurve femaleHeightDistribution;

	[Tooltip("Female operator weight per height unit is picked from this curve. X: Input in range [0, 1]. Y: Weight in kilograms per meter height.")]
	public AnimationCurve femaleWeightDistribution;

	[Tooltip("Operator primary stat values are picked from this curve. X: Input in range [0, 1]. Y: Stat value in range [0,1].")]
	public AnimationCurve statDistribution;

	[Tooltip("Operator skill maximum values are picked from this curve. X: Input in range [0, 1]. Y: Skill value in range [0,1].")]
	public AnimationCurve skillDistribution;

	[Tooltip("Operator skill current values are picked from this curve. X: Input in range [0, 1]. Y: Current skill value as ratio of maximum. Range [0,1].")]
	public AnimationCurve skillFreshnessDistribution;

	public UnityEvent onDayChanged = new UnityEvent();

	private string pendingFileOperationTarget;

	public bool CampaignActive { get; set; }

	public MissionParameters MissionParameters { get; set; }

	public CampaignMenuLocation MenuLocation { get; set; }

	private void Awake()
	{
		onDayChanged.RemoveListener(OnDayChanged);
		onDayChanged.AddListener(OnDayChanged);
	}

	public void ProgressOneDay()
	{
		if (!(NextMission() == null))
		{
			ProgressCampaign(1);
		}
	}

	public void ProgressOneWeek()
	{
		MissionParameters missionParameters = NextMission();
		if (!(missionParameters == null))
		{
			int day = missionParameters.day;
			int days = Mathf.Min(day - data.day, 7);
			ProgressCampaign(days);
		}
	}

	public void ProgressUntilNext()
	{
		MissionParameters missionParameters = NextMission();
		if (!(missionParameters == null))
		{
			int day = missionParameters.day;
			int days = day - data.day;
			ProgressCampaign(days);
		}
	}

	private void ProgressCampaign(int days)
	{
		for (int i = 0; i < days; i++)
		{
			data.roster.UpdateSkills();
			data.roster.PerformScheduledActivities(data.day);
			data.day++;
		}
		onDayChanged.Invoke();
	}

	private void OnDayChanged()
	{
		MissionParameters missionParameters = NextMission();
		if (!(missionParameters == null) && data.day == missionParameters.day)
		{
			Global.Context.CampaignUI.locationTemplate.Goto(CampaignMenuLocation.Mission);
		}
	}

	public void StartMission()
	{
		MissionParameters missionParameters = NextMission();
		if (!(missionParameters == null))
		{
			MissionParameters = Object.Instantiate(missionParameters);
			MissionParameters.name = missionParameters.name;
			MissionParameters.transform.SetParent(base.transform);
		}
	}

	public void SaveAndReturnToTitle()
	{
		data.Save();
		ReturnToTitle();
	}

	public void ReturnToTitle()
	{
		CampaignActive = false;
		MenuLocation = CampaignMenuLocation.Start;
		SceneManager.LoadScene(1);
	}

	public bool ContinueCampaign(string file)
	{
		data = new SerializableCampaign();
		if (data.Load(file))
		{
			Global.Context.CampaignUI.OnLoaded();
			return true;
		}
		return false;
	}

	public void AskDeleteCampaign(string file)
	{
		if (!File.Exists(file))
		{
			Global.Context.MessageBox.ShowOK("Oops. The file does not exist!");
			return;
		}
		if (!FileSystem.IsSubfolder(Global.Context.CampaignsFolder, file))
		{
			Global.Context.MessageBox.ShowOK("The file is not in the game data folder. I will not delete it!");
			return;
		}
		pendingFileOperationTarget = file;
		Global.Context.MessageBox.ShowOKCancel(string.Format("Are you sure you want to delete campaign '{0}'?", Path.GetFileNameWithoutExtension(file)), ExecutePendingDeleteCampaign);
	}

	private void ExecutePendingDeleteCampaign()
	{
		if (File.Exists(pendingFileOperationTarget) && FileSystem.IsSubfolder(Global.Context.CampaignsFolder, pendingFileOperationTarget))
		{
			File.Delete(pendingFileOperationTarget);
			Global.Context.CampaignUI.RefreshContinueList();
		}
		else
		{
			Global.Context.MessageBox.ShowOK("Failed to delete the campaign.");
		}
	}

	public List<CampaignFileInfo> CampaignsOnDisk()
	{
		List<CampaignFileInfo> list = new List<CampaignFileInfo>();
		if (!Directory.Exists(Global.Context.CampaignsFolder))
		{
			return list;
		}
		string[] files = Directory.GetFiles(Global.Context.CampaignsFolder);
		string[] array = files;
		foreach (string path in array)
		{
			list.Add(new CampaignFileInfo
			{
				name = Path.GetFileNameWithoutExtension(path),
				path = path
			});
		}
		return list;
	}

	public void CreateCampaign(string validName)
	{
		validName = FileSystem.MakeFilenameValid(validName).ToLower() + ".dat";
		data = GenerateCampaign(validName);
		data.Save();
	}

	private SerializableCampaign GenerateCampaign(string name)
	{
		int startOperatorCount = campaign.startOperatorCount;
		SerializableCampaign serializableCampaign = new SerializableCampaign();
		serializableCampaign.name = FileSystem.MakeFilenameValid(name);
		SerializableCampaign serializableCampaign2 = serializableCampaign;
		for (int i = 0; i < startOperatorCount; i++)
		{
			bool flag = Random.value > 0.25f;
			float num = ((!flag) ? femaleHeightDistribution.Evaluate(Random.value) : maleHeightDistribution.Evaluate(Random.value));
			float weight = num * ((!flag) ? femaleWeightDistribution.Evaluate(Random.value) : maleWeightDistribution.Evaluate(Random.value));
			SerializableOperatorAttributes serializableOperatorAttributes = new SerializableOperatorAttributes();
			serializableOperatorAttributes.communication = statDistribution.Evaluate(Random.value);
			serializableOperatorAttributes.empathy = statDistribution.Evaluate(Random.value);
			serializableOperatorAttributes.perception = statDistribution.Evaluate(Random.value);
			serializableOperatorAttributes.stressTolerance = statDistribution.Evaluate(Random.value);
			serializableOperatorAttributes.strictness = statDistribution.Evaluate(Random.value);
			SerializableOperatorAttributes attributes = serializableOperatorAttributes;
			List<SerializableOperatorSkill> list = new List<SerializableOperatorSkill>();
			foreach (SerializableOperatorSkillTemplate operatorSkill in operatorSkills)
			{
				list.Add(new SerializableOperatorSkill(operatorSkill, skillDistribution.Evaluate(Random.value), skillFreshnessDistribution.Evaluate(Random.value)));
			}
			List<SerializableActivity> list2 = new List<SerializableActivity>();
			for (int j = 0; j < 5; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					list2.Add(new SerializableActivity
					{
						day = j,
						slot = k,
						activity = 0
					});
				}
			}
			serializableCampaign2.roster.members.Add(new SerializableTeamMember
			{
				firstName = nameGenerator.FirstName(flag),
				lastName = nameGenerator.LastName(),
				attributes = attributes,
				skills = list,
				schedule = list2,
				male = flag,
				age = Mathf.RoundToInt(ageDistribution.Evaluate(Random.value)),
				height = num,
				weight = weight
			});
		}
		serializableCampaign2.roster.BuildShifts();
		return serializableCampaign2;
	}

	public MissionParameters NextMission()
	{
		int day = data.day;
		for (int i = 0; i < campaign.missions.Count; i++)
		{
			if (data.finishedMissions <= i && campaign.missions[i].day >= day)
			{
				return campaign.missions[i];
			}
		}
		return null;
	}

	public bool HasPendingMission()
	{
		int day = data.day;
		for (int i = 0; i < campaign.missions.Count; i++)
		{
			if (data.finishedMissions <= i && campaign.missions[i].day >= day)
			{
				if (campaign.missions[i].day > day)
				{
					break;
				}
				return true;
			}
		}
		return false;
	}

	public void SetMissionCompleted()
	{
		data.finishedMissions++;
		data.Save();
	}
}

}
