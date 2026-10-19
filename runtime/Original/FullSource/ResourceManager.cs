namespace Crisis.Runtime.FullSource
{
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager
{
	private class AsyncAudioLoad
	{
		public string path;

		public UnityAction<AudioClip> callback;
	}

	private static ResourceManager instance = null;

	private static readonly object padlock = new object();

	private Dictionary<string, XmlDocument> weapons = new Dictionary<string, XmlDocument>();

	private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

	private Dictionary<string, Material> materials = new Dictionary<string, Material>();

	private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

	private Material weaponMaterialPrefab;

	private List<AsyncAudioLoad> asyncAudioJobs = new List<AsyncAudioLoad>();

	private bool asyncLoadInProgress;

	public static ResourceManager Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new ResourceManager();
				}
				return instance;
			}
		}
	}

	public Material WeaponMaterialPrefab
	{
		get
		{
			if (weaponMaterialPrefab == null)
			{
				Debug.LogWarning("Accessing the weapon factory's material prefab before initialization!");
				return null;
			}
			return weaponMaterialPrefab;
		}
		private set
		{
			weaponMaterialPrefab = value;
		}
	}

	public void Initialize(Material material)
	{
		WeaponMaterialPrefab = material;
	}

	public void FreeResources()
	{
		weapons.Clear();
		foreach (KeyValuePair<string, Texture2D> texture in textures)
		{
			Object.Destroy(texture.Value);
		}
		foreach (KeyValuePair<string, Material> material in materials)
		{
			Object.Destroy(material.Value);
		}
		foreach (KeyValuePair<string, AudioClip> audioClip in audioClips)
		{
			Object.Destroy(audioClip.Value);
		}
	}

	public Texture2D GetTexture(string path, bool createMaterial = false)
	{
		if (textures.ContainsKey(path))
		{
			return textures[path];
		}
		Texture2D texture2D = null;
		if (File.Exists(path))
		{
			byte[] data = File.ReadAllBytes(path);
			texture2D = new Texture2D(2, 2);
			if (texture2D.LoadImage(data))
			{
				textures.Add(path, texture2D);
				if (createMaterial && !materials.ContainsKey(path))
				{
					Material material = Object.Instantiate(WeaponMaterialPrefab);
					if (material != null)
					{
						material.mainTexture = texture2D;
						materials.Add(path, material);
					}
				}
			}
			else
			{
				Object.Destroy(texture2D);
			}
		}
		return texture2D;
	}

	public Material GetMaterial(string path)
	{
		if (materials.ContainsKey(path))
		{
			return Object.Instantiate(materials[path]);
		}
		return null;
	}

	public AudioClip GetAudioClip(string path, UnityAction<AudioClip> callback)
	{
		if (audioClips.ContainsKey(path))
		{
			return audioClips[path];
		}
		asyncAudioJobs.Add(new AsyncAudioLoad
		{
			path = path,
			callback = callback
		});
		return null;
	}

	public void UpdateAsyncJobs()
	{
		if (!asyncLoadInProgress && asyncAudioJobs.Count != 0)
		{
			AsyncAudioLoad job = asyncAudioJobs[0];
			asyncAudioJobs.RemoveAt(0);
			Global.Context.StartCoroutine(AsyncLoadAudio(job));
			asyncLoadInProgress = true;
		}
	}

	private IEnumerator AsyncLoadAudio(AsyncAudioLoad job)
	{
		string url = "file://" + job.path;
		WWW www = new WWW(url);
		yield return www;
		OnAsyncAudioLoaded(clip: www.GetAudioClip(false, false), path: job.path, callback: job.callback);
	}

	private void OnAsyncAudioLoaded(string path, AudioClip clip, UnityAction<AudioClip> callback)
	{
		asyncLoadInProgress = false;
		audioClips.Add(path, clip);
		if (callback != null)
		{
			callback(clip);
		}
	}

	public EditorWeapon GetWeapon(string path)
	{
		if (weapons.ContainsKey(path))
		{
			return WeaponFactory.CreateWeapon(weapons[path]);
		}
		if (!File.Exists(path))
		{
			return null;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(path);
		weapons.Add(path, xmlDocument);
		return WeaponFactory.CreateWeapon(xmlDocument);
	}
}

}
