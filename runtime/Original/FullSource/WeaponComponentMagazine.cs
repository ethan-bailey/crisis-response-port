namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class WeaponComponentMagazine : WeaponComponent
{
	public int capacity;

	[Range(0f, 1f)]
	public float malfunctionRate;

	public void Drop()
	{
		GameObject gameObject = Object.Instantiate(base.gameObject);
		gameObject.transform.SetParent(null);
		gameObject.transform.position = base.transform.position;
		gameObject.transform.rotation = base.transform.rotation;
		base.gameObject.SetActive(false);
		Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
		rigidbody.constraints = (RigidbodyConstraints)56;
		rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
	}
}

}
