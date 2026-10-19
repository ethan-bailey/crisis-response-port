namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class MarksmanRound : MonoBehaviour
{
	public GameObject projectile;

	public GameObject primerIntact;

	public GameObject primerStruck;

	public float fullMass = 0.025f;

	public float emptyMass = 0.006f;

	public List<AudioClip> bounces = new List<AudioClip>();

	private AudioSource audioSource;

	private Rigidbody rigidBody;

	private float timestamp = float.PositiveInfinity;

	private float extent;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		rigidBody = GetComponent<Rigidbody>();
		rigidBody.maxAngularVelocity = float.PositiveInfinity;
		extent = GetComponent<Collider>().bounds.extents.x;
	}

	private void Update()
	{
		if (Time.time - timestamp > 3f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Detonate()
	{
		if (projectile != null)
		{
			Object.Destroy(projectile);
			Object.Destroy(primerIntact);
			primerStruck.SetActive(true);
		}
	}

	public void Eject(Vector3 force, Vector3 torque)
	{
		rigidBody.isKinematic = false;
		rigidBody.AddForce(force, ForceMode.VelocityChange);
		rigidBody.AddTorque(torque, ForceMode.Impulse);
		timestamp = Time.time;
	}

	public void Eject(float forceScale, Transform bolt, Vector3 axis)
	{
		rigidBody.mass = ((!(projectile == null)) ? fullMass : emptyMass);
		rigidBody.isKinematic = false;
		Vector3 vector = axis * forceScale;
		Vector3 vector2 = base.transform.TransformPoint(new Vector3(extent, 0f, 0f));
		rigidBody.AddForceAtPosition(vector, vector2, ForceMode.Impulse);
		Debug.DrawLine(vector2, vector2 + vector * 10f, Color.red, 5f);
		timestamp = Time.time;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (bounces.Count != 0)
		{
			float magnitude = collision.relativeVelocity.magnitude;
			audioSource.PlayOneShot(bounces[Random.Range(0, bounces.Count)], magnitude);
		}
	}
}

}
