namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeSpoon : MonoBehaviour
{
	public List<AudioClip> bounceAudio;

	private AudioSource audioSource;

	private Rigidbody rigidBody;

	private Transform container;

	private bool isThrown;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		audioSource.priority = 100;
		container = Global.Context.Mission.CasingsContainer;
	}

	private void Update()
	{
		if (float.IsNaN(base.transform.position.x))
		{
			Object.Destroy(base.gameObject);
		}
		if (base.transform.position.y < -0.5f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Throw(Vector3 velocity)
	{
		if (!isThrown)
		{
			isThrown = true;
			base.transform.SetParent(container, true);
			audioSource.enabled = true;
			rigidBody = base.gameObject.AddComponent<Rigidbody>();
			rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
			rigidBody.mass = 0.01f;
			rigidBody.drag = 0.1f;
			rigidBody.angularDrag = 0.1f;
			rigidBody.isKinematic = false;
			rigidBody.AddForce(velocity, ForceMode.VelocityChange);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (audioSource != null && rigidBody != null && rigidBody.velocity.sqrMagnitude > 0.01f)
		{
			audioSource.PlayOneShot(bounceAudio[Random.Range(0, bounceAudio.Count)]);
		}
	}

	private void OnCollisionStay(Collision colision)
	{
		if (rigidBody != null && rigidBody.velocity.sqrMagnitude < 0.01f)
		{
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
