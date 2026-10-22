namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public sealed class Global
{
	public const string PrefabName = "Global context";

	private static Global instance = null;

	private static readonly object padlock = new object();

	private static GameObject globalObject;

	public bool Initialized
	{
		get
		{
			return globalObject != null;
		}
	}

	public static Global Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new Global();
				}
				return instance;
			}
		}
	}

	public static GameObject Object
	{
		get
		{
			return globalObject;
		}
	}

	public static GlobalContext Context
	{
		get
		{
			return (!(globalObject != null)) ? null : globalObject.GetComponent<GlobalContext>();
		}
	}

	public void Initialize(GameObject globalContext)
	{
		globalObject = globalContext;
	}

	public static bool NewerVersionAvailable()
	{
		return VersionIsNewer(Context.AvailableVersion);
	}

	public static bool VersionIsNewer(string version)
	{
		if (string.IsNullOrEmpty(version))
		{
			return false;
		}
		Context.AvailableVersion = version;
		string[] array = Version.Instance.VersionString.Split('.');
		string[] array2 = version.Split('.');
		if (array.Length != 4 || array2.Length != 4)
		{
			return false;
		}
		for (int i = 0; i < 4; i++)
		{
			int result;
			int result2;
			if (!int.TryParse(array[i], out result) || !int.TryParse(array2[i], out result2))
			{
				return false;
			}
			if (result != result2)
			{
				return result < result2;
			}
		}
		return false;
	}
}

}
