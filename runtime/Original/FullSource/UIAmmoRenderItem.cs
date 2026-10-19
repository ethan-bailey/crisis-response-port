namespace Crisis.Runtime.FullSource
{
using Object = UnityEngine.Object;
using UnityEngine;

public class UIAmmoRenderItem : MonoBehaviour
{
	[Tooltip("Distance from the camera the model will be rendered at.")]
	public float distance = 0.2f;

	[Tooltip("Center offset for rotation.")]
	public float offset;

	public GameObject bullet;

	[Tooltip("Distance from the camera the bullet model will be rendered at.")]
	public float bulletDistance = 0.1f;

	[Tooltip("Center offset for bullet's rotation.")]
	public float bulletOffset;

	public GameObject Bullet { get; private set; }

	public void InstantiateBullet(Transform parent)
	{
		if (!(bullet == null) && Bullet == null)
		{
			Bullet = Object.Instantiate(bullet);
			Bullet.transform.SetParent(parent);
			Bullet.transform.localPosition = Bullet.transform.InverseTransformVector(new Vector3(0f, 0f, bulletOffset));
			Bullet.transform.eulerAngles = new Vector3(-45f, 0f, 0f);
		}
	}

	public void Display(bool display)
	{
		base.gameObject.SetActive(display);
		if (Bullet != null)
		{
			Bullet.SetActive(display);
		}
	}

	private void OnDestroy()
	{
		if (Bullet != null)
		{
			Object.Destroy(Bullet);
		}
	}
}

}
