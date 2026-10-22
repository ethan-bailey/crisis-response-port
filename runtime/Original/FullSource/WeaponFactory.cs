namespace Crisis.Runtime.FullSource
{
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;

public class WeaponFactory
{
	private static WeaponFactory instance = null;

	private static readonly object padlock = new object();

	private const string NodeFirearm = "firearm";

	private const string NodeTransform = "transform";

	private const string NodeQuad = "quad";

	private const string NodeBolt = "bolt";

	private const string AttributeName = "name";

	private const string AttributeAction = "action";

	private const string AttributeTexture = "texture";

	private const string AttributeOffset = "offset";

	private const string AttributeSize = "size";

	private const string AttributeUVs = "uvs";

	private const string AttributeBoltRange = "range";

	private const string AttributeBoltOpenTime = "open";

	private const string AttributeBoltCloseTime = "close";

	private const char Delimiter = ';';

	public static WeaponFactory Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new WeaponFactory();
				}
				return instance;
			}
		}
	}

	public static EditorWeapon CreateWeapon(XmlDocument data)
	{
		if (data == null)
		{
			return null;
		}
		XmlNode xmlNode = data.SelectSingleNode(string.Format("//{0}", "firearm"));
		string value = xmlNode.Attributes["name"].Value;
		string value2 = xmlNode.Attributes["action"].Value;
		string value3 = xmlNode.Attributes["texture"].Value;
		Texture2D texture = ResourceManager.Instance.GetTexture(value3, true);
		Material material = ResourceManager.Instance.GetMaterial(value3);
		GameObject gameObject = new GameObject(value);
		EditorWeapon editorWeapon = gameObject.AddComponent<EditorWeapon>();
		AssignClass(gameObject, value2);
		editorWeapon.texturePath = value3;
		editorWeapon.texture = texture;
		editorWeapon.material = material;
		RecursiveParseChildren(xmlNode, editorWeapon, gameObject.transform);
		return gameObject.GetComponent<EditorWeapon>();
	}

	public static void Traverse(Transform parent, UnityAction<Transform> callback)
	{
		foreach (Transform item in parent)
		{
			callback(item);
			Traverse(item, callback);
		}
	}

	private static bool AssignClass(GameObject weapon, string weaponClass)
	{
		if (weaponClass != null && weaponClass == "short-recoil")
		{
			weapon.AddComponent<FirearmShortRecoil>();
			return true;
		}
		Debug.LogWarningFormat("No valid class assigned for firearm '{0}'. Construction of the firearm fails.", weapon.name);
		return false;
	}

	private static void RecursiveParseChildren(XmlNode parent, EditorWeapon weapon, Transform parentTransform)
	{
		foreach (XmlNode childNode in parent.ChildNodes)
		{
			Transform parentTransform2 = null;
			switch (childNode.Name)
			{
			case "transform":
				parentTransform2 = ParseTransform(childNode, parentTransform);
				break;
			case "quad":
				ParseQuad(childNode, weapon, parentTransform);
				break;
			case "bolt":
				ParseBolt(childNode, parentTransform);
				break;
			default:
				Debug.LogWarningFormat("Unexpected node '{0}' when parsing parent: '{1}'", childNode.Name, parent.Name);
				return;
			}
			RecursiveParseChildren(childNode, weapon, parentTransform2);
		}
	}

	private static Transform ParseTransform(XmlNode node, Transform parent)
	{
		string value = node.Attributes["name"].Value;
		float result = 0f;
		float result2 = 0f;
		float result3 = 0f;
		string value2 = node.Attributes["offset"].Value;
		string[] array = value2.Split(';');
		float.TryParse(array[0], out result);
		float.TryParse(array[1], out result2);
		float.TryParse(array[2], out result3);
		Vector3 localPosition = new Vector3(result, result2, result3);
		result = (result2 = (result3 = 0f));
		value2 = node.Attributes["size"].Value;
		array = value2.Split(';');
		float.TryParse(array[0], out result);
		float.TryParse(array[1], out result2);
		Vector3 localSize = new Vector3(result, result2, 1f);
		return CreateTransform(value, parent, localPosition, localSize);
	}

	private static void ParseQuad(XmlNode node, EditorWeapon weapon, Transform parent)
	{
		float result = 0f;
		float result2 = 0f;
		string value = node.Attributes["uvs"].Value;
		string[] array = value.Split(';');
		float.TryParse(array[0], out result);
		float.TryParse(array[1], out result2);
		Vector2 lowerLeftNormalized = new Vector2(result, result2);
		float.TryParse(array[2], out result);
		float.TryParse(array[3], out result2);
		Vector2 upperRightNormalized = new Vector2(result, result2);
		SerializableTextureQuad textureQuad = new SerializableTextureQuad(lowerLeftNormalized, upperRightNormalized, parent.name);
		CreateQuad(parent, textureQuad, weapon.material);
	}

	private static void ParseBolt(XmlNode node, Transform parent)
	{
		FirearmBolt firearmBolt = parent.gameObject.AddComponent<FirearmBolt>();
		string value = node.Attributes["range"].Value;
		float result = 0.05f;
		if (float.TryParse(value, out result))
		{
			firearmBolt.range = result;
		}
		value = node.Attributes["open"].Value;
		if (float.TryParse(value, out result))
		{
			firearmBolt.openTime = result;
		}
		value = node.Attributes["close"].Value;
		if (float.TryParse(value, out result))
		{
			firearmBolt.closeTime = result;
		}
	}

	public static EditorWeaponComponent AddComponent(EditorWeapon weapon, string name, SerializableTextureQuad textureQuad, Vector3 localPosition, Vector3 localSize)
	{
		GameObject gameObject = new GameObject(name);
		EditorWeaponComponent editorWeaponComponent = gameObject.AddComponent<EditorWeaponComponent>();
		Mesh mesh = CreateComponentMesh(textureQuad, "Quad");
		editorWeaponComponent.Initialize(mesh, weapon.texture, textureQuad, weapon.material, weapon.transform, localPosition, localSize);
		return editorWeaponComponent;
	}

	public static EditorWeaponComponent CreateComponent(string name, Texture2D texture, Material material, SerializableTextureQuad textureQuad, Transform parent, Vector3 localPosition, Vector3 localSize)
	{
		GameObject gameObject = new GameObject(name);
		EditorWeaponComponent editorWeaponComponent = gameObject.AddComponent<EditorWeaponComponent>();
		Mesh mesh = CreateComponentMesh(textureQuad, "Quad");
		editorWeaponComponent.Initialize(mesh, texture, textureQuad, material, parent, localPosition, localSize);
		return editorWeaponComponent;
	}

	public static Transform CreateTransform(string name, Transform parent, Vector3 localPosition, Vector3 localSize)
	{
		GameObject gameObject = new GameObject(name);
		Transform transform = gameObject.transform;
		transform.SetParent(parent);
		transform.localPosition = localPosition;
		transform.localScale = localSize;
		return transform;
	}

	public static void CreateQuad(Transform parent, SerializableTextureQuad textureQuad, Material material)
	{
		GameObject gameObject = parent.gameObject;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = CreateComponentMesh(textureQuad, "Quad");
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = material;
		gameObject.AddComponent<MeshCollider>();
	}

	public static Mesh CreateComponentMesh(SerializableTextureQuad uvQuad, string name)
	{
		List<Vector3> list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		list.Add(new Vector3(1f, -1f, 0f));
		list.Add(new Vector3(0f, -1f, 0f));
		List<Vector3> list2 = list;
		List<int> list3 = new List<int>();
		list3.Add(0);
		list3.Add(1);
		list3.Add(2);
		list3.Add(0);
		list3.Add(2);
		list3.Add(3);
		List<int> list4 = list3;
		List<Vector2> list5 = new List<Vector2>();
		list5.Add(uvQuad.upperLeft);
		list5.Add(uvQuad.upperRight);
		list5.Add(uvQuad.lowerRight);
		list5.Add(uvQuad.lowerLeft);
		List<Vector2> list6 = list5;
		Mesh mesh = new Mesh();
		mesh.name = name;
		mesh.vertices = list2.ToArray();
		mesh.triangles = list4.ToArray();
		mesh.uv = list6.ToArray();
		Mesh mesh2 = mesh;
		mesh2.RecalculateNormals();
		return mesh2;
	}

	public static Material CreateMaterial(Texture2D texture)
	{
		Material material = Object.Instantiate(ResourceManager.Instance.WeaponMaterialPrefab);
		material.mainTexture = texture;
		return material;
	}

	public static bool Serialize(EditorWeapon weapon, string path)
	{
		string file = path + "\\" + weapon.name + ".xml";
		XmlWriter xmlWriter = XmlWriter.Create(file, new XmlWriterSettings
		{
			Indent = true
		});
		if (xmlWriter == null)
		{
			return false;
		}
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteStartElement("weapon");
		xmlWriter.WriteAttributeString("name", weapon.name);
		xmlWriter.WriteAttributeString("texture", weapon.texturePath);
		foreach (Transform item in weapon.transform)
		{
			EditorWeaponComponent component = item.GetComponent<EditorWeaponComponent>();
			if ((bool)component)
			{
				component.Serialize(xmlWriter);
			}
		}
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndDocument();
		xmlWriter.Close();
		return true;
	}
}

}
