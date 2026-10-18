namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public class DoorTrap : MonoBehaviour
{
	[Header("References")]
	public Transform grenadeTransform;

	public Transform explosiveTransform;

	public Vector2 grenadeRangeY;

	public Vector2 grenadeRangeZ;

	public Grenade grenadePrefab;

	public Explosive explosivePrefab;

	public DoorController doorController;

	[Tooltip("This sample is played before the door reaches the trigger angle.")]
	public AudioClip triggerCue;

	public AudioClip disarm;

	public AudioClip disarmComplete;

	[Header("Trigger")]
	[Tooltip("Door angle at which the trap triggers.")]
	[Range(0f, 200f)]
	public float trapTriggerAngle = 30f;

	private Settings settings;

	private TimeScaler timeScaler;

	private Grenade grenade;

	private Explosive explosive;

	private bool audioCueTriggered;

	private float audioCueTriggerAngle;

	public bool Armed { get; private set; }

	public bool Detonated { get; private set; }

	private void Start()
	{
		settings = Global.Context.Settings;
		timeScaler = Global.Context.Mission.TimeScaler;
		audioCueTriggerAngle = Mathf.Min(trapTriggerAngle - 10f, 0f);
		if (doorController == null)
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (!Armed)
		{
			return;
		}
		if (doorController.Angle >= trapTriggerAngle)
		{
			Trigger();
		}
		if (!audioCueTriggered && doorController.Angle > audioCueTriggerAngle)
		{
			audioCueTriggered = true;
			if (doorController != null && triggerCue != null)
			{
				doorController.TriggerTrapCue(triggerCue);
			}
		}
		else if (audioCueTriggered && doorController.Angle <= audioCueTriggerAngle)
		{
			audioCueTriggered = false;
		}
	}

	public void Initialize(DoorController controller)
	{
		Armed = true;
		base.name = "Door trap";
		doorController = controller;
		base.transform.SetParent(controller.hinge);
		base.transform.localPosition = new Vector3(0f, Random.Range(grenadeRangeY.x, grenadeRangeY.y), Random.Range(grenadeRangeZ.x, grenadeRangeZ.y));
		trapTriggerAngle = Random.Range(25f, 35f);
		if (Random.value <= 0.75f)
		{
			Object.Destroy(explosiveTransform.gameObject);
			grenade = Object.Instantiate(grenadePrefab);
			grenade.name = "Trap grenade";
			grenade.transform.SetParent(grenadeTransform);
			grenade.transform.localPosition = Vector3.zero;
			grenade.onDetonate.AddListener(OnGrenadeExploded);
			grenadeTransform.localPosition = new Vector3((controller.OpeningDirection != -1f) ? 0.02f : 0.105f, 0f, 0f);
			grenadeTransform.localEulerAngles = new Vector3(Random.Range(0f, 360f), 180f, 0f);
		}
		else
		{
			Object.Destroy(grenadeTransform.gameObject);
			explosive = Object.Instantiate(explosivePrefab);
			explosive.name = "Trap explosive";
			explosive.transform.SetParent(explosiveTransform);
			explosive.transform.localPosition = Vector3.zero;
			explosive.onDetonate.AddListener(OnExplosiveExploded);
			explosiveTransform.localPosition = new Vector3((controller.OpeningDirection != -1f) ? 0.01f : 0.075f, 0f, 0f);
			explosiveTransform.localEulerAngles = new Vector3(Random.Range(0f, 360f), 180f, 0f);
		}
	}

	public void PlayDisarmAudio()
	{
		doorController.AudioSource.loop = true;
		doorController.AudioSource.PlayOneShot(disarm);
	}

	public void StopDisarmAudio()
	{
		if (doorController.AudioSource.loop)
		{
			doorController.AudioSource.Stop();
			doorController.AudioSource.loop = false;
		}
	}

	public void Disarm()
	{
		Armed = false;
		doorController.OnTrapDisarmed(disarmComplete);
	}

	public void Arm()
	{
		if (Detonated)
		{
			return;
		}
		Armed = true;
		Operators operators = Global.Context.Mission.Operators;
		doorController.AudioSource.PlayOneShot(triggerCue);
		doorController.TrapState = DoorController.DoorTrapState.Unknown;
		if (!(operators != null))
		{
			return;
		}
		PlayerController playerController = operators.PlayerController;
		if (playerController != null)
		{
			CharacterBody body = playerController.Body;
			if (Mathf.Abs(base.transform.position.x - body.Position) < body.VisionRange)
			{
				doorController.TrapState = DoorController.DoorTrapState.HasTrap;
			}
		}
	}

	public void Trigger()
	{
		Armed = false;
		Detonated = true;
		doorController.TrapState = DoorController.DoorTrapState.NoTrap;
		if (grenade != null)
		{
			grenade.PullPin(null);
			grenade.StartFuse(null);
			grenade.ThrowSpoon(Random.onUnitSphere);
		}
		if (explosive != null)
		{
			explosive.Trigger();
		}
	}

	private void OnGrenadeExploded()
	{
		Object.Destroy(grenadeTransform.gameObject);
	}

	private void OnExplosiveExploded()
	{
		Object.Destroy(explosiveTransform.gameObject);
	}
}

}
