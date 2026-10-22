namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class Version
{
	private SerializedVersion version;

	private GameObject versionPrefabObject;

	private static string Filename = "version.txt";

	private static int MajorDigits = 1;

	private static int MinorDigits = 1;

	private static int PatchDigits = 1;

	private static int BuildDigits = 3;

	private static Version instance = null;

	private static readonly object padlock = new object();

	public string VersionString
	{
		get
		{
			return string.Format("{0}.{1}.{2}.{3}", version.major.ToString().PadLeft(MajorDigits, '0'), version.minor.ToString().PadLeft(MinorDigits, '0'), version.patch.ToString().PadLeft(PatchDigits, '0'), version.build.ToString().PadLeft(BuildDigits, '0'));
		}
	}

	public string VersionStringWithTags
	{
		get
		{
			return string.Format("{0}.{1}.{2}.{3}{4}", version.major.ToString().PadLeft(MajorDigits, '0'), version.minor.ToString().PadLeft(MinorDigits, '0'), version.patch.ToString().PadLeft(PatchDigits, '0'), version.build.ToString().PadLeft(BuildDigits, '0'), ((!version.candidate) ? string.Empty : string.Format("-rc{0}", version.candidateNumber)) + ((!version.testVersion) ? string.Empty : "-tv"));
		}
	}

	public string GameTitle
	{
		get
		{
			return "Crisis Response";
		}
	}

	public int Major
	{
		get
		{
			return version.major;
		}
	}

	public int Minor
	{
		get
		{
			return version.minor;
		}
	}

	public int Patch
	{
		get
		{
			return version.patch;
		}
	}

	public int Build
	{
		get
		{
			return version.build;
		}
	}

	public string Codename
	{
		get
		{
			return version.codename;
		}
	}

	public bool Candidate
	{
		get
		{
			return version.candidate;
		}
	}

	public int CandidateNumber
	{
		get
		{
			return version.candidateNumber;
		}
	}

	public bool Staging
	{
		get
		{
			return version.staging;
		}
	}

	public static Version Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new Version();
					instance.LoadPrefab();
				}
				return instance;
			}
		}
	}

	private void LoadPrefab()
	{
		versionPrefabObject = Resources.Load("Version") as GameObject;
		if (versionPrefabObject != null)
		{
			version = versionPrefabObject.GetComponent<SerializedVersion>();
		}
	}
}

}
