using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class DynamicInteractionMarker : MonoBehaviour
{
	public Transform entity;

	private void Update()
	{
		if (!(entity == null))
		{
			base.transform.position = new Vector3(entity.position.x, 0f, -2f);
		}
	}
}

}
