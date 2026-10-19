namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class DroppedWeaponProxy : MonoBehaviour
{
	public WeaponBase weapon;

	public void Initialize(WeaponBase weapon, Transform parent)
	{
		this.weapon = weapon;
		base.transform.SetParent(parent);
		base.transform.position = new Vector3(weapon.Position, 0.25f, -1.5f);
		GetComponent<BoxCollider>().size = Vector3.one * 0.4f;
		base.gameObject.layer = LayerMask.NameToLayer("Interactables");
		base.name = string.Format("{0} proxy", weapon.name);
	}

	private void Update()
	{
		if (weapon != null)
		{
			if (weapon.Owner == null)
			{
				base.transform.position = new Vector3(weapon.Position, 0.25f, -1.5f);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}

}
