namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class ControlRenderOrder : MonoBehaviour
{
	public int renderOrder = 3000;

	private void Start()
	{
		Object.Destroy(this);
	}
}

}
