namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.UI;

public class UISlowMotionSettings : MonoBehaviour
{
	public Toggle enableBulletTime;

	public GameObject entryContainer;

	public UIBulletTimeEntry playerFire;

	public UIBulletTimeEntry computerFire;

	public UIBulletTimeEntry marksmanFire;

	public UIBulletTimeEntry grenadeExplode;

	public UIBulletTimeEntry playerOpenDoor;

	public UIBulletTimeEntry playerHit;

	public UIBulletTimeEntry computerHit;

	public UINonLinearSlider slowMotionRate;

	public UINonLinearSlider slowMotionEaseInOut;

	[Header("Text headers")]
	public Text bulletTimeText;

	public Text slowMotionText;

	private Settings settings;

	private bool initialized;

	private void OnEnable()
	{
		settings = Global.Context.Settings;
		Deserialize(settings.Time);
		initialized = true;
		bulletTimeText.text = string.Format("Bullet time (toggle with {0})", Global.Context.InputHandler.Keys.bulletTime.KeyName);
		slowMotionText.text = string.Format("Slow motion (toggle with {0})", Global.Context.InputHandler.Keys.slowMotion.KeyName);
	}

	private void OnDisable()
	{
		Save();
	}

	private void Save()
	{
		if (initialized)
		{
			SerializableTimeSettings serializableTimeSettings = Serialize();
			if (serializableTimeSettings.Different(settings.Time))
			{
				settings.Time = serializableTimeSettings;
			}
		}
	}

	private void Awake()
	{
		settings = Global.Context.Settings;
		enableBulletTime.onValueChanged.RemoveAllListeners();
		enableBulletTime.onValueChanged.AddListener(OnToggleEnabled);
		initialized = true;
	}

	private void OnToggleEnabled(bool value)
	{
		entryContainer.SetActive(value);
	}

	private SerializableTimeSettings Serialize()
	{
		SerializableTimeSettings serializableTimeSettings = new SerializableTimeSettings();
		serializableTimeSettings.enableBulletTime = enableBulletTime.isOn;
		serializableTimeSettings.playerFire = playerFire.Serialize();
		serializableTimeSettings.computerFire = computerFire.Serialize();
		serializableTimeSettings.marksmanFire = marksmanFire.Serialize();
		serializableTimeSettings.grenadeExplode = grenadeExplode.Serialize();
		serializableTimeSettings.playerOpenDoor = playerOpenDoor.Serialize();
		serializableTimeSettings.playerHit = playerHit.Serialize();
		serializableTimeSettings.computerHit = computerHit.Serialize();
		serializableTimeSettings.slowMotionEaseInOut = slowMotionEaseInOut.TransformedValue;
		serializableTimeSettings.slowMotionRate = slowMotionRate.TransformedValue;
		return serializableTimeSettings;
	}

	private void Deserialize(SerializableTimeSettings data)
	{
		enableBulletTime.isOn = data.enableBulletTime;
		playerFire.Deserialize(data.playerFire);
		computerFire.Deserialize(data.computerFire);
		marksmanFire.Deserialize(data.marksmanFire);
		grenadeExplode.Deserialize(data.grenadeExplode);
		playerOpenDoor.Deserialize(data.playerOpenDoor);
		playerHit.Deserialize(data.playerHit);
		computerHit.Deserialize(data.computerHit);
		slowMotionEaseInOut.SetValue(data.slowMotionEaseInOut);
		slowMotionRate.SetValue(data.slowMotionRate);
		entryContainer.SetActive(data.enableBulletTime);
	}
}

}
