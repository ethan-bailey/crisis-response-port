namespace Crisis.Runtime.FullSource
{
using UnityEngine.Events;

public class PostClose : HTTPPost
{
	public PostClose(uint sessionId, UnityAction<HTTPRequest> callback = null)
		: base(callback)
	{
		Append("4");
		Append("{0}", sessionId);
	}
}

}
