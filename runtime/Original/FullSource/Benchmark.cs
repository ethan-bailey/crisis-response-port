namespace Crisis.Runtime.FullSource
{
using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class Benchmark : MonoBehaviour
{
	[Header("Variables")]
	public float fireInterval = 0.01f;

	public float explosionInterval = 0.1f;

	[Header("References")]
	public Text fpsText;

	public Text lowFpsText;

	public Text highFpsText;

	public GameObject finalResult;

	public Text finalResultText;

	public TextFader finalResultFader;

	public TextScaler finalResultScaler;

	public Button applyButton;

	public Button exitButton;

	public Room room;

	public InteriorCamera benchmarkCamera;

	public Transform container;

	public Transform projectilesContainer;

	public Transform resultsContainer;

	public UIBenchmarkResult resultPrefab;

	public GameObject resultSeparatorPrefab;

	public GameObject resultSpacerPrefab;

	public AmmunitionBase ammunition;

	public AmmunitionBase marksmanAmmunition;

	public Explosion explosionPrefab;

	private QualityLevels.QualityLevel originalQuality;

	private QualityLevels.QualityLevel level;

	private QualityLevels.QualityLevel recommendedLevel;

	private SerializablePerformanceSettings originalPerformanceSettings;

	private List<CharacterBody> bodies;

	private UIBenchmarkResult currentResultEntry;

	private List<uint> results;

	private MissionHelper helper;

	private GameObject spacer;

	private const float LevelPeriod = 5f;

	private float levelStartTime;

	private float lastFireTime;

	private float lastExplosionTime;

	private float minNoteTimestamp;

	private float maxNoteTimestamp;

	private int minNote;

	private int maxNote;

	private bool fireDirection;

	private bool started;

	private bool finished;

	private bool levelActive;

	private int explosionIndex;

	private uint levelFrames;

	private uint finalResultSum;

	private bool appliedRecommended;

	private bool bulletTimeWasEnabled;

	private void Awake()
	{
		QualitySettings.vSyncCount = 0;
		bodies = new List<CharacterBody>();
		results = new List<uint>();
	}

	private void Start()
	{
		originalQuality = (QualityLevels.QualityLevel)Global.Context.Settings.Performance.qualityLevel;
		originalPerformanceSettings = Global.Context.Settings.Performance.Copy();
		bulletTimeWasEnabled = Global.Context.Settings.Time.enableBulletTime;
		room.exteriorGeometry.gameObject.SetActive(false);
		helper = MissionHelper.Instance;
		level = QualityLevels.QualityLevel.Fastest;
		QualityLevels.SetQualityLevel(level);
		benchmarkCamera.SetManualTargetPosition(3f);
		PerformanceManager.Instance.OnPerformanceSettingsChanged(SerializablePerformanceSettings.GetPreset(level));
		Global.Context.Settings.Time.enableBulletTime = false;
		Global.Context.frameRateDisplay.gameObject.SetActive(false);
		Global.Context.Mixer.tensionSource.mute = true;
		Global.Context.Mixer.actionSource.mute = true;
		Global.Context.Mixer.endedSource.mute = true;
		spacer = UnityEngine.Object.Instantiate(resultSpacerPrefab);
		spacer.transform.SetParent(resultsContainer);
		spacer.transform.localScale = Vector3.one;
		spacer.SetActive(false);
		Coroutine.DelayedAction(1f, PrepareNextLevel, this);
	}

	private void OnDestroy()
	{
		Global.Context.Mixer.tensionSource.mute = false;
		Global.Context.Mixer.actionSource.mute = false;
		Global.Context.Mixer.endedSource.mute = false;
		Global.Context.Settings.Time.enableBulletTime = bulletTimeWasEnabled;
		Global.Context.frameRateDisplay.gameObject.SetActive(Global.Context.Settings.Graphics.showFrameRate);
		Global.Context.Settings.Graphics.ApplyVsyncCount();
		if (!appliedRecommended)
		{
			QualityLevels.SetQualityLevel(originalQuality);
			Global.Context.Settings.Performance.Overwrite(originalPerformanceSettings);
		}
	}

	private void Update()
	{
		int num = Mathf.RoundToInt(1f / Time.deltaTime);
		fpsText.text = string.Format("{0}", num);
		fpsText.color = Color.Lerp(Color.red, Color.white, (1f / Time.deltaTime - 30f) / 60f);
		if (num > maxNote)
		{
			maxNote = num;
			maxNoteTimestamp = Time.time;
			highFpsText.text = string.Format("{0}", maxNote);
		}
		else if (num < minNote)
		{
			minNote = num;
			minNoteTimestamp = Time.time;
			lowFpsText.text = string.Format("{0}", minNote);
		}
		if (Time.time - maxNoteTimestamp >= 1f)
		{
			maxNote = num;
			maxNoteTimestamp = Time.time;
			highFpsText.text = string.Format("{0}", maxNote);
		}
		if (Time.time - minNoteTimestamp >= 1f)
		{
			minNote = num;
			minNoteTimestamp = Time.time;
			lowFpsText.text = string.Format("{0}", minNote);
		}
		lowFpsText.color = Color.Lerp(Color.white, Color.gray, (Time.time - minNoteTimestamp) * 4f);
		highFpsText.color = Color.Lerp(Color.white, Color.gray, (Time.time - maxNoteTimestamp) * 4f);
		if (finished)
		{
			if (!appliedRecommended && (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)))
			{
				OnApplyButtonPressed();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				OnExitButtonPressed();
			}
		}
		if (started && !finished && levelActive)
		{
			levelFrames++;
			float num2 = Time.time - levelStartTime;
			if (num2 <= 3.75f && Time.time - lastFireTime >= fireInterval)
			{
				FireProjectile();
				lastFireTime = Time.time;
			}
			if (num2 >= 4f && explosionIndex < 1 && Time.time - lastExplosionTime >= explosionInterval)
			{
				SpawnExplosion();
				lastExplosionTime = Time.time;
				explosionIndex++;
			}
			if (num2 >= 5f)
			{
				EndCurrentLevel();
			}
		}
	}

	private void PrepareNextLevel()
	{
		ApplySettingsLevel(level);
		CleanPreviousLevel();
		SpawnBodies();
		Coroutine.DelayedAction(0.25f, InitializeNextLevelBodies, this);
		Coroutine.DelayedAction(1.25f, BeginNewLevel, this);
	}

	private void InitializeNextLevelBodies()
	{
		InitializeBodies();
	}

	private void BeginNewLevel()
	{
		explosionIndex = 0;
		levelStartTime = Time.time;
		started = (levelActive = true);
		AddLevelEntry();
	}

	private void SkipCurrentLevel()
	{
		levelFrames = 0u;
		EndCurrentLevel();
	}

	private void EndCurrentLevel()
	{
		DropBodies();
		levelStartTime = Time.time - 5f;
		uint num = levelFrames / 5;
		results.Add(num);
		SetLevelResult(num);
		Global.Context.AudioPlayer.PlayBenchmarkResult(num >= 55);
		if (level == QualityLevels.QualityLevel.Unlimited)
		{
			ulong num2 = 0uL;
			foreach (uint result in results)
			{
				num2 += result;
			}
			finished = true;
			Coroutine.DelayedAction(3f, OnFinished, this);
		}
		else
		{
			level++;
			levelFrames = 0u;
			levelActive = false;
			Coroutine.DelayedAction(3f, PrepareNextLevel, this);
		}
	}

	private void ApplySettingsLevel(QualityLevels.QualityLevel newLevel)
	{
		if (newLevel < QualityLevels.QualityLevel.Unlimited)
		{
			QualityLevels.SetQualityLevel(newLevel);
		}
		SerializablePerformanceSettings preset = SerializablePerformanceSettings.GetPreset(newLevel);
		Global.Context.Settings.BenchmarkPerformance = preset;
		PerformanceManager.Instance.OnPerformanceSettingsChanged(preset);
	}

	private void SpawnBodies()
	{
		for (int i = 0; i < 3; i++)
		{
			bodies.Add(SpawnOccupant(1.75f + (float)i));
		}
	}

	private void InitializeBodies()
	{
		foreach (CharacterBody body in bodies)
		{
			body.Turn(1f);
			body.SetUnconscious();
			body.Torso.SetKinematic(true);
		}
	}

	private void DropBodies()
	{
		foreach (CharacterBody body in bodies)
		{
			body.Torso.SetKinematic(false);
		}
	}

	private void CleanPreviousLevel()
	{
		PerformanceManager.Instance.RemoveAllResources();
		foreach (CharacterBody body in bodies)
		{
			UnityEngine.Object.Destroy(body.gameObject);
		}
		bodies.Clear();
		Transform bodyPartsContainer = helper.Mission.BodyPartsContainer;
		foreach (Transform item in bodyPartsContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	private void OnFinished()
	{
		CleanupOnFinish();
		int num = 0;
		int num2 = -1;
		Debug.Log("Benchmark results:");
		foreach (uint result in results)
		{
			object[] array = new object[2];
			QualityLevels.QualityLevel qualityLevel = (QualityLevels.QualityLevel)num;
			array[0] = qualityLevel.ToString();
			array[1] = result;
			Debug.LogFormat(" {0}: {1} fps", array);
			if (result >= 55)
			{
				num2++;
			}
			num++;
		}
		recommendedLevel = (QualityLevels.QualityLevel)Mathf.Max(0, num2);
		Coroutine.DelayedAction(1f, ShowFinalResult, this);
	}

	private void CleanupOnFinish()
	{
		CleanPreviousLevel();
		foreach (Transform item in helper.FloorPoolsContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in helper.GibsContainer)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		foreach (Transform item3 in helper.EffectsContainer)
		{
			UnityEngine.Object.Destroy(item3.gameObject);
		}
	}

	private void ShowFinalResult()
	{
		FinalResultPopNext();
		finalResult.gameObject.SetActive(true);
	}

	private void FinalResultPopNext()
	{
		if (results.Count > 0)
		{
			finalResultSum += results[0];
			finalResultText.text = string.Format("{0}", finalResultSum);
			finalResultScaler.FireOnce();
			results.RemoveAt(0);
			Global.Context.AudioPlayer.PlayBenchmarkResult(true);
			Coroutine.DelayedAction(0.3f, FinalResultPopNext, this);
		}
		else
		{
			applyButton.gameObject.SetActive(true);
			exitButton.gameObject.SetActive(true);
			finalResultFader.enabled = true;
		}
	}

	private void OnApplyButtonPressed()
	{
		SerializablePerformanceSettings preset = SerializablePerformanceSettings.GetPreset(recommendedLevel);
		Global.Context.Settings.Performance = preset;
		QualityLevels.SetQualityLevel(recommendedLevel);
		Global.Context.MessageBox.ShowOK("Recommended settings applied.");
		applyButton.gameObject.SetActive(false);
		appliedRecommended = true;
	}

	private void OnExitButtonPressed()
	{
		if (finalResult.activeInHierarchy)
		{
			finalResult.gameObject.SetActive(false);
			Coroutine.DelayedAction(0.25f, () =>
			{
				Global.Context.Mission.ReturnToTitle();
			}, this);
		}
	}

	private CharacterBody SpawnOccupant(float position)
	{
		GameObject gameObject = Resources.Load(OccupantSpawner.PrefabPath + "Occupant") as GameObject;
		if (gameObject == null)
		{
			return null;
		}
		CharacterBody characterBody = UnityEngine.Object.Instantiate(gameObject.GetComponent<CharacterBody>());
		characterBody.name = "Benchmark occupant";
		characterBody.weight = UnityEngine.Random.Range(75f, 85f);
		characterBody.transform.SetParent(container, true);
		characterBody.transform.position = new Vector3(position, 1.5f, -1.5f);
		characterBody.InitializeEquipment(SerializableEquipment.EmptyEquipment());
		characterBody.gameObject.AddComponent<Occupant>();
		BehaviorTree component = characterBody.GetComponent<BehaviorTree>();
		UnityEngine.Object.Destroy(component);
		Occupants occupants = Global.Context.Mission.Occupants;
		occupants.RegisterBody(characterBody);
		return characterBody;
	}

	private void FireProjectile()
	{
		float num = (Time.time - levelStartTime) % 1f;
		float y = 1f + Mathf.Sin(num * (float)Math.PI * 2f) * 0.5f;
		Vector3 position = new Vector3((!fireDirection) ? 5.9f : 0.1f, y, -1.5f);
		Vector3 forward = ((!fireDirection) ? Vector3.left : Vector3.right);
		int frameCount = Time.frameCount;
		Projectile projectile = UnityEngine.Object.Instantiate(ammunition.projectile);
		int id = (projectile.SwarmId = frameCount);
		projectile.Id = id;
		projectile.name = ammunition.projectile.name;
		projectile.transform.SetParent(projectilesContainer, false);
		projectile.transform.position = position;
		projectile.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
		projectile.InitializeBenchmark(ammunition);
		fireDirection = !fireDirection;
	}

	private void FireMarksmanProjectile(Vector3 from, Vector3 to)
	{
		Projectile projectile = UnityEngine.Object.Instantiate(marksmanAmmunition.projectile);
		int id = (projectile.SwarmId = Time.frameCount);
		projectile.Id = id;
		projectile.name = marksmanAmmunition.projectile.name;
		projectile.transform.SetParent(projectilesContainer, false);
		projectile.transform.position = from;
		projectile.transform.LookAt(to);
		projectile.InitializeMarksman(marksmanAmmunition, 9f);
	}

	private void SpawnExplosion()
	{
		Explosion explosion = UnityEngine.Object.Instantiate(explosionPrefab);
		explosion.transform.SetParent(helper.EffectsContainer);
		explosion.transform.position = new Vector3(3f + (float)explosionIndex, 1.5f, -1.5f);
	}

	private void AddLevelEntry()
	{
		if (resultsContainer.childCount > 1)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(resultSeparatorPrefab);
			gameObject.transform.SetParent(resultsContainer);
			gameObject.transform.localScale = Vector3.one;
		}
		spacer.transform.SetAsLastSibling();
		spacer.SetActive(resultsContainer.childCount > 1);
		currentResultEntry = UnityEngine.Object.Instantiate(resultPrefab);
		currentResultEntry.transform.SetParent(resultsContainer);
		currentResultEntry.transform.localScale = Vector3.one;
		currentResultEntry.resultText.text = "Running";
		currentResultEntry.resultText.color = Color.white;
		currentResultEntry.levelText.text = level.ToString();
	}

	private void SetLevelResult(uint result)
	{
		currentResultEntry.resultFader.enabled = false;
		currentResultEntry.resultText.text = ((result != 0) ? string.Format("{0}", result) : "X");
		Text resultText = currentResultEntry.resultText;
		Color color;
		switch (result)
		{
		default:
			color = Color.green;
			break;
		case 50u:
		case 51u:
		case 52u:
		case 53u:
		case 54u:
			color = Color.yellow;
			break;
		case 0u:
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
		case 8u:
		case 9u:
		case 10u:
		case 11u:
		case 12u:
		case 13u:
		case 14u:
		case 15u:
		case 16u:
		case 17u:
		case 18u:
		case 19u:
		case 20u:
		case 21u:
		case 22u:
		case 23u:
		case 24u:
		case 25u:
		case 26u:
		case 27u:
		case 28u:
		case 29u:
		case 30u:
		case 31u:
		case 32u:
		case 33u:
		case 34u:
		case 35u:
		case 36u:
		case 37u:
		case 38u:
		case 39u:
		case 40u:
		case 41u:
		case 42u:
		case 43u:
		case 44u:
		case 45u:
		case 46u:
		case 47u:
		case 48u:
		case 49u:
			color = Color.red;
			break;
		}
		resultText.color = color;
		spacer.SetActive(false);
	}
}

}
