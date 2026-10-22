namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UIExtraSettings : MonoBehaviour
{
	public Toggle playerSpectator;

	public Toggle playerInvisible;

	public Toggle playerInvulnerable;

	public Toggle playerDoesNotLimp;

	public Toggle playerMarksmanControl;

	public Toggle floatingBodies;

	public Toggle pinnedBodies;

	public Toggle disableOccupants;

	public Toggle disableLocks;

	public Toggle disableFireAlarms;

	public Toggle announceHits;

	public Toggle showPhysiologyData;

	public Toggle disableExteriorWalls;

	public Toggle disableEndCriteria;

	public Toggle weaponMalfunctions;

	public Slider forceMultiplier;

	public Slider emissionsModifier;

	public Slider woundPowerMultiplier;

	public Slider chunkSizeMultiplier;

	public Slider malfunctionsMultiplier;

	public Slider marksmanRecoil;

	public Button resetForceButton;

	public Button resetEmissionsButton;

	public Button resetWoundPowerButton;

	public Button resetChunkSizeButton;

	public Button resetMalfunctionsButton;

	public Button resetRecoilButton;

	[Tooltip("The game object containing the UI control for the malfunctions multiplier. For hiding/showing.")]
	public GameObject malfunctionSliderControl;

	private Settings settings;

	private SerializableExtraSettings configuration;

	private SerializableExtraSettings defaults = new SerializableExtraSettings();

	private bool initialized;

	private void OnEnable()
	{
		settings = Global.Context.Settings;
		Deserialize(settings.Extras);
		initialized = true;
	}

	private void OnDisable()
	{
		Save();
	}

	private void Start()
	{
		configuration = settings.Extras;
		showPhysiologyData.onValueChanged.AddListener((bool show) =>
		{
			configuration.onShowPhysiologyChanged.Invoke(show);
		});
		weaponMalfunctions.onValueChanged.AddListener(OnWeaponMalfunctionsChanged);
		OnWeaponMalfunctionsChanged(configuration.weaponMalfunctions);
		resetForceButton.onClick.AddListener(() =>
		{
			forceMultiplier.value = defaults.forceMultiplier;
		});
		resetEmissionsButton.onClick.AddListener(() =>
		{
			emissionsModifier.value = defaults.emissionsModifier;
		});
		resetWoundPowerButton.onClick.AddListener(() =>
		{
			woundPowerMultiplier.value = defaults.woundPowerMultiplier;
		});
		resetChunkSizeButton.onClick.AddListener(() =>
		{
			chunkSizeMultiplier.value = defaults.chunkSizeMultiplier;
		});
		resetMalfunctionsButton.onClick.AddListener(() =>
		{
			malfunctionsMultiplier.value = defaults.malfunctionsMultiplier;
		});
		resetRecoilButton.onClick.AddListener(() =>
		{
			marksmanRecoil.value = defaults.marksmanRecoil;
		});
	}

	private void OnWeaponMalfunctionsChanged(bool enable)
	{
		malfunctionSliderControl.SetActive(enable);
	}

	public void Save()
	{
		if (initialized)
		{
			SerializableExtraSettings serializableExtraSettings = Serialize();
			if (serializableExtraSettings.Different(settings.Extras))
			{
				settings.Extras = serializableExtraSettings;
			}
		}
	}

	public void SaveAndClose()
	{
		base.gameObject.SetActive(false);
		Save();
	}

	private SerializableExtraSettings Serialize()
	{
		SerializableExtraSettings serializableExtraSettings = new SerializableExtraSettings();
		serializableExtraSettings.playerSpectator = playerSpectator.isOn;
		serializableExtraSettings.playerInvisible = playerInvisible.isOn;
		serializableExtraSettings.playerInvulnerable = playerInvulnerable.isOn;
		serializableExtraSettings.playerDoesNotLimp = playerDoesNotLimp.isOn;
		serializableExtraSettings.playerMarksmanControl = playerMarksmanControl.isOn;
		serializableExtraSettings.disableOccupants = disableOccupants.isOn;
		serializableExtraSettings.disableLocks = disableLocks.isOn;
		serializableExtraSettings.disableFireAlarms = disableFireAlarms.isOn;
		serializableExtraSettings.disableExteriorWalls = disableExteriorWalls.isOn;
		serializableExtraSettings.disableEndCriteria = disableEndCriteria.isOn;
		serializableExtraSettings.weaponMalfunctions = weaponMalfunctions.isOn;
		serializableExtraSettings.floatingBodies = floatingBodies.isOn;
		serializableExtraSettings.pinnedBodies = pinnedBodies.isOn;
		serializableExtraSettings.announceHits = announceHits.isOn;
		serializableExtraSettings.showPhysiologyData = showPhysiologyData.isOn;
		serializableExtraSettings.forceMultiplier = forceMultiplier.value;
		serializableExtraSettings.emissionsModifier = emissionsModifier.value;
		serializableExtraSettings.woundPowerMultiplier = woundPowerMultiplier.value;
		serializableExtraSettings.chunkSizeMultiplier = chunkSizeMultiplier.value;
		serializableExtraSettings.malfunctionsMultiplier = malfunctionsMultiplier.value;
		serializableExtraSettings.marksmanRecoil = marksmanRecoil.value;
		return serializableExtraSettings;
	}

	private void Deserialize(SerializableExtraSettings data)
	{
		playerSpectator.isOn = data.playerSpectator;
		playerInvisible.isOn = data.playerInvisible;
		playerInvulnerable.isOn = data.playerInvulnerable;
		playerDoesNotLimp.isOn = data.playerDoesNotLimp;
		playerMarksmanControl.isOn = data.playerMarksmanControl;
		disableOccupants.isOn = data.disableOccupants;
		disableLocks.isOn = data.disableLocks;
		disableFireAlarms.isOn = data.disableFireAlarms;
		disableExteriorWalls.isOn = data.disableExteriorWalls;
		disableEndCriteria.isOn = data.disableEndCriteria;
		weaponMalfunctions.isOn = data.weaponMalfunctions;
		floatingBodies.isOn = data.floatingBodies;
		pinnedBodies.isOn = data.pinnedBodies;
		announceHits.isOn = data.announceHits;
		showPhysiologyData.isOn = data.showPhysiologyData;
		forceMultiplier.value = data.forceMultiplier;
		emissionsModifier.value = data.emissionsModifier;
		woundPowerMultiplier.value = data.woundPowerMultiplier;
		chunkSizeMultiplier.value = data.chunkSizeMultiplier;
		malfunctionsMultiplier.value = data.malfunctionsMultiplier;
		marksmanRecoil.value = data.marksmanRecoil;
	}
}

}
