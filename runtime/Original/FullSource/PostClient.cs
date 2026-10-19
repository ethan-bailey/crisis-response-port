namespace Crisis.Runtime.FullSource
{
using UnityEngine.Events;

public class PostClient : HTTPPost
{
	public PostClient(string guid, UnityAction<HTTPRequest> callback = null)
		: base(callback)
	{
		Append("0");
		Append(guid);
		Append(Version.Instance.VersionString);
		Append(Version.Instance.Codename);
	}
}

}
