namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;
using UnityEngine.UI;

public class UIGrenadeTimer : MonoBehaviour
{
	public Slider slider;

	private Grenade grenade;

	private Camera interiorCamera;

	public void Initialize(Grenade grenade)
	{
		base.name = "Grenade timer UI";
		this.grenade = grenade;
		grenade.onDetonate.AddListener(OnDetonation);
		Mission mission = Global.Context.Mission;
		base.transform.SetParent(mission.UI.transform);
		interiorCamera = mission.InteriorMode.interiorCamera.interiorCamera;
		SetPosition(grenade.transform.position);
	}

	private void Update()
	{
		slider.value = grenade.SecondsLeftOnFuse / grenade.OriginalFuse;
		SetPosition(grenade.transform.position);
	}

	private void SetPosition(Vector3 worldPosition)
	{
		Vector3 vector = interiorCamera.WorldToScreenPoint(worldPosition);
		base.transform.position = new Vector3(vector.x, vector.y, 0f);
	}

	private void OnDetonation()
	{
		Object.Destroy(base.gameObject);
	}
}

}
