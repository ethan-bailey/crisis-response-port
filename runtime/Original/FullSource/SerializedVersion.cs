namespace Crisis.Runtime.FullSource
{
using UnityEngine;

public class SerializedVersion : MonoBehaviour
{
	public const string PrefabName = "Version";

	public int major;

	public int minor;

	public int patch;

	public int build;

	public string codename;

	public bool staging;

	public bool candidate;

	public int candidateNumber;

	public bool testVersion;

	public void Set(int major, int minor, int patch, int build)
	{
		this.major = major;
		this.minor = minor;
		this.patch = patch;
		this.build = build;
	}
}

}
