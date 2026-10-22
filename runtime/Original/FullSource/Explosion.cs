namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	[Header("General")]
	public float lifetime = 1f;

	[Header("Physics")]
	[Tooltip("Rigid bodies within this radius will receive forces.")]
	public float forceRadius = 5f;

	[Tooltip("For how long period of time will forces be applied? In seconds.")]
	public float forceDuration = 0.1f;

	[Tooltip("This curve describes the intensity of force over time after the detonation.")]
	public AnimationCurve forceIntensity;

	[Tooltip("The peak force intensity. This is scaled by the force intensity curve.")]
	public float forcePeakIntensity = 100f;

	[Header("Physiology")]
	[Tooltip("Characters within this radius of the explosion will receive concussion.")]
	public float concussiveRadius = 7f;

	[Tooltip("Scales the amount of concussion applied to characters. Currently no trauma is applied.")]
	public float concussiveIntensity = 1f;

	[Tooltip("Characters with their head within this radius of the explosion will receive brain damage.")]
	public float traumaRadius = 1f;

	[Tooltip("Limbs within this radius of the explosion will be torn off.")]
	public float dismemberRadius = 0.5f;

	[Tooltip("Volume of brain tissue that is damaged when a character's head is right next to the explosion. Declines linearly with distance up to traumaRadius.")]
	public float maxTraumaVolume = 1f;

	[Tooltip("Characters within this radius of the explosion will be blinded. Primarily if they are facing it.")]
	public float blindRadius = 10f;

	[Header("Flash")]
	[Tooltip("Light source intensity as a function of the light source's lifetime.")]
	public AnimationCurve flashIntensity;

	public float flashPeakIntensity = 3f;

	public float flashDuration = 0.15f;

	[Header("Shockwave")]
	[Tooltip("Shockwave expansion as a function of the wave's normalized lifetime.")]
	public AnimationCurve waveExpansion;

	public float wavePeakRadius = 3f;

	[Tooltip("Shockwave refraction intensity as a function of the wave's normalized lifetime.")]
	public AnimationCurve waveIntensity;

	public float wavePeakIntensity = 0.15f;

	[Tooltip("The velocity of the wave front. In meters per second.")]
	public float waveVelocity = 1000f;

	public Transform shockwave;

	[Header("Fragmentation")]
	public bool spawnFragments;

	public int minCount = 20;

	public int maxCount = 30;

	public float minMassGrams = 4f;

	public float maxMassGrams = 12f;

	public float minDiameter = 4f;

	public float maxDiameter = 30f;

	public float minVelocity = 100f;

	public float maxVelocity = 500f;

	public float minOffsetMeters = 0.05f;

	public float maxOffsetMeters = 0.15f;

	public float minDisperseAngle = -1.5f;

	public float maxDisperseAngle = 1.5f;

	public AmmunitionBase ammunitionPrefab;

	private float timestamp;

	private float waveDuration;

	private float physiologyRadius;

	private float roomFactor;

	private float forceMultiplier;

	private int applyForceMask;

	private int shockwaveCollisionMask;

	private bool drawBurns;

	private float dismemberRadiusSqr;

	private float traumaRadiusSqr;

	private Light flash;

	private Mission mission;

	private Material waveMaterial;

	private List<Rigidbody> forceTargets = new List<Rigidbody>();

	private RaycastHit raycastHit = default(RaycastHit);

	private Room room;

	private void Awake()
	{
		timestamp = Time.time;
		flash = GetComponent<Light>();
		waveDuration = wavePeakRadius / waveVelocity;
		shockwave.transform.localScale = Vector3.zero;
		waveMaterial = shockwave.GetComponent<Renderer>().materials[0];
		applyForceMask = Masks.Instance.AffectedByExplosions;
		shockwaveCollisionMask = Masks.Instance.ExplosionShockwave;
		GetComponent<AudioSource>().priority = 15;
		physiologyRadius = Mathf.Max(traumaRadius, concussiveRadius, blindRadius);
		dismemberRadiusSqr = dismemberRadius * dismemberRadius;
		traumaRadiusSqr = traumaRadius * traumaRadius;
		if (spawnFragments)
		{
			mission = Global.Context.Mission;
		}
	}

	private void Start()
	{
		GlobalContext context = Global.Context;
		context.Mixer.PlayActionMusic(10f);
		room = context.Mission.RoomAtPosition(base.transform.position.x);
		drawBurns = context.Settings.Performance.woundDecals;
		forceMultiplier = context.Settings.Extras.forceMultiplier;
		ApplyPhysiologicalEffects();
		FindForceTargets();
		ApplyExplosionForces(1f);
		TriggerFireAlarms();
		if (spawnFragments)
		{
			Vector3 worldPosition = new Vector3(base.transform.position.x, base.transform.position.y, -1.5f);
			mission.SpawnFragments(Random.Range(minCount, maxCount), worldPosition, ammunitionPrefab, minMassGrams, maxMassGrams, minDiameter, maxDiameter, minVelocity, maxVelocity, minOffsetMeters, maxOffsetMeters, minDisperseAngle, maxDisperseAngle);
		}
	}

	private void Update()
	{
		float num = Time.time - timestamp;
		if (flash != null)
		{
			float num2 = num / flashDuration;
			flash.intensity = flashIntensity.Evaluate(num2) * flashPeakIntensity;
			if (num2 >= 1f)
			{
				Object.Destroy(flash);
			}
		}
		if (shockwave != null)
		{
			float num3 = num / waveDuration;
			waveMaterial.SetFloat("_Strength", waveIntensity.Evaluate(num3) * wavePeakIntensity);
			shockwave.transform.localScale = Vector3.one * waveExpansion.Evaluate(num3) * wavePeakRadius * 2f;
			if (num3 >= 1f)
			{
				Object.Destroy(shockwave.gameObject);
			}
		}
		float num4 = num / forceDuration;
		if (num4 < 1f)
		{
			ApplyExplosionForces(forceIntensity.Evaluate(num4) * forcePeakIntensity);
		}
		if (num >= lifetime)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void FindForceTargets()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, forceRadius, applyForceMask);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!(collider.attachedRigidbody == null))
			{
				forceTargets.Add(collider.attachedRigidbody);
			}
		}
	}

	private void ApplyExplosionForces(float intensity)
	{
		foreach (Rigidbody forceTarget in forceTargets)
		{
			Vector3 vector = forceTarget.transform.position - base.transform.position;
			Vector3 vector2 = vector * (1f - vector.magnitude / forceRadius) * forceMultiplier;
			Vector3 position = forceTarget.transform.position + Random.insideUnitSphere * 0.3f;
			forceTarget.AddForceAtPosition(vector2 * intensity * Time.deltaTime, position, ForceMode.Force);
		}
	}

	private void ApplyPhysiologicalEffects()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, physiologyRadius, Masks.Instance.Characters);
		roomFactor = ((!(room != null)) ? 0.5f : room.ConcussionFactor);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			ApplyPhysiologicalEffectsToCharacter(collider.GetComponent<CharacterBody>());
		}
	}

	private void ApplyPhysiologicalEffectsToCharacter(CharacterBody character)
	{
		Limb head = character.Head;
		Vector3 vector = head.transform.TransformPoint(head.aimpoint) - base.transform.position;
		float magnitude = vector.magnitude;
		if (Physics.Raycast(base.transform.position, vector.normalized, out raycastHit, magnitude, shockwaveCollisionMask))
		{
			return;
		}
		ApplyCharacterConcussion(character, magnitude);
		List<Limb> list = new List<Limb>();
		foreach (Limb limb2 in character.limbs)
		{
			Vector3 vector2 = limb2.transform.TransformPoint(limb2.aimpoint) - base.transform.position;
			vector2.z = 0f;
			float sqrMagnitude = vector2.sqrMagnitude;
			if (sqrMagnitude < dismemberRadiusSqr && limb2.id > LimbId.Neck)
			{
				list.Add(limb2);
			}
			if (drawBurns && sqrMagnitude < traumaRadiusSqr && limb2.limbRenderer != null)
			{
				Vector3 worldPosition = limb2.AimPoint + (base.transform.position - limb2.AimPoint).normalized * 0.2f;
				limb2.limbRenderer.DrawBurn(worldPosition, 200f, 0.3f);
			}
		}
		if (list.Count > 0)
		{
			Limb limb = list[Random.Range(0, list.Count)];
			limb.Detach();
		}
	}

	private void ApplyCharacterConcussion(CharacterBody character, float distance)
	{
		float a = Mathf.Clamp01(1f - distance / concussiveRadius);
		float num = Mathf.Max(a, (!(room == character.Room)) ? 0f : roomFactor);
		float num2 = ((!character.IsOperator) ? 1f : 0.25f);
		float value = num * concussiveIntensity * num2;
		float delay = distance / waveVelocity;
		character.Physiology.AddConcussion(value, delay);
		bool flag = Mathf.Sign(base.transform.position.x - character.transform.position.x) == character.Direction;
		float num3 = 1f - distance / blindRadius;
		float load = num3 * ((!flag) ? 0.25f : 1f) * num2;
		character.AddVisionLoad(load);
		if (distance < traumaRadius)
		{
			float num4 = Mathf.Clamp(1f - distance / traumaRadius, 0f, 1f);
			character.Physiology.AddBrainTrauma(maxTraumaVolume * num4, delay);
		}
	}

	private void TriggerFireAlarms()
	{
		if (room != null)
		{
			room.TriggerFireAlarm(1f, 3f);
		}
	}
}

}
