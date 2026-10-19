namespace Crisis.Runtime.FullSource
{
using UnityEngine.Events;

public class PostFeedbackQuery : HTTPPost
{
	public PostFeedbackQuery(string guid, UnityAction<HTTPRequest> callback = null)
		: base(callback)
	{
		Append("5");
		Append(guid);
	}
}

}
