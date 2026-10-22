namespace Crisis.Runtime.FullSource
{
using System.Xml;
using UnityEngine;

public class EditorWeaponComponent : MonoBehaviour
{
	public bool Selected { get; private set; }

	public Texture2D Texture { get; private set; }

	public Material Material { get; private set; }

	public SerializableTextureQuad TextureQuad { get; private set; }

	public MeshFilter MeshFilter { get; private set; }

	public MeshRenderer Renderer { get; private set; }

	private void OnDestroy()
	{
		Object.Destroy(Material);
	}

	public void Initialize(Mesh mesh, Texture2D texture, SerializableTextureQuad quad, Material material, Transform parent, Vector3 localPosition, Vector3 localScale)
	{
		Texture = texture;
		TextureQuad = quad;
		Material = material;
		MeshFilter = base.gameObject.AddComponent<MeshFilter>();
		MeshFilter.mesh = mesh;
		Material.mainTexture = texture;
		Renderer = base.gameObject.AddComponent<MeshRenderer>();
		Renderer.material = Material;
		base.transform.SetParent(parent);
		base.transform.localPosition = localPosition;
		base.transform.localScale = localScale;
		base.gameObject.AddComponent<MeshCollider>();
		Selected = true;
	}

	public void SetName(string newName)
	{
		base.name = newName;
		if (TextureQuad != null)
		{
			TextureQuad.label = newName;
		}
	}

	public void SetSelected(bool selected)
	{
		if (selected != Selected)
		{
			Color color = Renderer.material.color;
			color.a = ((!selected) ? 0.5f : 1f);
			Renderer.material.color = color;
		}
		Selected = selected;
	}

	public void Serialize(XmlWriter writer)
	{
		RecursiveSerialize(writer, base.transform);
	}

	private void RecursiveSerialize(XmlWriter writer, Transform parent)
	{
		EditorWeaponComponent component = parent.GetComponent<EditorWeaponComponent>();
		writer.WriteStartElement((!(component != null)) ? "transform" : "component");
		writer.WriteAttributeString("name", parent.name);
		writer.WriteAttributeString("offset", string.Format("{0};{1};{2}", base.transform.localPosition.x, base.transform.localPosition.y, base.transform.localPosition.z));
		writer.WriteAttributeString("size", string.Format("{0};{1}", base.transform.localScale.x, base.transform.localScale.y));
		if (component != null)
		{
			writer.WriteAttributeString("uvs", string.Format("{0};{1};{2};{3}", component.TextureQuad.lowerLeft.x, component.TextureQuad.lowerLeft.y, component.TextureQuad.upperRight.x, component.TextureQuad.upperRight.y));
		}
		foreach (Transform item in parent)
		{
			RecursiveSerialize(writer, item);
		}
		writer.WriteEndElement();
	}
}

}
