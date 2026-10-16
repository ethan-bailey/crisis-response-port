using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Usage;

public class Mission : MonoBehaviour
{
	public enum MissionView
	{
		EntryTeam = 0,
		Marksman = 1,
		Negotiation = 2,
		None = 3
	}

	private enum MissionResult
	{
		Success = 0,
		Failure = 1,
		Restarted = 2,
		Aborted = 3
	}

	public static string ObjectName = "Mission";

	public MissionUI UI;

	public Light ambientLight;

	[Tooltip("Add all mission light sources which casts shadows to this list. These are updated when the shadow setting changes.")]
	public List<Light> shadowLightSources = new List<Light>();

	public Marksmen marksmen;

	public MissionView sandboxStartMode;

	public NegotiationMode negotiationMode;

	public List<Mesh> projectileFragments = new List<Mesh>();

	public List<Room> rooms;

	public float projectileRange = 100f;

	[Header("Mission modes")]
	public InteriorMode interiorModePrefab;

	[Header("Resource prefabs")]
	public BloodStream bloodStreamPrefab;

	public FloorPoolPainter streamPoolPrefab;

	public FloorPoolPainter floorPoolPrefab;

	public WallStreakPainter wallStreakPrefab;

	public Emitter woundEmitterPrefab;

	public GameObject headCensorPrefab;

	public ParticleSystem weaponSmokeEmitterPrefab;

	[Header("Interaction markers")]
	[Tooltip("This marker is placed on dropped weapons when the mission enters the 'All clear' state.")]
	public DynamicInteractionMarker secureWeaponMarker;

	[Tooltip("This marker is placed on unrestrained occupants when the mission enters the 'All clear' state.")]
	public DynamicInteractionMarker restrainMarker;

	[Tooltip("This marker is placed on unstabilized occupants when the mission enters the 'All clear' state.")]
	public DynamicInteractionMarker stabilizeMarker;

	[Header("Interactions")]
	public ToggleElectricity toggleElectricity;

	public bool missionEnded;

	public bool missionSuccess;

	public UnityEventInt onPaintTextureResolutionChanged = new UnityEventInt();

	[Tooltip("This is triggered when the mission begins. Time is frozen before this.")]
	public UnityEventVoid onMissionBegin = new UnityEventVoid();

	[Tooltip("This is triggered when the building is safe.")]
	public UnityEventVoid onAllClear = new UnityEventVoid();

	[Tooltip("This is triggered when the mission ends. OnSuccess or OnFailure is triggered right after.")]
	public UnityEventVoid onMissionEnd = new UnityEventVoid();

	[Tooltip("This is triggered when the mission ends with success.")]
	public UnityEventVoid onMissionSuccess = new UnityEventVoid();

	[Tooltip("This is triggered when the mission ends with failure.")]
	public UnityEventVoid onMissionFailure = new UnityEventVoid();

	[Tooltip("This is triggered when a cell phone is delivered.")]
	public UnityEventVoid onPhoneDelivered = new UnityEventVoid();

	private InputHandler input;

	private AudioPropagator audioPropagator;

	private MissionView currentMode = MissionView.None;

	private DynamicLimbTexturePool texturePool;

	private PerformanceManager perfMgr;

	private float firstInjuryTimestamp;

	private float defaultAmbientLightIntensity = 0.15f;

	private bool dataHasBeenRecorded;

	private bool disableEndCriteria;

	public bool Started { get; private set; }

	public ScoreKeeper ScoreKeeper { get; private set; }

	public Mixer Mixer { get; private set; }

	public Operators Operators { get; private set; }

	public Marksman Marksman { get; set; }

	public Occupants Occupants { get; private set; }

	public Doors Doors { get; private set; }

	public CellPhone CellPhone { get; private set; }

	public InteriorMode InteriorMode { get; private set; }

	public Benchmark Benchmark { get; private set; }

	public MarksmanMode MarksmanMode { get; set; }

	public TimeScaler TimeScaler { get; private set; }

	public TutorialMode TutorialMode { get; private set; }

	public ActionScheduler ActionScheduler { get; private set; }

	public Emitter WoundEmitter { get; private set; }

	public ParticleSystem WeaponSmokeEmitter { get; private set; }

	public Transform Containers { get; private set; }

	public Transform WeaponsContainer { get; private set; }

	public Transform ProjectilesContainer { get; private set; }

	public Transform LiveGrenadesContainer { get; private set; }

	public Transform SpentGrenadesContainer { get; private set; }

	public Transform CasingsContainer { get; private set; }

	public Transform BodyPartsContainer { get; private set; }

	public Transform EffectsContainer { get; private set; }

	public Transform EmittersContainer { get; private set; }

	public Transform CensorBoxContainer { get; private set; }

	public Transform GibsContainer { get; private set; }

	public Transform FloorPoolsContainer { get; private set; }

	public bool AllRoomsChecked { get; private set; }

	public bool AllWeaponsSecured { get; private set; }

	public bool AllClear { get; private set; }

	public bool AllOperatorsDisabled { get; private set; }

	public float MissionStartTimestamp { get; private set; }

	public float AllClearTimestamp { get; private set; }

	public float EndTimestamp { get; private set; }

	public float LastWeaponFireTimestamp { get; set; }

	public float StartPosition { get; private set; }

	public float EndPosition { get; private set; }

	public float LevelLength { get; private set; }

	public bool Paused { get; set; }

	public float FirstInjuryTimestamp
	{
		get
		{
			return firstInjuryTimestamp;
		}
		set
		{
			if (firstInjuryTimestamp == 0f)
			{
				firstInjuryTimestamp = value;
			}
		}
	}

	public MissionMode Mode { get; private set; }

	public MissionView View
	{
		get
		{
			return currentMode;
		}
		set
		{
			SetMode(value);
		}
	}

	private void OnDestroy()
	{
		Object[] array = Object.FindObjectsOfType(typeof(Texture));
		Object[] array2 = array;
		foreach (Object obj in array2)
		{
			Object.Destroy(obj);
		}
		texturePool.ReleaseAll();
		perfMgr.Reset();
		if (Global.Context != null)
		{
			Global.Context.MissionMetaData.DeactivateTutorial();
			Global.Context.InputHandler.SuspendInput(false);
			Global.Context.Settings.ClearMissionListeners();
		}
		MissionHelper.Instance.Release();
		input.OnMissionEnd();
		Mixer.OnMissionUnload();
	}

	private void Awake()
	{
		CreateContainers();
		ActionScheduler = new ActionScheduler();
		TimeScaler = GetComponent<TimeScaler>();
		TutorialMode = GetComponent<TutorialMode>();
		ScoreKeeper = GetComponent<ScoreKeeper>();
		Operators = GetComponent<Operators>();
		Occupants = GetComponent<Occupants>();
		Doors = GetComponent<Doors>();
		Benchmark = GetComponent<Benchmark>();
		Mixer = Global.Context.Mixer;
		input = Global.Context.InputHandler;
		texturePool = DynamicLimbTexturePool.Instance;
		defaultAmbientLightIntensity = ((!(ambientLight != null)) ? 0.15f : ambientLight.intensity);
		disableEndCriteria = Global.Context.Settings.Extras.disableEndCriteria;
		input.OnMissionStart();
		WeaponSmokeEmitter = Object.Instantiate(weaponSmokeEmitterPrefab);
		WeaponSmokeEmitter.name = weaponSmokeEmitterPrefab.name;
		WeaponSmokeEmitter.transform.SetParent(EmittersContainer);
		if (interiorModePrefab != null)
		{
			InteriorMode = Object.Instantiate(interiorModePrefab);
			InteriorMode.name = interiorModePrefab.name;
			InteriorMode.Initialize();
		}
		MissionHelper.Instance.Initialize(this);
		perfMgr = PerformanceManager.Instance;
	}

	private void Start()
	{
		ApplySettings();
		SetWalkableRange();
		audioPropagator = AudioPropagator.Instance;
		audioPropagator.Restart();
		if (Global.Context.Settings.Graphics.showFrameRate)
		{
			Global.Context.frameRateDisplay.gameObject.SetActive(true);
		}
		Global.Context.Settings.Extras.onChanged.AddListener(OnExtrasChanged);
		Global.Context.Settings.Performance.onPaintTextureResolutionChanged.AddListener((int value) =>
		{
			onPaintTextureResolutionChanged.Invoke(value);
		});
		if (Operators != null)
		{
			Operators.allDisabledEvent.AddListener(OnAllOperatorsDisabled);
		}
		if (Occupants != null)
		{
			Occupants.allDisabledEvent.AddListener(OnAllOccupantsDisabled);
			Occupants.allRestrainedEvent.AddListener(OnAllOccupantsRestrained);
		}
		SetLevelExtents();
		PerformanceManager.Instance.OnMissionStart(GibsContainer);
		Mixer.StartMissionMusic();
		MissionStartTimestamp = Time.time;
		Global.Context.Usage.session.missions++;
		if (UI.missionIntro != null && (bool)TutorialMode && !TutorialMode.Active)
		{
			UI.PlayIntro();
		}
		else
		{
			BeginMission();
		}
		Global.Context.Settings.Graphics.onAmbientLightChanged.AddListener(OnAmbientLightChanged);
		Global.Context.Settings.Graphics.onCensoringChanged.AddListener(OnCensoringChanged);
		Global.Context.Settings.Performance.onShadowsChanged.AddListener(OnShadowsChanged);
	}

	private void CreateContainers()
	{
		Containers = new GameObject("Containers").transform;
		WeaponsContainer = new GameObject("Dropped weapons").transform;
		WeaponsContainer.SetParent(Containers, true);
		ProjectilesContainer = new GameObject("Projectiles").transform;
		ProjectilesContainer.SetParent(Containers, true);
		LiveGrenadesContainer = new GameObject("Live grenades").transform;
		LiveGrenadesContainer.SetParent(Containers, true);
		SpentGrenadesContainer = new GameObject("Spent grenades").transform;
		SpentGrenadesContainer.SetParent(Containers, true);
		CasingsContainer = new GameObject("Casings").transform;
		CasingsContainer.SetParent(Containers, true);
		BodyPartsContainer = new GameObject("Body parts").transform;
		BodyPartsContainer.SetParent(Containers);
		EmittersContainer = new GameObject("Emitters").transform;
		EmittersContainer.SetParent(Containers);
		EmittersContainer.position = Vector3.zero;
		EffectsContainer = new GameObject("Effects").transform;
		EffectsContainer.SetParent(Containers);
		EffectsContainer.position = Vector3.zero;
		GibsContainer = new GameObject("Gibs").transform;
		GibsContainer.SetParent(EffectsContainer);
		GibsContainer.position = Vector3.zero;
		CensorBoxContainer = new GameObject("Censor boxes").transform;
		CensorBoxContainer.SetParent(EffectsContainer);
		CensorBoxContainer.position = Vector3.zero;
		FloorPoolsContainer = new GameObject("Floor pools").transform;
		FloorPoolsContainer.SetParent(EffectsContainer);
		FloorPoolsContainer.position = Vector3.zero;
	}

	public void AddOperatorsComponent()
	{
		Operators = base.gameObject.AddComponent<Operators>();
	}

	public void AddOccupantsComponent()
	{
		Occupants = base.gameObject.AddComponent<Occupants>();
	}

	public void BeginMission()
	{
		Started = true;
		onMissionBegin.Invoke();
		PerformanceCounter.Instance.StartMeasurement();
		SetMode(sandboxStartMode);
		if (!Global.Context.MissionMetaData.tutorialActive)
		{
			input.EnableAll();
			input.SuspendInput(false);
			input.EnableCategory(InputKey.Category.Weapons, false);
			Coroutine.DelayedAction(0.25f, () =>
			{
				input.EnableCategory(InputKey.Category.Weapons);
			}, this);
		}
	}

	private void Update()
	{
		ReadPlayerInput();
		audioPropagator.Update();
		ActionScheduler.Update(Time.time, Time.realtimeSinceStartup);
		UpdateMissionState();
		texturePool.Update();
	}

	private void LateUpdate()
	{
		perfMgr.LateUpdate();
	}

	private void ReadPlayerInput()
	{
		if (input.Keys.entryTeam.Down)
		{
			SetMode(MissionView.EntryTeam);
		}
		else if (input.Keys.marksman.Down)
		{
			if (Marksman != null)
			{
				SetMode(MissionView.Marksman);
			}
			else
			{
				UI.indicatorUpperRight.Indicate("No marksman", 2f);
			}
		}
		else if (Input.GetKeyUp(KeyCode.Escape) && !UI.missionEndMenu.Visible && !UI.endOverlay.Visible && Global.Context.SceneTransition.CurrentScene != SceneTransition.Scene.Benchmark)
		{
			PauseAndShowMenu();
		}
	}

	private void ApplySettings()
	{
		Settings settings = Global.Context.Settings;
		OnShadowsChanged(settings.Performance.shadows);
		OnAmbientLightChanged(settings.Graphics.ambientLightScale);
	}

	private void OnExtrasChanged(SerializableExtraSettings newSettings)
	{
		disableEndCriteria = newSettings.disableEndCriteria;
	}

	public void SetAmbientLightScale(float scale)
	{
		if (ambientLight != null)
		{
			ambientLight.intensity = defaultAmbientLightIntensity * scale;
		}
	}

	private void OnAmbientLightChanged(float scale)
	{
		if (ambientLight != null)
		{
			ambientLight.intensity = defaultAmbientLightIntensity * scale;
		}
	}

	private void OnShadowsChanged(int value)
	{
		foreach (Light shadowLightSource in shadowLightSources)
		{
			shadowLightSource.shadows = (LightShadows)value;
		}
		foreach (Room room in rooms)
		{
			room.SetLightShadowsType((LightShadows)value);
		}
	}

	private void OnCensoringChanged(bool censored)
	{
		foreach (CharacterBody body in Occupants.Bodies)
		{
			body.SetCensored(censored);
		}
		foreach (CharacterBody body2 in Operators.Bodies)
		{
			body2.SetCensored(censored);
		}
		if (censored)
		{
			foreach (Transform item in FloorPoolsContainer)
			{
				item.GetComponent<FloorPoolPainter>().SetCensored();
			}
			return;
		}
		foreach (Transform item2 in CensorBoxContainer)
		{
			Object.Destroy(item2.gameObject);
		}
	}

	private void RecordMissionData(MissionResult result, int score = 0)
	{
		if (!dataHasBeenRecorded)
		{
			Statistics usage = Global.Context.Usage;
			switch (result)
			{
			case MissionResult.Success:
				usage.session.successful++;
				usage.session.finished++;
				usage.RecordMissionScore(score);
				break;
			case MissionResult.Failure:
				usage.session.failed++;
				usage.session.finished++;
				usage.RecordMissionScore(score);
				break;
			case MissionResult.Aborted:
				usage.session.aborted++;
				break;
			case MissionResult.Restarted:
				usage.session.restarted++;
				break;
			}
			usage.RecordMissionFramerate();
			dataHasBeenRecorded = true;
			usage.PersistSessionData();
		}
	}

	public void ReturnToTitle()
	{
		Mixer.OnMissionUnload();
		RecordMissionData(MissionResult.Aborted);
		SceneTransition.Scene scene = ((!Global.Context.CampaignManager.CampaignActive) ? SceneTransition.Scene.Title : SceneTransition.Scene.Campaign);
		SceneManager.LoadScene(SceneTransition.SceneIndex(scene));
	}

	public void EndMission(float uiDelay)
	{
		EndTimestamp = Time.time;
		missionSuccess = !AllOperatorsDisabled && Occupants != null && Occupants.AllRenderedSafe && AllRoomsChecked;
		UI.PauseMenu.Hide();
		Mixer.StartEndedMusic(missionSuccess);
		if (ScoreKeeper != null)
		{
			ScoreKeeper.Calculate();
			RecordMissionData(MissionResult.Failure, ScoreKeeper.Score);
		}
		if (AllOperatorsDisabled)
		{
			ActionScheduler.Schedule(UI.endOverlay.Show, uiDelay, ActionScheduler.DelayType.UnscaledTime);
			return;
		}
		if (Global.Context.CampaignManager.CampaignActive)
		{
			Global.Context.CampaignManager.SetMissionCompleted();
		}
		InteriorMode.UI.weaponPointer.Hide();
		TimeScaler.TimeRate(0.1f);
		UI.missionEndMenu.Show();
	}

	public void SetMode(MissionView mode)
	{
		if (currentMode != mode)
		{
			if (Mode != null)
			{
				Mode.Hide();
			}
			currentMode = mode;
			switch (mode)
			{
			case MissionView.EntryTeam:
				Mode = InteriorMode;
				break;
			case MissionView.Negotiation:
				Mode = negotiationMode;
				break;
			case MissionView.Marksman:
				Mode = MarksmanMode;
				break;
			}
			if (Mode != null)
			{
				Mode.Show();
			}
		}
	}

	public void EntryTeamStandDown()
	{
		Operators.StandDown();
		SetMode(MissionView.Negotiation);
	}

	private void UpdateMissionState()
	{
		if (InteriorMode == null || (TutorialMode != null && TutorialMode.Active) || disableEndCriteria)
		{
			return;
		}
		bool flag = rooms.Count > 0;
		foreach (Room room in rooms)
		{
			if (room.gameObject.activeInHierarchy && !room.cleared)
			{
				flag = false;
				break;
			}
		}
		if (flag && !AllRoomsChecked)
		{
			AllRoomsChecked = true;
			UI.indicatorUpperRight.Indicate("All rooms checked", 3f);
			UI.missionStatusIndicator.Indicate();
		}
		if (AllRoomsChecked && !(Occupants == null))
		{
			if (AllRoomsChecked && (Occupants.AllRestrained || Occupants.Bodies.Count == 0) && !AllClear)
			{
				AllClear = true;
				AllClearTimestamp = Time.time;
				TriggerAllClear();
			}
			if (Occupants.AllRenderedSafe && !AllWeaponsSecured && WeaponsContainer.childCount == 0)
			{
				AllWeaponsSecured = true;
				UI.indicatorUpperRight.Indicate("All weapons secured", 3f);
				UI.missionStatusIndicator.Indicate();
			}
			if (!missionEnded && (Occupants.AllRestrained || Occupants.Bodies.Count == 0) && AllRoomsChecked)
			{
				missionEnded = true;
			}
		}
	}

	private void SetWalkableRange()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 1f;
		if (rooms.Count == 0)
		{
			num2 = 0f;
		}
		Room room = rooms[0];
		num = room.LeftEdge + num3;
		Room room2 = rooms[rooms.Count - 1];
		num2 = room2.RightEdge - num3;
	}

	private void SetLevelExtents()
	{
		if (rooms.Count == 0)
		{
			float num = (LevelLength = 0f);
			num = (EndPosition = num);
			StartPosition = num;
			return;
		}
		Room room = rooms[0];
		StartPosition = ((!(room == null)) ? (room.LeftEdge + 3f) : 0f);
		for (int num4 = rooms.Count - 1; num4 >= 0; num4--)
		{
			Room room2 = rooms[num4];
			if (!(room2 == null) && room2.gameObject.activeInHierarchy)
			{
				EndPosition = room2.LeftEdge + Mathf.Min(room2.Width * 0.75f, room2.Width - 1f);
				break;
			}
		}
		LevelLength = Mathf.Abs(EndPosition - StartPosition);
	}

	public Room RoomAtPosition(float xCoordinate)
	{
		RaycastHit hitInfo = default(RaycastHit);
		if (!Physics.Raycast(new Vector3(xCoordinate, 5f, -1.5f), Vector3.down, out hitInfo, 10f, Masks.Instance.Rooms))
		{
			return null;
		}
		return hitInfo.collider.GetComponent<Room>();
	}

	public void Pause()
	{
		Mode.OnPause();
		Global.Context.Mission.TimeScaler.Pause(true);
	}

	public void Resume()
	{
		Mode.OnResume();
		TimeScaler.Pause(false);
	}

	public void PauseAndShowMenu()
	{
		Paused = true;
		Mode.OnPause();
		UI.PauseMenu.Show();
		Global.Context.Mission.TimeScaler.Pause(true);
	}

	public void ResumeAndHideMenu()
	{
		Paused = false;
		Mode.OnResume();
		UI.PauseMenu.Hide();
		TimeScaler.Pause(false);
	}

	public void Restart()
	{
		RecordMissionData(MissionResult.Restarted);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void OnAllOperatorsDisabled()
	{
		AllOperatorsDisabled = true;
		missionEnded = true;
		missionSuccess = false;
		if (!disableEndCriteria)
		{
			EndMission(2f);
		}
	}

	private void OnAllOccupantsDisabled()
	{
		UI.indicatorUpperRight.Indicate("All occupants pacified", 3f);
		UI.missionStatusIndicator.Indicate();
		onAllClear.Invoke();
		if (InteriorMode == null && !disableEndCriteria)
		{
			EndMission(2f);
		}
	}

	private void OnAllOccupantsRestrained()
	{
		UI.indicatorUpperRight.Indicate("All occupants restrained", 3f);
		UI.missionStatusIndicator.Indicate();
	}

	public void TriggerAllClear()
	{
		UI.indicatorUpperLeft.Indicate("All clear!", 3f);
		onAllClear.Invoke();
	}

	public static string TimeToString(float time, bool shortFormat = true)
	{
		int num = Mathf.FloorToInt(time / 60f);
		string text = string.Empty;
		if (num > 0)
		{
			text = ((!shortFormat) ? string.Format("{0} minute{1}, ", num, (num != 1) ? "s" : string.Empty) : string.Format("{0}{1}:", (num <= 9) ? "0" : string.Empty, num));
		}
		float num2 = time - (float)num * 60f;
		string text2 = ((!shortFormat) ? string.Format("{0} second{1}", Mathf.FloorToInt(num2), (Mathf.FloorToInt(num2) != 1) ? "s" : string.Empty) : string.Format("{0}{1}", (!(num2 > 9f)) ? "0" : string.Empty, Mathf.FloorToInt(num2)));
		return text + text2;
	}

	public void SpawnCellPhone(Vector3 position)
	{
		if (!(negotiationMode == null))
		{
			CellPhone cellPhonePrefab = Global.Context.EquipmentList.cellPhonePrefab;
			if (!(cellPhonePrefab == null) && !(CellPhone != null))
			{
				CellPhone = Object.Instantiate(cellPhonePrefab);
				CellPhone.name = cellPhonePrefab.name;
				CellPhone.Initialize(position, negotiationMode.OnPhoneAnswered);
				negotiationMode.PhoneDelivered = true;
				onPhoneDelivered.Invoke();
			}
		}
	}

	public List<CharacterBody> GetSpatiallySortedBodies(bool operators = true, bool occupants = true)
	{
		List<CharacterBody> list = new List<CharacterBody>();
		if (operators && Operators != null)
		{
			list.AddRange(Operators.Bodies);
		}
		if (occupants && Occupants != null)
		{
			list.AddRange(Occupants.Bodies);
		}
		list.Sort(CharacterComparer.AbsolutePositionAscending());
		return list;
	}

	public void SpawnFragments(int count, Vector3 worldPosition, AmmunitionBase ammunition, float minMassGrams, float maxMassGrams, float minDiameter, float maxDiameter, float minVelocity, float maxVelocity, float minOffsetMeters, float maxOffsetMeters, float minDisperseAngle, float maxDisperseAngle)
	{
		if (!(ammunition == null) && !(ammunition.projectile == null))
		{
			int frameCount = Time.frameCount;
			for (int i = 0; i < count; i++)
			{
				Projectile projectile = Object.Instantiate(ammunition.projectile);
				float mass = Random.Range(minMassGrams, maxMassGrams);
				float diameter = Random.Range(minDiameter, maxDiameter);
				float velocity = Random.Range(minVelocity, maxVelocity);
				float disperseOffset = Random.Range(minOffsetMeters, maxOffsetMeters);
				float alpha = Random.Range(minDisperseAngle, maxDisperseAngle);
				projectile.InitializeExplosionFragment(ammunition, frameCount, ProjectilesContainer, worldPosition, mass, diameter, velocity, disperseOffset, alpha);
			}
		}
	}
}
