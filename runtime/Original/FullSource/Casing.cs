namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class Casing : MonoBehaviour
{
	[Tooltip("Mass of the empty casing. In grams.")]
	public float mass = 5f;

	public float drag = 0.1f;

	public float angularDrag = 0.1f;

	[Header("Audio")]
	public AudioSource audioSource;

	public List<AudioClip> bounceAudio;

	private Rigidbody rigidBody;

	public Vector3 Force { get; set; }

	public Vector3 Torque { get; set; }

	public float Offset { get; private set; }

	public bool Spent { get; private set; }

	public bool Live
	{
		get
		{
			return !Spent;
		}
	}

	private void Awake()
	{
		Offset = GetComponent<BoxCollider>().size.x * 0.5f;
		audioSource.priority = 100;
	}

	private void Update()
	{
		if (float.IsNaN(base.transform.position.x))
		{
			Debug.LogErrorFormat("Destroyed casing with NaN transform: {0}", base.name);
			Object.Destroy(base.gameObject);
		}
		else if (base.transform.position.y < -0.5f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Detonate()
	{
		Spent = true;
		if (base.transform.childCount > 0)
		{
			Object.Destroy(base.transform.GetChild(0).gameObject);
		}
	}

	public void Eject()
	{
		GetComponent<Collider>().enabled = true;
		audioSource.enabled = true;
		rigidBody = base.gameObject.AddComponent<Rigidbody>();
		rigidBody.mass = mass * 0.001f;
		rigidBody.drag = drag;
		rigidBody.angularDrag = angularDrag;
		rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
		rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		rigidBody.maxAngularVelocity = float.PositiveInfinity;
		rigidBody.AddForce(Force, ForceMode.VelocityChange);
		rigidBody.AddTorque(Torque, ForceMode.Impulse);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (audioSource != null && rigidBody.velocity.sqrMagnitude > 0.01f)
		{
			audioSource.PlayOneShot(bounceAudio[Random.Range(0, bounceAudio.Count)]);
		}
	}

	private void OnCollisionStay(Collision colision)
	{
		if (rigidBody.velocity.sqrMagnitude < 0.01f)
		{
			if (base.transform.position.z < -2.8f && base.transform.position.y > 0.5f)
			{
				Object.Destroy(base.gameObject);
			}
			Disable();
		}
	}

	private void Disable()
	{
		Object.Destroy(audioSource);
		Object.Destroy(rigidBody);
		Object.Destroy(this);
	}
}

}
