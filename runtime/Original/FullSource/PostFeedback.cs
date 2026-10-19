namespace Crisis.Runtime.FullSource
{
using System;
using System.Text;
using UnityEngine.Events;

public class PostFeedback : HTTPPost
{
	public PostFeedback(string guid, string version, string message, string contact, UnityAction<HTTPRequest> callback = null)
		: base(callback)
	{
		Append("6");
		Append(guid);
		Append(version);
		Append(Convert.ToBase64String(Encoding.ASCII.GetBytes(message)));
		Append(Convert.ToBase64String(Encoding.ASCII.GetBytes(contact)));
	}
}

}
