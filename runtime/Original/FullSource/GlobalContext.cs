namespace Crisis.Runtime.FullSource
{
using System;
using System.IO;
using Microsoft.Win32;
using UnityEngine;
using UnityEngine.Audio;
using Usage;

public class GlobalContext : MonoBehaviour
{
	public const string DiscordURL = "https://discord.gg/rEzzjFH";

	[Header("Game")]
	public string persistentDataPath = "Gets set at run-time.";

	public string downloadURL;

	[Header("References")]
	public AudioMixer mixer;

	public UIFrameRateDisplay frameRateDisplay;

	public Canvas canvas;

	public StagingDebugUI debugUI;

	public UIFeedbackWindow feedbackWindowPrefab;

	private UIFeedbackWindow feedbackWindow;

	private bool startup = true;

	private InputMapping inputMapping;

	private ResourceManager diskResourceMgr;

	public InputHandler InputHandler { get; private set; }

	public Settings Settings { get; private set; }

	public MissionList MissionList { get; private set; }

	public EquipmentList EquipmentList { get; private set; }

	public CampaignManager CampaignManager { get; private set; }

	public SceneTransition SceneTransition { get; private set; }

	public PerformanceCounter PerformanceCounter { get; private set; }

	public UITooltipManager Tooltip { get; private set; }

	public MessageBoxManager MessageBox { get; private set; }

	public ServerConnection Connection { get; private set; }

	public MissionMetaData MissionMetaData { get; private set; }

	public OperatorSpawnData OperatorSpawnData { get; private set; }

	public OccupantSpawnData OccupantSpawnData { get; private set; }

	public EngagementRules EngagementRules { get; private set; }

	public AudioPlayer AudioPlayer { get; private set; }

	public CommandHandler CommandHandler { get; private set; }

	public Mixer Mixer { get; private set; }

	public Statistics Usage { get; private set; }

	public bool Suspended { get; set; }

	public UIFeedbackWindow FeedbackWindow
	{
		get
		{
			if (feedbackWindow == null)
			{
				feedbackWindow = UnityEngine.Object.Instantiate(feedbackWindowPrefab, canvas.transform);
			}
			return feedbackWindow;
		}
	}

	public string ClientGUID
	{
		get
		{
			return SystemInfo.deviceUniqueIdentifier;
		}
	}

	public string AvailableVersion { get; set; }

	public string CampaignsFolder
	{
		get
		{
			return persistentDataPath + "/" + SerializableCampaign.DirName;
		}
	}

	public MainMenu MainMenu
	{
		get
		{
			MainMenu result = null;
			GameObject gameObject = GameObject.Find(MainMenu.ObjectName);
			if (gameObject != null)
			{
				result = gameObject.GetComponent<MainMenu>();
			}
			return result;
		}
	}

	public Mission Mission
	{
		get
		{
			Mission result = null;
			GameObject gameObject = GameObject.Find(Mission.ObjectName);
			if (gameObject != null)
			{
				result = gameObject.GetComponent<Mission>();
			}
			return result;
		}
	}

	public UICampaign CampaignUI
	{
		get
		{
			UICampaign result = null;
			GameObject gameObject = GameObject.Find(UICampaign.ObjectName);
			if (gameObject != null)
			{
				result = gameObject.GetComponent<UICampaign>();
			}
			return result;
		}
	}

	private void Awake()
	{
		persistentDataPath = Application.persistentDataPath;
		Debug.LogFormat("Application data path is: {0}", Application.dataPath);
		Debug.LogFormat("Persistent data path is: {0}", persistentDataPath);
		PlayerPrefs.SetString("persistentDir", persistentDataPath);
		AvailableVersion = "0.0.0.0";
		diskResourceMgr = ResourceManager.Instance;
		if (Version.Instance.Staging)
		{
			Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
			Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
			Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
		}
		else
		{
			Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.None);
		}
		HandleStartingNewVersion();
		Global instance = Global.Instance;
		FetchEarlyComponents();
		if (instance.Initialized)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		instance.Initialize(base.gameObject);
		if (startup)
		{
			StartupInitialize();
		}
		FetchLateComponents();
		Settings.Load();
		Apply();
	}

	private void OnApplicationFocus(bool focused)
	{
		Debug.Log("Application focus changed.");
		Suspend(!focused);
	}

	private void OnDestroy()
	{
		HandleExitCommandLineArgs();
		Debug.Log("Bye now!");
	}

	private void Update()
	{
		PerformanceCounter.Update(Time.unscaledDeltaTime);
		diskResourceMgr.UpdateAsyncJobs();
	}

	public void GetCanvasResolution(ref int width, ref int height)
	{
		Rect rect = canvas.GetComponent<RectTransform>().rect;
		width = Mathf.FloorToInt(rect.width * canvas.scaleFactor);
		height = Mathf.FloorToInt(rect.height * canvas.scaleFactor);
	}

	public float GetCanvasAspectRatio()
	{
		Rect rect = canvas.GetComponent<RectTransform>().rect;
		return rect.width / rect.height;
	}

	private void HandleStartingNewVersion()
	{
		string text = PlayerPrefs.GetString("version");
		string versionString = Version.Instance.VersionString;
		if (string.IsNullOrEmpty(text) || text != versionString)
		{
			PlayerPrefs.SetString("version", versionString);
		}
	}

	private void StartupInitialize()
	{
		LogCommandLineOptions();
		if (!Version.Instance.Staging)
		{
			UnityEngine.Object.Destroy(debugUI.gameObject);
		}
		else
		{
			debugUI.gameObject.SetActive(true);
		}
		HandleStartupCommandLineArgs();
		startup = false;
	}

	private void FetchEarlyComponents()
	{
		Connection = GetComponent<ServerConnection>();
		Usage = GetComponent<Statistics>();
		CampaignManager = GetComponent<CampaignManager>();
		SceneTransition = GetComponent<SceneTransition>();
		OperatorSpawnData = GetComponent<OperatorSpawnData>();
		OccupantSpawnData = GetComponent<OccupantSpawnData>();
		EngagementRules = GetComponent<EngagementRules>();
		MissionList = GetComponent<MissionList>();
		MessageBox = GetComponent<MessageBoxManager>();
		AudioPlayer = canvas.GetComponent<AudioPlayer>();
		MissionMetaData = GetComponent<MissionMetaData>();
		CommandHandler = GetComponent<CommandHandler>();
		Mixer = GetComponent<Mixer>();
	}

	private void FetchLateComponents()
	{
		Settings = GetComponent<Settings>();
		InputHandler = GetComponent<InputHandler>();
		EquipmentList = GetComponent<EquipmentList>();
		EquipmentList.Initialize(Version.Instance.Staging);
		Tooltip = GetComponent<UITooltipManager>();
		inputMapping = InputHandler.Keys;
	}

	private void Apply()
	{
		Settings.Graphics.ApplyVsyncCount();
		QualityLevels.SetQualityLevel(Settings.Performance.qualityLevel);
		PerformanceCounter = PerformanceCounter.Instance;
		frameRateDisplay.gameObject.SetActive(Settings.Graphics.showFrameRate);
		InputHandler.SetMapping(Settings.Controls);
	}

	private void Suspend(bool suspend)
	{
		if (suspend == Suspended)
		{
			Debug.LogFormat("Already {0}. Will take no action.", (!Suspended) ? "running" : "suspended");
			return;
		}
		Suspended = suspend;
		Debug.LogFormat("Application status changed to: {0}", (!Suspended) ? "Running" : "Suspended");
		if (suspend && Mission != null && !Mission.missionEnded)
		{
			Debug.Log("Pause mission and show menu.");
			Mission.PauseAndShowMenu();
		}
	}

	private void LogCommandLineOptions()
	{
		Debug.Log("Use these command line options to clear saved settings if the game starts in a bad state:");
		Debug.Log(" -clearAudio : Reset audio settings.");
		Debug.Log(" -clearGeneral : Reset general settings.");
		Debug.Log(" -clearPerformance : Reset performance settings.");
		Debug.Log(" -clearVideo: Reset video settings.");
		Debug.Log(" -clearInput : Reset input bindings.");
		Debug.Log(" -clearTime : Reset time settings.");
		Debug.Log(" -clearExtras : Reset extra settings.");
		Debug.Log(" -clearOccupants : Reset occupant settings.");
		Debug.Log(" -clearOperators : Reset operators and equipment.");
		Debug.Log(" -clearAll : Reset everything.");
	}

	private void HandleStartupCommandLineArgs()
	{
		Debug.Log("Parsing command line arguments.");
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string[] array = commandLineArgs;
		for (int i = 0; i < array.Length; i++)
		{
			switch (array[i])
			{
			case "-clearAudio":
				ClearSettingsFile("audio.dat", "audio");
				break;
			case "-clearGeneral":
				ClearSettingsFile("general.dat", "general");
				break;
			case "-clearPerformance":
				ClearSettingsFile("performance.dat", "performance");
				break;
			case "-clearVideo":
				ClearStartupVideoSettings();
				break;
			case "-clearInput":
				ClearSettingsFile(InputMapping.Filename, "input");
				break;
			case "-clearTime":
				ClearSettingsFile("time.dat", "time");
				break;
			case "-clearExtras":
				ClearSettingsFile("extras.dat", "extra");
				break;
			case "-clearOccupants":
				ClearSettingsFile("occupants.dat", "occupant");
				break;
			case "-clearOperators":
				ClearSettingsFile("operators.dat", "operator");
				break;
			case "-clearLocation":
				ClearSettingsFile("location.dat", "location");
				break;
			case "-clearAll":
				ClearAllSettings();
				break;
			}
		}
	}

	private void HandleExitCommandLineArgs()
	{
		Debug.Log("Parsing command line arguments.");
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string[] array = commandLineArgs;
		foreach (string text in array)
		{
			if (text != null && text == "-clearVideo")
			{
				ClearExitVideoSettings();
			}
		}
	}

	public void ClearSettingsFile(string filename, string groupName, bool showDialog = true)
	{
		ConsoleLog.Instance.WriteLine(string.Format("Clearing {0} settings.", groupName));
		ConsoleLog.Instance.WriteLine(string.Format("Delete: {0}", filename));
		string path = persistentDataPath + "/" + filename;
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		string text = string.Format("Done clearing {0} settings.", groupName);
		ConsoleLog.Instance.WriteLine(text);
		if (showDialog)
		{
			MessageBox.ShowOK(text);
		}
	}

	public void ClearStartupVideoSettings(bool showDialog = true)
	{
		ConsoleLog.Instance.WriteLine("Clearing video settings.");
		string path = persistentDataPath + "/graphics.dat";
		ConsoleLog.Instance.WriteLine(string.Format("Delete: {0}", "graphics.dat"));
		File.Delete(path);
		ConsoleLog.Instance.WriteLine("Video settings stored on file have been reset. You need to exit the game for it to clear settings stored in the Windows registry.");
		if (showDialog)
		{
			MessageBox.ShowOK("Video settings stored on file have been reset. You need to exit the game for it to clear settings stored in the Windows registry.");
		}
	}

	public void ClearExitVideoSettings()
	{
		ClearStartupVideoSettings();
		RemoveUnityRegistryValues();
	}

	private void RemoveUnityRegistryValues()
	{
		try
		{
			Registry.CurrentUser.DeleteSubKey("Software\\Antimony\\Crisis Response");
			Debug.Log("Removed registry values.");
		}
		catch (Exception)
		{
			Debug.LogWarning("Failed to remove registry values.");
		}
	}

	public void ClearAllSettings(bool showDialog = true)
	{
		ConsoleLog.Instance.WriteLine("Clearing all saved settings.");
		DirectoryInfo directoryInfo = new DirectoryInfo(persistentDataPath);
		FileInfo[] files = directoryInfo.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			ConsoleLog.Instance.WriteLine(string.Format(" Delete: {0}", fileInfo.Name));
			fileInfo.Delete();
		}
		if (showDialog)
		{
			MessageBox.ShowOK("All settings on file have been cleared. When the game exits, video settings stored in the Windows registry will be cleared as well.");
		}
	}
}

}
