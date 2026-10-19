namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using UnityEngine;

public class EditorWeapon : MonoBehaviour
{
	public string texturePath;

	public Texture2D texture;

	public Material material;

	private Firearm firearm;

	private void Awake()
	{
		firearm = GetComponent<Firearm>();
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		Object.Destroy(material);
	}

	public List<SerializableTextureQuad> ComponentTextureQuads()
	{
		List<SerializableTextureQuad> list = new List<SerializableTextureQuad>();
		RecursiveFindTextureQuads(base.transform, ref list);
		return list;
	}

	public void DeselectComponents()
	{
		RecursiveDeselectComponents(base.transform);
	}

	private void RecursiveFindTextureQuads(Transform parent, ref List<SerializableTextureQuad> list)
	{
		foreach (Transform item in parent)
		{
			EditorWeaponComponent component = item.GetComponent<EditorWeaponComponent>();
			if (component != null && component.TextureQuad != null)
			{
				list.Add(component.TextureQuad);
			}
			RecursiveFindTextureQuads(item, ref list);
		}
	}

	private void RecursiveDeselectComponents(Transform parent)
	{
		foreach (Transform item in parent)
		{
			RecursiveDeselectComponents(item);
			EditorWeaponComponent component = item.GetComponent<EditorWeaponComponent>();
			if ((bool)component)
			{
				component.SetSelected(false);
			}
		}
	}
}

}
