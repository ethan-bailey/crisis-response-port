namespace Crisis.Runtime.FullSource
{
using UnityEngine.Events;
using Usage;

public class PostSession : HTTPPost
{
	public PostSession(bool full, string guid, SerializableSessionData data, UnityAction<HTTPRequest> callback = null)
		: base(callback)
	{
		Append("2");
		Append(guid);
		if (full)
		{
			Append("{0}", data.id);
			Append("{0}", data.duration);
			Append("{0}", data.averageScore);
			Append("{0}", data.missions);
			Append("{0}", data.finished);
			Append("{0}", data.successful);
			Append("{0}", data.failed);
			Append("{0}", data.restarted);
			Append("{0}", data.aborted);
			Append("{0}", data.operatorTutorials);
			Append("{0}", data.teamTutorials);
			Append("{0}", data.marksmanTutorials);
			Append("{0}", data.lowFrameRateAverage);
			Append("{0}", data.highFrameRateAverage);
			Append("{0}", data.clicksIDB);
			Append("{0}", data.clicksFB);
			Append("{0}", data.clicksTw);
			Append("{0}", data.clicksYt);
			Append("{0}", data.clicksCRG);
			Append("{0}", data.clicksDL);
		}
	}
}

}
