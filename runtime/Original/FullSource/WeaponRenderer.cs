namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class WeaponRenderer : MonoBehaviour
{
	public Camera renderCamera;

	public Light renderLight;

	private int count;

	public void Render(WeaponBase weapon, RenderTexture texture)
	{
		if (!(renderCamera == null) && !(renderLight == null))
		{
			Vector3 position = weapon.transform.position;
			Quaternion rotation = weapon.transform.rotation;
			Rect rect = weapon.RenderBounds();
			float num = rect.width / rect.height;
			float num2 = (float)texture.width / (float)texture.height;
			texture.name = string.Format("Weapon render {0}", count);
			if (num2 >= num)
			{
				renderCamera.orthographicSize = (rect.height + 0.01f) * 0.5f;
			}
			else
			{
				renderCamera.orthographicSize = (rect.width + 0.01f) * 0.5f / num2;
			}
			Vector3 localPosition = weapon.transform.GetChild(0).localPosition;
			float num3 = 0.5f;
			Vector3 vector = new Vector3(rect.center.x, rect.center.y, 0f);
			Vector3 vector2 = new Vector3(0f, 0f, num3);
			weapon.transform.position = renderCamera.transform.position - vector + vector2;
			weapon.transform.rotation = renderCamera.transform.rotation;
			renderCamera.targetTexture = texture;
			renderCamera.nearClipPlane = num3 - 0.05f;
			renderCamera.farClipPlane = num3 + 0.05f;
			renderLight.enabled = true;
			renderCamera.Render();
			renderLight.enabled = false;
			count++;
			weapon.transform.position = position;
			weapon.transform.rotation = rotation;
		}
	}
}

}
