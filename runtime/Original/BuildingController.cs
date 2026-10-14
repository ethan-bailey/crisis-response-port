using Object = UnityEngine.Object;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
	public bool powered = true;

	public void SetPower(bool state)
	{
		Light[] array = Object.FindObjectsOfType<Light>();
		Light[] array2 = array;
		foreach (Light light in array2)
		{
			Transform parent = light.transform.parent;
			if (!(parent == null) && parent.gameObject.isStatic)
			{
				light.enabled = state;
			}
		}
		powered = state;
	}
}
