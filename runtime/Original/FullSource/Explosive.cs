namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;
using UnityEngine.Events;

public class Explosive : MonoBehaviour
{
	public bool destroyOnDetonation;

	public GameObject explosionPrefab;

	public UnityEvent onDetonate = new UnityEvent();

	private Settings settings;

	private TimeScaler timeScaler;

	private AudioPropagator audioPropagator;

	private float soundAmplitude = 180f;

	public CharacterBody Owner { get; private set; }

	public bool Detonated { get; private set; }

	private void Awake()
	{
		audioPropagator = AudioPropagator.Instance;
	}

	private void Start()
	{
		settings = Global.Context.Settings;
		Mission mission = Global.Context.Mission;
		timeScaler = mission.TimeScaler;
	}

	public void Trigger()
	{
		Detonate();
	}

	private void Detonate()
	{
		if (!Detonated)
		{
			Detonated = true;
			timeScaler.BulletTime(settings.Time.grenadeExplode);
			SpawnDetonationEffect();
			audioPropagator.Emit(base.transform.position.x, soundAmplitude, AudioStimuliType.Explosion, (!(Owner != null)) ? null : Owner.AudioInstigator);
			onDetonate.Invoke();
			if (destroyOnDetonation)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void SpawnDetonationEffect()
	{
		if (!(explosionPrefab == null))
		{
			GameObject gameObject = Object.Instantiate(explosionPrefab);
			gameObject.transform.SetParent(MissionHelper.Instance.EffectsContainer);
			gameObject.transform.position = new Vector3(base.transform.position.x, Mathf.Max(base.transform.position.y, 0.1f), base.transform.position.z);
		}
	}
}

}
