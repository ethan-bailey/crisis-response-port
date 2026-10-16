using UnityEngine;
using UnityEngine.Events;

public class Grenade : MonoBehaviour
{
	public enum Type
	{
		Stun = 0,
		Frag = 1
	}

	public Type type;

	public float fuseMinSeconds = 1f;

	public float fuseMaxSeconds = 1f;

	[Range(0.1f, 0.5f)]
	[Tooltip("The mass to use for the rigidbody that is added to the grenade on activation. In kilograms.")]
	public float mass = 0.236f;

	public bool destroyOnDetonation;

	public float minThrowForce = 1.5f;

	public float maxThrowForce = 2f;

	[Header("References")]
	public GameObject pin;

	public GrenadeSpoon spoon;

	public GameObject detonationEffect;

	public UIGrenadeTimer timerUI;

	public AudioClip pullPinSample;

	public AudioClip spoonReleaseSample;

	public UnityEvent onDetonate = new UnityEvent();

	private const float MinBounceAudioInterval = 0.2f;

	private Settings settings;

	private Rigidbody rigidBody;

	private TimeScaler timeScaler;

	private AudioSource audioSource;

	private AudioPropagator audioPropagator;

	private Transform spentGrenadesTransform;

	private UIGrenadeTimer timerUIInstance;

	private float soundAmplitude = 180f;

	private float timestamp = float.PositiveInfinity;

	private float bounceTimestamp;

	private bool destroyTimerUiWhenThrown;

	public CharacterBody Owner { get; private set; }

	public AudioSurface AudioSurface { get; private set; }

	public float SecondsLeftOnFuse { get; private set; }

	public float OriginalFuse { get; private set; }

	public bool FuseBurning { get; private set; }

	public bool Detonated { get; private set; }

	public bool IsThrown { get; private set; }

	public bool IsHeldByOwner
	{
		get
		{
			return Owner != null && !IsThrown;
		}
	}

	public float Position
	{
		get
		{
			return base.transform.position.x;
		}
	}

	public bool PinRemoved { get; private set; }

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		audioSource.volume = 0.5f;
		audioSource.priority = 100;
		audioPropagator = AudioPropagator.Instance;
	}

	private void Start()
	{
		settings = Global.Context.Settings;
		Mission mission = Global.Context.Mission;
		timeScaler = mission.TimeScaler;
		spentGrenadesTransform = mission.SpentGrenadesContainer;
	}

	private void Update()
	{
		if (FuseBurning && !Detonated)
		{
			SecondsLeftOnFuse -= Time.deltaTime;
			if (SecondsLeftOnFuse <= 0f)
			{
				Detonate();
				Object.Destroy(this);
			}
		}
	}

	public void PullPin(CharacterBody body)
	{
		if (!PinRemoved)
		{
			PinRemoved = true;
			if (pin != null)
			{
				pin.SetActive(false);
			}
			if (audioSource != null)
			{
				audioSource.PlayOneShot(pullPinSample);
			}
			audioPropagator.Emit(base.transform.position.x, 3f, AudioStimuliType.Equipment, (!(body != null)) ? null : body.AudioInstigator);
		}
	}

	public void ReturnPin(CharacterBody body)
	{
		PinRemoved = false;
		if (pin != null)
		{
			pin.SetActive(true);
		}
		if (audioSource != null)
		{
			audioSource.PlayOneShot(pullPinSample);
			audioPropagator.Emit(base.transform.position.x, 3f, AudioStimuliType.Equipment, (!(body != null)) ? null : body.AudioInstigator);
		}
	}

	public void StartFuse(CharacterBody caster)
	{
		if (!Detonated && !FuseBurning)
		{
			Owner = caster;
			FuseBurning = true;
			timestamp = Time.time;
			float originalFuse = (SecondsLeftOnFuse = Random.Range(fuseMinSeconds, fuseMaxSeconds));
			OriginalFuse = originalFuse;
			SpawnTimerUI(caster);
		}
	}

	public void Throw(Vector3 direction)
	{
		IsThrown = true;
		Vector3 force = direction * Random.Range(minThrowForce, maxThrowForce);
		rigidBody = base.gameObject.AddComponent<Rigidbody>();
		rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
		rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rigidBody.mass = mass;
		rigidBody.AddForce(force, ForceMode.Impulse);
		rigidBody.AddTorque(Random.insideUnitSphere, ForceMode.Impulse);
		ThrowSpoon(direction * 7f);
		if (destroyTimerUiWhenThrown && timerUIInstance != null)
		{
			Object.Destroy(timerUIInstance.gameObject);
		}
	}

	public void ThrowSpoon(Vector3 velocity)
	{
		if (!(spoon == null))
		{
			spoon.Throw(velocity + Random.onUnitSphere * 0.25f);
			if (audioSource != null)
			{
				audioSource.PlayOneShot(spoonReleaseSample);
			}
			audioPropagator.Emit(base.transform.position.x, 3f, AudioStimuliType.Equipment, (!(Owner != null)) ? null : Owner.AudioInstigator);
		}
	}

	private void SpawnTimerUI(CharacterBody owner)
	{
		if (timerUI == null)
		{
			return;
		}
		switch (Global.Context.Settings.General.GetGrenadeTimerMode)
		{
		case SerializableGeneralSettings.GrenadeTimerMode.Never:
			return;
		case SerializableGeneralSettings.GrenadeTimerMode.OnlyForPlayer:
			if (owner == null || !owner.IsPlayer)
			{
				return;
			}
			break;
		case SerializableGeneralSettings.GrenadeTimerMode.OnlyWhenBaking:
			if (!IsHeldByOwner || !FuseBurning)
			{
				return;
			}
			destroyTimerUiWhenThrown = true;
			break;
		}
		timerUIInstance = Object.Instantiate(timerUI);
		timerUIInstance.Initialize(this);
	}

	private void Detonate()
	{
		Detonated = true;
		InvokeBulletTime();
		SpawnEffect();
		if (rigidBody != null)
		{
			rigidBody.AddForceAtPosition((Vector3.up + Random.insideUnitSphere).normalized * 0.5f, base.transform.position + Random.insideUnitSphere * 0.25f, ForceMode.Impulse);
		}
		Room room = Global.Context.Mission.RoomAtPosition(base.transform.position.x);
		if (room != null)
		{
			room.HasBeenStunned = true;
		}
		base.transform.SetParent(spentGrenadesTransform, false);
		audioPropagator.Emit(base.transform.position.x, soundAmplitude, AudioStimuliType.Explosion, (!(Owner != null)) ? null : Owner.AudioInstigator);
		onDetonate.Invoke();
		if (Owner != null && Owner.IsOperator)
		{
			switch (type)
			{
			case Type.Stun:
				Owner.StunGrenadesUsed++;
				break;
			case Type.Frag:
				Owner.FragGrenadesUsed++;
				break;
			}
		}
		if (destroyOnDetonation)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void InvokeBulletTime()
	{
		timeScaler.BulletTime(settings.Time.grenadeExplode);
	}

	protected void SpawnEffect()
	{
		if (!(detonationEffect == null))
		{
			GameObject gameObject = Object.Instantiate(detonationEffect);
			gameObject.transform.SetParent(MissionHelper.Instance.EffectsContainer);
			gameObject.transform.position = new Vector3(base.transform.position.x, Mathf.Max(base.transform.position.y, 0.1f), base.transform.position.z);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		bounceTimestamp = Time.time;
		SetAudioSurface(collision.collider);
		if (!(AudioSurface == null))
		{
			PlayBounce(collision.relativeVelocity.magnitude * 2f);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (!(Time.time - bounceTimestamp < 0.2f))
		{
			SetAudioSurface(collision.collider);
			if (!(AudioSurface == null))
			{
				PlayBounce(collision.relativeVelocity.magnitude * 2f);
				bounceTimestamp = Time.time;
			}
		}
	}

	private void SetAudioSurface(Collider collider)
	{
		if (!(collider == null))
		{
			AudioSurface component = collider.GetComponent<AudioSurface>();
			if (!(component == null))
			{
				AudioSurface = component;
			}
		}
	}

	private void PlayBounce(float intensity)
	{
		audioSource.PlayOneShot(AudioSurface.RandomGrenadeBounce(), intensity);
		audioPropagator.Emit(base.transform.position.x, intensity * 0.5f, AudioStimuliType.Explosion, Owner.AudioInstigator);
	}
}
