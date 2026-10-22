namespace Crisis.Runtime.FullSource
{
using UnityEngine.Events;

public class GetLatestVersion : HTTPPost
{
	public GetLatestVersion(UnityAction<HTTPRequest> callback)
		: base(callback)
	{
		Append("1");
	}
}

}
