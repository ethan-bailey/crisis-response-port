namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class LimbAudio : MonoBehaviour
{
	private static float UpdateInterval = 0.05f;

	[Header("Movement")]
	public bool useMovementAudio = true;

	[Tooltip("Audio will only be played/propagated when the measured angle of the limb is in this range. In degrees.")]
	public Vector2 activeRange = new Vector2(0f, 360f);

	[Tooltip("If this transform is set, the angle for the movement range will be compared to this transform's rotation.")]
	public Transform relativeTo;

	public float movementCoolDown = 0.2f;

	[Tooltip("The least and max amount of movement between two updates, used for intensity calculations. In degrees.")]
	public Vector2 movementRange = new Vector2(4f, 45f);

	[Tooltip("The amount of movement is re-mapped into this range for the audio clip volume.")]
	public Vector2 volumeRange = new Vector2(0f, 1f);

	[Tooltip("Should the audio propagate? (Be perceivable to the artificial intelligence.)")]
	public bool propagate;

	public List<AudioClip> movementSamples;

	[Header("Collision")]
	public bool useCollisionAudio = true;

	public List<AudioClip> collisionSamples;

	private Limb limb;

	private AudioPropagator propagator;

	private AudioSource audioSource;

	private Vector2 intensityRange = new Vector2(0f, 5f);

	private float updateTimestamp;

	private float movementTimestamp;

	private float thudTimestamp;

	private float previousAngle;

	private float movementDivider;

	private void Awake()
	{
		limb = GetComponent<Limb>();
		propagator = AudioPropagator.Instance;
		previousAngle = limb.transform.localEulerAngles.z;
		movementDivider = movementRange.y - movementRange.x;
		updateTimestamp = (movementTimestamp = Time.time);
		if (movementSamples.Count == 0)
		{
			useMovementAudio = false;
		}
		if (collisionSamples.Count == 0)
		{
			useCollisionAudio = false;
		}
	}

	private void Start()
	{
		audioSource = limb.Body.Audio.equipmentAudio;
	}

	private void Update()
	{
		if (!useMovementAudio || Time.time - updateTimestamp < UpdateInterval)
		{
			return;
		}
		updateTimestamp = Time.time;
		float num = Time.time - movementTimestamp;
		float num2 = ((!(relativeTo != null)) ? limb.transform.localEulerAngles.z : Mathf.Abs(base.transform.eulerAngles.z - relativeTo.transform.eulerAngles.z));
		if (num >= movementCoolDown)
		{
			if (num2 >= activeRange.x && num2 <= activeRange.y)
			{
				float num3 = Mathf.Abs(previousAngle - num2);
				if (num3 > 300f)
				{
					num3 = 360f - num3;
				}
				if (num3 < movementRange.x)
				{
					return;
				}
				float t = (num3 - movementRange.x) / (movementDivider * Time.timeScale);
				float num4 = Mathf.Lerp(volumeRange.x, volumeRange.y, t);
				if (num4 < 0.25f)
				{
					return;
				}
				audioSource.PlayOneShot(movementSamples[Random.Range(0, movementSamples.Count)], num4);
				if (propagate)
				{
					float power = Mathf.Lerp(intensityRange.x, intensityRange.y, t);
					propagator.Emit(base.transform.position.x, power, AudioStimuliType.Equipment, limb.Body.AudioInstigator);
				}
			}
			movementTimestamp = Time.time;
		}
		previousAngle = num2;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!useCollisionAudio)
		{
			return;
		}
		float num = Time.time - thudTimestamp;
		if (!(num < 1f))
		{
			thudTimestamp = Time.time;
			float magnitude = collision.relativeVelocity.magnitude;
			if (audioSource != null)
			{
				audioSource.PlayOneShot(collisionSamples[Random.Range(0, collisionSamples.Count)], magnitude);
			}
		}
	}
}

}
