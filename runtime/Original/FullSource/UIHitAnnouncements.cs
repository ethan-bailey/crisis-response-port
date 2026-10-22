namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine;

public class UIHitAnnouncements : MonoBehaviour
{
	public Transform container;

	public UIHitAnnouncement instancePrefab;

	private Camera interiorCamera;

	private Camera marksmanCamera;

	private void Start()
	{
		InteriorMode interiorMode = Global.Context.Mission.InteriorMode;
		MarksmanMode marksmanMode = Global.Context.Mission.MarksmanMode;
		interiorCamera = ((!(interiorMode != null)) ? null : interiorMode.interiorCamera.interiorCamera);
		marksmanCamera = ((!(marksmanMode != null)) ? null : marksmanMode.marksmanCamera.marksmanCamera);
	}

	public void Announce(string text, Projectile projectile)
	{
		if (!(container == null) && !(instancePrefab == null))
		{
			Camera camera = null;
			if (projectile.FiredByMarksman && marksmanCamera != null)
			{
				camera = marksmanCamera;
			}
			else if (interiorCamera != null)
			{
				camera = interiorCamera;
			}
			if (!(camera == null))
			{
				UIHitAnnouncement uIHitAnnouncement = Object.Instantiate(instancePrefab);
				uIHitAnnouncement.Text = text;
				uIHitAnnouncement.transform.SetParent(container);
				uIHitAnnouncement.Initialize(camera.WorldToScreenPoint(projectile.transform.position));
			}
		}
	}
}

}
