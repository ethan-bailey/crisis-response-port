using Object = UnityEngine.Object;
using UnityEngine;

public class ObjectRemover : MonoBehaviour
{
	[Tooltip("Child objects will be removed when their count exceeds this.")]
	public int maxObjects = 200;

	private void Update()
	{
		if (base.transform.childCount != 0 && base.transform.childCount > maxObjects)
		{
			Object.Destroy(base.transform.GetChild(0).gameObject);
		}
	}
}
